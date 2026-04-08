using AJAI_Server.Data;
using AJAI_Server.Dto;
using AJAI_Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AJAI_Server.Controllers
{
    public class AiResult
    {
        public bool IsThreat { get; set; } = false;
        public string ThreatName { get; set; } = string.Empty;
        public string AnnotatedImage { get; set; } = string.Empty;
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _scopeFactory;

        private const string AI_URL = "http://172.20.10.7:8000/analyze";
        private const int CONFIRM_THRESHOLD = 3;
        private const int COOLDOWN_MINUTES = 2;

        private static readonly Dictionary<string, Queue<bool>> _threatBuffer = new();
        private static readonly Dictionary<string, DateTime> _lastAlertTime = new();

        public AlertsController(
            IHubContext<DashboardHub> hubContext,
            IHttpClientFactory httpClientFactory,
            IServiceScopeFactory scopeFactory)
        {
            _hubContext = hubContext;
            _httpClient = httpClientFactory.CreateClient();
            _scopeFactory = scopeFactory;
        }

        // ══════════════════════════════════════════════════════════
        // POST /api/alerts — تنبيه مباشر
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> CreateAlert(CreateAlertDto request)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var alert = new Alert
            {
                CameraEmail = request.CameraEmail,
                AlertType = request.AlertType,
                Base64Image = request.Base64Image,
                CreatedAt = DateTime.Now
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNewAlert", alert);
            return Ok(new { Message = "Alert saved and broadcasted" });
        }

        // ══════════════════════════════════════════════════════════
        // GET /api/alerts
        // ══════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var alerts = await context.Alerts
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ToListAsync();
            return Ok(alerts);
        }

        // ══════════════════════════════════════════════════════════
        // DELETE /api/alerts/{id} — حذف تنبيه واحد
        // ══════════════════════════════════════════════════════════
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var alert = await context.Alerts.FindAsync(id);
            if (alert == null)
                return NotFound(new { Message = "Alert not found" });

            context.Alerts.Remove(alert);
            await context.SaveChangesAsync();

            return Ok(new { Message = $"Alert {id} deleted" });
        }

        // ══════════════════════════════════════════════════════════
        // DELETE /api/alerts — حذف كل التنبيهات
        // ══════════════════════════════════════════════════════════
        [HttpDelete]
        public async Task<IActionResult> DeleteAllAlerts()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var count = await context.Alerts.CountAsync();
            context.Alerts.RemoveRange(context.Alerts);
            await context.SaveChangesAsync();

            return Ok(new { Message = $"Deleted {count} alerts" });
        }

        // ══════════════════════════════════════════════════════════
        // POST /api/alerts/stream
        // ══════════════════════════════════════════════════════════
        [HttpPost("stream")]
        public async Task<IActionResult> UpdateLiveFrame(CreateAlertDto request)
        {
            _ = Task.Run(() => AnalyzeInBackground(request.CameraEmail, request.Base64Image));
            return Ok();
        }

        // ══════════════════════════════════════════════════════════
        // AnalyzeInBackground
        // ══════════════════════════════════════════════════════════
        private async Task AnalyzeInBackground(string email, string base64Image)
        {
            try
            {
                var aiResult = await CallAI(base64Image);

                await _hubContext.Clients.All.SendAsync("ReceiveLiveFrame", new
                {
                    cameraEmail = email,
                    base64Image = aiResult.AnnotatedImage
                });

                bool confirmed;
                lock (_threatBuffer)
                {
                    if (!_threatBuffer.ContainsKey(email))
                        _threatBuffer[email] = new Queue<bool>();

                    var buffer = _threatBuffer[email];
                    buffer.Enqueue(aiResult.IsThreat);

                    while (buffer.Count > CONFIRM_THRESHOLD)
                        buffer.Dequeue();

                    confirmed = buffer.Count == CONFIRM_THRESHOLD
                             && buffer.All(x => x == true);

                    if (confirmed)
                        buffer.Clear();
                    else if (!aiResult.IsThreat)
                        buffer.Clear();
                }

                if (!confirmed) return;

                var cooldownKey = $"{email}|{aiResult.ThreatName}";
                var now = DateTime.Now;

                lock (_lastAlertTime)
                {
                    if (_lastAlertTime.TryGetValue(cooldownKey, out var lastTime)
                        && (now - lastTime).TotalMinutes < COOLDOWN_MINUTES)
                        return;

                    _lastAlertTime[cooldownKey] = now;
                }

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var alert = new Alert
                {
                    CameraEmail = email,
                    AlertType = aiResult.ThreatName,
                    Base64Image = aiResult.AnnotatedImage,
                    CreatedAt = now
                };
                context.Alerts.Add(alert);
                await context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("ReceiveNewAlert", alert);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI Background] Error: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════
        // CallAI
        // ══════════════════════════════════════════════════════════
        private async Task<AiResult> CallAI(string base64Image)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(AI_URL, new
                {
                    base64Image = base64Image
                });

                if (!response.IsSuccessStatusCode)
                    return SafeDefault(base64Image);

                var result = await response.Content.ReadFromJsonAsync<AiResult>();
                return result ?? SafeDefault(base64Image);
            }
            catch
            {
                return SafeDefault(base64Image);
            }
        }

        private static AiResult SafeDefault(string base64Image) => new()
        {
            IsThreat = false,
            ThreatName = string.Empty,
            AnnotatedImage = base64Image
        };
    }
}
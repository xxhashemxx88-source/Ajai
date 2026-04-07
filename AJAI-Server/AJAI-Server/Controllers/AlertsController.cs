using AJAI_Server.Data;
using AJAI_Server.Dto;
using AJAI_Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AJAI_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class AlertsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<DashboardHub> _hubContext;

        public AlertsController(AppDbContext context, IHubContext<DashboardHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAlert(CreateAlertDto request)
        {
            // إنشاء كائن التنبيه لحفظه في الداتابيز
            var alert = new Alert
            {
                CameraEmail = request.CameraEmail,
                AlertType = request.AlertType,
                Base64Image = request.Base64Image,
                CreatedAt = DateTime.Now
            };

            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNewAlert", alert);

            return Ok(new { Message = "Alert saved and broadcasted successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            var alerts = await _context.Alerts
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ToListAsync();

            return Ok(alerts);
        }

        [HttpPost("stream")]
        public async Task<IActionResult> UpdateLiveFrame(CreateAlertDto request)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLiveFrame", new
            {
                cameraEmail = request.CameraEmail,
                base64Image = request.Base64Image
            });

            return Ok();
        }
    }
}
using AJAI_Server.Data;
using AJAI_Server.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AJAI_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                    return BadRequest("Information error");

                string token = CreateToken(user.Email, 24);

                return Ok(new
                {
                    Role = "User",
                    Token = token,
                    Message = "Logged in successfully"
                });
            }
            var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.Email == request.Email);

            if (camera != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, camera.Password))
                    return BadRequest("Information error");

                camera.IsActive = true;
                camera.LastPing = DateTime.Now;
                await _context.SaveChangesAsync();

                string token = CreateToken(camera.Email, 8760);

                return Ok(new
                {
                    Role = "Camera",
                    Token = token, 
                    Message = "Camera activated successfully"
                });
            }

            return BadRequest("Information error");
        }

        // ----------------------------------------------------
        // دالة توليد التوكن (محدثة لاستقبال مدة الانتهاء بالساعات)
        private string CreateToken(string email, int expireHours)
        {
            List<Claim> claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, email)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(expireHours), // استخدام المتغير الجديد هنا
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ----------------------------------------------------
        // دالة مساعدة لتوليد الـ JWT
        private string CreateToken(string email)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
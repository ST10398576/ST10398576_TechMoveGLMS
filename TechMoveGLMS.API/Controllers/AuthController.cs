using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;

namespace ST10398576_TechMoveGLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            // Demo validation — replace with DB user lookup later
            if (login.Username == "admin" && login.Password == "password")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, login.Username),
                    new Claim(ClaimTypes.Role, "Administrator")
                };

                var jwtKeyConfig = _config["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(jwtKeyConfig))
                {
                    return StatusCode(500, "JWT key is not configured. Set Jwt:Key in configuration to a Base64 or sufficiently long plain-text key.");
                }

                byte[] keyBytes;
                try
                {
                    // Try interpreting the configured key as Base64 (recommended)
                    keyBytes = Convert.FromBase64String(jwtKeyConfig);
                }
                catch (FormatException)
                {
                    // Fallback to UTF8 bytes for existing plain-text keys
                    keyBytes = Encoding.UTF8.GetBytes(jwtKeyConfig);
                }

                if (keyBytes.Length < 32) // 32 bytes == 256 bits
                {
                    return StatusCode(500, $"Configured JWT key is too short: {keyBytes.Length * 8} bits. Require at least 256 bits for HS256.");
                }

                var key = new SymmetricSecurityKey(keyBytes);
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Unauthorized();
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

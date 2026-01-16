using backend.Interfaces;
using backend.Models.Dtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _cfg;

        public AuthService(IConfiguration cfg) => _cfg = cfg;

        public async Task<AuthResponse?> LoginAsync(AuthRequest req)
        {
            if (req.Email != "demo@demo.com" || req.Password != "demo")
                return null;

            var key = _cfg["Jwt:SigningKey"]!;
            var minutes = int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "120");

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Email, req.Email)
        };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(minutes);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponse
            {
                AccessToken = tokenStr,
                ExpiresInSeconds = (int)TimeSpan.FromMinutes(minutes).TotalSeconds
            };
        }
    }
}

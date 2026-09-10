using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FantasyApp.Common.Interfaces;
using FantasyApp.Common.Settings;
using FantasyApp.Entity.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FantasyApp.Common.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        public (string Token, DateTime ExpiresAt) GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomBytes);
            var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            return (token, expiresAt);
        }
    }
}

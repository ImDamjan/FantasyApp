using System;
using FantasyApp.Entity.Models;

namespace FantasyApp.Common.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user);
        (string Token, DateTime ExpiresAt) GenerateRefreshToken();
    }
}

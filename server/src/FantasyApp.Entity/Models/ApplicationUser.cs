using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace FantasyApp.Entity.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}

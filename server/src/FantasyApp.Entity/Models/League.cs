using System;
using System.Collections.Generic;

namespace FantasyApp.Entity.Models
{
    public class League
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public bool IsOfficial { get; set; }

        public long? OwnerUserId { get; set; }
        public ApplicationUser? OwnerUser { get; set; }

        public int MaxMembers { get; set; } = 50;
        public DateTime CreatedAt { get; set; }

        public ICollection<LeagueMembership> Memberships { get; set; } = new List<LeagueMembership>();
    }
}

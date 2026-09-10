using System;

namespace FantasyApp.Entity.Models
{
    public class LeagueMembership
    {
        public long Id { get; set; }

        public long LeagueId { get; set; }
        public League? League { get; set; }

        public long UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}

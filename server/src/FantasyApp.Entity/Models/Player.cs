using System;

namespace FantasyApp.Entity.Models
{
    public class Player
    {
        public long Id { get; set; }
        public int FplId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string SecondName { get; set; } = string.Empty;
        public string WebName { get; set; } = string.Empty;

        public PlayerPosition Position { get; set; }

        public long TeamId { get; set; }
        public Team? Team { get; set; }

        public int PriceTenths { get; set; }
        public int TotalPoints { get; set; }
        public decimal Form { get; set; }
        public string Status { get; set; } = "a";
        public int? ChanceOfPlayingThisRound { get; set; }

        public DateTime LastSyncedAt { get; set; }
    }
}

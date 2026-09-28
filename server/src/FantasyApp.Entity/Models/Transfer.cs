using System;

namespace FantasyApp.Entity.Models
{
    public class Transfer
    {
        public long Id { get; set; }

        public long FantasyTeamId { get; set; }
        public FantasyTeam? FantasyTeam { get; set; }

        public long GameweekId { get; set; }
        public Gameweek? Gameweek { get; set; }

        public long PlayerOutId { get; set; }
        public Player? PlayerOut { get; set; }

        public long PlayerInId { get; set; }
        public Player? PlayerIn { get; set; }

        public bool WasFreeTransfer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

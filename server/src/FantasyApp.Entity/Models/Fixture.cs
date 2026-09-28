using System;

namespace FantasyApp.Entity.Models
{
    public class Fixture
    {
        public long Id { get; set; }
        public int FplId { get; set; }

        public long? GameweekId { get; set; }
        public Gameweek? Gameweek { get; set; }

        public long HomeTeamId { get; set; }
        public Team? HomeTeam { get; set; }

        public long AwayTeamId { get; set; }
        public Team? AwayTeam { get; set; }

        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }

        public DateTime? KickoffTime { get; set; }

        public int HomeDifficulty { get; set; }
        public int AwayDifficulty { get; set; }

        public bool IsFinished { get; set; }
    }
}

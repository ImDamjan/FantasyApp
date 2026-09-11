using System.Collections.Generic;
using FantasyApp.Entity.Dtos.Players;

namespace FantasyApp.Entity.Dtos.Points
{
    public class SquadPlayerPointsDto
    {
        public long PlayerId { get; set; }
        public string WebName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string TeamShortName { get; set; } = string.Empty;
        public decimal PriceMillions { get; set; }
        public bool IsStarting { get; set; }
        public bool IsCaptain { get; set; }
        public bool IsViceCaptain { get; set; }

        public int GameweekPoints { get; set; }
        public int Minutes { get; set; }
        public int GoalsScored { get; set; }
        public int Assists { get; set; }
        public int CleanSheets { get; set; }
        public int GoalsConceded { get; set; }
        public int Saves { get; set; }
        public int Bonus { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }

        public decimal Form { get; set; }
        public List<UpcomingFixtureDto> NextFixtures { get; set; } = new();
    }
}

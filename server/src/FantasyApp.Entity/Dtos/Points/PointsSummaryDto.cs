namespace FantasyApp.Entity.Dtos.Points
{
    public class PointsSummaryDto
    {
        public string TeamName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string CurrentGameweekName { get; set; } = string.Empty;
        public int CurrentGameweekPoints { get; set; }
        public int TotalPoints { get; set; }
        public int AverageGameweekPoints { get; set; }
        public int HighestGameweekPoints { get; set; }
        public int OverallRank { get; set; }
        public int TotalPlayers { get; set; }
    }
}

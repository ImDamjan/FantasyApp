namespace FantasyApp.Entity.Models
{
    public class PlayerGameweekStat
    {
        public long Id { get; set; }

        public long PlayerId { get; set; }
        public Player? Player { get; set; }

        public long GameweekId { get; set; }
        public Gameweek? Gameweek { get; set; }

        public int TotalPoints { get; set; }
        public int Minutes { get; set; }
        public int GoalsScored { get; set; }
        public int Assists { get; set; }
        public int CleanSheets { get; set; }
        public int GoalsConceded { get; set; }
        public int Saves { get; set; }
        public int Bonus { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
    }
}

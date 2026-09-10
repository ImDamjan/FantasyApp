using System.Collections.Generic;

namespace FantasyApp.Entity.Dtos.Leagues
{
    public class LeagueStandingEntryDto
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int GameweekPoints { get; set; }
        public int TotalPoints { get; set; }
        public int Rank { get; set; }
    }

    public class LeagueStandingsDto
    {
        public string LeagueName { get; set; } = string.Empty;
        public List<LeagueStandingEntryDto> Entries { get; set; } = new();
    }
}

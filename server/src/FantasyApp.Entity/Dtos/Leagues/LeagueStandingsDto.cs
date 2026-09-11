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
        /// <summary>Rank as of the previous gameweek; 0 if there is no previous gameweek to compare against.</summary>
        public int PreviousRank { get; set; }
    }

    public class LeagueStandingsDto
    {
        public string LeagueName { get; set; } = string.Empty;
        public List<LeagueStandingEntryDto> Entries { get; set; } = new();
    }
}

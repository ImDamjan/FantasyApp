using System;
using System.Collections.Generic;

namespace FantasyApp.Entity.Dtos.Players
{
    public class PlayerDetailDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string SecondName { get; set; } = string.Empty;
        public string WebName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public long TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamShortName { get; set; } = string.Empty;
        public decimal PriceMillions { get; set; }
        public int TotalPoints { get; set; }
        public decimal Form { get; set; }
        public decimal AveragePoints { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<UpcomingFixtureDto> NextFixtures { get; set; } = new();
    }

    public class UpcomingFixtureDto
    {
        public string OpponentShortName { get; set; } = string.Empty;
        public bool IsHome { get; set; }
        public int Difficulty { get; set; }
        public DateTime? KickoffTime { get; set; }
    }
}

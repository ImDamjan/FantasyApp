using System.Collections.Generic;

namespace FantasyApp.Entity.Dtos.Points
{
    public class SquadPointsDto
    {
        public string GameweekName { get; set; } = string.Empty;
        public bool IsScoring { get; set; }
        public string? ChipUsed { get; set; }
        public List<SquadPlayerPointsDto> Players { get; set; } = new();
    }
}

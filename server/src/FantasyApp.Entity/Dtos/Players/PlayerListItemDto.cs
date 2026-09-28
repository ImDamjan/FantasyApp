using System.Collections.Generic;

namespace FantasyApp.Entity.Dtos.Players
{
    public class PlayerListItemDto
    {
        public long Id { get; set; }
        public string WebName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public long TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamShortName { get; set; } = string.Empty;
        public decimal PriceMillions { get; set; }
        public int TotalPoints { get; set; }
        public decimal Form { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PlayerListResultDto
    {
        public List<PlayerListItemDto> Players { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

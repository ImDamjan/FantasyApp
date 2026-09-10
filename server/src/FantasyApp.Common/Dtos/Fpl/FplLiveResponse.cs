using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FantasyApp.Common.Dtos.Fpl
{
    public class FplLiveResponse
    {
        [JsonPropertyName("elements")]
        public List<FplLiveElementDto> Elements { get; set; } = new();
    }

    public class FplLiveElementDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("stats")]
        public FplLiveStatsDto Stats { get; set; } = new();
    }

    public class FplLiveStatsDto
    {
        [JsonPropertyName("total_points")]
        public int TotalPoints { get; set; }

        [JsonPropertyName("minutes")]
        public int Minutes { get; set; }

        [JsonPropertyName("goals_scored")]
        public int GoalsScored { get; set; }

        [JsonPropertyName("assists")]
        public int Assists { get; set; }

        [JsonPropertyName("clean_sheets")]
        public int CleanSheets { get; set; }

        [JsonPropertyName("goals_conceded")]
        public int GoalsConceded { get; set; }

        [JsonPropertyName("saves")]
        public int Saves { get; set; }

        [JsonPropertyName("bonus")]
        public int Bonus { get; set; }

        [JsonPropertyName("yellow_cards")]
        public int YellowCards { get; set; }

        [JsonPropertyName("red_cards")]
        public int RedCards { get; set; }
    }
}

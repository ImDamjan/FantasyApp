using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FantasyApp.Common.Dtos.Fpl
{
    public class FplBootstrapResponse
    {
        [JsonPropertyName("teams")]
        public List<FplTeamDto> Teams { get; set; } = new();

        [JsonPropertyName("elements")]
        public List<FplElementDto> Elements { get; set; } = new();

        [JsonPropertyName("events")]
        public List<FplEventDto> Events { get; set; } = new();
    }

    public class FplTeamDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonPropertyName("strength_overall_home")]
        public int StrengthOverallHome { get; set; }

        [JsonPropertyName("strength_overall_away")]
        public int StrengthOverallAway { get; set; }
    }

    public class FplElementDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("second_name")]
        public string SecondName { get; set; } = string.Empty;

        [JsonPropertyName("web_name")]
        public string WebName { get; set; } = string.Empty;

        [JsonPropertyName("element_type")]
        public int ElementType { get; set; }

        [JsonPropertyName("team")]
        public int Team { get; set; }

        [JsonPropertyName("now_cost")]
        public int NowCost { get; set; }

        [JsonPropertyName("total_points")]
        public int TotalPoints { get; set; }

        [JsonPropertyName("form")]
        public string Form { get; set; } = "0";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "a";

        [JsonPropertyName("chance_of_playing_this_round")]
        public int? ChanceOfPlayingThisRound { get; set; }
    }

    public class FplEventDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("deadline_time")]
        public DateTime DeadlineTime { get; set; }

        [JsonPropertyName("is_current")]
        public bool IsCurrent { get; set; }

        [JsonPropertyName("is_next")]
        public bool IsNext { get; set; }

        [JsonPropertyName("finished")]
        public bool Finished { get; set; }
    }
}

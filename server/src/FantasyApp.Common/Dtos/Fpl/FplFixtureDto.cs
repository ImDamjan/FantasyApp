using System;
using System.Text.Json.Serialization;

namespace FantasyApp.Common.Dtos.Fpl
{
    public class FplFixtureDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("event")]
        public int? Event { get; set; }

        [JsonPropertyName("team_h")]
        public int TeamH { get; set; }

        [JsonPropertyName("team_a")]
        public int TeamA { get; set; }

        [JsonPropertyName("team_h_score")]
        public int? TeamHScore { get; set; }

        [JsonPropertyName("team_a_score")]
        public int? TeamAScore { get; set; }

        [JsonPropertyName("kickoff_time")]
        public DateTime? KickoffTime { get; set; }

        [JsonPropertyName("team_h_difficulty")]
        public int TeamHDifficulty { get; set; }

        [JsonPropertyName("team_a_difficulty")]
        public int TeamADifficulty { get; set; }

        [JsonPropertyName("finished")]
        public bool Finished { get; set; }
    }
}

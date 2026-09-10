using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FantasyApp.Entity.Dtos.Squad
{
    public class BenchSlotDto
    {
        public long PlayerId { get; set; }
        public int Order { get; set; }
    }

    public class PickSquadRequestDto
    {
        [Required]
        public List<long> PlayerIds { get; set; } = new();

        [Required]
        public List<long> StartingPlayerIds { get; set; } = new();

        public long CaptainPlayerId { get; set; }
        public long ViceCaptainPlayerId { get; set; }

        [Required]
        public List<BenchSlotDto> BenchOrder { get; set; } = new();
    }

    public class UpdateLineupRequestDto
    {
        [Required]
        public List<long> StartingPlayerIds { get; set; } = new();

        public long CaptainPlayerId { get; set; }
        public long ViceCaptainPlayerId { get; set; }

        [Required]
        public List<BenchSlotDto> BenchOrder { get; set; } = new();
    }

    public class SetCaptainRequestDto
    {
        public long CaptainPlayerId { get; set; }
        public long ViceCaptainPlayerId { get; set; }
    }

    public class ActivateChipRequestDto
    {
        public string? Chip { get; set; }
    }
}

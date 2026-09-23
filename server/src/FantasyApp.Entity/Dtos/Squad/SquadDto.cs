using System.Collections.Generic;

namespace FantasyApp.Entity.Dtos.Squad
{
    public class SquadDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal BudgetRemainingMillions { get; set; }
        public int FreeTransfersAvailable { get; set; }
        public bool UnlimitedTransfers { get; set; }
        public bool HasPickedInitialSquad { get; set; }
        public bool TripleCaptainUsed { get; set; }
        public bool BenchBoostUsed { get; set; }
        public bool WildCardUsed { get; set; }
        public string? ActiveChip { get; set; }
        public List<SquadPlayerDto> Players { get; set; } = new();
    }

    public class SquadPlayerDto
    {
        public long PlayerId { get; set; }
        public string WebName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public long TeamId { get; set; }
        public string TeamShortName { get; set; } = string.Empty;
        public decimal PriceMillions { get; set; }
        public bool IsStarting { get; set; }
        public int? BenchOrder { get; set; }
        public bool IsCaptain { get; set; }
        public bool IsViceCaptain { get; set; }
    }
}

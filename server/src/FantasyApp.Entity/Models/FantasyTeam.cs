using System.Collections.Generic;

namespace FantasyApp.Entity.Models
{
    public class FantasyTeam
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string Name { get; set; } = string.Empty;

        public int BudgetRemainingTenths { get; set; } = 1000;
        public int FreeTransfersAvailable { get; set; } = 1;

        public bool TripleCaptainUsed { get; set; }
        public bool BenchBoostUsed { get; set; }
        public bool WildCardUsed { get; set; }

        public ChipType? ActiveChip { get; set; }
        public long? ActiveChipGameweekId { get; set; }

        public bool HasPickedInitialSquad { get; set; }
        public long? LastSnapshotGameweekId { get; set; }

        public ICollection<SquadPlayer> SquadPlayers { get; set; } = new List<SquadPlayer>();
    }
}

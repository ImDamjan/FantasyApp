using FantasyApp.Entity.Dtos.Squad;

namespace FantasyApp.Entity.Dtos.Transfers
{
    public class TransferResultDto
    {
        public SquadDto Squad { get; set; } = new();
        public int TransfersMade { get; set; }
        public int FreeTransfersUsed { get; set; }
        public int PaidTransfers { get; set; }
        public int PointsCost { get; set; }
    }
}

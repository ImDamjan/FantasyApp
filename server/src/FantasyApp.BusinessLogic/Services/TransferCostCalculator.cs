using System.Collections.Generic;
using System.Linq;
using FantasyApp.Entity.Models;

namespace FantasyApp.BusinessLogic.Services
{
    public static class TransferCostCalculator
    {
        public static int CalculateNetTransferCount(
            IReadOnlyCollection<long> finalSquadPlayerIds,
            IEnumerable<Transfer> windowTransfersDescByCreatedAt)
        {
            var baseline = new HashSet<long>(finalSquadPlayerIds);
            foreach (var transfer in windowTransfersDescByCreatedAt)
            {
                baseline.Remove(transfer.PlayerInId);
                baseline.Add(transfer.PlayerOutId);
            }

            return finalSquadPlayerIds.Count(id => !baseline.Contains(id));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using FantasyApp.Entity.Models;

namespace FantasyApp.BusinessLogic.Services
{
    public static class TransferCostCalculator
    {
        /// <summary>
        /// Counts how many players in the final squad are genuinely new compared to the squad
        /// as it stood before this gameweek's transfer window started, collapsing any chains
        /// that were later undone (e.g. swap A-&gt;B then B-&gt;A before the deadline cancels out).
        /// </summary>
        /// <param name="windowTransfersDescByCreatedAt">All transfers for this team/gameweek, most recent first.</param>
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

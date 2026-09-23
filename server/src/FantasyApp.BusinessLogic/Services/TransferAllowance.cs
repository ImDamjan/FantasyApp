using System;
using System.Collections.Generic;
using System.Linq;
using FantasyApp.Entity.Models;

namespace FantasyApp.BusinessLogic.Services
{
    public record TransferAllowance(bool Unlimited, int NetTransferCount, int FreeTransfersUsed, int PaidTransfers)
    {
        public int PointsCost => PaidTransfers * ScoringRules.TransferHitPoints;

        public static bool HasUnlimitedTransfers(FantasyTeam team, long targetGameweekId) =>
            team.LastSnapshotGameweekId == null ||
            (team.ActiveChip == ChipType.WildCard && team.ActiveChipGameweekId == targetGameweekId);

        public static TransferAllowance Calculate(
            FantasyTeam team,
            long targetGameweekId,
            IEnumerable<Transfer> windowTransfersDescByCreatedAt)
        {
            var squadIds = team.SquadPlayers.Select(sp => sp.PlayerId).ToList();
            var netCount = TransferCostCalculator.CalculateNetTransferCount(squadIds, windowTransfersDescByCreatedAt);

            if (HasUnlimitedTransfers(team, targetGameweekId))
            {
                return new TransferAllowance(true, netCount, 0, 0);
            }

            var freeUsed = Math.Min(netCount, team.FreeTransfersAvailable);
            return new TransferAllowance(false, netCount, freeUsed, netCount - freeUsed);
        }
    }
}

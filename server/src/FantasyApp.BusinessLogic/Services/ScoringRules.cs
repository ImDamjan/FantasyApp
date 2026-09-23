using System.Collections.Generic;
using System.Linq;
using FantasyApp.Entity.Models;

namespace FantasyApp.BusinessLogic.Services
{
    public static class ScoringRules
    {
        public const int TransferHitPoints = 4;
        public const int MaxFreeTransfers = 5;

        public static long? ResolveEffectiveCaptain(
            IReadOnlyCollection<GameweekPick> picks,
            Dictionary<long, PlayerGameweekStat> statsByPlayerId)
        {
            var captain = picks.FirstOrDefault(p => p.IsCaptain);
            if (captain != null && statsByPlayerId.TryGetValue(captain.PlayerId, out var captainStat) && captainStat.Minutes > 0)
            {
                return captain.PlayerId;
            }

            var vice = picks.FirstOrDefault(p => p.IsViceCaptain);
            if (vice != null && statsByPlayerId.TryGetValue(vice.PlayerId, out var viceStat) && viceStat.Minutes > 0)
            {
                return vice.PlayerId;
            }

            return null;
        }

        public static int GetMultiplier(GameweekPick pick, long? effectiveCaptainPlayerId, ChipType? chip)
        {
            if (!pick.IsStarting && chip != ChipType.BenchBoost)
            {
                return 0;
            }

            if (pick.PlayerId == effectiveCaptainPlayerId)
            {
                return chip == ChipType.TripleCaptain ? 3 : 2;
            }

            return 1;
        }

        public static int CalculateRawPoints(
            IReadOnlyCollection<GameweekPick> picks,
            Dictionary<long, PlayerGameweekStat> statsByPlayerId,
            ChipType? chip)
        {
            var effectiveCaptainPlayerId = ResolveEffectiveCaptain(picks, statsByPlayerId);
            return picks.Sum(pick =>
            {
                var points = statsByPlayerId.TryGetValue(pick.PlayerId, out var stat) ? stat.TotalPoints : 0;
                return points * GetMultiplier(pick, effectiveCaptainPlayerId, chip);
            });
        }
    }
}

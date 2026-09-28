using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class GameweekSnapshotService : IGameweekSnapshotService
    {
        private static readonly SemaphoreSlim SnapshotLock = new(1, 1);

        private readonly IGameweekRepository _gameweekRepository;
        private readonly IFantasyTeamRepository _fantasyTeamRepository;
        private readonly IGameweekPickRepository _pickRepository;
        private readonly ITransferRepository _transferRepository;
        private readonly IUserGameweekScoreRepository _scoreRepository;

        public GameweekSnapshotService(
            IGameweekRepository gameweekRepository,
            IFantasyTeamRepository fantasyTeamRepository,
            IGameweekPickRepository pickRepository,
            ITransferRepository transferRepository,
            IUserGameweekScoreRepository scoreRepository)
        {
            _gameweekRepository = gameweekRepository;
            _fantasyTeamRepository = fantasyTeamRepository;
            _pickRepository = pickRepository;
            _transferRepository = transferRepository;
            _scoreRepository = scoreRepository;
        }

        public async Task EnsureSnapshotsAsync()
        {
            await SnapshotLock.WaitAsync();
            try
            {
                var pending = await _gameweekRepository.GetDeadlinePassedWithoutSnapshotAsync();
                if (pending.Count == 0)
                {
                    return;
                }

                var gameweek = pending[^1];
                var pendingIds = pending.Select(g => g.Id).ToHashSet();
                foreach (var missed in pending)
                {
                    missed.SquadsSnapshotted = true;
                }

                var scores = await _scoreRepository.GetForGameweekAsync(gameweek.Id);
                var teams = await _fantasyTeamRepository.GetAllWithSquadAsync();

                foreach (var team in teams)
                {
                    await SnapshotTeamAsync(team, gameweek, pendingIds, scores.GetValueOrDefault(team.UserId));
                }

                await _gameweekRepository.SaveChangesAsync();
            }
            finally
            {
                SnapshotLock.Release();
            }
        }

        private async Task SnapshotTeamAsync(
            FantasyTeam team,
            Gameweek gameweek,
            HashSet<long> pendingGameweekIds,
            UserGameweekScore? score)
        {
            await _pickRepository.AddRangeAsync(team.SquadPlayers.Select(sp => new GameweekPick
            {
                FantasyTeamId = team.Id,
                GameweekId = gameweek.Id,
                PlayerId = sp.PlayerId,
                IsStarting = sp.IsStarting,
                BenchOrder = sp.BenchOrder,
                IsCaptain = sp.IsCaptain,
                IsViceCaptain = sp.IsViceCaptain
            }));

            var chip = team.ActiveChipGameweekId == gameweek.Id ? team.ActiveChip : null;
            var windowTransfers = await _transferRepository.GetByFantasyTeamAndGameweekDescAsync(team.Id, gameweek.Id);
            var allowance = TransferAllowance.Calculate(team, gameweek.Id, windowTransfers);

            if (score == null)
            {
                score = new UserGameweekScore { UserId = team.UserId, GameweekId = gameweek.Id };
                await _scoreRepository.AddAsync(score);
            }

            score.ChipUsed = chip;
            score.TransferCost = allowance.PointsCost;
            score.NetPoints = score.RawPoints - score.TransferCost;

            team.FreeTransfersAvailable = team.LastSnapshotGameweekId == null
                ? 1
                : Math.Min(ScoringRules.MaxFreeTransfers, team.FreeTransfersAvailable - allowance.FreeTransfersUsed + 1);
            team.LastSnapshotGameweekId = gameweek.Id;

            if (team.ActiveChipGameweekId.HasValue && pendingGameweekIds.Contains(team.ActiveChipGameweekId.Value))
            {
                team.ActiveChip = null;
                team.ActiveChipGameweekId = null;
            }
        }
    }
}

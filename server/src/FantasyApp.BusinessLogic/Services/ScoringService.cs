using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class ScoringService : IScoringService
    {
        private readonly IGameweekRepository _gameweekRepository;
        private readonly IPlayerGameweekStatRepository _playerGameweekStatRepository;
        private readonly IFantasyTeamRepository _fantasyTeamRepository;
        private readonly ITransferRepository _transferRepository;
        private readonly IUserGameweekScoreRepository _scoreRepository;

        public ScoringService(
            IGameweekRepository gameweekRepository,
            IPlayerGameweekStatRepository playerGameweekStatRepository,
            IFantasyTeamRepository fantasyTeamRepository,
            ITransferRepository transferRepository,
            IUserGameweekScoreRepository scoreRepository)
        {
            _gameweekRepository = gameweekRepository;
            _playerGameweekStatRepository = playerGameweekStatRepository;
            _fantasyTeamRepository = fantasyTeamRepository;
            _transferRepository = transferRepository;
            _scoreRepository = scoreRepository;
        }

        public async Task RecalculateGameweekScoresAsync(long gameweekId)
        {
            var gameweek = await _gameweekRepository.GetByIdAsync(gameweekId);
            if (gameweek == null)
            {
                return;
            }

            var statsByPlayerId = await _playerGameweekStatRepository.GetForGameweekAsync(gameweekId);
            var fantasyTeams = await _fantasyTeamRepository.GetAllWithSquadAsync();

            foreach (var team in fantasyTeams)
            {
                var chipActiveThisGameweek = team.ActiveChipGameweekId == gameweekId ? team.ActiveChip : null;
                var benchBoost = chipActiveThisGameweek == ChipType.BenchBoost;
                var tripleCaptain = chipActiveThisGameweek == ChipType.TripleCaptain;
                var wildcard = chipActiveThisGameweek == ChipType.WildCard;

                var effectiveCaptainPlayerId = ResolveEffectiveCaptain(team, statsByPlayerId);

                var rawPoints = 0;
                var scoringSlots = team.SquadPlayers.Where(sp => sp.IsStarting || benchBoost);
                foreach (var slot in scoringSlots)
                {
                    var points = statsByPlayerId.TryGetValue(slot.PlayerId, out var stat) ? stat.TotalPoints : 0;
                    if (slot.PlayerId == effectiveCaptainPlayerId)
                    {
                        points *= tripleCaptain ? 3 : 2;
                    }
                    rawPoints += points;
                }

                var paidTransfers = wildcard
                    ? 0
                    : await _transferRepository.GetPaidTransferCountAsync(team.Id, gameweekId);
                var transferCost = paidTransfers * 4;
                var netPoints = rawPoints - transferCost;

                var score = await _scoreRepository.GetAsync(team.UserId, gameweekId);
                if (score == null)
                {
                    score = new UserGameweekScore { UserId = team.UserId, GameweekId = gameweekId };
                    await _scoreRepository.AddAsync(score);
                }

                score.RawPoints = rawPoints;
                score.TransferCost = transferCost;
                score.NetPoints = netPoints;
                score.ChipUsed = chipActiveThisGameweek;
                score.IsFinal = gameweek.IsFinished;

                if (gameweek.IsFinished && team.LastFreeTransferGameweekId != gameweekId)
                {
                    team.FreeTransfersAvailable += 1;
                    team.LastFreeTransferGameweekId = gameweekId;

                    if (team.ActiveChipGameweekId == gameweekId)
                    {
                        team.ActiveChip = null;
                        team.ActiveChipGameweekId = null;
                    }
                }
            }

            await _scoreRepository.SaveChangesAsync();
        }

        private static long? ResolveEffectiveCaptain(
            FantasyTeam team,
            Dictionary<long, PlayerGameweekStat> statsByPlayerId)
        {
            var captain = team.SquadPlayers.FirstOrDefault(sp => sp.IsCaptain);
            if (captain != null && statsByPlayerId.TryGetValue(captain.PlayerId, out var captainStat) && captainStat.Minutes > 0)
            {
                return captain.PlayerId;
            }

            var vice = team.SquadPlayers.FirstOrDefault(sp => sp.IsViceCaptain);
            if (vice != null && statsByPlayerId.TryGetValue(vice.PlayerId, out var viceStat) && viceStat.Minutes > 0)
            {
                return vice.PlayerId;
            }

            return null;
        }
    }
}

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
        private readonly IGameweekPickRepository _pickRepository;
        private readonly IUserGameweekScoreRepository _scoreRepository;
        private readonly IFixtureRepository _fixtureRepository;

        public ScoringService(
            IGameweekRepository gameweekRepository,
            IPlayerGameweekStatRepository playerGameweekStatRepository,
            IGameweekPickRepository pickRepository,
            IUserGameweekScoreRepository scoreRepository,
            IFixtureRepository fixtureRepository)
        {
            _gameweekRepository = gameweekRepository;
            _playerGameweekStatRepository = playerGameweekStatRepository;
            _pickRepository = pickRepository;
            _scoreRepository = scoreRepository;
            _fixtureRepository = fixtureRepository;
        }

        public async Task RecalculateGameweekScoresAsync(long gameweekId)
        {
            var gameweek = await _gameweekRepository.GetByIdAsync(gameweekId);
            if (gameweek == null)
            {
                return;
            }

            var statsByPlayerId = await _playerGameweekStatRepository.GetForGameweekAsync(gameweekId);
            var picksByTeam = (await _pickRepository.GetForGameweekAsync(gameweekId))
                .Where(p => p.FantasyTeam != null)
                .GroupBy(p => p.FantasyTeam!.UserId);
            var scores = await _scoreRepository.GetForGameweekAsync(gameweekId);
            var teamIdsWithMatchesLeft = await _fixtureRepository.GetTeamIdsWithMatchesLeftAsync(gameweekId);

            foreach (var teamPicks in picksByTeam)
            {
                var userId = teamPicks.Key;
                if (!scores.TryGetValue(userId, out var score))
                {
                    score = new UserGameweekScore { UserId = userId, GameweekId = gameweekId };
                    await _scoreRepository.AddAsync(score);
                }

                score.RawPoints = ScoringRules.CalculateRawPoints(teamPicks.ToList(), statsByPlayerId, teamIdsWithMatchesLeft, score.ChipUsed);
                score.NetPoints = score.RawPoints - score.TransferCost;
                score.IsFinal = gameweek.IsFinished;
            }

            if (gameweek.IsFinished)
            {
                gameweek.ScoresFinalized = true;
            }

            await _scoreRepository.SaveChangesAsync();
        }
    }
}

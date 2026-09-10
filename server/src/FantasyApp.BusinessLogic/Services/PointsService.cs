using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Points;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class PointsService : IPointsService
    {
        private readonly IUserGameweekScoreRepository _scoreRepository;
        private readonly IGameweekRepository _gameweekRepository;

        public PointsService(IUserGameweekScoreRepository scoreRepository, IGameweekRepository gameweekRepository)
        {
            _scoreRepository = scoreRepository;
            _gameweekRepository = gameweekRepository;
        }

        public async Task<PointsSummaryDto> GetSummaryAsync(long userId)
        {
            var totalPoints = (await _scoreRepository.GetTotalPointsByUserIdsAsync(new[] { userId }))
                .GetValueOrDefault(userId);

            var currentGameweek = await _gameweekRepository.GetCurrentAsync();
            var currentPoints = currentGameweek != null
                ? (await _scoreRepository.GetPointsForGameweekByUserIdsAsync(new[] { userId }, currentGameweek.Id))
                    .GetValueOrDefault(userId)
                : 0;

            return new PointsSummaryDto
            {
                CurrentGameweekPoints = currentPoints,
                TotalPoints = totalPoints
            };
        }

        public async Task<List<PointsHistoryItemDto>> GetHistoryAsync(long userId)
        {
            var scores = await _scoreRepository.GetByUserIdAsync(userId);

            return scores.Select(s => new PointsHistoryItemDto
            {
                GameweekName = s.Gameweek?.Name ?? string.Empty,
                RawPoints = s.RawPoints,
                TransferCost = s.TransferCost,
                NetPoints = s.NetPoints,
                ChipUsed = s.ChipUsed?.ToString(),
                IsFinal = s.IsFinal
            }).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Gameweeks;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class GameweekService : IGameweekService
    {
        private readonly IGameweekRepository _gameweekRepository;

        public GameweekService(IGameweekRepository gameweekRepository)
        {
            _gameweekRepository = gameweekRepository;
        }

        public async Task<List<GameweekDeadlineDto>> GetUpcomingDeadlinesAsync()
        {
            var all = await _gameweekRepository.GetAllAsync();
            var now = DateTime.UtcNow;

            return all
                .Where(g => !g.IsFinished || g.DeadlineTime >= now.AddDays(-1))
                .OrderBy(g => g.DeadlineTime)
                .Select(g => new GameweekDeadlineDto
                {
                    Name = g.Name,
                    DeadlineTime = g.DeadlineTime,
                    IsCurrent = g.IsCurrent,
                    IsNext = g.IsNext
                })
                .ToList();
        }
    }
}

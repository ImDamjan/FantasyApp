using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class UserGameweekScoreRepository : IUserGameweekScoreRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserGameweekScoreRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Dictionary<long, int>> GetTotalPointsByUserIdsAsync(IEnumerable<long> userIds)
        {
            return await _dbContext.UserGameweekScores
                .Where(s => userIds.Contains(s.UserId))
                .GroupBy(s => s.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(s => s.NetPoints) })
                .ToDictionaryAsync(x => x.UserId, x => x.Total);
        }

        public async Task<Dictionary<long, int>> GetPointsForGameweekByUserIdsAsync(IEnumerable<long> userIds, long gameweekId)
        {
            return await _dbContext.UserGameweekScores
                .Where(s => userIds.Contains(s.UserId) && s.GameweekId == gameweekId)
                .ToDictionaryAsync(s => s.UserId, s => s.RawPoints);
        }

        public async Task<Dictionary<long, int>> GetAllPointsForGameweekAsync(long gameweekId)
        {
            return await _dbContext.UserGameweekScores
                .Where(s => s.GameweekId == gameweekId)
                .ToDictionaryAsync(s => s.UserId, s => s.RawPoints);
        }

        public async Task<Dictionary<long, int>> GetAllTotalPointsAsync()
        {
            return await _dbContext.UserGameweekScores
                .GroupBy(s => s.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(s => s.NetPoints) })
                .ToDictionaryAsync(x => x.UserId, x => x.Total);
        }

        public async Task<UserGameweekScore?> GetAsync(long userId, long gameweekId)
        {
            return await _dbContext.UserGameweekScores
                .SingleOrDefaultAsync(s => s.UserId == userId && s.GameweekId == gameweekId);
        }

        public async Task<List<UserGameweekScore>> GetByUserIdAsync(long userId)
        {
            return await _dbContext.UserGameweekScores
                .Where(s => s.UserId == userId)
                .Include(s => s.Gameweek)
                .OrderBy(s => s.Gameweek!.FplId)
                .ToListAsync();
        }

        public async Task<Dictionary<long, UserGameweekScore>> GetForGameweekAsync(long gameweekId)
        {
            return await _dbContext.UserGameweekScores
                .Where(s => s.GameweekId == gameweekId)
                .ToDictionaryAsync(s => s.UserId);
        }

        public async Task AddAsync(UserGameweekScore score)
        {
            await _dbContext.UserGameweekScores.AddAsync(score);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

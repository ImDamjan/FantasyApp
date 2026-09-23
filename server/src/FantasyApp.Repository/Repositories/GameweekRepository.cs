using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class GameweekRepository : IGameweekRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public GameweekRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Gameweek?> GetByFplIdAsync(int fplId)
        {
            return await _dbContext.Gameweeks.SingleOrDefaultAsync(g => g.FplId == fplId);
        }

        public async Task<Gameweek?> GetByIdAsync(long id)
        {
            return await _dbContext.Gameweeks.SingleOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<Gameweek>> GetAllAsync()
        {
            return await _dbContext.Gameweeks.OrderBy(g => g.FplId).ToListAsync();
        }

        public async Task<Gameweek?> GetCurrentAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Gameweeks
                .Where(g => g.DeadlineTime <= now)
                .OrderByDescending(g => g.DeadlineTime)
                .FirstOrDefaultAsync();
        }

        public async Task<Gameweek?> GetNextAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Gameweeks
                .Where(g => g.DeadlineTime > now)
                .OrderBy(g => g.DeadlineTime)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Gameweek>> GetDeadlinePassedWithoutSnapshotAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Gameweeks
                .Where(g => g.DeadlineTime <= now && !g.SquadsSnapshotted)
                .OrderBy(g => g.DeadlineTime)
                .ToListAsync();
        }

        public async Task<List<Gameweek>> GetStartedWithoutFinalScoresAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Gameweeks
                .Where(g => !g.ScoresFinalized &&
                            _dbContext.Fixtures.Any(f => f.GameweekId == g.Id && f.KickoffTime <= now))
                .OrderBy(g => g.DeadlineTime)
                .ToListAsync();
        }

        public async Task AddAsync(Gameweek gameweek)
        {
            await _dbContext.Gameweeks.AddAsync(gameweek);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

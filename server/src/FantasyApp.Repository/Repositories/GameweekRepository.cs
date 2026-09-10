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
            return await _dbContext.Gameweeks.SingleOrDefaultAsync(g => g.IsCurrent);
        }

        public async Task<Gameweek?> GetNextAsync()
        {
            return await _dbContext.Gameweeks.SingleOrDefaultAsync(g => g.IsNext);
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

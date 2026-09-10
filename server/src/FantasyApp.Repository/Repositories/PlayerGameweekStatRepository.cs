using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class PlayerGameweekStatRepository : IPlayerGameweekStatRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PlayerGameweekStatRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PlayerGameweekStat?> GetAsync(long playerId, long gameweekId)
        {
            return await _dbContext.PlayerGameweekStats
                .SingleOrDefaultAsync(s => s.PlayerId == playerId && s.GameweekId == gameweekId);
        }

        public async Task<Dictionary<long, PlayerGameweekStat>> GetForGameweekAsync(long gameweekId)
        {
            return await _dbContext.PlayerGameweekStats
                .Where(s => s.GameweekId == gameweekId)
                .ToDictionaryAsync(s => s.PlayerId, s => s);
        }

        public async Task AddAsync(PlayerGameweekStat stat)
        {
            await _dbContext.PlayerGameweekStats.AddAsync(stat);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

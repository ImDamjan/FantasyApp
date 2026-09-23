using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class GameweekPickRepository : IGameweekPickRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public GameweekPickRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<GameweekPick>> GetForGameweekAsync(long gameweekId)
        {
            return await _dbContext.GameweekPicks
                .Include(gp => gp.FantasyTeam)
                .Include(gp => gp.Player)
                .Where(gp => gp.GameweekId == gameweekId)
                .ToListAsync();
        }

        public async Task<List<GameweekPick>> GetForTeamAndGameweekAsync(long fantasyTeamId, long gameweekId)
        {
            return await _dbContext.GameweekPicks
                .Include(gp => gp.Player)
                    .ThenInclude(p => p!.Team)
                .Where(gp => gp.FantasyTeamId == fantasyTeamId && gp.GameweekId == gameweekId)
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<GameweekPick> picks)
        {
            await _dbContext.GameweekPicks.AddRangeAsync(picks);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

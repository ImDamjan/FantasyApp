using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class FixtureRepository : IFixtureRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public FixtureRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Fixture?> GetByFplIdAsync(int fplId)
        {
            return await _dbContext.Fixtures.SingleOrDefaultAsync(f => f.FplId == fplId);
        }

        public async Task<List<Fixture>> GetUpcomingForTeamAsync(long teamId, int count)
        {
            return await _dbContext.Fixtures
                .Where(f => !f.IsFinished && (f.HomeTeamId == teamId || f.AwayTeamId == teamId))
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .OrderBy(f => f.KickoffTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task AddAsync(Fixture fixture)
        {
            await _dbContext.Fixtures.AddAsync(fixture);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

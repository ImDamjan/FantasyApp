using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Team?> GetByFplIdAsync(int fplId)
        {
            return await _dbContext.Teams.SingleOrDefaultAsync(t => t.FplId == fplId);
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await _dbContext.Teams.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task AddAsync(Team team)
        {
            await _dbContext.Teams.AddAsync(team);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

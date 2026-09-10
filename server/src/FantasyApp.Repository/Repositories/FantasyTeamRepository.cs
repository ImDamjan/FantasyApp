using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class FantasyTeamRepository : IFantasyTeamRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public FantasyTeamRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FantasyTeam?> GetByUserIdAsync(long userId)
        {
            return await _dbContext.FantasyTeams.SingleOrDefaultAsync(ft => ft.UserId == userId);
        }

        public async Task<FantasyTeam?> GetByUserIdWithSquadAsync(long userId)
        {
            return await _dbContext.FantasyTeams
                .Include(ft => ft.SquadPlayers)
                    .ThenInclude(sp => sp.Player)
                        .ThenInclude(p => p!.Team)
                .SingleOrDefaultAsync(ft => ft.UserId == userId);
        }

        public async Task<List<FantasyTeam>> GetAllWithSquadAsync()
        {
            return await _dbContext.FantasyTeams
                .Include(ft => ft.SquadPlayers)
                .Where(ft => ft.HasPickedInitialSquad)
                .ToListAsync();
        }

        public async Task AddAsync(FantasyTeam fantasyTeam)
        {
            await _dbContext.FantasyTeams.AddAsync(fantasyTeam);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

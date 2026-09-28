using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class LeagueRepository : ILeagueRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public LeagueRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<League?> GetOfficialLeagueAsync()
        {
            return await _dbContext.Leagues.SingleOrDefaultAsync(l => l.IsOfficial);
        }

        public async Task<League?> GetByIdAsync(long id)
        {
            return await _dbContext.Leagues.SingleOrDefaultAsync(l => l.Id == id);
        }

        public async Task<League?> GetByJoinCodeAsync(string joinCode)
        {
            return await _dbContext.Leagues.SingleOrDefaultAsync(l => l.JoinCode == joinCode);
        }

        public async Task<int> GetMemberCountAsync(long leagueId)
        {
            return await _dbContext.LeagueMemberships.CountAsync(m => m.LeagueId == leagueId);
        }

        public async Task<bool> IsMemberAsync(long leagueId, long userId)
        {
            return await _dbContext.LeagueMemberships.AnyAsync(m => m.LeagueId == leagueId && m.UserId == userId);
        }

        public async Task<List<League>> GetUserLeaguesAsync(long userId)
        {
            return await _dbContext.LeagueMemberships
                .Where(m => m.UserId == userId)
                .Select(m => m.League!)
                .OrderByDescending(l => l.IsOfficial)
                .ThenBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<List<LeagueMembership>> GetMembershipsWithUsersAsync(long leagueId)
        {
            return await _dbContext.LeagueMemberships
                .Where(m => m.LeagueId == leagueId)
                .Include(m => m.User)
                .ToListAsync();
        }

        public async Task AddAsync(League league)
        {
            await _dbContext.Leagues.AddAsync(league);
        }

        public async Task AddMembershipAsync(LeagueMembership membership)
        {
            await _dbContext.LeagueMemberships.AddAsync(membership);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

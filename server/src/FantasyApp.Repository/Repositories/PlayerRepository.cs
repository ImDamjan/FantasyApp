using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PlayerRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Player?> GetByFplIdAsync(int fplId)
        {
            return await _dbContext.Players.SingleOrDefaultAsync(p => p.FplId == fplId);
        }

        public async Task<Player?> GetByIdWithTeamAsync(long id)
        {
            return await _dbContext.Players
                .Include(p => p.Team)
                .SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Player>> GetByIdsAsync(IEnumerable<long> ids)
        {
            return await _dbContext.Players
                .Include(p => p.Team)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<(List<Player> Players, int TotalCount)> GetFilteredAsync(
            PlayerPosition? position,
            int? maxPriceTenths,
            string? search,
            long? teamId,
            int page,
            int pageSize)
        {
            var query = _dbContext.Players.Include(p => p.Team).AsQueryable();

            if (position.HasValue)
            {
                query = query.Where(p => p.Position == position.Value);
            }

            if (maxPriceTenths.HasValue)
            {
                query = query.Where(p => p.PriceTenths <= maxPriceTenths.Value);
            }

            if (teamId.HasValue)
            {
                query = query.Where(p => p.TeamId == teamId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.WebName.Contains(search) || p.SecondName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var players = await query
                .OrderByDescending(p => p.TotalPoints)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (players, totalCount);
        }

        public async Task AddAsync(Player player)
        {
            await _dbContext.Players.AddAsync(player);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Repositories
{
    public class TransferRepository : ITransferRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TransferRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Transfer>> GetByFantasyTeamIdAsync(long fantasyTeamId)
        {
            return await _dbContext.Transfers
                .Where(t => t.FantasyTeamId == fantasyTeamId)
                .Include(t => t.PlayerOut)
                .Include(t => t.PlayerIn)
                .Include(t => t.Gameweek)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Transfer>> GetByFantasyTeamAndGameweekDescAsync(long fantasyTeamId, long gameweekId)
        {
            return await _dbContext.Transfers
                .Where(t => t.FantasyTeamId == fantasyTeamId && t.GameweekId == gameweekId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Transfer transfer)
        {
            await _dbContext.Transfers.AddAsync(transfer);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

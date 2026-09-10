using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface ITransferRepository
    {
        Task<List<Transfer>> GetByFantasyTeamIdAsync(long fantasyTeamId);
        Task<int> GetPaidTransferCountAsync(long fantasyTeamId, long gameweekId);
        Task AddAsync(Transfer transfer);
        Task SaveChangesAsync();
    }
}

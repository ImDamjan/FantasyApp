using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface IPlayerGameweekStatRepository
    {
        Task<PlayerGameweekStat?> GetAsync(long playerId, long gameweekId);
        Task<Dictionary<long, PlayerGameweekStat>> GetForGameweekAsync(long gameweekId);
        Task AddAsync(PlayerGameweekStat stat);
        Task SaveChangesAsync();
    }
}

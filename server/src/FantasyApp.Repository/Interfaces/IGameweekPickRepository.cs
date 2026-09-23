using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface IGameweekPickRepository
    {
        Task<List<GameweekPick>> GetForGameweekAsync(long gameweekId);
        Task<List<GameweekPick>> GetForTeamAndGameweekAsync(long fantasyTeamId, long gameweekId);
        Task AddRangeAsync(IEnumerable<GameweekPick> picks);
        Task SaveChangesAsync();
    }
}

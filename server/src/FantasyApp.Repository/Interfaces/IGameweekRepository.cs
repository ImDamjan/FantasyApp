using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface IGameweekRepository
    {
        Task<Gameweek?> GetByFplIdAsync(int fplId);
        Task<Gameweek?> GetByIdAsync(long id);
        Task<List<Gameweek>> GetAllAsync();
        Task<Gameweek?> GetCurrentAsync();
        Task<Gameweek?> GetNextAsync();
        Task AddAsync(Gameweek gameweek);
        Task SaveChangesAsync();
    }
}

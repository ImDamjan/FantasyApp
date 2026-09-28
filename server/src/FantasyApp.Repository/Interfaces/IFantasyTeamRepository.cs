using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface IFantasyTeamRepository
    {
        Task<FantasyTeam?> GetByUserIdAsync(long userId);
        Task<FantasyTeam?> GetByUserIdWithSquadAsync(long userId);
        Task<List<FantasyTeam>> GetAllWithSquadAsync();
        Task AddAsync(FantasyTeam fantasyTeam);
        Task SaveChangesAsync();
    }
}

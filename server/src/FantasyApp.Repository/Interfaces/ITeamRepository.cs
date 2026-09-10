using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface ITeamRepository
    {
        Task<Team?> GetByFplIdAsync(int fplId);
        Task<List<Team>> GetAllAsync();
        Task AddAsync(Team team);
        Task SaveChangesAsync();
    }
}

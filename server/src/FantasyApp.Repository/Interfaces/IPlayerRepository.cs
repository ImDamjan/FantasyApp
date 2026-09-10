using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player?> GetByFplIdAsync(int fplId);
        Task<Player?> GetByIdWithTeamAsync(long id);
        Task<List<Player>> GetByIdsAsync(IEnumerable<long> ids);
        Task<(List<Player> Players, int TotalCount)> GetFilteredAsync(
            PlayerPosition? position,
            int? maxPriceTenths,
            string? search,
            long? teamId,
            int page,
            int pageSize);
        Task AddAsync(Player player);
        Task SaveChangesAsync();
    }
}

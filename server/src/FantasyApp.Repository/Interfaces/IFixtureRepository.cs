using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface IFixtureRepository
    {
        Task<Fixture?> GetByFplIdAsync(int fplId);
        Task<List<Fixture>> GetUpcomingForTeamAsync(long teamId, int count);
        Task<HashSet<long>> GetTeamIdsWithMatchesLeftAsync(long gameweekId);
        Task AddAsync(Fixture fixture);
        Task SaveChangesAsync();
    }
}

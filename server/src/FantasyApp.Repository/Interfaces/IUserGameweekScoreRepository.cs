using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface IUserGameweekScoreRepository
    {
        Task<Dictionary<long, int>> GetTotalPointsByUserIdsAsync(IEnumerable<long> userIds);
        Task<Dictionary<long, int>> GetPointsForGameweekByUserIdsAsync(IEnumerable<long> userIds, long gameweekId);
        Task<Dictionary<long, int>> GetAllPointsForGameweekAsync(long gameweekId);
        Task<Dictionary<long, int>> GetAllTotalPointsAsync();
        Task<UserGameweekScore?> GetAsync(long userId, long gameweekId);
        Task<List<UserGameweekScore>> GetByUserIdAsync(long userId);
        Task AddAsync(UserGameweekScore score);
        Task SaveChangesAsync();
    }
}

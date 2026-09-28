using System.Threading.Tasks;
using FantasyApp.Entity.Models;

namespace FantasyApp.Repository.Interfaces
{
    public interface ILeagueRepository
    {
        Task<League?> GetOfficialLeagueAsync();
        Task<League?> GetByIdAsync(long id);
        Task<League?> GetByJoinCodeAsync(string joinCode);
        Task<int> GetMemberCountAsync(long leagueId);
        Task<bool> IsMemberAsync(long leagueId, long userId);
        Task<List<League>> GetUserLeaguesAsync(long userId);
        Task<List<LeagueMembership>> GetMembershipsWithUsersAsync(long leagueId);
        Task AddAsync(League league);
        Task AddMembershipAsync(LeagueMembership membership);
        Task SaveChangesAsync();
    }
}

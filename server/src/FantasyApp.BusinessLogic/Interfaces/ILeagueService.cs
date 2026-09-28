using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Dtos.Leagues;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface ILeagueService
    {
        Task<ServiceResult<LeagueDto>> CreateLeagueAsync(long userId, CreateLeagueRequestDto request);
        Task<ServiceResult<LeagueDto>> JoinLeagueAsync(long userId, JoinLeagueRequestDto request);
        Task<List<LeagueDto>> GetMyLeaguesAsync(long userId);
        Task<ServiceResult<LeagueStandingsDto>> GetStandingsAsync(long userId, long leagueId);
    }
}

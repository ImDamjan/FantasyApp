using System.Threading.Tasks;
using FantasyApp.Entity.Dtos.Players;
using FantasyApp.Entity.Models;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerListResultDto> GetPlayersAsync(
            PlayerPosition? position,
            decimal? maxPriceMillions,
            string? search,
            long? teamId,
            int page,
            int pageSize);

        Task<ServiceResult<PlayerDetailDto>> GetPlayerDetailAsync(long id);
    }
}

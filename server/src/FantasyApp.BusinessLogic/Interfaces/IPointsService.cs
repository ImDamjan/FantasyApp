using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Dtos.Points;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface IPointsService
    {
        Task<PointsSummaryDto> GetSummaryAsync(long userId, string username);
        Task<List<PointsHistoryItemDto>> GetHistoryAsync(long userId);
        Task<List<SquadPlayerPointsDto>> GetSquadPointsAsync(long userId);
    }
}

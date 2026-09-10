using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Dtos.Points;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface IPointsService
    {
        Task<PointsSummaryDto> GetSummaryAsync(long userId);
        Task<List<PointsHistoryItemDto>> GetHistoryAsync(long userId);
    }
}

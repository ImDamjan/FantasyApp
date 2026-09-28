using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Dtos.Transfers;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface ITransferService
    {
        Task<ServiceResult<TransferResultDto>> SubmitTransfersAsync(long userId, SubmitTransfersRequestDto request);
        Task<ServiceResult<List<TransferHistoryItemDto>>> GetHistoryAsync(long userId);
    }
}

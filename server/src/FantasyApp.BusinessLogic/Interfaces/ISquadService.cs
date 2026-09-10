using System.Threading.Tasks;
using FantasyApp.Entity.Dtos.Squad;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface ISquadService
    {
        Task<ServiceResult<SquadDto>> GetSquadAsync(long userId);
        Task<ServiceResult<SquadDto>> PickInitialSquadAsync(long userId, PickSquadRequestDto request);
        Task<ServiceResult<SquadDto>> UpdateLineupAsync(long userId, UpdateLineupRequestDto request);
        Task<ServiceResult<SquadDto>> SetCaptainAsync(long userId, SetCaptainRequestDto request);
        Task<ServiceResult<SquadDto>> ActivateChipAsync(long userId, ActivateChipRequestDto request);
    }
}

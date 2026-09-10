using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Common.Dtos.Fpl;

namespace FantasyApp.Common.Interfaces
{
    public interface IFplApiClient
    {
        Task<FplBootstrapResponse> GetBootstrapStaticAsync();
        Task<List<FplFixtureDto>> GetFixturesAsync();
        Task<FplLiveResponse> GetGameweekLiveAsync(int eventId);
    }
}

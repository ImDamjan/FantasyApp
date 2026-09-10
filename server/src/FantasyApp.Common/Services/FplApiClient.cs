using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FantasyApp.Common.Dtos.Fpl;
using FantasyApp.Common.Interfaces;

namespace FantasyApp.Common.Services
{
    public class FplApiClient : IFplApiClient
    {
        private readonly HttpClient _httpClient;

        public FplApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<FplBootstrapResponse> GetBootstrapStaticAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<FplBootstrapResponse>("bootstrap-static/");
            return response ?? new FplBootstrapResponse();
        }

        public async Task<List<FplFixtureDto>> GetFixturesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<FplFixtureDto>>("fixtures/");
            return response ?? new List<FplFixtureDto>();
        }

        public async Task<FplLiveResponse> GetGameweekLiveAsync(int eventId)
        {
            var response = await _httpClient.GetFromJsonAsync<FplLiveResponse>($"event/{eventId}/live/");
            return response ?? new FplLiveResponse();
        }
    }
}

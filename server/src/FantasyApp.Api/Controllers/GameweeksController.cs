using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/gameweeks")]
    public class GameweeksController : ControllerBase
    {
        private readonly IGameweekService _gameweekService;

        public GameweeksController(IGameweekService gameweekService)
        {
            _gameweekService = gameweekService;
        }

        [HttpGet("deadlines")]
        public async Task<IActionResult> GetDeadlines()
        {
            var deadlines = await _gameweekService.GetUpcomingDeadlinesAsync();
            return Ok(deadlines);
        }
    }
}

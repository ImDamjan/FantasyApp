using System.Threading.Tasks;
using FantasyApp.Api.Extensions;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Leagues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/leagues")]
    public class LeaguesController : ControllerBase
    {
        private readonly ILeagueService _leagueService;

        public LeaguesController(ILeagueService leagueService)
        {
            _leagueService = leagueService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeague([FromBody] CreateLeagueRequestDto request)
        {
            var result = await _leagueService.CreateLeagueAsync(User.GetUserId(), request);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinLeague([FromBody] JoinLeagueRequestDto request)
        {
            var result = await _leagueService.JoinLeagueAsync(User.GetUserId(), request);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyLeagues()
        {
            var leagues = await _leagueService.GetMyLeaguesAsync(User.GetUserId());
            return Ok(leagues);
        }

        [HttpGet("{id}/standings")]
        public async Task<IActionResult> GetStandings(long id)
        {
            var result = await _leagueService.GetStandingsAsync(User.GetUserId(), id);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }
    }
}

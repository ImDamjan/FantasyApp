using System.Security.Claims;
using System.Threading.Tasks;
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
            var result = await _leagueService.CreateLeagueAsync(GetUserId(), request);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinLeague([FromBody] JoinLeagueRequestDto request)
        {
            var result = await _leagueService.JoinLeagueAsync(GetUserId(), request);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyLeagues()
        {
            var leagues = await _leagueService.GetMyLeaguesAsync(GetUserId());
            return Ok(leagues);
        }

        [HttpGet("{id}/standings")]
        public async Task<IActionResult> GetStandings(long id)
        {
            var result = await _leagueService.GetStandingsAsync(GetUserId(), id);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        private long GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return long.Parse(idClaim!);
        }
    }
}

using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/players")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayersController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers(
            [FromQuery] PlayerPosition? position,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? search,
            [FromQuery] long? teamId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _playerService.GetPlayersAsync(position, maxPrice, search, teamId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(long id)
        {
            var result = await _playerService.GetPlayerDetailAsync(id);
            if (!result.Succeeded)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }
    }
}

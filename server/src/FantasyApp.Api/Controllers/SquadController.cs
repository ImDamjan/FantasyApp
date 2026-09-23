using System.Threading.Tasks;
using FantasyApp.Api.Extensions;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Squad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/squad")]
    public class SquadController : ControllerBase
    {
        private readonly ISquadService _squadService;

        public SquadController(ISquadService squadService)
        {
            _squadService = squadService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSquad()
        {
            var result = await _squadService.GetSquadAsync(User.GetUserId());
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> PickSquad([FromBody] PickSquadRequestDto request)
        {
            var result = await _squadService.PickInitialSquadAsync(User.GetUserId(), request);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpPut("lineup")]
        public async Task<IActionResult> UpdateLineup([FromBody] UpdateLineupRequestDto request)
        {
            var result = await _squadService.UpdateLineupAsync(User.GetUserId(), request);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpPut("captain")]
        public async Task<IActionResult> SetCaptain([FromBody] SetCaptainRequestDto request)
        {
            var result = await _squadService.SetCaptainAsync(User.GetUserId(), request);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpPut("chip")]
        public async Task<IActionResult> ActivateChip([FromBody] ActivateChipRequestDto request)
        {
            var result = await _squadService.ActivateChipAsync(User.GetUserId(), request);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }
    }
}

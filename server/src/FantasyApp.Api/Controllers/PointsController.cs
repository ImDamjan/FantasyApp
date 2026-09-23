using System.Security.Claims;
using System.Threading.Tasks;
using FantasyApp.Api.Extensions;
using FantasyApp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/points")]
    public class PointsController : ControllerBase
    {
        private readonly IPointsService _pointsService;

        public PointsController(IPointsService pointsService)
        {
            _pointsService = pointsService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var summary = await _pointsService.GetSummaryAsync(User.GetUserId(), username);
            return Ok(summary);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var history = await _pointsService.GetHistoryAsync(User.GetUserId());
            return Ok(history);
        }

        [HttpGet("squad")]
        public async Task<IActionResult> GetSquadPoints()
        {
            var squadPoints = await _pointsService.GetSquadPointsAsync(User.GetUserId());
            return Ok(squadPoints);
        }
    }
}

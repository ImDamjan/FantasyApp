using System.Security.Claims;
using System.Threading.Tasks;
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
            var summary = await _pointsService.GetSummaryAsync(GetUserId());
            return Ok(summary);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var history = await _pointsService.GetHistoryAsync(GetUserId());
            return Ok(history);
        }

        private long GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return long.Parse(idClaim!);
        }
    }
}

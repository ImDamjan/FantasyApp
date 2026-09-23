using System.Threading.Tasks;
using FantasyApp.Api.Extensions;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Transfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/transfers")]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService _transferService;

        public TransfersController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTransfers([FromBody] SubmitTransfersRequestDto request)
        {
            var result = await _transferService.SubmitTransfersAsync(User.GetUserId(), request);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var result = await _transferService.GetHistoryAsync(User.GetUserId());
            return Ok(result.Data);
        }
    }
}

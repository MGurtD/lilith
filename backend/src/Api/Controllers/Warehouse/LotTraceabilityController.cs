using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Warehouse
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotTraceabilityController(ILotTraceabilityService service) : ControllerBase
    {
        [HttpGet("Backward/{lotId:guid}")]
        public async Task<IActionResult> GetBackward(Guid lotId)
        {
            var result = await service.GetBackwardTraceability(lotId);
            if (result is null) return NotFound();

            return Ok(result);
        }

        [HttpGet("Forward/{lotId:guid}")]
        public async Task<IActionResult> GetForward(Guid lotId)
        {
            var result = await service.GetForwardTraceability(lotId);
            if (result is null) return NotFound();

            return Ok(result);
        }

        [HttpGet("Recall/{lotId:guid}")]
        public async Task<IActionResult> GetRecall(Guid lotId)
        {
            var result = await service.GetRecallReport(lotId);
            if (result is null) return NotFound();

            return Ok(result);
        }
    }
}

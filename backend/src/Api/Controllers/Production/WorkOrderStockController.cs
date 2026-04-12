using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production;

[ApiController]
[Route("api/[controller]")]
public class WorkOrderStockController(IWorkOrderStockService service) : ControllerBase
{
    [HttpPost("MoveToWorkcenterSupply")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MoveToWorkcenterSupply([FromBody] MoveStockToWorkcenterSupplyRequest request)
    {
        var response = await service.MoveToWorkcenterSupply(request);

        if (response.Result) return Ok(response);
        else return BadRequest(response);
    }

    [HttpPost("ReturnFromWorkcenterSupply")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReturnFromWorkcenterSupply([FromBody] ReturnStockFromSupplyRequest request)
    {
        var response = await service.ReturnFromWorkcenterSupply(request);

        if (response.Result) return Ok(response);
        else return BadRequest(response);
    }

    [HttpPost("ConsumePhaseStock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConsumePhaseStock([FromBody] ConsumePhaseStockRequest request)
    {
        var response = await service.ConsumePhaseStock(request);

        if (response.Result) return Ok(response);
        else return BadRequest(response);
    }

    [HttpGet("PhaseConsumptions/{workOrderPhaseId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPhaseConsumptions(Guid workOrderPhaseId)
    {
        var consumptions = service.GetPhaseConsumptions(workOrderPhaseId);
        return Ok(consumptions);
    }
}

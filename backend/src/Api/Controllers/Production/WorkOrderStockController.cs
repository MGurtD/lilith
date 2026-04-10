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
}

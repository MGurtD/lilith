using Application.Contracts;
using Domain.Entities.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Warehouse
{
    [ApiController]
    [Route("api/[controller]")]

    public class StockMovementController(IStockMovementService service) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(StockMovement request)
        {
            var response = await service.Create(request);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet]
        public IActionResult GetMovement(DateTime startTime, DateTime endTime, Guid? locationId)
        {
            var stockMovements =  service.GetBetweenDates(startTime, endTime, locationId);

            if (stockMovements != null) return Ok(stockMovements);
            else return BadRequest();
        }

    }
}
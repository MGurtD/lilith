using Application.Contracts;
using Domain.Entities.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Warehouse
{
    [ApiController]
    [Route("api/[controller]")]

    public class StockController(IStockService service) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(Stock request)
        {
            var response = await service.Create(request);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet]
        public IActionResult GetStock(Guid? locationId, Guid? referenceId)
        {
            IEnumerable<Stock> stock;
            if (locationId.HasValue)
            {
                stock = service.GetByLocation(locationId.Value);
            }
            else if (referenceId.HasValue)
            {
                stock = service.GetByReference(referenceId.Value);
            }
            else
            {
                stock = service.GetAll();
            }

            if (stock != null) return Ok(stock);
            else return BadRequest();
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Stock request)
        {
            if (id != request.Id) return BadRequest();

            var response = await service.Update(request);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

    }
}
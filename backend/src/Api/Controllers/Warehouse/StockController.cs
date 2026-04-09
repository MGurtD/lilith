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
        public async Task<IActionResult> GetStock(Guid? locationId, Guid? referenceId)
        {
            var stock = await service.GetAll(locationId, referenceId);

            if (stock != null) return Ok(stock);
            else return BadRequest();
        }
        [HttpGet("ByBillOfMaterials/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStockByBillOfMaterials(Guid id)
        {
            var stock = await service.GetStockByWorkOrderPhaseBillOfMaterialsId(id);
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
using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionPartController(IProductionPartService service) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var productionPart = await service.GetById(id);
            if (productionPart == null) return NotFound();
            else return Ok(productionPart);
        }

        [HttpGet("WorkOrder/{id:guid}")]
        public IActionResult GetByWorkOrderId(Guid id)
        {
            var productionParts = service.GetByWorkOrderId(id);
            return Ok(productionParts);
        }

        [HttpGet]
        public IActionResult GetBetweenDates(DateTime startTime, DateTime endTime, Guid? workcenterId, Guid? operatorId, Guid? workorderId)
        {
            var productionParts = service.GetBetweenDates(startTime, endTime, workcenterId, operatorId, workorderId);
            return Ok(productionParts);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductionPart productionPart)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.Create(productionPart);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = productionPart.Id }) ?? $"/{productionPart.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, ProductionPart request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await service.Update(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.Remove(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}

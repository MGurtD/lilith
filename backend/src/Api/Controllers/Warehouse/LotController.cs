using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Api.Controllers.Warehouse
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotController(ILotService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(Guid? referenceId)
        {
            if (referenceId.HasValue)
            {
                var openLots = await service.GetOpenLotsByReference(referenceId.Value);
                return Ok(openLots);
            }

            var lots = await service.GetAll();
            return Ok(lots);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lot = await service.GetById(id);
            if (lot is not null)
                return Ok(lot);
            else
                return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Lot request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.Create(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Lot request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
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
            var response = await service.Remove(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}

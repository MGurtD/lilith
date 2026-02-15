using Application.Contracts;
using Domain.Entities.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Warehouse
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController(IWarehouseService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await service.GetAll();

            return Ok(entities);
        }

        [HttpGet("Site/{id:guid}")]
        public async Task<IActionResult> GetBySiteId(Guid id)
        {
            var entities = await service.GetBySiteId(id);
            return Ok(entities);
        }

        [HttpGet("WithLocations")]
        public async Task<IActionResult> GetAllWithLocations()
        {
            var entities = await service.GetAllWithLocations();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.GetById(id);
            if (entity is null) return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Domain.Entities.Warehouse.Warehouse request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

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
        public async Task<IActionResult> Update(Guid Id, Domain.Entities.Warehouse.Warehouse request)
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

        [HttpPost("Location")]
        public async Task<IActionResult> CreateLocation(Location request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.CreateLocation(request);
            if (response.Result)
                return Ok(response);
            else
                return Conflict(response);
        }

        [HttpPut("Location/{id:guid}")]
        public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] Location request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
            if (id != request.Id) return BadRequest();

            var response = await service.UpdateLocation(request);
            if (response.Result)
                return Ok(response);
            else
                return NotFound(response);
        }

        [HttpDelete("Location/{id:guid}")]
        public async Task<IActionResult> DeleteLocation(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.RemoveLocation(id);
            if (response.Result)
                return Ok(response);
            else
                return NotFound(response);
        }
    }
}

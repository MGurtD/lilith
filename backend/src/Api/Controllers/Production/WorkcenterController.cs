using Application.Contracts;
using Domain.Constants;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkcenterController(IWorkcenterService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Workcenter request)
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await service.GetAll();
            return Ok(entities);
        }

        [HttpGet("plant")]
        public async Task<IActionResult> GetVisibleInPlant()
        {
            var entities = await service.GetVisibleInPlant();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.GetById(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpGet("workcenterload")]
        public async Task<IActionResult> GetWorkcenterLoadBetweenDatesByWorkcenterType(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var workorders = await service.GetWorkcenterLoadBetweenDatesByWorkcenterType(startDate, endDate);
            return Ok(workorders);
        }

        [HttpGet("{id:guid}/locations/{locationType}")]
        public async Task<IActionResult> GetLocationsByType(Guid id, string locationType)
        {
            if (!LocationTypeConstants.IsValid(locationType))
                return BadRequest($"Invalid location type '{locationType}'. Valid values: {string.Join(", ", LocationTypeConstants.All)}");

            var locations = await service.GetLocationsByType(id, locationType);
            return Ok(locations);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Workcenter request)
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

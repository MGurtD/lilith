using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production;

[ApiController]
[Route("api/[controller]")]
public class WorkcenterLocationController(IWorkcenterLocationService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(WorkcenterLocation request)
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await service.GetById(id);
        if (entity is not null)
            return Ok(entity);
        else
            return NotFound();
    }

    [HttpGet("workcenter/{workcenterId:guid}")]
    public async Task<IActionResult> GetByWorkcenterId(Guid workcenterId)
    {
        var entities = await service.GetByWorkcenterId(workcenterId);
        return Ok(entities);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, WorkcenterLocation request)
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
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        var response = await service.Remove(id);
        if (response.Result)
            return Ok(response.Content);
        else
            return NotFound(response);
    }
}

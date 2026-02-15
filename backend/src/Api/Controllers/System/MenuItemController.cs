using Application.Contracts;
using Domain.Entities.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class MenuItemController(IMenuItemService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool hierarchy = false)
    {
        var resp = await service.GetAll(hierarchy);
        return Ok(resp.Content);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var resp = await service.Get(id);
        if (!resp.Result) return NotFound(resp);
        return Ok(resp.Content);
    }

    [HttpPost]
    public async Task<IActionResult> Create(MenuItem item)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
        var resp = await service.Create(item);
        if (!resp.Result) return Conflict(resp);
        return Ok(resp.Content);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, MenuItem item)
    {
        if (id != item.Id) return BadRequest();
        var resp = await service.Update(item);
        if (!resp.Result) return BadRequest(resp);
        return Ok(resp.Content);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var resp = await service.Delete(id);
        if (!resp.Result) return BadRequest(resp);
        return NoContent();
    }
}
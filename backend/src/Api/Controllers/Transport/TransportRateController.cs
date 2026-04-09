using Application.Contracts;
using Domain.Entities.Transport;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Transport;

[ApiController]
[Route("api/[controller]")]
public class TransportRateController(ITransportRateService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(TransportRate request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

        var response = await service.CreateTransportRate(request);
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
        var entities = await service.GetAllTransportRates();
        return Ok(entities);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await service.GetTransportRateById(id);
        if (entity is null)
            return NotFound();
            
        return Ok(entity);
    }

    [HttpGet("Supplier/{supplierId:guid}")]
    public async Task<IActionResult> GetBySupplierId(Guid supplierId)
    {
        var entities = await service.GetTransportRatesBySupplierId(supplierId);
        return Ok(entities);
    }

    [HttpGet("Supplier/{supplierId:guid}/Current")]
    public async Task<IActionResult> GetCurrentBySupplierId(Guid supplierId)
    {
        var entities = await service.GetCurrentTransportRatesBySupplierId(supplierId);
        return Ok(entities);
    }

    [HttpGet("Supplier/{supplierId:guid}/Rate")]
    public async Task<IActionResult> GetRateByWeightAndDistance(Guid supplierId, [FromQuery] double weight, [FromQuery] double distance)
    {
        var entities = await service.GetTransportRateByWeightAndDistance(supplierId, weight, distance);
        return Ok(entities);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TransportRate request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);
        if (id != request.Id)
            return BadRequest();

        var response = await service.UpdateTransportRate(id, request);
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

        var response = await service.RemoveTransportRate(id);
        if (response.Result)
            return Ok(response.Content);
        else
            return NotFound(response);
    }

    [HttpGet("Detail/{transportRateId:guid}")]
    public async Task<IActionResult> GetDetails(Guid transportRateId)
    {
        var entities = await service.GetTransportRateDetails(transportRateId);
        return Ok(entities);
    }

    [HttpPost("Detail")]
    public async Task<IActionResult> CreateDetail(TransportRateDetail request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

        var response = await service.CreateTransportRateDetail(request);
        if (response.Result)
            return Ok(response.Content);
        else
            return Conflict(response);
    }

    [HttpPut("Detail/{id:guid}")]
    public async Task<IActionResult> UpdateDetail(Guid id, TransportRateDetail request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
        if (id != request.Id) return BadRequest();

        var response = await service.UpdateTransportRateDetail(request);
        if (response.Result)
            return Ok(response.Content);
        else
            return NotFound(response);
    }

    [HttpDelete("Detail/{id:guid}")]
    public async Task<IActionResult> DeleteDetail(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

        var response = await service.RemoveTransportRateDetail(id);
        if (response.Result)
            return Ok(response.Content);
        else
            return NotFound(response);
    }
}

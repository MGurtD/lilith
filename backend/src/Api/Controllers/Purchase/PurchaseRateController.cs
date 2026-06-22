using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Purchase;

[ApiController]
[Route("api/[controller]")]
public class PurchaseRateController(IPurchaseRateService service) : ControllerBase
{
    [HttpGet("Supplier/{supplierId:guid}")]
    public async Task<IActionResult> GetBySupplierId(Guid supplierId)
    {
        var entities = await service.GetPurchaseRatesBySupplierId(supplierId);
        return Ok(entities);
    }

    [HttpGet("Reference/{referenceId:guid}")]
    public async Task<IActionResult> GetByReferenceId(Guid referenceId)
    {
        var entities = await service.GetPurchaseRatesByReferenceId(referenceId);
        return Ok(entities);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await service.GetPurchaseRateById(id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PurchaseRate request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

        var response = await service.CreatePurchaseRate(request);
        if (response.Result)
        {
            var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
            return Created(location, response.Content);
        }
        return Conflict(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PurchaseRate request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
        if (id != request.Id) return BadRequest();

        var response = await service.UpdatePurchaseRate(id, request);
        if (response.Result) return Ok(response.Content);
        return NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await service.RemovePurchaseRate(id);
        if (response.Result) return Ok(response.Content);
        return NotFound(response);
    }

    [HttpPost("{id:guid}/Duplicate")]
    public async Task<IActionResult> Duplicate(Guid id, [FromBody] DuplicatePurchaseRateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

        var response = await service.DuplicatePurchaseRate(id, request.Name, request.ValidFrom, request.ValidTo);
        if (response.Result) return Ok(response.Content);
        return Conflict(response);
    }

    // --- Details ---

    [HttpGet("Detail/{purchaseRateId:guid}")]
    public async Task<IActionResult> GetDetails(Guid purchaseRateId)
    {
        var entities = await service.GetPurchaseRateDetails(purchaseRateId);
        return Ok(entities);
    }

    [HttpPost("Detail")]
    public async Task<IActionResult> CreateDetail(PurchaseRateDetail request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

        var response = await service.CreatePurchaseRateDetail(request);
        if (response.Result) return Ok(response.Content);
        return Conflict(response);
    }

    [HttpPut("Detail/{id:guid}")]
    public async Task<IActionResult> UpdateDetail(Guid id, PurchaseRateDetail request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
        if (id != request.Id) return BadRequest();

        var response = await service.UpdatePurchaseRateDetail(request);
        if (response.Result) return Ok(response.Content);
        return NotFound(response);
    }

    [HttpDelete("Detail/{id:guid}")]
    public async Task<IActionResult> DeleteDetail(Guid id)
    {
        var response = await service.RemovePurchaseRateDetail(id);
        if (response.Result) return Ok(response.Content);
        return NotFound(response);
    }
}

public record DuplicatePurchaseRateRequest(string Name, DateOnly ValidFrom, DateOnly ValidTo);

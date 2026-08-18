using Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class MenuItemController(IMenuItemService service) : ControllerBase
{
    private const long MaxImportFileSize = 5 * 1024 * 1024;
    private static readonly JsonSerializerOptions TransferJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

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
    public async Task<IActionResult> Create(CreateMenuItemRequest item)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
        var resp = await service.Create(item);
        if (!resp.Result) return Conflict(resp);
        return Ok(resp.Content);
    }

    [HttpGet("translations")]
    public async Task<IActionResult> GetTranslationMatrix()
    {
        var resp = await service.GetTranslationMatrix();
        return Ok(resp.Content);
    }

    [HttpPatch("translations")]
    public async Task<IActionResult> UpdateTranslations(UpdateMenuItemTranslationsRequest request)
    {
        var resp = await service.UpdateTranslations(request);
        if (!resp.Result) return BadRequest(resp);
        return Ok(resp.Content);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var resp = await service.Export();
        if (!resp.Result) return BadRequest(resp);

        var content = JsonSerializer.SerializeToUtf8Bytes(resp.Content, TransferJsonOptions);
        return File(content, "application/json", $"menu-items-{DateTime.UtcNow:yyyyMMdd}.json");
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxImportFileSize + (64 * 1024))]
    public async Task<IActionResult> Import(IFormFile? file)
    {
        GenericResponse resp;
        if (file is null)
        {
            resp = await service.Import(null);
        }
        else
        {
            await using var stream = file.OpenReadStream();
            resp = await service.Import(stream);
        }

        return resp.Result ? Ok(resp.Content) : BadRequest(resp);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMenuItemRequest item)
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

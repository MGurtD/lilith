using Application.Contracts.Services.Geolocalization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared;

[ApiController]
[Route("api/[controller]")]
public class GeoapifyController(IGeoapifyService service) : ControllerBase
{
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] AddressAutocompleteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Text is required");

        var results = await service.AutocompleteAsync(request);
        return Ok(results);
    }
}

using Application.Contracts;
using Api.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production;

[ApiController]
[Route("api/[controller]")]
public class BrandingController(IBrandingService brandingService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent() => Ok(await brandingService.GetCurrent());

    [AllowAnonymous]
    [HttpGet("current/logo/{slot}")]
    public async Task<IActionResult> GetCurrentLogo(string slot)
    {
        if (!TryParseSlot(slot, out var logoSlot))
            return NotFound();

        var logo = await brandingService.GetCurrentLogo(logoSlot);
        if (logo is null)
            return NotFound();

        Response.Headers.CacheControl = "public,max-age=86400";
        Response.Headers.Append("X-Content-Type-Options", "nosniff");
        if (logo.LastModified != default)
            Response.Headers.LastModified = logo.LastModified.ToUniversalTime().ToString("R");

        return File(logo.Content, logo.ContentType);
    }

    [HttpPut("current")]
    [Authorize(Policy = AuthorizationPolicies.BrandingWrite)]
    public async Task<IActionResult> UpdateCurrent(BrandingUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await brandingService.UpdateCurrent(request);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }

    [HttpPut("current/logo/{slot}")]
    [Authorize(Policy = AuthorizationPolicies.BrandingWrite)]
    [RequestSizeLimit((2 * 1024 * 1024) + (64 * 1024))]
    public async Task<IActionResult> UploadCurrentLogo(string slot, IFormFile? file)
    {
        if (!TryParseSlot(slot, out var logoSlot))
            return NotFound();

        var response = await brandingService.UploadCurrentLogo(logoSlot, file);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }

    [HttpDelete("current/logo/{slot}")]
    [Authorize(Policy = AuthorizationPolicies.BrandingWrite)]
    public async Task<IActionResult> RemoveCurrentLogo(string slot)
    {
        if (!TryParseSlot(slot, out var logoSlot))
            return NotFound();

        var response = await brandingService.RemoveCurrentLogo(logoSlot);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }

    [HttpPut("{enterpriseId:guid}/logo/{slot}")]
    [Authorize(Policy = AuthorizationPolicies.BrandingWrite)]
    [RequestSizeLimit((2 * 1024 * 1024) + (64 * 1024))]
    public async Task<IActionResult> UploadLogo(Guid enterpriseId, string slot, IFormFile? file)
    {
        if (!TryParseSlot(slot, out var logoSlot))
            return NotFound();

        var response = await brandingService.UploadLogo(enterpriseId, logoSlot, file);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }

    [HttpDelete("{enterpriseId:guid}/logo/{slot}")]
    [Authorize(Policy = AuthorizationPolicies.BrandingWrite)]
    public async Task<IActionResult> RemoveLogo(Guid enterpriseId, string slot)
    {
        if (!TryParseSlot(slot, out var logoSlot))
            return NotFound();

        var response = await brandingService.RemoveLogo(enterpriseId, logoSlot);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }

    private static bool TryParseSlot(string value, out BrandingLogoSlot slot)
    {
        if (string.Equals(value, "main", StringComparison.OrdinalIgnoreCase))
        {
            slot = BrandingLogoSlot.Main;
            return true;
        }

        if (string.Equals(value, "sidebar", StringComparison.OrdinalIgnoreCase))
        {
            slot = BrandingLogoSlot.Sidebar;
            return true;
        }

        slot = default;
        return false;
    }
}

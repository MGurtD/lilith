using Application.Contracts.Services.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    /// <summary>
    /// Lightweight branding endpoint used by the frontend at boot
    /// (see issue #64). Returns only the 5 branding fields of an Enterprise,
    /// not the full entity.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BrandingController(IBrandingService service) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dto = await service.GetBrandingAsync(id);
            if (dto is null)
            {
                return NotFound();
            }
            return Ok(dto);
        }
    }
}
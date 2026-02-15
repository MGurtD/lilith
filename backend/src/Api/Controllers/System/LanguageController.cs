using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguageController(ILanguageCatalog catalog, ILocalizationService localizationService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await catalog.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var item = await catalog.GetByCodeAsync(code);
            if (item == null)
            {
                var message = localizationService.GetLocalizedString("LanguageNotFound", code);
                return NotFound(new GenericResponse(false, message));
            }
            return Ok(item);
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefault()
        {
            var item = await catalog.GetDefaultAsync();
            if (item == null)
            {
                var message = localizationService.GetLocalizedString("DefaultLanguageNotFound");
                return NotFound(new GenericResponse(false, message));
            }
            return Ok(item);
        }
    }
}
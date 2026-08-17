using Application.Contracts.Migration;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataMigrationController(IDataMigrationService dataMigrationService) : ControllerBase
    {
        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        [HttpGet("Entities")]
        public IActionResult GetEntities()
        {
            return Ok(dataMigrationService.GetAvailableEntities());
        }

        [HttpGet("Template")]
        public IActionResult Template([FromQuery] string[] keys)
        {
            if (keys is null || keys.Length == 0)
                return BadRequest();

            var content = dataMigrationService.GenerateTemplate(keys);
            return File(content, XlsxContentType, "migration-template.xlsx");
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export([FromQuery] string[] keys)
        {
            if (keys is null || keys.Length == 0)
                return BadRequest();

            var content = await dataMigrationService.Export(keys);
            return File(content, XlsxContentType, "migration-export.xlsx");
        }

        [HttpPost("Import")]
        public async Task<IActionResult> Import(IFormFile file, [FromForm] string[] keys)
        {
            if (file is null || file.Length == 0 || keys is null || keys.Length == 0)
                return BadRequest();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            var report = await dataMigrationService.Import(stream, keys);
            return Ok(report);
        }
    }
}

using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController(IFileService fileService) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetEntityFiles(string entity, Guid entityId)
        {
            var files = fileService.GetEntityFiles(entity, entityId);
            return Ok(files);
        }

        [Route("Documents")]
        [HttpGet]
        public IActionResult GetEntityDocuments(string entity, Guid entityId)
        {
            var files = fileService.GetEntityDocuments(entity, entityId);
            return Ok(files);
        }

        [Route("Images")]
        [HttpGet]
        public IActionResult GetEntityImages(string entity, Guid entityId)
        {
            var files = fileService.GetEntityImages(entity, entityId);
            return Ok(files);
        }

        [Route("Download")]
        [HttpPost]
        public async Task<IActionResult> Download(Domain.Entities.File file)
        {
            if (!System.IO.File.Exists(file.Path))
                return NotFound();

            var memory = new MemoryStream();
            await using (var stream = new FileStream(file.Path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            
            // Use original filename to preserve extension
            var fileName = !string.IsNullOrEmpty(file.OriginalName) 
                ? file.OriginalName 
                : Path.GetFileName(file.Path);

            // Get proper MIME type for the file
            var contentType = Application.Services.System.FileService.GetContentType(file.Path);
            
            // CRITICAL: Use 'inline' disposition to prevent Android from renaming files
            // Encode filename using RFC 2231 for better compatibility with special characters
            var encodedFileName = Uri.EscapeDataString(fileName);
            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"; filename*=UTF-8''{encodedFileName}");
            Response.Headers.Append("Content-Type", contentType);
            Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            return File(memory, contentType, fileName);
        }

        [Route("Upload")]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string entity, [FromForm] string id)
        {
            var response = await fileService.UploadFile(file, entity, Guid.Parse(id));
            return response.Result ? Ok(response.Content) : BadRequest(response.Errors);
        }
        
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await fileService.RemoveFile(id);
            return response.Result ? Ok(response.Content) : BadRequest(response.Errors);
        }
    }
}

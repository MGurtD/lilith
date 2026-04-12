using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeyController(IApiKeyService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var keys = await service.GetAll();
            return Ok(keys.Select(k => new
            {
                k.Id,
                k.Name,
                k.Description,
                k.KeyPrefix,
                k.Scopes,
                k.ExpiresOn,
                k.LastUsedOn,
                k.Disabled,
                k.CreatedOn,
                k.UpdatedOn,
            }));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var key = await service.Get(id);
            if (key is null)
                return NotFound();

            return Ok(new
            {
                key.Id,
                key.Name,
                key.Description,
                key.KeyPrefix,
                key.Scopes,
                key.ExpiresOn,
                key.LastUsedOn,
                key.Disabled,
                key.CreatedOn,
                key.UpdatedOn,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateApiKeyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.Create(request);
            if (!response.Result)
                return Conflict(response);

            return Ok(response.Content);
        }

        [HttpPost("{id:guid}/disable")]
        public async Task<IActionResult> Disable(Guid id)
        {
            var response = await service.Disable(id);
            if (!response.Result)
                return NotFound(response);

            return Ok(response.Content);
        }
    }
}

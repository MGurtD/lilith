using Application.Contracts;
using Domain.Entities.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserFilterController(IUserFilterService service) : ControllerBase
    {
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> Get(Guid userId)
        {
            var userFilters = await service.GetUserFiltersByUserId(userId);
            if (userFilters == null || !userFilters.Any())
                return NotFound();

            return Ok(userFilters);
        }

        [HttpGet("{userId:guid}/{page}/{key}")]
        public async Task<IActionResult> Get(Guid userId, string page, string key)
        {
            var userFilter = await service.GetUserFilterByUserIdPageKey(userId, page, key);
            if (userFilter == null)
                return NotFound();

            return Ok(userFilter);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate(UserFilter request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            // Check if filter exists
            var existing = await service.GetUserFilterByUserIdPageKey(request.UserId, request.Page, request.Key);
            
            GenericResponse response;
            if (existing == null)
            {
                // Create new filter
                response = await service.CreateUserFilter(request);
                if (response.Result)
                {
                    var location = Url.Action(nameof(Get), new { userId = request.UserId, page = request.Page, key = request.Key })
                        ?? $"/{request.UserId}/{request.Page}/{request.Key}";
                    return Created(location, response.Content);
                }
            }
            else
            {
                // Update existing filter
                response = await service.UpdateUserFilter(request);
                if (response.Result)
                    return Ok(response.Content);
            }

            return BadRequest(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.RemoveUserFilter(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}

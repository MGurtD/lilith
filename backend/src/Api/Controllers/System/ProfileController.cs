using Application.Contracts;
using Domain.Entities.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController(IProfileService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await service.GetAll();
            return Ok(response.Content);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await service.Get(id);
            if (!response.Result) return NotFound(response);
            return Ok(response.Content);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Profile profile)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
            var response = await service.Create(profile);
            if (!response.Result) return Conflict(response);
            return Ok(response.Content);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, Profile profile)
        {
            if (id != profile.Id) return BadRequest();
            var response = await service.Update(profile);
            if (!response.Result) return NotFound(response);
            return Ok(response.Content);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await service.Delete(id);
            if (!response.Result) return BadRequest(response);
            return NoContent();
        }

        public record AssignMenuRequest(IEnumerable<Guid> MenuItemIds, Guid? DefaultMenuItemId);

        [HttpPost("{id:guid}/menu")]
        public async Task<IActionResult> AssignMenu(Guid id, AssignMenuRequest request)
        {
            var response = await service.AssignMenu(id, request.MenuItemIds, request.DefaultMenuItemId);
            if (!response.Result) return BadRequest(response);
            return Ok(response.Content);
        }

        [HttpGet("{id:guid}/menu")]
        public async Task<IActionResult> GetAssignedMenu(Guid id)
        {
            var response = await service.GetMenuForProfile(id);
            if (!response.Result) return NotFound(response);
            return Ok(response.Content);
        }

        [HttpGet("user/{userId:guid}/menu")]
        public async Task<IActionResult> GetMenuForUser(Guid userId)
        {
            var response = await service.GetMenuForUser(userId);
            if (!response.Result) return NotFound(response);
            return Ok(response.Content);
        }

        public record SetUserProfileRequest(Guid ProfileId);

        [HttpPost("user/{userId:guid}/profile")]
        public async Task<IActionResult> SetUserProfile(Guid userId, SetUserProfileRequest request)
        {
            var response = await service.SetUserProfile(userId, request.ProfileId);
            if (!response.Result) return BadRequest(response);
            return Ok(response.Content);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Domain.Entities.Auth;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController(IRoleService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Role request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.CreateRole(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = request.Id })
                    ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await service.GetAllRoles();
            return Ok(roles);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var role = await service.GetRoleById(id);
            if (role is not null)
                return Ok(role);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, Role request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var response = await service.UpdateRole(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.RemoveRole(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}
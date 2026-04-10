using Application.Contracts;
using Domain.Entities.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(User request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.CreateUser(request);
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

        [HttpPost("managed")]
        public async Task<IActionResult> CreateManaged(CreateManagedUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await service.CreateManagedUser(request);
            if (response.Result)
            {
                var user = response.Content as User;
                var location = user is not null
                    ? Url.Action(nameof(GetById), new { id = user.Id }) ?? $"/{user.Id}"
                    : string.Empty;
                return Created(location, response.Content);
            }

            return Conflict(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await service.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await service.GetUserById(id);
            if (user is not null)
                return Ok(user);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, User request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var response = await service.UpdateUser(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}

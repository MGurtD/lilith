using Application.Contracts;
using Domain.Entities.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTableViewController(IUserTableViewService service) : ControllerBase
    {
        // GET /api/usertableview/{userId:guid} — all views for a user
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var views = await service.GetByUserAndPage(userId, string.Empty);
            if (views == null || !views.Any())
                return NotFound();

            return Ok(views);
        }

        // GET /api/usertableview/{userId:guid}/{page} — views for a specific page
        [HttpGet("{userId:guid}/{page}")]
        public async Task<IActionResult> GetByUserIdAndPage(Guid userId, string page)
        {
            var views = await service.GetByUserAndPage(userId, page);
            if (views == null || !views.Any())
                return Ok(Array.Empty<UserTableView>());

            return Ok(views);
        }

        // GET /api/usertableview/detail/{id:guid} — single view by ID
        [HttpGet("detail/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var view = await service.GetById(id);
            if (view == null)
                return NotFound();

            return Ok(view);
        }

        // POST /api/usertableview — create new view
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserTableView userTableView)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.Create(userTableView);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = userTableView.Id })
                    ?? $"/{userTableView.Id}";
                return Created(location, response.Content);
            }

            return BadRequest(response);
        }

        // PUT /api/usertableview/{id:guid} — update existing view
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserTableView userTableView)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.Update(id, userTableView);
            if (response.Result)
                return Ok(response.Content);

            return BadRequest(response);
        }

        // DELETE /api/usertableview/{id:guid} — delete view
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await service.Delete(id);
            if (response.Result)
                return Ok(response.Content);

            return BadRequest(response);
        }

// PATCH /api/usertableview/{id:guid}/default — set or unset as default
        [HttpPatch("{id:guid}/default")]
        public async Task<IActionResult> SetDefault(Guid id, [FromQuery] bool isDefault = true)
        {
            var response = await service.SetDefault(id, isDefault);
            if (response.Result)
                return Ok(response.Content);

            return BadRequest(response);
        }

        // POST /api/usertableview/ensure-default — idempotent get-or-create
        // for the per-user, per-page default view. Returns 200 with the view
        // whether it was newly created or already existed.
        [HttpPost("ensure-default")]
        public async Task<IActionResult> EnsureDefault([FromBody] EnsureDefaultRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.EnsureDefault(request);
            if (response.Result)
                return Ok(response.Content);

            return BadRequest(response);
        }
    }
}
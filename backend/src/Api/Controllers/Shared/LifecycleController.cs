using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class LifecycleController(ILifecycleService lifecycleService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Lifecycle request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.CreateLifecycle(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
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
            var entities = await lifecycleService.GetAllLifecycles();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await lifecycleService.GetLifecycleById(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("Name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var entity = await lifecycleService.GetLifecycleByName(name);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Lifecycle request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await lifecycleService.UpdateLifecycle(request);
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

            var response = await lifecycleService.RemoveLifecycle(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Status")]
        [HttpPost]
        public async Task<IActionResult> CreateStatus(Status request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.CreateStatus(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return BadRequest(response);
        }

        [Route("Status/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateStatus(Guid id, Status request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.UpdateStatus(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Status/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveContact(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.RemoveStatus(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("StatusTransition")]
        [HttpPost]
        public async Task<IActionResult> CreateStatusTransition(StatusTransition request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.CreateStatusTransition(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return BadRequest(response);
        }

        [Route("StatusTransition/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateStatusTransition(Guid id, StatusTransition request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.UpdateStatusTransition(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("StatusTransition/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveStatusTransition(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.RemoveStatusTransition(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Status/{statusId:guid}/AvailableTransitions")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AvailableStatusTransitionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableTransitions(Guid statusId)
        {
            var transitions = await lifecycleService.GetAvailableTransitions(statusId);
            return Ok(transitions);
        }

        [Route("Tag/{id:guid}")]
        [HttpGet]
        public async Task<IActionResult> GetTagById(Guid id)
        {
            var tag = await lifecycleService.GetTagById(id);
            if (tag is not null)
                return Ok(tag);
            else
                return NotFound();
        }

        [Route("Tag/Lifecycle/{lifecycleId:guid}")]
        [HttpGet]
        public async Task<IActionResult> GetTagsByLifecycle(Guid lifecycleId)
        {
            var tags = await lifecycleService.GetTagsByLifecycle(lifecycleId);
            return Ok(tags);
        }

        [Route("Tag/Lifecycle/{lifecycleId:guid}")]
        [HttpPost]
        public async Task<IActionResult> CreateTag(Guid lifecycleId, LifecycleTag request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.CreateTag(lifecycleId, request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetTagById), new { id = request.Id }) ?? $"/Tag/{request.Id}";
                return Created(location, response.Content);
            }
            else if (response.Errors.Any(e => e.Contains("already exists")))
                return Conflict(response);
            else
                return BadRequest(response);
        }

        [Route("Tag/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateTag(Guid id, LifecycleTag request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
            if (id != request.Id) return BadRequest();

            var response = await lifecycleService.UpdateTag(request);
            if (response.Result)
                return Ok(response.Content);
            else if (response.Errors.Any(e => e.Contains("already exists")))
                return Conflict(response);
            else if (response.Errors.Any(e => e.Contains("not found")))
                return NotFound(response);
            else
                return BadRequest(response);
        }

        [Route("Tag/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveTag(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.RemoveTag(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Tag/Status/{statusId:guid}")]
        [HttpGet]
        public async Task<IActionResult> GetTagsByStatus(Guid statusId)
        {
            var tags = await lifecycleService.GetTagsByStatus(statusId);
            return Ok(tags);
        }

        [Route("Status/{statusId:guid}/Tag/{tagId:guid}")]
        [HttpPost]
        public async Task<IActionResult> AssignTagToStatus(Guid statusId, Guid tagId)
        {
            var response = await lifecycleService.AssignTagToStatus(statusId, tagId);
            if (response.Result)
                return Ok(response);
            else if (response.Errors.Any(e => e.Contains("not found")))
                return NotFound(response);
            else
                return BadRequest(response);
        }

        [Route("Status/{statusId:guid}/Tag/{tagId:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveTagFromStatus(Guid statusId, Guid tagId)
        {
            var response = await lifecycleService.RemoveTagFromStatus(statusId, tagId);
            if (response.Result)
                return Ok(response);
            else
                return NotFound(response);
        }
    }
}

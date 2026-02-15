using Application.Contracts;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferenceTypeController(IReferenceTypeService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(ReferenceType request)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState.ValidationState);

            var response = await service.CreateReferenceType(request);
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
            var entities = await service.GetAllReferenceTypes();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.GetReferenceTypeById(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, ReferenceType request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var response = await service.UpdateReferenceType(request);
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

            var response = await service.RemoveReferenceType(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}

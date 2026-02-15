using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineStatusController(IMachineStatusService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(MachineStatus request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.Create(request);
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
        public async Task<IActionResult> GetAll([FromQuery] bool includeReasons = false)
        {
            IEnumerable<MachineStatus> entities = includeReasons
                ? await service.GetAllWithReasons()
                : await service.GetAll();

            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.GetById(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, MachineStatus request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await service.Update(request);
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

            var response = await service.Remove(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        #region Reasons
        [HttpGet("Reason/{id:guid}")]
        public async Task<IActionResult> GetReasonById(Guid id)
        {
            var entity = await service.GetReasonById(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPost("Reason")]
        public async Task<IActionResult> CreateReason(MachineStatusReason request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.CreateReason(request);
            if (response.Result)
                return Ok(response);
            else
                return Conflict(response);
        }

        [HttpPut("Reason/{id:guid}")]
        public async Task<IActionResult> UpdateReason(Guid id, MachineStatusReason request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var response = await service.UpdateReason(request);
            if (response.Result)
                return Ok(response);
            else
                return NotFound(response);
        }

        [HttpDelete("Reason/{id:guid}")]
        public async Task<IActionResult> DeleteReason(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.RemoveReason(id);
            if (response.Result)
                return Ok(response);
            else
                return NotFound(response);
        }
        #endregion
    }
}

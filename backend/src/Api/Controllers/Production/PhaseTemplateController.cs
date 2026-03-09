using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhaseTemplateController(IPhaseTemplateService service) : ControllerBase
    {
        #region Template CRUD

        [HttpPost]
        public async Task<IActionResult> Create(PhaseTemplate request)
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
        public async Task<IActionResult> GetAll()
        {
            var entities = await service.GetAll();
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
        public async Task<IActionResult> Update(Guid id, PhaseTemplate request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
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

        #endregion

        #region Details

        [HttpGet("Detail/{id:guid}")]
        [SwaggerOperation("GetPhaseTemplateDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetailById(Guid id)
        {
            var detail = await service.GetDetailById(id);
            if (detail is not null)
                return Ok(detail);
            else
                return NotFound();
        }

        [HttpPost("Detail")]
        [SwaggerOperation("CreatePhaseTemplateDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDetail(PhaseTemplateDetail request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.CreateDetail(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetDetailById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("UpdatePhaseTemplateDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, PhaseTemplateDetail request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var response = await service.UpdateDetail(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("DeletePhaseTemplateDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteDetail(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.RemoveDetail(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        #endregion
    }
}

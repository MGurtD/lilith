using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftController(IShiftService service, IShiftDetailService detailService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Shift request)
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
        public async Task<IActionResult> Update(Guid Id, Shift request)
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

            var response = await service.Remove(id);  // Soft delete in service
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        #region Details
        [HttpPost("Detail")]
        public async Task<IActionResult> CreateDetail(ShiftDetail request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await detailService.Create(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return Conflict(response);
        }

        [HttpPut("Detail/{id:guid}")]
        public async Task<IActionResult> UpdateDetail(Guid Id, ShiftDetail request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await detailService.Update(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpDelete("Detail/{id:guid}")]
        public async Task<IActionResult> DeleteDetail(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await detailService.Remove(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpGet("Detail/{id:guid}")]
        public async Task<IActionResult> GetDetailsByShiftId(Guid id)
        {
            var details = await detailService.GetByShiftId(id);
            return Ok(details);
        }

        [HttpPost("Detail/ByIdBetweenHours")]
        public async Task<IActionResult> GetDetailsByShiftIdBetweenHours(Guid id, TimeOnly currentTime)
        {
            var shift = await service.GetById(id);
            if (shift is null)
                return NotFound();

            // Use repository for this specific query
            var details = await detailService.GetByShiftId(id);
            var detail = details.FirstOrDefault(e =>  e.StartTime <= currentTime && e.EndTime > currentTime);

            if (detail is null)
                return NotFound();

            return Ok(detail);
        }
        #endregion
    }
}

using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Domain.Entities.Shared;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParameterController(IParameterService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Parameter request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.Create(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }

            return Conflict(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var parameters = await service.GetAll();
            return Ok(parameters);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var parameter = await service.GetById(id);
            return parameter is not null ? Ok(parameter) : NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, Parameter request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var response = await service.Update(request);
            return response.Result ? Ok(response.Content) : NotFound(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await service.Remove(id);
            return response.Result ? Ok(response.Content) : NotFound(response);
        }
    }
}

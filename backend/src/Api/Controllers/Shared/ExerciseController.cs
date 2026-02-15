using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Domain.Entities;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciseController(IExerciseService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Exercise request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.Create(request);
            if (response.Result)
            {
                return Ok(response.Content);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var exercises = await service.GetAll();
            return Ok(exercises);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var exercise = await service.GetById(id);
            if (exercise is not null)
            {
                return Ok(exercise);
            } 
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, [FromBody]Exercise request)
        {
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
            var response = await service.Remove(id);
            if (response.Result)
                return NoContent();
            else
                return NotFound(response);
        }

    }
}
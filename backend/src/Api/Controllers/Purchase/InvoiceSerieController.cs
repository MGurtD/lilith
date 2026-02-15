using Application.Contracts;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceSerieController(IInvoiceSerieService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(InvoiceSerie request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.CreateInvoiceSerie(request);
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
            var entities = await service.GetAllInvoiceSeries();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.GetInvoiceSerieById(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, InvoiceSerie request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await service.UpdateInvoiceSerie(request);
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

            var response = await service.RemoveInvoiceSerie(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}

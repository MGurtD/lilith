using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Domain.Entities;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodController(IPaymentMethodService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(PaymentMethod request)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState.ValidationState);

            var response = await service.CreatePaymentMethod(request);
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
            var paymentMethods = await service.GetAllPaymentMethods();
            return Ok(paymentMethods);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var paymentMethod = await service.GetPaymentMethodById(id);
            if (paymentMethod is not null)
            {
                return Ok(paymentMethod);
            } 
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, PaymentMethod request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var response = await service.UpdatePaymentMethod(request);
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

            var response = await service.RemovePaymentMethod(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}
using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController(ICustomerService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Customer request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.CreateCustomer(request);
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
            var entities = await service.GetAllCustomers();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.GetCustomerById(id);
            if (entity is not null) return Ok(entity);
            else return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Customer request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await service.UpdateCustomer(request);
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

            var response = await service.RemoveCustomer(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Contact")]
        [HttpPost]
        public async Task<IActionResult> CreateContact(CustomerContact request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.CreateContact(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Contact/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateContact(Guid id, CustomerContact request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.UpdateContact(id, request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Contact/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveContact(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.RemoveContact(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Address")]
        [HttpPost]
        public async Task<IActionResult> CreateAddress(CustomerAddress request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.CreateAddress(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Address/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateAddress(Guid id, CustomerAddress request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.UpdateAddress(id, request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Address/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveAddress(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.RemoveAddress(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}

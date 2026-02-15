using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferenceController(IReferenceService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var references = await service.GetAllReferences();
            return Ok(references);
        }

        [HttpGet("Sales")]
        public async Task<IActionResult> GetAllSales()
        {
            var references = await service.GetAllSales();
            return Ok(references);
        }

        [HttpGet("Sales/{customerId:guid}")]
        public async Task<IActionResult> GetAllSalesByCustomerId(Guid customerId)
        {
            var references = await service.GetAllSalesByCustomerId(customerId);
            return Ok(references);
        }

        [HttpGet("Purchase/{categoryName?}")]
        public async Task<IActionResult> GetAllPurchase(string? categoryName)
        {
            var references = await service.GetAllPurchase(categoryName);
            return Ok(references);
        }

        [HttpGet("Production")]
        public async Task<IActionResult> GetAllProduction()
        {
            var references = await service.GetAllProduction();
            return Ok(references);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var reference = await service.GetReferenceById(id);
            if (reference is not null)
                return Ok(reference);
            else
                return NotFound();
        }

        [HttpGet("{referenceId:guid}/suppliers")]
        public async Task<IActionResult> GetReferenceSuppliers(Guid referenceId)
        {
            var suppliers = await service.GetReferenceSuppliers(referenceId);
            return Ok(suppliers);
        }

        [HttpGet("{referenceId:guid}/price/{supplierId:guid}")]
        public async Task<IActionResult> GetPrice(Guid referenceId, Guid supplierId)
        {
            decimal price = await service.GetPrice(referenceId, supplierId);
            return Ok(price);
        }

        [HttpGet("{referenceId:guid}/price")]
        public async Task<IActionResult> GetPriceNoSupplier(Guid referenceId)
        {
            decimal price = await service.GetPrice(referenceId, null);
            return Ok(price);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Domain.Entities.Shared.Reference request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.CreateReference(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = request.Id })
                    ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, Domain.Entities.Shared.Reference request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var response = await service.UpdateReference(request);
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

            var canDeleteResponse = service.CanDelete(id);
            if (!canDeleteResponse.Result)
                return BadRequest(canDeleteResponse);

            var response = await service.RemoveReference(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpGet("Formats")]
        public async Task<IActionResult> GetReferenceFormats()
        {
            var formats = await service.GetReferenceFormats();
            return Ok(formats);
        }

        [HttpGet("Formats/{id:guid}")]
        public async Task<IActionResult> GetReferenceFormatById(Guid id)
        {
            var format = await service.GetReferenceFormatById(id);
            if (format is not null)
                return Ok(format);
            else
                return NotFound();
        }
    }
}

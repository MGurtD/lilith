using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController(ISupplierService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Supplier request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.CreateSupplier(request);
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
            var entities = await service.GetAllSuppliers();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.GetSupplierById(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Supplier request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await service.UpdateSupplier(request);
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

            var response = await service.RemoveSupplier(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        #region Contacts
        [Route("Contact")]
        [HttpPost]
        public async Task<IActionResult> CreateContact(SupplierContact request)
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
        public async Task<IActionResult> UpdateContact(Guid id, SupplierContact request)
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
        #endregion

        #region References
        [Route("{supplierId:guid}/Reference/{referenceId:guid}")]
        [HttpGet]
        public async Task<IActionResult> GetSupplierReferenceBySupplierIdAndReferenceId(Guid supplierId, Guid referenceId)
        {
            var reference = await service.GetSupplierReferenceBySupplierIdAndReferenceId(supplierId, referenceId);
            return Ok(reference);
        }

        [Route("Reference/{supplierReferenceId:guid}")]
        [HttpGet]
        public async Task<IActionResult> GetReference(Guid supplierReferenceId)
        {
            var references = await service.GetSupplierReferenceById(supplierReferenceId);
            return Ok(references);
        }

        [Route("{id:guid}/Reference")]
        [HttpGet]
        public IActionResult GetReferences(Guid id)
        {
            var references = service.GetSupplierReferences(id);
            return Ok(references);
        }

        [Route("Reference/GetSupplierByReference/{referenceId:guid}")]
        [HttpGet]
        public IActionResult GetSuppliersByReference(Guid referenceId)
        {
            var suppliers = service.GetSuppliersByReference(referenceId);
            return Ok(suppliers);
        }

        [Route("Reference")]
        [HttpPost]
        public async Task<IActionResult> CreateReference(SupplierReference request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.CreateSupplierReference(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return BadRequest(response);
        }

        [Route("Reference/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateReference(Guid id, SupplierReference request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.UpdateSupplierReference(id, request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Reference/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveReference(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await service.RemoveSupplierReference(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
        #endregion
    }
}

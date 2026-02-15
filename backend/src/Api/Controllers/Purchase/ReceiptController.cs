using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptController(
        IReceiptService service, 
        IReferenceService referenceService, 
        ILifecycleService lifecycleService,
        ILocalizationService localizationService) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetReceipts(DateTime startTime, DateTime endTime, Guid? supplierId, Guid? statusId)
        {
            IEnumerable<Receipt> receipts = new List<Receipt>();
            if (supplierId.HasValue)
                receipts = service.GetBetweenDatesAndSupplier(startTime, endTime, supplierId.Value);
            else if (statusId.HasValue)
                receipts = service.GetBetweenDatesAndStatus(startTime, endTime, statusId.Value);
            else
                receipts = service.GetBetweenDates(startTime, endTime);

            if (receipts != null) return Ok(receipts.OrderByDescending(e => e.Number));
            else return BadRequest();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var receipt = await service.GetById(id);

            if (receipt == null) return BadRequest();
            else return Ok(receipt);
        }

        [HttpGet("ByReferenceId/{id:guid}")]
        public async Task<IActionResult> GetByReferenceId(Guid id)
        {
            var receipts = await service.GetReceiptsByReferenceId(id);

            if (receipts == null) return BadRequest();
            else return Ok(receipts);
        }

        [HttpGet("ToInvoice/{supplierId:guid}")]
        public IActionResult GetSupplierInvoiceableReceipts(Guid supplierId)
        {
            IEnumerable<Receipt> receipts = service.GetBySupplier(supplierId, true);

            if (receipts != null) return Ok(receipts.OrderBy(e => e.Number));
            else return BadRequest();
        }

        [HttpGet("Invoice/{id:guid}")]
        public IActionResult GetReceiptsByInvoiceId(Guid id)
        {
            IEnumerable<Receipt> receipts = service.GetByInvoice(id);

            if (receipts != null) return Ok(receipts.OrderBy(e => e.Number));
            else return BadRequest();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreatePurchaseDocumentRequest createRequest)
        {
            var response = await service.Create(createRequest);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Receipt request)
        {
            if (id != request.Id) return BadRequest();
            var receipt = await service.GetById(request.Id);
            if (receipt == null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("ReceiptNotFound", request.Id)));

            var moveToWarehouseStatus = await lifecycleService.GetStatusByName(StatusConstants.Lifecycles.Receipts, StatusConstants.Statuses.Recepcionat);
            if (moveToWarehouseStatus == null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("ReceiptStatusNotFound")));

            var warehouseResponse = new GenericResponse(true);
            if (receipt.StatusId != moveToWarehouseStatus.Id && request.StatusId == moveToWarehouseStatus.Id)
                warehouseResponse = await service.MoveToWarehose(request);                  

            if (receipt.StatusId == moveToWarehouseStatus.Id && request.StatusId != moveToWarehouseStatus.Id)
                warehouseResponse = await service.RetriveFromWarehose(request);

            var globalResponse = new GenericResponse(true);
            if (warehouseResponse.Result)
            {
                globalResponse = await service.Update(request);
                var updatedReceipt = await service.GetById(request.Id);
                if (updatedReceipt != null)
                    await referenceService.UpdatePriceFromReceipt(updatedReceipt);
            }

            if (globalResponse.Result && warehouseResponse.Result) return Ok(globalResponse);
            else return BadRequest(globalResponse);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Remove(Guid id)
        {
            var response = await service.Remove(id);

            if (response.Result) return Ok();
            else return BadRequest(response);
        }

        #region Details
        [HttpPost("Detail")]
        [SwaggerOperation("ReceiptDetailCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetail(ReceiptDetail detail)
        {
            var response = await service.AddDetail(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("ReceiptDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] ReceiptDetail detail)
        {
            var response = await service.UpdateDetail(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("ReceiptDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDetail(Guid id)
        {
            var response = await service.RemoveDetail(id);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpPost("Detail/Calculate")]
        [SwaggerOperation("ReceiptDetailCalculate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CalculateDetailPriceAndWeight(ReceiptDetail detail)
        {
            var response = await service.CalculateDetailWeightAndPrice(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }
        #endregion

        #region Receiptions

        [HttpGet("{id:guid}/Receptions")]
        [SwaggerOperation("PurchaseOrderReceiptsFromReceipt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReceptions(Guid id)
        {
            if (id == Guid.Empty) return BadRequest();
            var receipt = await service.GetById(id);
            if (receipt == null) return NotFound();

            var receptions = await service.GetReceptions(id);
            return Ok(receptions);
        }

        [HttpPost("AddReceptions")]
        [SwaggerOperation("AddPurchaseOrderReceipts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddReceptions(AddReceptionsRequest request)
        {
            var response = await service.AddReceptions(request);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        #endregion

    }
}

using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryNoteController(IDeliveryNoteService service, IDeliveryNoteReportService reportService, ILifecycleService lifecycleService, IDeliveryNotePdfService pdfService) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var receipt = await service.GetById(id);
            if (receipt == null) return BadRequest();
            else return Ok(receipt);
        }

        [HttpGet("Report/{id:guid}/pdf")]
        [Produces("application/pdf")]
        public async Task<IActionResult> GetPdf(Guid id, bool showPrices = true)
        {
            var report = await reportService.GetReportById(id, showPrices);
            return report is null ? NotFound() : File(pdfService.Generate(report), "application/pdf", $"albara-{report.DeliveryNote!.Number}.pdf");
        }

        [HttpGet("Report/{id:guid}")]
        public async Task<IActionResult> GetDeliveryNoteForReport(Guid id, bool showPrices = true)
        {
            var deliveryNote = await reportService.GetReportById(id, showPrices);
            return Ok(deliveryNote);
        }

        [HttpGet]
        public IActionResult GetDeliveryNotes(DateTime startTime, DateTime endTime, Guid? customerId, Guid? statusId)
        {
            IEnumerable<DeliveryNote> receipts = new List<DeliveryNote>();
            if (customerId.HasValue)
                receipts = service.GetBetweenDatesAndCustomer(startTime, endTime, customerId.Value);
            else if (statusId.HasValue)
                receipts = service.GetBetweenDatesAndStatus(startTime, endTime, statusId.Value);
            else
                receipts = service.GetBetweenDates(startTime, endTime);

            if (receipts != null) return Ok(receipts.OrderByDescending(e => e.Number));
            else return BadRequest();
        }

        [HttpGet("Invoice/{id:guid}")]
        public IActionResult GetDeliveryNotesByInvoiceId(Guid id)
        {
            var salesOrders = service.GetBySalesInvoice(id);
            return Ok(salesOrders);
        }

        [HttpGet("ByReferenceId/{id:guid}")]
        public async Task<IActionResult> GetByReferenceId(Guid id)
        {
            var deliveryNotes = await service.GetByReferenceId(id);
            return Ok(deliveryNotes);
        }

        [HttpGet("ToInvoice")]
        public IActionResult GetDeliveryNotesToInvoice(Guid customerId)
        {
            var salesOrderHeaders = service.GetDeliveryNotesToInvoice(customerId);
            return Ok(salesOrderHeaders.OrderBy(e => e.Number));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateHeaderRequest createRequest)
        {
            var response = await service.Create(createRequest);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPost("FromSalesOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFromSalesOrder([FromBody] SalesOrderHeader salesOrder)
        {
            var response = await service.CreateFromSalesOrder(salesOrder);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] DeliveryNote request)
        {
            if (id != request.Id) return BadRequest();

            var deliveryNote = await service.GetById(request.Id);
            if (deliveryNote == null)
                return NotFound(new GenericResponse(false, $"Albará amb ID {request.Id} inexistent"));

            var deliveredStatus = await lifecycleService.GetStatusByName(StatusConstants.Lifecycles.DeliveryNote, StatusConstants.Statuses.Entregat);
            if (deliveredStatus == null)
                return NotFound(new GenericResponse(false, $"Estat '{StatusConstants.Statuses.Entregat}' inexistent" ));

            var warehouseResponse = new GenericResponse(true);
            if (deliveryNote.StatusId != deliveredStatus.Id && request.StatusId == deliveredStatus.Id)
                warehouseResponse = await service.Deliver(request);
            if (deliveryNote.StatusId == deliveredStatus.Id && request.StatusId != deliveredStatus.Id)
                warehouseResponse = await service.UnDeliver(request);

            var globalResponse = new GenericResponse(true);
            if (warehouseResponse.Result)
                globalResponse = await service.Update(request);

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
        [HttpPost("{id:guid}/AddOrder")]
        [SwaggerOperation("DeliveryNoteAddOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddOrder(Guid id, [FromBody] SalesOrderHeader order)
        {
            var response = await service.AddOrder(id, order);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpPost("{id:guid}/RemoveOrder")]
        [SwaggerOperation("DeliveryNoteRemoveOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveOrder(Guid id, [FromBody] SalesOrderHeader order)
        {
            var response = await service.RemoveOrder(id, order);
            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }
        #endregion

    }
}

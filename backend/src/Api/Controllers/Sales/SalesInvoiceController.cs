using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesInvoiceController(ISalesInvoiceService service, ISalesInvoiceReportService reportService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateHeaderRequest createInvoiceRequest)
        {
            var response = await service.Create(createInvoiceRequest);
            return Ok(response);
        }

        [HttpPost("Rectificative")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRectificative([FromBody] CreateRectificativeInvoiceRequest dto)
        {
            var response = await service.CreateRectificative(dto);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var invoice = await service.GetById(id);
            if (invoice == null)
                return NotFound();
            else
                return Ok(invoice);
        }
        [HttpGet("Header/{id:guid}")]
        public async Task<IActionResult> GetHeaderById(Guid id)
        {
            var invoice = await service.GetById(id);
            if (invoice == null)
                return NotFound();
            else
                return Ok(invoice);
        }

        [HttpGet("Report/{id:guid}")]
        public async Task<IActionResult> GetInvoiceForReport(Guid id)
        {
            var invoiceDto = await reportService.GetReportById(id);
            if (invoiceDto is null) return NotFound();

            return Ok(invoiceDto);
        }

        [HttpGet]
        public IActionResult GetInvoices(DateTime startTime, DateTime endTime, Guid? customerId, Guid? statusId, Guid? exerciceId, Guid? excludeStatusId)
        {
            IEnumerable<SalesInvoice> invoices;
            if (exerciceId.HasValue)
                invoices = service.GetByExercise(exerciceId.Value);
            else if (customerId.HasValue)
                invoices = service.GetBetweenDatesAndCustomer(startTime, endTime, customerId.Value);
            else if (statusId.HasValue)
                invoices = service.GetBetweenDatesAndStatus(startTime, endTime, statusId.Value);
            else if (excludeStatusId.HasValue)
                invoices = service.GetBetweenDatesAndExcludeStatus(startTime, endTime, excludeStatusId.Value);
            else
                invoices = service.GetBetweenDates(startTime, endTime);

            if (invoices != null) return Ok(invoices.OrderByDescending(e => e.InvoiceNumber));
            else return BadRequest();
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SalesInvoice invoice)
        {
            var response = await service.Update(invoice);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPost]
        [Route("UpdateStatuses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatuses(ChangeStatusOfInvoicesRequest request)
        {
            if (!ModelState.IsValid) return BadRequest();

            var response = await service.ChangeStatuses(request);
            if (response.Result)
                return Ok();
            else
                return BadRequest(response.Errors);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Remove(Guid id)
        {
            var response = await service.Remove(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        #region DeliveryNote
        [HttpPost("{id:guid}/AddDeliveryNote")]
        [SwaggerOperation("SalesInvoiceAddDeliveryNote")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetailsFromDeliveryNote(Guid id, [FromBody] DeliveryNote deliveryNote)
        {
            var response = await service.AddDeliveryNote(id, deliveryNote);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPost("{id:guid}/RemoveDeliveryNote")]
        [SwaggerOperation("SalesInvoiceRemoveDeliveryNote")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveOrder(Guid id, [FromBody] DeliveryNote deliveryNote)
        {
            var response = await service.RemoveDeliveryNote(id, deliveryNote);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }
        #endregion

        #region Details
        [HttpPost("Detail")]
        [SwaggerOperation("AddSalesInvoiceDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddSalesInvoiceDetail(SalesInvoiceDetail detail)
        {
            var response = await service.AddDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("SalesInvoiceDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] SalesInvoiceDetail detail)
        {
            var response = await service.UpdateDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("SalesInvoiceDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDetail(Guid id)
        {
            var response = await service.RemoveDetail(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
        #endregion        

    }
}

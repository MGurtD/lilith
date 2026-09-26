using Application.Contracts;
using Application.Contracts.Ingestion;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseInvoiceController(
        IPaymentMethodService paymentMethodService,
        IPurchaseInvoiceService service,
        IDueDateService dueDateService,
        ILocalizationService localizationService,
        IInvoiceIngestionService ingestionService) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var invoice = await service.GetById(id);

            if (invoice == null) return BadRequest();
            else return Ok(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseInvoices(DateTime startTime, DateTime endTime, Guid? supplierId, Guid? statusId, Guid? excludeStatusId, Guid? exerciceId, Guid? paymentMethodId, DateTime? dueDateStartTime, DateTime? dueDateEndTime, string? accountNumber)
        {
            IEnumerable<PurchaseInvoice> purchaseInvoices = [];
            if (exerciceId.HasValue)
                purchaseInvoices = await service.GetByExercise(exerciceId.Value);
            else
                purchaseInvoices = service.GetFiltered(startTime, endTime, supplierId, statusId, excludeStatusId, paymentMethodId, dueDateStartTime, dueDateEndTime, accountNumber);

            if (purchaseInvoices != null) return Ok(purchaseInvoices.OrderByDescending(e => e.Number));
            else return BadRequest();
        }

        [HttpPost]
        [Route("DueDates")]
        public async Task<IActionResult> GetDueDates(PurchaseInvoice invoice)
        {
            // Recuperar metode de pagament
            var paymentMethod = await paymentMethodService.GetPaymentMethodById(invoice.PaymentMethodId);
            if (paymentMethod == null || paymentMethod.Disabled)
            {
                return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("PaymentMethodNotFoundOrDisabled", invoice.PaymentMethodId)));
            }

            var invoiceDueDates = new List<PurchaseInvoiceDueDate>();
            dueDateService.GenerateDueDates(paymentMethod, invoice.PurchaseInvoiceDate, invoice.NetAmount)
                .ForEach(dueDate => invoiceDueDates.Add(new PurchaseInvoiceDueDate()
                {
                    DueDate = dueDate.Date,
                    Amount = dueDate.Amount,
                    PurchaseInvoiceId = invoice.Id
                }));
            return Ok(invoiceDueDates);
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(PurchaseInvoice purchaseInvoice)
        {
            var response = await service.Create(purchaseInvoice);

            if (response.Result)
                return Ok();
            else
                return BadRequest(response.Errors);
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

        [HttpPost]
        [Route("RecreateDueDates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecreateDueDates(PurchaseInvoice request)
        {
            if (!ModelState.IsValid) return BadRequest();

            var response = await service.RecreateDueDates(request);
            if (response.Result)
                return Ok();
            else
                return BadRequest(response.Errors);

        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] PurchaseInvoice purchaseInvoice)
        {
            if (id != purchaseInvoice.Id) return BadRequest();

            var response = await service.Update(purchaseInvoice);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
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

        // POST /api/PurchaseInvoice/Ingest
        // Reads a supplier invoice PDF and returns a draft to review; nothing is persisted.
        // Configuration: Ingestion__ApiKey, Ingestion__ProjectId, Ingestion__BaseUrl (EU by default).
        [HttpPost("Ingest")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [ProducesResponseType(typeof(IngestPurchaseInvoiceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Ingest(
            [FromForm(Name = "pdfFile")] IFormFileCollection pdfFiles,
            CancellationToken ct)
        {
            var pdfFile = pdfFiles.FirstOrDefault();
            if (pdfFile is null || pdfFile.Length == 0)
            {
                return BadRequest(new GenericResponse(
                    false, localizationService.GetLocalizedString("InvoiceIngestionInvalidFile")));
            }

            try
            {
                await using var stream = pdfFile.OpenReadStream();
                return Ok(await ingestionService.IngestAsync(stream, pdfFile.FileName, ct));
            }
            catch (IngestionException ex)
            {
                var status = ex.Kind switch
                {
                    IngestionFailureKind.InvalidFile => StatusCodes.Status400BadRequest,
                    IngestionFailureKind.Unparseable => StatusCodes.Status422UnprocessableEntity,
                    IngestionFailureKind.NotConfigured or IngestionFailureKind.Unavailable => StatusCodes.Status503ServiceUnavailable,
                    _ => StatusCodes.Status502BadGateway,
                };
                return StatusCode(status, new GenericResponse(false, ex.Message, errorCode: ex.Kind.ToString()));
            }
        }

        #region Imports
        [HttpPost("Import")]
        [SwaggerOperation("PurchaseInvoiceImportCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddImport(PurchaseInvoiceImport import)
        {
            var response = await service.AddImport(import);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Import/{id:guid}")]
        [SwaggerOperation("PurchaseInvoiceImportUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateImport(Guid id, [FromBody] PurchaseInvoiceImport import)
        {
            var response = await service.UpdateImport(import);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("Import/{id:guid}")]
        [SwaggerOperation("PurchaseInvoiceImportDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveImport(Guid id)
        {
            var response = await service.RemoveImport(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
        #endregion

        #region DueDates
        [HttpPost("DueDate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDueDates([FromBody] IEnumerable<PurchaseInvoiceDueDate> dueDates)
        {
            var response = await service.AddDueDates(dueDates);
            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("DueDate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDueDates([FromQuery] IEnumerable<Guid> ids)
        {
            var response = await service.RemoveDueDates(ids);
            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
        #endregion

    }
}

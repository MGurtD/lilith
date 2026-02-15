using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderController(IPurchaseOrderService service, ILocalizationService localizationService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(DateTime startTime, DateTime endTime, Guid? supplierId, Guid? statusId)
        {
            var orders = await service.GetBetweenDates(startTime, endTime, supplierId, statusId);

            if (orders != null) return Ok(orders.OrderByDescending(e => e.Number));
            else return BadRequest();
        }

        [HttpGet("Report/{id:guid}")]
        public async Task<IActionResult> GetSalesOrderForReport(Guid id)
        {
            var reportDto = await service.GetDtoForReportingById(id);
            return Ok(reportDto);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var receipt = await service.GetById(id);

            if (receipt == null) return BadRequest();
            else return Ok(receipt);
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
        [HttpPost("FromWo")]
        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFromWo(PurchaseOrderFromWO[] request)
        {
            var response = await service.CreateFromWo(request);
            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] PurchaseOrder request)
        {
            if (id != request.Id) return BadRequest();
            var order = await service.GetById(request.Id);
            if (order == null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("PurchaseOrderNotFound", request.Id)));
            var response = await service.Update(request);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
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
        [SwaggerOperation("PurchaseOrderDetailCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetail(PurchaseOrderDetail detail)
        {
            var response = await service.AddDetail(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("PurchaseOrderDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] PurchaseOrderDetail detail)
        {
            var response = await service.UpdateDetail(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("PurchaseOrderDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDetail(Guid id)
        {
            var response = await service.RemoveDetail(id);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }
        #endregion

        #region Receiptions

        [HttpGet("ToReceipt")]
        [SwaggerOperation("GetOrdersToReceiptBySupplier")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOrdersToReceiptBySupplier(Guid supplierId)
        {
            var groupedOrderDetails = await service.GetGroupedOrdersWithDetailsToReceiptBySupplier(supplierId);
            if (groupedOrderDetails != null) return Ok(groupedOrderDetails.OrderBy(e => e.Reference.Code));
            else return BadRequest();
        }

        [HttpGet("{id:guid}/Receptions")]
        [SwaggerOperation("PurchaseOrderReceiptsFromOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReceptions(Guid id)
        {
            if (id == Guid.Empty) return BadRequest();
            var order = await service.GetById(id);
            if (order == null) return NotFound();

            var receptions = await service.GetReceptions(id);
            return Ok(receptions);
        }

        #endregion

    }
}

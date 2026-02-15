using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SalesOrderDetail = Domain.Entities.Sales.SalesOrderDetail;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderController(ISalesOrderService service, ISalesOrderReportService reportService) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var salesOrder = await service.GetById(id);
            if (salesOrder == null)
            {
                return NotFound();
            }
            else { return Ok(salesOrder); }
        }

        [HttpGet("Budget/{id:guid}")]
        public IActionResult GetOrderFromBudget(Guid id)
        {
            var salesOrders = service.GetOrderFromBudget(id);
            return Ok(salesOrders);
        }

        [HttpGet("Report/{id:guid}")]
        public async Task<IActionResult> GetSalesOrderReport(Guid id, bool showPrices = true)
        {
            var salesOrders = await reportService.GetReportById(id, showPrices);
            return Ok(salesOrders);
        }

        [HttpGet]
        public IActionResult GetSalesOrders(DateTime startTime, DateTime endTime, Guid? customerId)
        {
            IEnumerable<SalesOrderHeader> salesOrderHeaders = [];
            if (customerId.HasValue)
                salesOrderHeaders = service.GetBetweenDatesAndCustomer(startTime, endTime, customerId.Value);
            else
                salesOrderHeaders = service.GetBetweenDates(startTime, endTime);         
            if (salesOrderHeaders != null) return Ok(salesOrderHeaders.OrderByDescending(e => e.Number));
            else return BadRequest();
        }

        [HttpGet("DeliveryNote/{id:guid}")]
        public IActionResult GetDeliveryNoteOrders(Guid id)
        {
            var salesOrders = service.GetByDeliveryNoteId(id);
            return Ok(salesOrders);
        }

        [HttpGet("ToDeliver")]
        public IActionResult GetOrdersToDeliver(Guid customerId)
        {
            var salesOrderHeaders = service.GetOrdersToDeliver(customerId);
            return Ok(salesOrderHeaders.OrderBy(e => e.Number));
        }

        [HttpPost("FromBudget")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFromBudget(Budget budget)
        {
            var response = await service.CreateFromBudget(budget);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateHeaderRequest salesOrder)
        {
            var response = await service.Create(salesOrder);

            if (response.Result)
                return Ok(response.Content);
            else
                return BadRequest(response);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SalesOrderHeader salesOrder)
        {
            if (id != salesOrder.Id) return BadRequest();

            var response = await service.Update(salesOrder);

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

        [HttpPost("Detail")]
        [SwaggerOperation("SalesOrderDetailCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetail(SalesOrderDetail detail)
        {
            var response = await service.AddDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Cost/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCosts(Guid id)
        {
            var response = await service.UpdateCosts(id);
            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }


        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("SalesOrderDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] SalesOrderDetail detail)
        {
            var response = await service.UpdateDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("SalesOrderDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveImport(Guid id)
        {
            var response = await service.RemoveDetail(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
    }
}

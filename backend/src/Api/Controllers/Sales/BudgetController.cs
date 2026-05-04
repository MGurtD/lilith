using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController(IBudgetService service, IBudgetReportService reportService) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var budget = await service.GetById(id);
            if (budget == null)
            {
                return NotFound();
            }
            else { return Ok(budget); }
        }

        [HttpGet("Report/{id:guid}")]
        public async Task<IActionResult> GetSalesOrderForReport(Guid id)
        {
            var salesOrders = await reportService.GetReportById(id);
            return Ok(salesOrders);
        }

        [HttpGet]
        public IActionResult GetByPeriodAndCustomer([FromQuery] DateTime startTime, [FromQuery] DateTime endTime, [FromQuery] Guid? customerId)
        {
            IEnumerable<Budget> salesOrderHeaders = new List<Budget>();
            if (customerId.HasValue)
                salesOrderHeaders = service.GetBetweenDatesAndCustomer(startTime, endTime, customerId.Value);
            else
                salesOrderHeaders = service.GetBetweenDates(startTime, endTime);
            if (salesOrderHeaders != null) return Ok(salesOrderHeaders.OrderByDescending(e => e.Number));
            else return BadRequest();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateHeaderRequest salesOrder)
        {
            var response = await service.Create(salesOrder);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Budget budget)
        {
            if (id != budget.Id) return BadRequest();

            var response = await service.Update(budget);

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
        [SwaggerOperation("BudgetDetailCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetail(BudgetDetail detail)
        {
            var response = await service.AddDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("BudgetDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] BudgetDetail detail)
        {
            var response = await service.UpdateDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("BudgetDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDetail(Guid id)
        {
            var response = await service.RemoveDetail(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPost("Transport")]
        [SwaggerOperation("BudgetTransportCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddTransport(BudgetTransport transport)
        {
            var response = await service.AddTransport(transport);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Transport/{id:guid}")]
        [SwaggerOperation("BudgetTransportUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTransport(Guid id, [FromBody] BudgetTransport transport)
        {
            var response = await service.UpdateTransport(transport);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("Transport/{id:guid}")]
        [SwaggerOperation("BudgetTransportDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveTransport(Guid id)
        {
            var response = await service.RemoveTransport(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Transport/DistributeCosts/{id:guid}")]
        [SwaggerOperation("BudgetDistributeTransportCosts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DistributeTransportCosts(Guid id)
        {
            var response = await service.DistributeTransportCosts(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("DistributeAllCosts/{id:guid}")]
        [SwaggerOperation("BudgetDistributeAllCosts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DistributeAllCosts(Guid id)
        {
            var response = await service.DistributeAllCosts(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("ExternalService/{id:guid}")]
        [SwaggerOperation("BudgetExternalServiceUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateExternalService(Guid id, [FromBody] BudgetExternalServices externalService)
        {
            if (id != externalService.Id) return BadRequest();
            
            var response = await service.UpdateExternalService(externalService);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
    }
}

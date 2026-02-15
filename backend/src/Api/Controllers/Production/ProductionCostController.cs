using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionCostController(IProductionCostService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var entities = await service.GetAll();
            return Ok(entities);
        }

        [HttpGet]
        [Route("GroupedByMonthAndWorkcenterType")]
        public async Task<IActionResult> GetByMonthAndWorkcenterType(DateTime startTime, DateTime endTime)
        {
            var groupedData = await service.GetGroupedByMonthAndWorkcenterType(startTime, endTime);
            return Ok(groupedData);
        }

        [HttpGet]
        [Route("GroupedByMonthAndWorkcenter")]
        public async Task<IActionResult> GetByMonthAndWorkcenter(DateTime startTime, DateTime endTime)
        {
            var groupedData = await service.GetGroupedByMonthAndWorkcenter(startTime, endTime);
            return Ok(groupedData);
        }

        [HttpGet]
        [Route("GroupedByMonthAndOperator")]
        public async Task<IActionResult> GroupedByMonthAndOperator(DateTime startTime, DateTime endTime)
        {
            var groupedData = await service.GetGroupedByMonthAndOperator(startTime, endTime);
            return Ok(groupedData);
        }
    }
}

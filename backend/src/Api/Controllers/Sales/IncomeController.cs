using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncomeController(IIncomeService service) : ControllerBase
    {
        [HttpGet]
        [Route("Consolidated")]
        public IActionResult GetConsolidatedBetweenDates(DateTime startTime, DateTime endTime)
        {
            var entities = service.GetConsolidatedBetweenDates(startTime, endTime);
            return Ok(entities);
        }
    }
}

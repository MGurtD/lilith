using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetConversionController(IBudgetConversionService budgetConversionService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(DateTime startTime, DateTime endTime, Guid? customerId)
        {
            var result = await budgetConversionService.GetConversion(startTime, endTime, customerId);
            return Ok(result);
        }
    }
}

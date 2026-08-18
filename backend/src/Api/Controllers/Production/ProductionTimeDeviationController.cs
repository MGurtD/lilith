using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionTimeDeviationController(IProductionTimeDeviationService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(DateTime startTime, DateTime endTime, Guid? workOrderId)
        {
            var result = await service.GetDeviation(startTime, endTime, workOrderId);
            return Ok(result);
        }
    }
}

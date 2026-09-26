using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbcAnalysisController(IAbcAnalysisService service) : ControllerBase
    {
        [HttpGet("customers")]
        public async Task<IActionResult> Customers(DateTime startTime, DateTime endTime)
        {
            return Ok(await service.GetCustomerAbc(startTime, endTime));
        }

        [HttpGet("suppliers")]
        public async Task<IActionResult> Suppliers(DateTime startTime, DateTime endTime)
        {
            return Ok(await service.GetSupplierAbc(startTime, endTime));
        }
    }
}

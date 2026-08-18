using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManagementDashboardController(IManagementDashboardService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await service.GetDashboard());
        }
    }
}

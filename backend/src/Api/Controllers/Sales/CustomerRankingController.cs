using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerRankingController(ICustomerRankingService customerRankingService, ILocalizationService localizationService) : ControllerBase
    {
        [HttpGet]
        [Route("Annual")]
        public async Task<IActionResult> GetAnnualRanking(int year)
        {
            if (year < 1900 || year > 2100)
            {
                return BadRequest(localizationService.GetLocalizedString("CustomerRanking.InvalidYear"));
            }

            var rankings = await customerRankingService.GetAnnualRanking(year);

            if (rankings == null || !rankings.Any())
            {
                return Ok(Enumerable.Empty<CustomerSalesRanking>());
            }

            return Ok(rankings);
        }
    }
}

using Application.Contracts;

namespace Application.Contracts
{
    public interface ICustomerRankingService
    {
        Task<IEnumerable<CustomerSalesRanking>> GetAnnualRanking(int year);
    }
}

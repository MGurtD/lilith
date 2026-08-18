namespace Application.Contracts
{
    public interface IAbcAnalysisService
    {
        Task<AbcAnalysisResult> GetCustomerAbc(DateTime startDate, DateTime endDate);
        Task<AbcAnalysisResult> GetSupplierAbc(DateTime startDate, DateTime endDate);
    }
}

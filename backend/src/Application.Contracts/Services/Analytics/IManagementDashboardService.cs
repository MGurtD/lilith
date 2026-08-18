namespace Application.Contracts
{
    public interface IManagementDashboardService
    {
        Task<ManagementDashboardResult> GetDashboard();
    }
}

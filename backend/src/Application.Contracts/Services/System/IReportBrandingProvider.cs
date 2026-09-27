namespace Application.Contracts;

public interface IReportBrandingProvider
{
    Task<ReportBranding> GetCurrent(CancellationToken cancellationToken = default);
}

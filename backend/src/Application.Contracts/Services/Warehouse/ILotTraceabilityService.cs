namespace Application.Contracts;

public interface ILotTraceabilityService
{
    Task<LotBackwardTraceabilityDto?> GetBackwardTraceability(Guid lotId);
    Task<LotForwardTraceabilityDto?> GetForwardTraceability(Guid lotId);
    Task<LotRecallReportDto?> GetRecallReport(Guid lotId);
}

namespace Application.Contracts;

public interface IWorkOrderPhaseCloseChannel
{
    ValueTask EnqueueAsync(WorkOrderPhaseCloseRequest request, CancellationToken ct = default);
    IAsyncEnumerable<WorkOrderPhaseCloseRequest> ReadAllAsync(CancellationToken ct = default);
}

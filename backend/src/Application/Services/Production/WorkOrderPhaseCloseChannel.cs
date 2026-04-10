using System.Threading.Channels;
using Application.Contracts;

namespace Application.Services.Production;

public class WorkOrderPhaseCloseChannel : IWorkOrderPhaseCloseChannel
{
    private readonly Channel<WorkOrderPhaseCloseRequest> _channel =
        Channel.CreateUnbounded<WorkOrderPhaseCloseRequest>(new UnboundedChannelOptions
        {
            SingleReader = true
        });

    public ValueTask EnqueueAsync(WorkOrderPhaseCloseRequest request, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(request, ct);

    public IAsyncEnumerable<WorkOrderPhaseCloseRequest> ReadAllAsync(CancellationToken ct = default)
        => _channel.Reader.ReadAllAsync(ct);
}

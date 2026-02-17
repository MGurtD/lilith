using System.Threading.Channels;
using Application.Contracts;

namespace Application.Services.Production;

public class ProductionPartChannel : IProductionPartChannel
{
    private readonly Channel<GenerateProductionPartsRequest> _channel =
        Channel.CreateUnbounded<GenerateProductionPartsRequest>(new UnboundedChannelOptions
        {
            SingleReader = true
        });

    public ValueTask EnqueueAsync(GenerateProductionPartsRequest request, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(request, ct);

    public IAsyncEnumerable<GenerateProductionPartsRequest> ReadAllAsync(CancellationToken ct = default)
        => _channel.Reader.ReadAllAsync(ct);
}

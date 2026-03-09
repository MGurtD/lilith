namespace Application.Contracts;

public interface IProductionPartChannel
{
    ValueTask EnqueueAsync(GenerateProductionPartsRequest request, CancellationToken ct = default);
    IAsyncEnumerable<GenerateProductionPartsRequest> ReadAllAsync(CancellationToken ct = default);
}

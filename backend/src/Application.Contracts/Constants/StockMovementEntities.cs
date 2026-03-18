namespace Application.Contracts;

public static class StockMovementEntities
{
    public const string WorkOrderPhase = "WorkOrderPhase";
    public const string DeliveryNote = "DeliveryNote";
    public const string Receipt = "Receipt";

    public static readonly IReadOnlyList<string> All =
    [
        WorkOrderPhase,
        DeliveryNote,
        Receipt
    ];

    public static bool IsValid(string value) =>
        All.Contains(value, StringComparer.OrdinalIgnoreCase);
}

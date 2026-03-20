namespace Application.Contracts;

public static class StockMovementEntities
{
    public const string WorkOrderPhase = "WorkOrderPhase";
    public const string DeliveryNote = "DeliveryNote";
    public const string Receipt = "Receipt";
    public const string WorkOrder = "WorkOrder";

    public static readonly IReadOnlyList<string> All =
    [
        WorkOrderPhase,
        DeliveryNote,
        Receipt,
        WorkOrder
    ];

    public static bool IsValid(string value) =>
        All.Contains(value, StringComparer.OrdinalIgnoreCase);
}

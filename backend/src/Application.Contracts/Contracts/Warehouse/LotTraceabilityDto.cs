namespace Application.Contracts;

/// <summary>
/// One lot in the traceability tree. For backward trees, Children are the consumed (raw) lots;
/// for forward trees, Children are the produced (manufactured) lots. Leaves carry either
/// PurchaseOrigins (backward) or SalesDestinations (forward).
/// </summary>
public class LotTraceabilityNode
{
    public Guid LotId { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    public List<LotTraceabilityNode> Children { get; set; } = [];
    public List<PurchaseOriginDto> PurchaseOrigins { get; set; } = [];
    public List<SalesDestinationDto> SalesDestinations { get; set; } = [];
}

public class PurchaseOriginDto
{
    public Guid LotId { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
}

public class SalesDestinationDto
{
    public Guid LotId { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid DeliveryNoteId { get; set; }
    public string DeliveryNoteNumber { get; set; } = string.Empty;
    public DateTime? DeliveryDate { get; set; }
}

public class LotBackwardTraceabilityDto
{
    public Guid LotId { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceDescription { get; set; } = string.Empty;
    public LotTraceabilityNode Root { get; set; } = new();
}

public class LotForwardTraceabilityDto
{
    public Guid LotId { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceDescription { get; set; } = string.Empty;
    public LotTraceabilityNode Root { get; set; } = new();
}

public class LotRecallReportDto
{
    public Guid LotId { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceDescription { get; set; } = string.Empty;
    public int TotalAffectedDeliveryNotes { get; set; }
    public decimal TotalAffectedQuantity { get; set; }
    public List<RecallCustomerDto> AffectedCustomers { get; set; } = [];
}

public class RecallCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<RecallDeliveryNoteDto> DeliveryNotes { get; set; } = [];
}

public class RecallDeliveryNoteDto
{
    public Guid DeliveryNoteId { get; set; }
    public string DeliveryNoteNumber { get; set; } = string.Empty;
    public DateTime? DeliveryDate { get; set; }
    public Guid LotId { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

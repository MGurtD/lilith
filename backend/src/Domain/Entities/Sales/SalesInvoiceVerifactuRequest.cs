namespace Domain.Entities.Sales;

public class SalesInvoiceVerifactuRequest : Entity
{
    public required Guid SalesInvoiceId { get; set; }
    public SalesInvoice? SalesInvoice { get; set; }
    public required string Hash { get; set; }
    public required string Request { get; set; }
    public required string Status { get; set; }
    public required bool Success { get; set; }
    public string? Response { get; set; }
    public DateTime? TimestampResponse { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? QrCodeBase64 { get; set; }
}

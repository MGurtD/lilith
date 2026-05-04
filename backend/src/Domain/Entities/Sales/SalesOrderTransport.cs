namespace Domain.Entities.Sales;

public class SalesOrderTransport : Entity
{
    public Guid SalesOrderHeaderId { get; set; }    
    public Guid TransportRateDetailId { get; set; }    
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public decimal Distance { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
}

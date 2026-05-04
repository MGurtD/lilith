namespace Domain.Entities.Sales;

public class SalesOrderExternalServiceDetail : Entity
{
    public Guid SalesOrderExternalServiceId { get; set; }
    public SalesOrderExternalServices? SalesOrderExternalService { get; set; }
    public Guid SalesOrderDetailId { get; set; }
    public SalesOrderDetail? SalesOrderDetail { get; set; }
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public decimal Quantity { get; set; }
}

namespace Application.Contracts;

public class SalesOrderDetailReportDto
{
    public decimal Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

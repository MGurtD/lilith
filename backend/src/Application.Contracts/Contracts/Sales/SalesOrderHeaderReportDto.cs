namespace Application.Contracts;

public class SalesOrderHeaderReportDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
}

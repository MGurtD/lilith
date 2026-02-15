using Domain.Entities.Production;

namespace Application.Contracts;

public interface IProductionCostService
{
    Task<IEnumerable<ProductionCost>> GetAll();
    Task<IEnumerable<MonthWorkcenterTypeGroupResult>> GetGroupedByMonthAndWorkcenterType(DateTime startTime, DateTime endTime);
    Task<IEnumerable<MonthWorkcenterGroupResult>> GetGroupedByMonthAndWorkcenter(DateTime startTime, DateTime endTime);
    Task<IEnumerable<MonthOperatorGroupResult>> GetGroupedByMonthAndOperator(DateTime startTime, DateTime endTime);
}

public class MonthWorkcenterTypeGroupResult
{
    public string WorkcenterTypeName { get; set; } = string.Empty;
    public double Year { get; set; }
    public double Month { get; set; }
    public decimal TotalTime { get; set; }
    public decimal TotalCost { get; set; }
}

public class MonthWorkcenterGroupResult
{
    public string WorkcenterName { get; set; } = string.Empty;
    public string WorkcenterTypeName { get; set; } = string.Empty;
    public double Year { get; set; }
    public double Month { get; set; }
    public decimal TotalTime { get; set; }
    public decimal TotalCost { get; set; }
}

public class MonthOperatorGroupResult
{
    public string OperatorCode { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public double Year { get; set; }
    public double Month { get; set; }
    public decimal TotalTime { get; set; }
    public decimal TotalCost { get; set; }
}

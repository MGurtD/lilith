using Application.Contracts;

namespace Application.Services.Production;

public class ProductionCostService(IUnitOfWork unitOfWork) : IProductionCostService
{
    public async Task<IEnumerable<ProductionCost>> GetAll()
    {
        return await unitOfWork.ProductionCosts.GetAll();
    }

    public async Task<IEnumerable<MonthWorkcenterTypeGroupResult>> GetGroupedByMonthAndWorkcenterType(
        DateTime startTime, DateTime endTime)
    {
        var entities = await unitOfWork.ProductionCosts.FindAsync(p => p.Date >= startTime && p.Date < endTime);
        
        if (entities.Count == 0)
        {
            return [];
        }

        var groupedData = entities
            .GroupBy(x => new { x.WorkcenterTypeName, x.Year, x.Month })
            .Select(g => new MonthWorkcenterTypeGroupResult
            {
                WorkcenterTypeName = g.Key.WorkcenterTypeName!,
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalTime = g.Sum(x => x.WorkcenterTime) / 60,
                TotalCost = g.Sum(x => x.PartWorkcenterCost)
            });

        return groupedData;
    }

    public async Task<IEnumerable<MonthWorkcenterGroupResult>> GetGroupedByMonthAndWorkcenter(
        DateTime startTime, DateTime endTime)
    {
        var entities = await unitOfWork.ProductionCosts.FindAsync(p => p.Date >= startTime && p.Date < endTime);
        
        if (entities.Count == 0)
        {
            return [];
        }

        var groupedData = entities
            .GroupBy(x => new { x.WorkcenterName, x.WorkcenterTypeName, x.Year, x.Month })
            .Select(g => new MonthWorkcenterGroupResult
            {
                WorkcenterName = g.Key.WorkcenterName!,
                WorkcenterTypeName = g.Key.WorkcenterTypeName!,
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalTime = g.Sum(x => x.WorkcenterTime) / 60,
                TotalCost = g.Sum(x => x.PartWorkcenterCost)
            });

        return groupedData;
    }

    public async Task<IEnumerable<MonthOperatorGroupResult>> GetGroupedByMonthAndOperator(
        DateTime startTime, DateTime endTime)
    {
        var entities = await unitOfWork.ProductionCosts.FindAsync(p => p.Date >= startTime && p.Date < endTime);
        
        if (entities.Count == 0)
        {
            return [];
        }

        var groupedData = entities
            .GroupBy(x => new { x.OperatorCode, x.OperatorName, x.Year, x.Month })
            .Select(g => new MonthOperatorGroupResult
            {
                OperatorCode = g.Key.OperatorCode!,
                OperatorName = g.Key.OperatorName!,
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalTime = g.Sum(x => x.OperatorTime) / 60,
                TotalCost = g.Sum(x => x.PartOperatorCost)
            });

        return groupedData;
    }
}

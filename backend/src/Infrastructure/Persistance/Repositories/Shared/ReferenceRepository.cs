using Application.Contracts;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Domain.Entities.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class ReferenceRepository(ApplicationDbContext context) : Repository<Reference, Guid>(context), IReferenceRepository
{
    // Sales order, purchase order and delivery note lines, stock, stock movements,
    // production routes, work orders and bills of materials reference the reference
    // with a cascading foreign key, so deleting a reference in use would silently
    // delete them; the other uses would block the delete.
    public async Task<ReferenceUsage> GetUsage(Guid referenceId)
    {
        var usage = ReferenceUsage.None;

        if (await context.Set<SalesOrderDetail>().AnyAsync(e => e.ReferenceId == referenceId)
            || await context.Set<SalesOrderExternalServices>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.SalesOrders;
        if (await context.Set<PurchaseOrderDetail>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.PurchaseOrders;
        if (await context.Set<DeliveryNoteDetail>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.DeliveryNotes;
        if (await context.Set<BudgetDetail>().AnyAsync(e => e.ReferenceId == referenceId)
            || await context.Set<BudgetExternalServices>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.Budgets;
        if (await context.Set<ReceiptDetail>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.Receipts;
        if (await context.Set<Stock>().AnyAsync(e => e.ReferenceId == referenceId)
            || await context.Set<Lot>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.Stock;
        if (await context.Set<StockMovement>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.StockMovements;
        if (await context.Set<WorkMaster>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.ProductionRoute;
        if (await context.Set<WorkOrder>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.WorkOrders;
        if (await context.Set<WorkMasterPhaseBillOfMaterials>().AnyAsync(e => e.ReferenceId == referenceId)
            || await context.Set<WorkOrderPhaseBillOfMaterials>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.BillOfMaterials;
        if (await context.Set<WorkMasterPhase>().AnyAsync(e => e.ServiceReferenceId == referenceId)
            || await context.Set<WorkOrderPhase>().AnyAsync(e => e.ServiceReferenceId == referenceId))
            usage |= ReferenceUsage.ExternalServicePhases;
        if (await context.Set<PurchaseRateDetail>().AnyAsync(e => e.ReferenceId == referenceId))
            usage |= ReferenceUsage.PurchaseRates;

        return usage;
    }
}

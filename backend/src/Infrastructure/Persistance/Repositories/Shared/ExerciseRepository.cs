using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class ExerciseRepository(ApplicationDbContext context) : Repository<Exercise, Guid>(context), IExerciseRepository
{
    // Delivery notes, purchase orders and work orders reference their exercise with
    // a cascading foreign key, so deleting an exercise in use would silently delete
    // them; the other documents would block the delete.
    public async Task<bool> IsInUse(Guid exerciseId)
    {
        return await context.Set<DeliveryNote>().AnyAsync(e => e.ExerciseId == exerciseId)
            || await context.Set<PurchaseOrder>().AnyAsync(e => e.ExerciseId == exerciseId)
            || await context.Set<WorkOrder>().AnyAsync(e => e.ExerciseId == exerciseId)
            || await context.Set<Receipt>().AnyAsync(e => e.ExerciseId == exerciseId)
            || await context.Set<PurchaseInvoice>().AnyAsync(e => e.ExerciceId == exerciseId)
            || await context.Set<SalesInvoice>().AnyAsync(e => e.ExerciseId == exerciseId)
            || await context.Set<SalesOrderHeader>().AnyAsync(e => e.ExerciseId == exerciseId)
            || await context.Set<Budget>().AnyAsync(e => e.ExerciseId == exerciseId);
    }
}

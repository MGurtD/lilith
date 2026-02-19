# Escenaris Detallats: Adding Backend Entity

Exemples concrets per als 4 casos d'ús més comuns.

## Scenario 1: Simple Entity with Generic Repository (Pattern A)

**User request**: "Create a new Site entity with Name, Address, and EnterpriseId"

**Pattern**: Generic Repository (no custom operations needed)

**Estimated time**: ~30 minutes (9 steps)

**Steps to follow**:
1. Define Entity in `Domain/Entities/Production/Site.cs`
2. ~~Skip Step 2~~ (no custom repository interface)
3. Add to IUnitOfWork: `IRepository<Site, Guid> Sites { get; }`
4. ~~Skip Step 4~~ (no custom repository implementation)
5. Create `SiteBuilder.cs` in `Infrastructure/Persistance/EntityConfiguration/Production/`
6. Update UnitOfWork: `Sites = new Repository<Site, Guid>(context);`
7–11. Service, Controller, Migration (standard steps)

**Real project examples**: `Enterprise.cs`, `Operator.cs`, `OperatorType.cs`

---

## Scenario 2: Complex Entity with Specific Repository (Pattern B)

**User request**: "Create Area entity with custom GetByEnterprise() method"

**Pattern**: Specific Repository (needs custom queries)

**Estimated time**: ~45 minutes (11 steps)

**Steps to follow**:

1. Define Entity in `Domain/Entities/Production/Area.cs`

2. Create `IAreaRepository.cs` in `Application.Contracts/Persistance/Repositories/Production/`:
   ```csharp
   public interface IAreaRepository : IRepository<Area, Guid>
   {
       Task<IEnumerable<Area>> GetByEnterprise(Guid enterpriseId);
   }
   ```

3. Add to IUnitOfWork: `IAreaRepository Areas { get; }`

4. Implement `AreaRepository.cs` in `Infrastructure/Persistance/Repositories/Production/`:
   ```csharp
   public class AreaRepository : Repository<Area, Guid>, IAreaRepository
   {
       public AreaRepository(ApplicationDbContext context) : base(context) { }
       
       public async Task<IEnumerable<Area>> GetByEnterprise(Guid enterpriseId)
       {
           return await dbSet
               .Include(a => a.Site)
               .Where(a => a.Site.EnterpriseId == enterpriseId)
               .ToListAsync();
       }
   }
   ```

5. Create `AreaBuilder.cs` (not AreaConfiguration.cs!)

6. Update UnitOfWork: `Areas = new AreaRepository(context);`

7–11. Service, Controller, Migration (standard steps)

**Real project examples**: `AreaRepository.cs`, `WorkcenterRepository.cs`

---

## Scenario 3: Entity with Foreign Key Relationship

**User request**: "Create a Workcenter entity belonging to an Area"

**Pattern**: Generic Repository with FK validation in service

**Estimated time**: ~40 minutes (9 steps)

1. Define Entity in `Domain/Entities/Production/Workcenter.cs`:
   ```csharp
   public Guid AreaId { get; set; }
   public Area? Area { get; set; }
   ```

2. ~~Skip Step 2~~ (using generic repository)

3. Add to IUnitOfWork: `IRepository<Workcenter, Guid> Workcenters { get; }`

4. ~~Skip Step 4~~ (no custom implementation)

5. Create `WorkcenterBuilder.cs` with relationship:
   ```csharp
   builder.HasOne(w => w.Area)
       .WithMany()
       .HasForeignKey(w => w.AreaId)
       .OnDelete(DeleteBehavior.Restrict);
   ```

6. Update UnitOfWork: `Workcenters = new Repository<Workcenter, Guid>(context);`

7. **Service with FK validation**:
   ```csharp
   public async Task<GenericResponse> Create(Workcenter workcenter)
   {
       var area = await unitOfWork.Areas.Get(workcenter.AreaId);
       if (area == null)
           return new GenericResponse(false,
               localizationService.GetLocalizedString("AreaNotFound"));
       
       await unitOfWork.Workcenters.Add(workcenter);
       return new GenericResponse(true, workcenter);
   }
   ```

8–11. Controller, DI, Migration (standard steps)

**Real project examples**: `Workcenter.cs`, `WorkcenterService.cs`

---

## Scenario 4: Entity with Lifecycle Status (Pattern B)

**User request**: "Create Budget entity with workflow (Draft → Pending → Accepted/Rejected)"

**Pattern**: Specific Repository (needs custom queries like GetByStatus)

**Estimated time**: ~50 minutes (11 steps)

1. Define Entity in `Domain/Entities/Sales/Budget.cs`:
   ```csharp
   public Guid? StatusId { get; set; }
   public Status? Status { get; set; }
   ```

2. Create `IBudgetRepository.cs` with status queries:
   ```csharp
   public interface IBudgetRepository : IRepository<Budget, Guid>
   {
       Task<IEnumerable<Budget>> GetByStatus(Guid statusId);
       Task<IEnumerable<Budget>> GetPendingApproval();
   }
   ```

3. Add to IUnitOfWork: `IBudgetRepository Budgets { get; }`

4. Implement `BudgetRepository.cs` with custom queries

5. Create `BudgetBuilder.cs` with Status FK

6. Update UnitOfWork: `Budgets = new BudgetRepository(context);`

7. **Service with lifecycle initialization**:
   ```csharp
   public async Task<GenericResponse> Create(Budget budget)
   {
       var lifecycle = unitOfWork.Lifecycles
           .Find(l => l.Name == StatusConstants.Lifecycles.Budget)
           .FirstOrDefault();
       
       if (lifecycle == null)
           return new GenericResponse(false,
               localizationService.GetLocalizedString("LifecycleNotFound"));
       
       budget.StatusId = lifecycle.InitialStatusId;
       
       await unitOfWork.Budgets.Add(budget);
       return new GenericResponse(true, budget);
   }
   ```

8–11. Controller, DI, Migration (standard steps)

**Key learning**: ALWAYS use `StatusConstants.Lifecycles.Budget`, NEVER magic strings!

**Real project examples**: `Budget.cs`, `BudgetRepository.cs`, `BudgetService.cs`

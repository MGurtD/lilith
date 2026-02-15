---
name: adding-backend-entity
description: Complete 11-step workflow for adding new entities to the .NET 10 Clean Architecture backend. Use when (1) Creating new domain objects (Customer, Product, Warehouse, Material, etc.), (2) Adding database tables with EF Core migrations, (3) Implementing CRUD API endpoints, (4) Building entities with relationships (foreign keys, one-to-many, many-to-many), (5) Adding entities with lifecycle status management, (6) Creating header-detail patterns (SalesOrder + SalesOrderLines, Budget + BudgetLines), (7) Troubleshooting "entity not found", "repository not registered", or "service not found" errors. Covers all 6 architectural layers (Domain → Application.Contracts → Application → Infrastructure → Api), Repository pattern with IUnitOfWork, Service layer with ILocalizationService and GenericResponse, Controller validation patterns, EF Core entity configuration with FluentAPI, migration commands, and testing with Swagger.
---

# Adding Backend Entity

Complete workflow for adding entities across all 6 architectural layers.

## Workflow Overview

Entity addition follows this **11-step sequence** across 6 architectural layers.

**Estimated time**: 30-40 minutes for simple entity (Pattern A), 45-50 minutes for complex entity (Pattern B)

## Two Patterns: Generic vs Specific

**This project uses TWO patterns** depending on entity complexity:

### Pattern A: Generic Repository (Simple entities - 80% of cases)

Use for entities with **only basic CRUD operations** (User, Enterprise, Site, etc.):

**Steps**: 1, 3, 5, 6, 7-11 (9 steps total)

**What to skip**:
- ❌ Step 2: NO custom repository interface
- ❌ Step 4: NO custom repository implementation

**What changes**:
- ✅ Step 3: Use `IRepository<Entity, Guid>` in IUnitOfWork
- ✅ Step 5: Use `EntityBuilder.cs` (not `EntityConfiguration.cs`)
- ✅ Step 6: Initialize with `new Repository<Entity, Guid>(context)`

**Example**: Enterprise, Site, Operator, User

**Time estimate**: ~30-40 minutes

### Pattern B: Specific Repository (Complex entities - 20% of cases)

Use when entity needs **custom queries or operations** (Area, Workcenter, Budget, etc.):

**Steps**: All 11 steps (1-11)

**What changes**:
- ✅ Step 2: Create in `Persistance/Repositories/{Module}/IEntityRepository.cs` (not `Contracts/`)
- ✅ Step 5: Use `EntityBuilder.cs` (not `EntityConfiguration.cs`)

**Example**: Area, WorkOrder, Budget, Supplier

**Time estimate**: ~45-50 minutes

**When to use Specific Repository?**
- Entity needs `.Include()` with multiple navigation properties
- Custom filtering methods (GetByStatus, GetPending, etc.)
- Complex joins or aggregations
- Special business logic in queries

## Checklist

Choose your pattern (Generic vs Specific) and track progress:

### Pattern A: Generic Repository (Simple entities)

```
Entity Addition Progress (✓ = Verifiable | ⏱ = Time | ⊗ = Skip):

- [ ] 1. Define Entity (Domain) - ⏱ 5 min | ✓ Build succeeds
- [⊗] 2. Create Repository Interface - SKIP (use generic)
- [ ] 3. Add to IUnitOfWork - IRepository<Entity, Guid> - ⏱ 2 min | ✓ Build succeeds  
- [⊗] 4. Implement Repository - SKIP (use generic)
- [ ] 5. Create Entity Builder (Infrastructure) - ⏱ 5 min | ✓ No EF errors
- [ ] 6. Update UnitOfWork - new Repository<Entity, Guid>() - ⏱ 2 min | ✓ Build succeeds
- [ ] 7. Create Service Interface (Application.Contracts) - ⏱ 3 min | ✓ No errors
- [ ] 8. Implement Service (Application) - ⏱ 10 min | ✓ Build succeeds
- [ ] 9. Register Service (Api) - ⏱ 1 min | ✓ Check ApplicationServicesSetup.cs
- [ ] 10. Create Controller (Api) - ⏱ 8 min | ✓ Swagger shows endpoints
- [ ] 11. Create Migration - ⏱ 3 min | ✓ Database updated

Total: 9 steps | ~39 minutes
```

### Pattern B: Specific Repository (Complex entities)

```
Entity Addition Progress (✓ = Verifiable | ⏱ = Time | ⊗ = Skip):

- [ ] 1. Define Entity (Domain) - ⏱ 5 min | ✓ Build succeeds
- [ ] 2. Create IEntityRepository (Persistance/Repositories/{Module}/) - ⏱ 3 min | ✓ No errors
- [ ] 3. Add to IUnitOfWork - IEntityRepository - ⏱ 2 min | ✓ Build succeeds
- [ ] 4. Implement EntityRepository (Infrastructure) - ⏱ 8 min | ✓ Build succeeds
- [ ] 5. Create Entity Builder (Infrastructure) - ⏱ 5 min | ✓ No EF errors
- [ ] 6. Update UnitOfWork - new EntityRepository() - ⏱ 3 min | ✓ Build succeeds
- [ ] 7. Create Service Interface (Application.Contracts) - ⏱ 3 min | ✓ No errors
- [ ] 8. Implement Service (Application) - ⏱ 10 min | ✓ Build succeeds
- [ ] 9. Register Service (Api) - ⏱ 1 min | ✓ Check ApplicationServicesSetup.cs
- [ ] 10. Create Controller (Api) - ⏱ 8 min | ✓ Swagger shows endpoints
- [ ] 11. Create Migration - ⏱ 3 min | ✓ Database updated

Total: 11 steps | ~51 minutes
```

**Verification checkpoints:**
- **After step 7**: Run `dotnet build` → Should succeed with no errors
- **After step 10**: Verify service registered in `src/Api/Setup/ApplicationServicesSetup.cs`
- **After step 11**: Run `dotnet run --project src/Api/` → Check Swagger UI at https://localhost:5001/swagger
- **After step 12**: Run `dotnet ef migrations list --project src/Infrastructure/` → See your migration listed

**Automated validation** (recommended):
```bash
# Validate all 12 steps automatically
python .skills/adding-backend-entity/scripts/validate_entity.py YourEntity

# Example
python .skills/adding-backend-entity/scripts/validate_entity.py Supplier
```

The validation script checks all 12 steps and reports missing or incomplete steps. See `scripts/README.md` for details.

## Example Usage Scenarios

### Scenario 1: Simple Entity with Generic Repository (Pattern A)

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
7-11. Service, Controller, Migration (standard)

**Real project example**: See `Enterprise.cs`, `Operator.cs`, `OperatorType.cs`

### Scenario 2: Complex Entity with Specific Repository (Pattern B)

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
7-11. Service, Controller, Migration (standard)

**Real project example**: See `AreaRepository.cs`, `WorkcenterRepository.cs`

### Scenario 3: Entity with Foreign Key Relationship (Pattern A + Validation)

**User request**: "Create a Workcenter entity belonging to an Area"

**Pattern**: Generic Repository with FK validation in service

**Estimated time**: ~40 minutes (9 steps)

**Steps to follow**:
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
   public void Configure(EntityTypeBuilder<Workcenter> builder)
   {
       builder.ConfigureBase();
       
       builder.HasOne(w => w.Area)
           .WithMany()
           .HasForeignKey(w => w.AreaId)
           .OnDelete(DeleteBehavior.Restrict);  // ← Prevent cascade delete
   }
   ```

6. Update UnitOfWork: `Workcenters = new Repository<Workcenter, Guid>(context);`

7. **Service with FK validation**:
   ```csharp
   public async Task<GenericResponse> Create(Workcenter workcenter)
   {
       // Validate foreign key exists
       var area = await unitOfWork.Areas.Get(workcenter.AreaId);
       if (area == null)
           return new GenericResponse(false,
               localizationService.GetLocalizedString("AreaNotFound"));
       
       await unitOfWork.Workcenters.Add(workcenter);
       return new GenericResponse(true, workcenter);
   }
   ```

8-11. Controller, DI, Migration (standard)

**Key learning**: Even with Pattern A (generic), you can add business validation in the service layer.

**Real project example**: See `Workcenter.cs` and `WorkcenterService.cs`

### Scenario 4: Entity with Lifecycle Status (Pattern B)

**User request**: "Create Budget entity with workflow (Draft → Pending → Accepted/Rejected)"

**Pattern**: Specific Repository (needs custom queries like GetByStatus, GetPendingApproval)

**Estimated time**: ~50 minutes (11 steps)

**Steps to follow**:
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
       // Get lifecycle and set initial status
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

8-11. Controller, DI, Migration (standard)

**Key learning**: ALWAYS use `StatusConstants.Lifecycles.Budget`, never magic strings!

**Real project example**: See `Budget.cs`, `BudgetRepository.cs`, `BudgetService.cs`
       
       if (lifecycle == null)
           return new GenericResponse(false,
               localizationService.GetLocalizedString("LifecycleNotFound"));
       
       budget.StatusId = lifecycle.InitialStatusId;
       
       await unitOfWork.Budgets.Add(budget);
       return new GenericResponse(true, budget);
   }
   ```

9-12. Controller, DI, Migration (standard)

**Key learning**: ALWAYS use `StatusConstants.Lifecycles.Budget`, never magic strings!

**Real project example**: See `Budget.cs`, `BudgetRepository.cs`, `BudgetService.cs`

## Detailed Steps

## 1. Define Entity (Domain)

Location: `src/Domain/Entities/YourModule/YourEntity.cs`

```csharp
namespace Domain.Entities.YourModule;

public class YourEntity : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? StatusId { get; set; }
    public Status? Status { get; set; }
}
```

**Key points:**
- Inherit from `Entity` (provides Id, CreatedOn, UpdatedOn, Disabled)
- Non-nullable strings: `= string.Empty;`
- Nullable navigation: `Status? Status { get; set; }`
- NO dependencies on any other layer

## 2. Create Repository Interface (Application.Contracts) - ONLY for Pattern B

⚠️ **SKIP THIS STEP if using Pattern A (Generic Repository)**

Location: `src/Application.Contracts/Persistance/Repositories/{Module}/IYourEntityRepository.cs`

**Example**: `Application.Contracts/Persistance/Repositories/Production/IAreaRepository.cs`

```csharp
namespace Application.Contracts.Persistance.Repositories.Production;

public interface IAreaRepository : IRepository<Area, Guid>
{
    Task<IEnumerable<Area>> GetByEnterprise(Guid enterpriseId);
    Task<IEnumerable<Area>> GetActive();
}
```

**When to create specific interface:**
- Need custom queries (filtering, complex joins)
- Need eager loading logic
- Need business-specific methods

**When to skip (Pattern A):**
- Simple CRUD only
- No custom queries needed
- Examples: Enterprise, Site, Operator, OperatorType

## 3. Add to IUnitOfWork (Application.Contracts)

Update: `src/Application.Contracts/Persistance/IUnitOfWork.cs`

**Pattern A (Generic Repository):**
```csharp
public interface IUnitOfWork
{
    // ... existing repositories
    IRepository<Enterprise, Guid> Enterprises { get; }
    IRepository<Site, Guid> Sites { get; }
    
    Task<int> CompleteAsync();
}
```

**Pattern B (Specific Repository):**
```csharp
public interface IUnitOfWork
{
    // ... existing repositories
    IAreaRepository Areas { get; }
    IWorkcenterRepository Workcenters { get; }
    
    Task<int> CompleteAsync();
}
```

**Key points:**
- Property name is plural: `Enterprises`, `Areas`
- Pattern A uses `IRepository<T, Guid>`, Pattern B uses custom interface

## 4. Implement Repository (Infrastructure) - ONLY for Pattern B

⚠️ **SKIP THIS STEP if using Pattern A (Generic Repository)**

Location: `src/Infrastructure/Persistance/Repositories/{Module}/YourEntityRepository.cs`

**Example**: `Infrastructure/Persistance/Repositories/Production/AreaRepository.cs`

```csharp
namespace Infrastructure.Persistance.Repositories.Production;

public class AreaRepository : Repository<Area, Guid>, IAreaRepository
{
    public AreaRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Area>> GetByEnterprise(Guid enterpriseId)
    {
        return await dbSet
            .Include(a => a.Site)
            .ThenInclude(s => s.Enterprise)
            .Where(a => a.Site.EnterpriseId == enterpriseId)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Area>> GetActive()
    {
        return await dbSet
            .Where(a => !a.Disabled)
            .Include(a => a.Site)
            .ToListAsync();
    }
}
```

**Key points:**
- Use `Include()` for navigation properties
- Use `ThenInclude()` for nested relationships
- Use `AsNoTracking()` for read-only queries
- Override `Get()` if you need custom eager loading for single-entity retrieval

**When to create custom implementation:**
- Need filtering methods (GetByStatus, GetByDate, etc.)
- Need complex joins across multiple entities
- Need custom ordering/pagination
- Examples: Area, Workcenter, Budget

## 5. Configure Entity (Infrastructure) - Create EntityBuilder

Location: `src/Infrastructure/Persistance/EntityConfiguration/{Module}/YourEntityBuilder.cs`

⚠️ **IMPORTANT**: File name is `*Builder.cs`, NOT `*Configuration.cs`!

**Example**: `Infrastructure/Persistance/EntityConfiguration/Production/AreaBuilder.cs`

```csharp
namespace Infrastructure.Persistance.EntityConfiguration.Production;

public class AreaBuilder : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        // Apply base configuration (Id, timestamps, soft delete)
        builder.ConfigureBase();

        // Configure properties
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(a => a.Description)
            .HasMaxLength(500);

        // Configure relationships
        builder.HasOne(a => a.Site)
            .WithMany()
            .HasForeignKey(a => a.SiteId)
            .OnDelete(DeleteBehavior.Restrict);  // ← Prevent cascade delete
    }
}
```

**Common patterns:**
- Decimals: `.HasColumnType("decimal(18,4)")`
- Foreign keys: `.OnDelete(DeleteBehavior.Restrict)` for references (prevents accidental cascade)
- Collections: `.OnDelete(DeleteBehavior.Cascade)` for owned entities (details deleted with header)
- Strings: Always set `.HasMaxLength()` to prevent `nvarchar(max)`

**Real examples:**
- `AreaBuilder.cs` - entity with FK to Site
- `WorkcenterBuilder.cs` - entity with FK to Area
- `OperatorBuilder.cs` - entity with FK to OperatorType

**Note**: Entity is auto-discovered via `ApplyConfigurationsFromAssembly()` - no DbSet needed!

## 6. Update UnitOfWork (Infrastructure)

Update: `src/Infrastructure/Persistance/UnitOfWork.cs`

**Pattern A (Generic Repository):**
```csharp
public class UnitOfWork : IUnitOfWork
{
    // Add property (matches IUnitOfWork interface)
    public IRepository<Enterprise, Guid> Enterprises { get; }
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        // Initialize with generic Repository<T, TKey>
        Enterprises = new Repository<Enterprise, Guid>(context);
        Sites = new Repository<Site, Guid>(context);
    }
}
```

**Pattern B (Specific Repository):**
```csharp
public class UnitOfWork : IUnitOfWork
{
    // Add property (matches IUnitOfWork interface)
    public IAreaRepository Areas { get; }
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        // Initialize with custom repository implementation
        Areas = new AreaRepository(context);
        Workcenters = new WorkcenterRepository(context);
    }
}
```

**Key points:**
- Property name matches IUnitOfWork interface (plural: `Enterprises`, `Areas`)
- Pattern A uses `new Repository<T, Guid>(context)` - generic
- Pattern B uses `new YourEntityRepository(context)` - specific implementation

## 7. Create Service Interface (Application.Contracts)

Location: `src/Application.Contracts/Services/IYourEntityService.cs`

```csharp
namespace Application.Contracts;

public interface IYourEntityService
{
    Task<YourEntity?> GetById(Guid id);
    IEnumerable<YourEntity> GetAll();
    Task<GenericResponse> Create(YourEntity entity);
    Task<GenericResponse> Update(Guid id, YourEntity entity);
    Task<GenericResponse> Remove(Guid id);
}
```

## 8. Implement Service (Application)

Location: `src/Application/Services/YourEntityService.cs`

```csharp
namespace Application.Services;

public class YourEntityService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IYourEntityService
{
    public async Task<GenericResponse> Create(YourEntity entity)
    {
        // 1. Validate entity doesn't exist
        var exists = unitOfWork.YourEntities.Find(e => e.Id == entity.Id).Any();
        if (exists)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityAlreadyExists"));

        // 2. Validate related entities (if applicable)
        if (entity.StatusId.HasValue)
        {
            var status = await unitOfWork.Statuses.Get(entity.StatusId.Value);
            if (status == null)
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("StatusNotFound", entity.StatusId));
        }

        // 3. Persist
        await unitOfWork.YourEntities.Add(entity);
        return new GenericResponse(true, entity);
    }

    public async Task<YourEntity?> GetById(Guid id)
    {
        return await unitOfWork.YourEntities.Get(id);
    }

    public IEnumerable<YourEntity> GetAll()
    {
        return unitOfWork.YourEntities.GetAll();
    }
}
```

**Critical patterns:**
- ALWAYS inject `ILocalizationService`
- ALWAYS return `GenericResponse` for write operations
- ALWAYS use localized error messages
- Validate before persisting

## 9. Register Service (Api)

Update: `src/Api/Setup/ApplicationServicesSetup.cs`

```csharp
services.AddScoped<IYourEntityService, YourEntityService>();
```

## 10. Create Controller (Api)

Location: `src/Api/Controllers/YourEntityController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class YourEntityController(
    IYourEntityService service,
    ILocalizationService localization) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(YourEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await service.Create(entity);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await service.GetById(id);
        if (entity == null)
            return NotFound(new GenericResponse(false,
                localization.GetLocalizedString("EntityNotFound", id)));

        return Ok(entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, YourEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await service.Update(id, entity);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        var response = await service.Remove(id);
        return response.Result ? Ok() : NotFound(response);
    }
}
```

**Controller rules:**
- Validate `ModelState` before calling service
- Map `GenericResponse.Result` to HTTP status
- NO business logic in controllers

## 11. Create Migration

**Quick commands** (most common):
```bash
# Create migration
dotnet ef migrations add AddYourEntity --project src/Infrastructure/

# Review SQL BEFORE applying (IMPORTANT!)
dotnet ef migrations script --project src/Infrastructure/

# Apply to database
dotnet ef database update --project src/Infrastructure/

# Verify migration applied
dotnet ef migrations list --project src/Infrastructure/
```

**Best practices:**
- ALWAYS review generated SQL before applying
- Use descriptive names: `AddWorkOrderPhases`, `AddCustomerTypeColumn`
- Test against dev database first
- Check migration file in `src/Infrastructure/Migrations/` folder

**Complete reference**: See [references/MIGRATION_COMMANDS.md](references/MIGRATION_COMMANDS.md) for:
- Rollback commands
- Viewing SQL without applying
- Removing unapplied migrations
- Troubleshooting migration errors
- List all migrations and their status

## Common Pitfalls

### ❌ Mistake 1: Forgetting to Inject ILocalizationService

**Problem**: Service doesn't inject `ILocalizationService` in constructor

```csharp
// ❌ BAD: Missing ILocalizationService
public class ProductService(IUnitOfWork unitOfWork) : IProductService
{
    public async Task<GenericResponse> Create(Product product)
    {
        return new GenericResponse(false, "Product not found");  // Hardcoded!
    }
}
```

**Symptoms**:
- Error messages always in Catalan
- Not multilingual
- Violates localization pattern

**Fix**: Always inject `ILocalizationService`
```csharp
// ✅ GOOD: Inject ILocalizationService
public class ProductService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IProductService  // ← Add parameter
{
    public async Task<GenericResponse> Create(Product product)
    {
        return new GenericResponse(false,
            localizationService.GetLocalizedString("ProductNotFound"));
    }
}
```

### ❌ Mistake 2: Wrong Repository File Location

**Problem**: Created repository interface in wrong directory or forgot subdirectories

```csharp
// ❌ BAD: Wrong location
// src/Application.Contracts/Contracts/IAreaRepository.cs  ← WRONG!

// ❌ BAD: Missing module subdirectory
// src/Application.Contracts/Persistance/Repositories/IAreaRepository.cs  ← Missing Production/
```

**Symptoms**:
- Can't find repository interface when adding to IUnitOfWork
- Inconsistent with project structure
- Difficult to navigate large projects

**Fix**: Use correct location with module subdirectory
```csharp
// ✅ GOOD: Correct location with subdirectory
// src/Application.Contracts/Persistance/Repositories/Production/IAreaRepository.cs
// src/Infrastructure/Persistance/Repositories/Production/AreaRepository.cs

namespace Application.Contracts.Persistance.Repositories.Production;
public interface IAreaRepository : IRepository<Area, Guid> { }

namespace Infrastructure.Persistance.Repositories.Production;
public class AreaRepository : Repository<Area, Guid>, IAreaRepository { }
```

**Module subdirectories**: Sales, Purchase, Production, Warehouse, System

### ❌ Mistake 3: Not Overriding Get() in Custom Repository (Pattern B Only)

**Problem**: Repository doesn't override `Get()` to include navigation properties

```csharp
// ❌ BAD: Using base Get() without Include
public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
        // No override of Get()
    }
}
```

**Symptoms**:
- `product.Category` is null even when CategoryId has value
- Lazy loading doesn't work (not enabled in EF Core by default)
- Extra database queries when accessing navigation properties

**Fix**: Override `Get()` with `.Include()`
```csharp
// ✅ GOOD: Override Get() with eager loading
public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }
    
    public override async Task<Product?> Get(Guid id)
    {
        return await dbSet
            .Include(p => p.Category)     // ← Load related entities
            .Include(p => p.Status)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

### ❌ Mistake 3: Not Overriding Get() in Custom Repository (Pattern B Only)

**Problem**: Repository doesn't override `Get()` to include navigation properties (only applies to Pattern B - custom repositories)

```csharp
// ❌ BAD: Using base Get() without Include
public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
        // No override of Get()
    }
}
```

**Symptoms**:
- `product.Category` is null even when CategoryId has value
- Lazy loading doesn't work (not enabled in EF Core by default)
- Extra database queries when accessing navigation properties

**Fix**: Override `Get()` with `.Include()`
```csharp
// ✅ GOOD: Override Get() with eager loading
public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }
    
    public override async Task<Product?> Get(Guid id)
    {
        return await dbSet
            .Include(p => p.Category)     // ← Load related entities
            .Include(p => p.Status)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

**Note**: Pattern A (generic repository) doesn't need this - handle eager loading in service if needed.

### ❌ Mistake 4: Using "Configuration.cs" Instead of "Builder.cs"

**Problem**: Created entity configuration with wrong file name

```csharp
// ❌ BAD: Wrong naming convention
// src/Infrastructure/Persistance/EntityConfiguration/Production/AreaConfiguration.cs

public class AreaConfiguration : IEntityTypeConfiguration<Area>  // ← Wrong name!
{
    public void Configure(EntityTypeBuilder<Area> builder) { }
}
```

**Symptoms**:
- Inconsistent with project conventions
- Difficult to find files (search for "Builder" won't find "Configuration")
- Code reviews will request rename

**Fix**: Use `*Builder.cs` naming convention
```csharp
// ✅ GOOD: Correct naming convention
// src/Infrastructure/Persistance/EntityConfiguration/Production/AreaBuilder.cs

public class AreaBuilder : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ConfigureBase();
        // ... configuration
    }
}
```

**Why "Builder"?**: This project uses "Builder" suffix for all EF Core entity configurations to distinguish from other configuration types.

### ❌ Mistake 5: Using Magic Strings for Lifecycle/Status

**Problem**: Hardcoded strings for lifecycle or status names

```csharp
// ❌ BAD: Magic strings
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == "Budget")  // Typo-prone!
    .FirstOrDefault();
```

**Symptoms**:
- Null reference when typo occurs
- No compile-time checking
- Difficult to refactor

**Fix**: Use `StatusConstants`
```csharp
// ✅ GOOD: Type-safe constants
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == StatusConstants.Lifecycles.Budget)
    .FirstOrDefault();

entity.StatusId = lifecycle.InitialStatusId;
```

### ❌ Mistake 6: Forgetting to Register Service in DI

**Problem**: Created service but didn't register in `ApplicationServicesSetup.cs`

```csharp
// Step 9 SKIPPED: Not added to ApplicationServicesSetup.cs
```

**Symptoms**:
```
System.InvalidOperationException: Unable to resolve service for type 'IProductService'
```

**Fix**: Add to `src/Api/Setup/ApplicationServicesSetup.cs`
```csharp
// ✅ GOOD: Register service
services.AddScoped<IProductService, ProductService>();
```

### ❌ Mistake 7: Not Testing Migration SQL Before Applying

**Problem**: Applied migration without reviewing generated SQL (Step 11)

```bash
# ❌ BAD: Apply immediately
dotnet ef migrations add AddProduct --project src/Infrastructure/
dotnet ef database update --project src/Infrastructure/  # Applied without review!
```

**Symptoms**:
- Wrong column types (string(max) instead of varchar(200))
- Missing foreign key constraints
- Incorrect nullable settings

**Fix**: Review SQL before applying
```bash
# ✅ GOOD: Review first
dotnet ef migrations add AddProduct --project src/Infrastructure/

# Review generated SQL
dotnet ef migrations script --project src/Infrastructure/

# Check migration file in src/Infrastructure/Migrations/

# Then apply
dotnet ef database update --project src/Infrastructure/
```

### ❌ Mistake 8: Circular Dependencies Between Layers

**Problem**: Domain layer references Infrastructure

```csharp
// ❌ BAD: Domain referencing Infrastructure
using Infrastructure.Persistance;  // ❌ Domain can't reference Infrastructure!

namespace Domain.Entities;
public class Product : Entity { }
```

**Symptoms**:
- Violates Clean Architecture
- Build errors
- Tight coupling

**Fix**: Follow dependency rules
```
✅ Dependency flow (arrows show "depends on"):
API → Application → Application.Contracts → Domain
  ↘    ↓                                      ↑
    Infrastructure ─────────────────────────┘

Domain has NO dependencies
Infrastructure depends on Domain
Application depends on Application.Contracts
API depends on everything (composition root)
```

## Common Patterns

### Entity with Details (Header-Detail)

**Entity configuration:**
```csharp
builder.HasMany(h => h.Details)
    .WithOne()
    .HasForeignKey(d => d.HeaderId)
    .OnDelete(DeleteBehavior.Cascade);
```

**Repository with nested repository:**
```csharp
public class YourHeaderRepository : Repository<YourHeader, Guid>, IYourHeaderRepository
{
    public IRepository<YourDetail, Guid> Details { get; }
    
    public YourHeaderRepository(ApplicationDbContext context) : base(context)
    {
        Details = new Repository<YourDetail, Guid>(context);
    }
}
```

### Entity with Lifecycle Status

**Service initialization:**
```csharp
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == StatusConstants.Lifecycles.YourEntity)
    .FirstOrDefault();

if (lifecycle == null)
    return new GenericResponse(false,
        localizationService.GetLocalizedString("LifecycleNotFound",
            StatusConstants.Lifecycles.YourEntity));

entity.StatusId = lifecycle.InitialStatusId;
```

**Remember**: ALWAYS use `StatusConstants`, never magic strings.

## Troubleshooting

**Migration errors:**
- Ensure PostgreSQL is running
- Verify connection string in `appsettings.Development.json`
- Check all foreign keys reference existing tables

**Build errors:**
- Run `dotnet restore`
- Check project references
- Ensure correct `using` statements

**Runtime errors:**
- Service not registered? Check `ApplicationServicesSetup.cs`
- Tool not found? Ensure interface and implementation match
- Null reference? Check navigation property includes in repository

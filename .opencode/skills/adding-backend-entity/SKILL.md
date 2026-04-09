---
name: adding-backend-entity
description: Complete 11-step workflow for adding new entities to the .NET 10 Clean Architecture backend. Use when (1) Creating new domain objects (Customer, Product, Warehouse, Material, etc.), (2) Adding database tables with EF Core migrations, (3) Implementing CRUD API endpoints, (4) Building entities with relationships (foreign keys, one-to-many, many-to-many), (5) Adding entities with lifecycle status management, (6) Creating header-detail patterns (SalesOrder + SalesOrderLines, Budget + BudgetLines), (7) Troubleshooting "entity not found", "repository not registered", or "service not found" errors. Covers all 6 architectural layers (Domain → Application.Contracts → Application → Infrastructure → Api), Repository pattern with IUnitOfWork, Service layer with ILocalizationService and GenericResponse, Controller validation patterns, EF Core entity configuration with FluentAPI, migration commands, and testing with Swagger.
---

# Adding Backend Entity

Complete workflow for adding entities across all 6 architectural layers.

## Two Patterns: Generic vs Specific Repository

**Pattern A: Generic Repository** (80% of cases — simple CRUD entities)

- Skip steps 2 and 4
- Use `IRepository<Entity, Guid>` in IUnitOfWork
- Use `new Repository<Entity, Guid>(context)` in UnitOfWork
- Examples: Enterprise, Site, Operator, OperatorType

**Pattern B: Specific Repository** (20% of cases — complex queries needed)

- All 11 steps
- Create custom `IEntityRepository` + `EntityRepository`
- Use when: `Include()` with multiple navigations, custom filtering, complex joins
- Examples: Area, WorkOrder, Budget, Supplier

## Checklist

### Pattern A (9 steps, ~35 min)
```
- [ ] 1. Define Entity (Domain)                            ⏱ 5 min
- [⊗] 2. Repository Interface — SKIP
- [ ] 3. Add IRepository<Entity, Guid> to IUnitOfWork      ⏱ 2 min
- [⊗] 4. Repository Implementation — SKIP
- [ ] 5. Create EntityBuilder.cs (Infrastructure)          ⏱ 5 min
- [ ] 6. Update UnitOfWork with new Repository<>()         ⏱ 2 min
- [ ] 7. Create Service Interface (Application.Contracts)  ⏱ 3 min
- [ ] 8. Implement Service (Application)                   ⏱ 10 min
- [ ] 9. Register Service in ApplicationServicesSetup.cs   ⏱ 1 min
- [ ] 10. Create Controller (Api)                          ⏱ 8 min
- [ ] 11. Create Migration                                 ⏱ 3 min
```

### Pattern B (11 steps, ~51 min)
```
- [ ] 1. Define Entity (Domain)                            ⏱ 5 min
- [ ] 2. Create IEntityRepository (Application.Contracts)  ⏱ 3 min
- [ ] 3. Add IEntityRepository to IUnitOfWork              ⏱ 2 min
- [ ] 4. Implement EntityRepository (Infrastructure)       ⏱ 8 min
- [ ] 5. Create EntityBuilder.cs (Infrastructure)          ⏱ 5 min
- [ ] 6. Update UnitOfWork with new EntityRepository()     ⏱ 3 min
- [ ] 7. Create Service Interface (Application.Contracts)  ⏱ 3 min
- [ ] 8. Implement Service (Application)                   ⏱ 10 min
- [ ] 9. Register Service in ApplicationServicesSetup.cs   ⏱ 1 min
- [ ] 10. Create Controller (Api)                          ⏱ 8 min
- [ ] 11. Create Migration                                 ⏱ 3 min
```

**Verification**: `dotnet build` must succeed after steps 1, 3, 6, 7, 9, 10. Swagger at https://localhost:5001/swagger after step 10.

**Automated validation**:
```bash
python .opencode/skills/adding-backend-entity/scripts/validate_entity.py YourEntity
```

## Detailed Steps

### 1. Define Entity (Domain)

`src/Domain/Entities/{Module}/YourEntity.cs`

```csharp
namespace Domain.Entities.Production;

public class YourEntity : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? StatusId { get; set; }
    public Status? Status { get; set; }
}
```

- Inherit from `Entity` (Id, CreatedOn, UpdatedOn, Disabled)
- Non-nullable strings: `= string.Empty;`
- Navigation properties: nullable (`Status? Status`)
- NO dependencies on other layers

### 2. Create Repository Interface — Pattern B only

`src/Application.Contracts/Persistance/Repositories/{Module}/IYourEntityRepository.cs`

```csharp
namespace Application.Contracts.Persistance.Repositories.Production;

public interface IYourEntityRepository : IRepository<YourEntity, Guid>
{
    Task<IEnumerable<YourEntity>> GetByStatus(Guid statusId);
}
```

### 3. Add to IUnitOfWork

`src/Application.Contracts/Persistance/IUnitOfWork.cs`

```csharp
// Pattern A
IRepository<YourEntity, Guid> YourEntities { get; }

// Pattern B
IYourEntityRepository YourEntities { get; }
```

Property name is **plural**.

### 4. Implement Repository — Pattern B only

`src/Infrastructure/Persistance/Repositories/{Module}/YourEntityRepository.cs`

```csharp
namespace Infrastructure.Persistance.Repositories.Production;

public class YourEntityRepository : Repository<YourEntity, Guid>, IYourEntityRepository
{
    public YourEntityRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<YourEntity>> GetByStatus(Guid statusId)
    {
        return await dbSet
            .Include(e => e.Status)
            .Where(e => e.StatusId == statusId)
            .ToListAsync();
    }

    // Override Get() to include navigation properties
    public override async Task<YourEntity?> Get(Guid id)
    {
        return await dbSet
            .Include(e => e.Status)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
```

### 5. Create Entity Builder

`src/Infrastructure/Persistance/EntityConfiguration/{Module}/YourEntityBuilder.cs`

⚠️ File name: `*Builder.cs` — NEVER `*Configuration.cs`

```csharp
namespace Infrastructure.Persistance.EntityConfiguration.Production;

public class YourEntityBuilder : IEntityTypeConfiguration<YourEntity>
{
    public void Configure(EntityTypeBuilder<YourEntity> builder)
    {
        builder.ConfigureBase(); // Id, timestamps, soft delete

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Foreign key (use Restrict to prevent cascade)
        builder.HasOne(e => e.Status)
            .WithMany()
            .HasForeignKey(e => e.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

Common types: `decimal(18,4)` for amounts, `HasMaxLength()` always on strings.  
Entity auto-discovered via `ApplyConfigurationsFromAssembly()` — no DbSet needed.

### 6. Update UnitOfWork

`src/Infrastructure/Persistance/UnitOfWork.cs`

```csharp
// Pattern A
public IRepository<YourEntity, Guid> YourEntities { get; }
// In constructor:
YourEntities = new Repository<YourEntity, Guid>(context);

// Pattern B
public IYourEntityRepository YourEntities { get; }
// In constructor:
YourEntities = new YourEntityRepository(context);
```

### 7. Create Service Interface

`src/Application.Contracts/Services/{Module}/IYourEntityService.cs`

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

### 8. Implement Service

`src/Application/Services/{Module}/YourEntityService.cs`

```csharp
namespace Application.Services;

public class YourEntityService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IYourEntityService
{
    public async Task<GenericResponse> Create(YourEntity entity)
    {
        var exists = unitOfWork.YourEntities.Find(e => e.Id == entity.Id).Any();
        if (exists)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityAlreadyExists"));

        await unitOfWork.YourEntities.Add(entity);
        return new GenericResponse(true, entity);
    }

    public async Task<YourEntity?> GetById(Guid id) =>
        await unitOfWork.YourEntities.Get(id);

    public IEnumerable<YourEntity> GetAll() =>
        unitOfWork.YourEntities.GetAll();

    public async Task<GenericResponse> Update(Guid id, YourEntity entity)
    {
        var existing = await unitOfWork.YourEntities.Get(id);
        if (existing == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));

        await unitOfWork.YourEntities.Update(entity);
        return new GenericResponse(true, entity);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var existing = await unitOfWork.YourEntities.Get(id);
        if (existing == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));

        await unitOfWork.YourEntities.Remove(existing);
        return new GenericResponse(true);
    }
}
```

### 9. Register Service

`src/Api/Setup/ApplicationServicesSetup.cs`

```csharp
services.AddScoped<IYourEntityService, YourEntityService>();
```

### 10. Create Controller

`src/Api/Controllers/{Module}/YourEntityController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class YourEntityController(
    IYourEntityService service,
    ILocalizationService localization) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(service.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await service.GetById(id);
        if (entity == null)
            return NotFound(new GenericResponse(false,
                localization.GetLocalizedString("EntityNotFound", id)));
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create(YourEntity entity)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var response = await service.Create(entity);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, YourEntity entity)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
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

### 11. Create Migration

```bash
# Create
dotnet ef migrations add AddYourEntity --project src/Infrastructure/

# Review SQL BEFORE applying
dotnet ef migrations script --project src/Infrastructure/

# Apply
dotnet ef database update --project src/Infrastructure/
```

**Complete reference**: See [references/MIGRATION_COMMANDS.md](references/MIGRATION_COMMANDS.md) for rollback, troubleshooting, and advanced commands.

## Common Pitfalls

| # | Mistake | Symptom | Fix |
|---|---------|---------|-----|
| 1 | Missing `ILocalizationService` | Hardcoded error strings, not multilingual | Always inject in service constructor |
| 2 | Wrong repository location | Interface not found, inconsistent structure | Pattern B repos: `Application.Contracts/Persistance/Repositories/{Module}/` |
| 3 | Not overriding `Get()` in Pattern B | Navigation properties null | Override `Get()` with `.Include()` |
| 4 | Using `*Configuration.cs` | Build inconsistency, reviewer rejections | Always use `*Builder.cs` suffix |
| 5 | Magic strings for lifecycle/status | NullRef on typos, no compile-time check | Use `StatusConstants.Lifecycles.X` |
| 6 | Forgetting DI registration | `InvalidOperationException: Unable to resolve IYourEntityService` | Add `AddScoped<>` in `ApplicationServicesSetup.cs` |
| 7 | Applying migration without reviewing SQL | Wrong column types, missing constraints | Always `migrations script` before `database update` |
| 8 | Domain referencing Infrastructure | Build error, Clean Architecture violation | Dependencies only flow inward |

## Common Patterns

### Entity with Details (Header-Detail)

```csharp
// Builder
builder.HasMany(h => h.Details)
    .WithOne()
    .HasForeignKey(d => d.HeaderId)
    .OnDelete(DeleteBehavior.Cascade); // Cascade on owned details

// Repository
public class HeaderRepository : Repository<Header, Guid>, IHeaderRepository
{
    public IRepository<Detail, Guid> Details { get; }
    public HeaderRepository(ApplicationDbContext context) : base(context)
    {
        Details = new Repository<Detail, Guid>(context);
    }
}
```

### Entity with Lifecycle Status

```csharp
// Service - initialize with lifecycle's initial status
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == StatusConstants.Lifecycles.YourEntity)
    .FirstOrDefault();

if (lifecycle == null)
    return new GenericResponse(false,
        localizationService.GetLocalizedString("LifecycleNotFound",
            StatusConstants.Lifecycles.YourEntity));

entity.StatusId = lifecycle.InitialStatusId;
```

## Detailed Scenarios

See [references/SCENARIOS.md](references/SCENARIOS.md) for complete worked examples:
- Scenario 1: Simple entity (Pattern A) — Site with Name + Address
- Scenario 2: Complex entity (Pattern B) — Area with custom GetByEnterprise()
- Scenario 3: Entity with FK relationship — Workcenter belonging to Area
- Scenario 4: Entity with lifecycle status — Budget with workflow

**When to read scenarios**: Implementing a specific entity type for the first time, or unsure which pattern to apply.

## Troubleshooting

- **Migration fails**: Ensure PostgreSQL is running, check connection string in `appsettings.Development.json`
- **Build error**: Run `dotnet restore`, check correct `using` statements
- **Service not found at runtime**: Check `ApplicationServicesSetup.cs` registration
- **Navigation property null**: Override `Get()` with `.Include()` in repository (Pattern B)

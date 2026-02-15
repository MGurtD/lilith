# Architectural Patterns

This document explains the key design patterns used throughout the Lilith Backend with concise code examples.

## Table of Contents

1. [Repository Pattern](#repository-pattern)
2. [Unit of Work Pattern](#unit-of-work-pattern)
3. [Service Layer Pattern](#service-layer-pattern)
4. [GenericResponse Pattern](#genericresponse-pattern)
5. [Primary Constructor Pattern](#primary-constructor-pattern)
6. [Entity Configuration Pattern](#entity-configuration-pattern)

---

## Repository Pattern

Abstracts data access logic with a generic interface and specialized implementations.

### Generic Interface

```csharp
public interface IRepository<TEntity, TId> where TEntity : class
{
    Task<TEntity?> Get(TId id);
    Task<IEnumerable<TEntity>> GetAll();
    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task Add(TEntity entity);
    Task Update(TEntity entity);
    Task Remove(TEntity entity);
}
```

### Specialized Repository Interface

```csharp
public interface IBudgetRepository : IRepository<Budget, Guid>
{
    IRepository<BudgetDetail, Guid> Details { get; }
}
```

### Implementation

```csharp
public class BudgetRepository : Repository<Budget, Guid>, IBudgetRepository
{
    public IRepository<BudgetDetail, Guid> Details { get; }

    public BudgetRepository(ApplicationDbContext context) : base(context)
    {
        Details = new Repository<BudgetDetail, Guid>(context);
    }

    public override async Task<Budget?> Get(Guid id)
    {
        return await dbSet
            .Include(b => b.Details)
                .ThenInclude(d => d.Reference)
            .Include(b => b.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
```

**Key Points:**

- Generic base handles simple CRUD
- Specialized repositories override for complex queries
- Use `Include()` for eager loading
- Use `AsNoTracking()` for read-only operations
- Nested repositories for child entities (e.g., `Details`)

---

## Unit of Work Pattern

Manages repositories and ensures transactional consistency.

### Interface

```csharp
public interface IUnitOfWork
{
    // Sales
    IBudgetRepository Budgets { get; }
    ISalesOrderHeaderRepository SalesOrderHeaders { get; }
    ISalesInvoiceRepository SalesInvoices { get; }

    // Production
    IWorkOrderRepository WorkOrders { get; }
    IWorkMasterRepository WorkMasters { get; }

    // Purchase
    IPurchaseOrderRepository PurchaseOrders { get; }

    // Shared
    IRepository<Reference, Guid> References { get; }
    IRepository<Lifecycle, Guid> Lifecycles { get; }
    IRepository<Exercise, Guid> Exercises { get; }

    Task<int> CompleteAsync();
    void Dispose();
}
```

### Implementation

```csharp
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;

    public IBudgetRepository Budgets { get; }
    public ISalesOrderHeaderRepository SalesOrderHeaders { get; }
    // ... initialize all repositories in constructor

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Budgets = new BudgetRepository(context);
        SalesOrderHeaders = new SalesOrderHeaderRepository(context);
        // ...
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

**Usage in Services:**

```csharp
public async Task<GenericResponse> CreateBudget(Budget budget)
{
    await unitOfWork.Budgets.Add(budget);
    // SaveChanges called automatically in repository
    return new GenericResponse(true, budget);
}
```

**Key Points:**

- Single instance per request (scoped lifetime)
- All repositories share the same `DbContext`
- `CompleteAsync()` commits all changes in one transaction
- Dispose releases database connection

---

## Service Layer Pattern

Encapsulates business logic and orchestrates repository operations.

### Interface

```csharp
public interface IBudgetService
{
    Task<Budget?> GetById(Guid id);
    IEnumerable<Budget> GetBetweenDates(DateTime startDate, DateTime endDate);
    Task<GenericResponse> Create(CreateHeaderRequest request);
    Task<GenericResponse> Update(Budget budget);
    Task<GenericResponse> Remove(Guid id);
    Task<GenericResponse> AddDetail(BudgetDetail detail);
}
```

### Implementation

```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    IExerciseService exerciseService,
    ILocalizationService localizationService) : IBudgetService
{
    public async Task<GenericResponse> Create(CreateHeaderRequest request)
    {
        // 1. Validate customer exists
        var customer = await unitOfWork.Customers.Get(request.CustomerId);
        if (customer == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerNotFound"));

        // 2. Generate document number
        var counterResponse = await exerciseService.GetNextCounter(
            request.ExerciseId, "budget");
        if (!counterResponse.Result)
            return counterResponse;

        // 3. Get initial lifecycle status
        var lifecycle = unitOfWork.Lifecycles
            .Find(l => l.Name == StatusConstants.Lifecycles.Budget)
            .FirstOrDefault();

        if (lifecycle == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("LifecycleNotFound",
                    StatusConstants.Lifecycles.Budget));

        // 4. Create entity
        var budget = new Budget
        {
            Id = request.Id,
            Number = counterResponse.Content.ToString()!,
            Date = request.Date,
            CustomerId = request.CustomerId,
            ExerciseId = request.ExerciseId,
            StatusId = lifecycle.InitialStatusId
        };

        // 5. Persist
        await unitOfWork.Budgets.Add(budget);
        return new GenericResponse(true, budget);
    }

    public async Task<Budget?> GetById(Guid id)
    {
        return await unitOfWork.Budgets.Get(id);
    }
}
```

**Key Points:**

- **Primary constructor** for clean dependency injection
- **Always localize** error messages
- **Use StatusConstants** for lifecycle/status names
- **Return `GenericResponse`** for operations that can fail
- **Return entities directly** for read operations
- **Validate before persisting** - check entity existence, business rules

---

## GenericResponse Pattern

Standardized response wrapper for service operations.

### Class Definition

```csharp
public class GenericResponse
{
    public bool Result { get; }
    public IList<string> Errors { get; }
    public object? Content { get; }

    public GenericResponse(bool result, object? content = null)
    {
        Result = result;
        Content = content;
        Errors = new List<string>();
    }

    public GenericResponse(bool result, string error, object? content = null)
    {
        Result = result;
        Errors = new List<string> { error };
        Content = content;
    }

    public GenericResponse(bool result, IList<string> errors, object? content = null)
    {
        Result = result;
        Errors = errors;
        Content = content;
    }
}
```

### Usage in Services

```csharp
// Success with content
return new GenericResponse(true, budget);

// Success without content
return new GenericResponse(true);

// Single error
return new GenericResponse(false,
    localizationService.GetLocalizedString("BudgetNotFound"));

// Multiple errors
return new GenericResponse(false,
    new List<string> { "Error 1", "Error 2" });

// Error with partial content
return new GenericResponse(false, "Validation failed", partialData);
```

### Usage in Controllers

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateHeaderRequest request)
{
    var response = await service.Create(request);

    if (response.Result)
        return Ok(response.Content);
    else
        return BadRequest(response);
}

[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, Budget budget)
{
    var response = await service.Update(id, budget);

    if (!response.Result)
        return Conflict(response);

    return Ok(response.Content);
}
```

**Key Points:**

- **`Result = true`** means operation succeeded
- **`Result = false`** means operation failed (check `Errors`)
- **`Content`** can contain returned entity or additional data
- **Multiple errors** supported for complex validation
- Controllers map to HTTP status codes (200/201 OK, 400 Bad Request, 409 Conflict)

---

## Primary Constructor Pattern

Modern C# 12 syntax for cleaner dependency injection.

### Old Pattern (Verbose)

```csharp
public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExerciseService _exerciseService;
    private readonly ILocalizationService _localizationService;

    public BudgetService(
        IUnitOfWork unitOfWork,
        IExerciseService exerciseService,
        ILocalizationService localizationService)
    {
        _unitOfWork = unitOfWork;
        _exerciseService = exerciseService;
        _localizationService = localizationService;
    }

    public async Task<Budget?> GetById(Guid id)
    {
        return await _unitOfWork.Budgets.Get(id);
    }
}
```

### New Pattern (Primary Constructor)

```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    IExerciseService exerciseService,
    ILocalizationService localizationService) : IBudgetService
{
    public async Task<Budget?> GetById(Guid id)
    {
        return await unitOfWork.Budgets.Get(id);  // Direct usage
    }

    public async Task<GenericResponse> Create(CreateHeaderRequest request)
    {
        var counter = await exerciseService.GetNextCounter(request.ExerciseId, "budget");
        // ... use parameters directly
    }
}
```

**Benefits:**

- **Less boilerplate** - No field declarations or assignments
- **Same functionality** - Parameters captured and available throughout class
- **Consistent style** - Used across all services, controllers, middleware

**Applied to:**

- Services (`BudgetService`, `SalesOrderService`, etc.)
- Controllers (`BudgetController`, `SalesOrderController`, etc.)
- Middleware (`ErrorHandlerMiddleware`, `CorrelationMiddleware`)

---

## Entity Configuration Pattern

Fluent API configuration for Entity Framework Core.

### Base Configuration

```csharp
public static class EntityBaseConfiguration
{
    public static void ConfigureBase<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : Entity
    {
        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnType("uuid");

        builder.Property(e => e.Disabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedOn)
            .HasColumnType("timestamp without time zone")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedOn)
            .HasColumnType("timestamp without time zone")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("NOW()");
    }
}
```

### Entity-Specific Configuration

```csharp
public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        // Apply base configuration
        builder.ConfigureBase();

        // Budget-specific configuration
        builder.Property(b => b.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.Amount)
            .HasColumnType("decimal(18,4)");

        builder.HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Status)
            .WithMany()
            .HasForeignKey(b => b.StatusId);

        builder.HasMany(b => b.Details)
            .WithOne()
            .HasForeignKey(d => d.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### Registration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Automatically applies all IEntityTypeConfiguration in assembly
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
```

**Key Points:**

- **`ConfigureBase()`** applies common properties (Id, timestamps, soft delete)
- **UUID keys** with `ValueGeneratedNever()` (client-generated)
- **Decimal precision** - (18,4) for amounts, (18,2) for prices
- **Timestamps** - PostgreSQL `timestamp without time zone`
- **Delete behavior** - `Restrict` for references, `Cascade` for owned entities
- **Auto-discovery** - `ApplyConfigurationsFromAssembly()` finds all configurations

---

## Pattern Combination Example

Complete example showing all patterns working together:

### 1. Entity (Domain)

```csharp
public class Budget : Entity
{
    public string Number { get; set; }
    public DateTime Date { get; set; }
    public Guid CustomerId { get; set; }
    public Guid StatusId { get; set; }
    public Customer Customer { get; set; }
}
```

### 2. Repository Interface (Application.Contracts)

```csharp
public interface IBudgetRepository : IRepository<Budget, Guid>
{
    IRepository<BudgetDetail, Guid> Details { get; }
}
```

### 3. Service Interface (Application.Contracts)

```csharp
public interface IBudgetService
{
    Task<GenericResponse> Create(CreateHeaderRequest request);
}
```

### 4. Repository Implementation (Infrastructure)

```csharp
public class BudgetRepository : Repository<Budget, Guid>, IBudgetRepository
{
    public IRepository<BudgetDetail, Guid> Details { get; }

    public BudgetRepository(ApplicationDbContext context) : base(context)
    {
        Details = new Repository<BudgetDetail, Guid>(context);
    }
}
```

### 5. Service Implementation (Application)

```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IBudgetService
{
    public async Task<GenericResponse> Create(CreateHeaderRequest request)
    {
        var lifecycle = unitOfWork.Lifecycles
            .Find(l => l.Name == StatusConstants.Lifecycles.Budget)
            .FirstOrDefault();

        var budget = new Budget { /* ... */ };
        await unitOfWork.Budgets.Add(budget);

        return new GenericResponse(true, budget);
    }
}
```

### 6. Controller (Api)

```csharp
[ApiController]
[Route("api/[controller]")]
public class BudgetController(IBudgetService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateHeaderRequest request)
    {
        var response = await service.Create(request);
        return response.Result ? Ok(response.Content) : BadRequest(response);
    }
}
```

---

## Summary

| Pattern                  | Purpose                    | Location              |
| ------------------------ | -------------------------- | --------------------- |
| **Repository**           | Data access abstraction    | Infrastructure        |
| **Unit of Work**         | Transaction management     | Infrastructure        |
| **Service Layer**        | Business logic             | Application           |
| **GenericResponse**      | Standardized results       | Application.Contracts |
| **Primary Constructor**  | Clean dependency injection | All layers            |
| **Entity Configuration** | EF Core mapping            | Infrastructure        |

**Key Principles:**

- ✅ Separate concerns across layers
- ✅ Use interfaces for testability
- ✅ Localize all user-facing messages
- ✅ Return `GenericResponse` for operations that can fail
- ✅ Use primary constructors for modern syntax
- ✅ Configure entities with Fluent API

For request flow using these patterns, see [Request Flow](request-flow.md).  
For specific implementation guides, see [How to Create Endpoints](how-to-create-endpoints.md).

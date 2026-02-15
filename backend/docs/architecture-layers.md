# Architecture Layers

This document explains the 6 projects in the Lilith Backend solution and their responsibilities within the Clean Architecture pattern.

## Layer Dependency Rules

```
        API (Composition Root)
          ↓         ↓
   Application  Infrastructure
          ↓         ↓
    Application.Contracts
          ↓
        Domain (Pure Core)
```

**Golden Rule:** Dependencies only flow inward. Inner layers have no knowledge of outer layers.

---

## 1. Domain Layer

**Project:** `src/Domain/`  
**Dependencies:** None (purest layer)  
**Namespace:** `Domain.Entities.*`

### Responsibilities

- Define core business entities
- Contain domain logic (if any)
- Remain framework-agnostic

### Key Contents

```
Domain/
├── Entities/
│   ├── Sales/          # Customer, Budget, SalesOrderHeader, SalesInvoice
│   ├── Purchase/       # Supplier, PurchaseOrder, Receipt, PurchaseInvoice
│   ├── Production/     # WorkMaster, WorkOrder, ProductionPart, Workcenter
│   ├── Warehouse/      # Stock, Location, StockMovement, Warehouse
│   └── Shared/         # Reference, Lifecycle, Status, Exercise, User
```

### Base Entity

All entities inherit from:

```csharp
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public bool Disabled { get; set; } = false;  // Soft delete
}
```

### Design Principles

- **Pure C#** - No references to EF Core, ASP.NET, or external frameworks
- **Anemic model** - Minimal logic, mostly data containers
- **Soft delete** - `Disabled` property instead of physical deletion

---

## 2. Application.Contracts Layer

**Project:** `src/Application.Contracts/`  
**Dependencies:** Domain  
**Namespace:** `Application.Contracts` (flat structure)

### Responsibilities

- Define service interfaces
- Define repository interfaces
- Define DTOs and request/response models
- Define constants for database values

### Key Contents

```
Application.Contracts/
├── Contracts/              # All interfaces (IServices, IRepositories)
├── Constants/              # StatusConstants (lifecycle/status names)
├── Configuration/          # JWT settings, file management
├── Persistance/            # GenericResponse, IUnitOfWork
└── Services/               # DTOs (CreateHeaderRequest, etc.)
```

### Important Interfaces

**Generic Repository:**

```csharp
public interface IRepository<TEntity, TId> where TEntity : class
{
    Task<TEntity?> Get(TId id);
    Task<IEnumerable<TEntity>> GetAll();
    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    Task Add(TEntity entity);
    Task Update(TEntity entity);
    Task Remove(TEntity entity);
}
```

**Unit of Work:**

```csharp
public interface IUnitOfWork
{
    IBudgetRepository Budgets { get; }
    ISalesOrderHeaderRepository SalesOrderHeaders { get; }
    IWorkOrderRepository WorkOrders { get; }
    // ... all repositories

    Task<int> CompleteAsync();
}
```

**Service Example:**

```csharp
public interface IBudgetService
{
    Task<Budget?> GetById(Guid id);
    IEnumerable<Budget> GetBetweenDates(DateTime startDate, DateTime endDate);
    Task<GenericResponse> Create(CreateHeaderRequest request);
    Task<GenericResponse> Update(Budget budget);
}
```

### Design Principles

- **Flat namespace** - All interfaces in `Application.Contracts` namespace
- **Abstraction only** - No implementations
- **Constants over magic strings** - `StatusConstants.Lifecycles.Budget`

---

## 3. Application Layer

**Project:** `src/Application/`  
**Dependencies:** Domain, Application.Contracts  
**Namespace:** `Application.Services.*`

### Responsibilities

- Implement business logic
- Implement service interfaces
- Orchestrate repository operations via `IUnitOfWork`
- Handle workflow and lifecycle management

### Key Contents

```
Application/
└── Services/
    ├── AuthenticationService.cs
    ├── BudgetService.cs
    ├── SalesOrderService.cs
    ├── WorkOrderService.cs
    ├── ExerciseService.cs
    ├── LocalizationService.cs
    ├── BudgetBackgroundService.cs   # Background job
    └── ...
```

### Service Pattern

```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    IExerciseService exerciseService,
    ILocalizationService localizationService) : IBudgetService
{
    public async Task<GenericResponse> Create(CreateHeaderRequest request)
    {
        // 1. Validation
        var customer = await unitOfWork.Customers.Get(request.CustomerId);
        if (customer == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerNotFound"));

        // 2. Generate document number
        var counter = await exerciseService.GetNextCounter(request.ExerciseId, "budget");

        // 3. Create entity
        var budget = new Budget { /* ... */ };

        // 4. Set lifecycle status
        var lifecycle = unitOfWork.Lifecycles.Find(l =>
            l.Name == StatusConstants.Lifecycles.Budget).FirstOrDefault();
        budget.StatusId = lifecycle.InitialStatusId;

        // 5. Persist
        await unitOfWork.Budgets.Add(budget);
        return new GenericResponse(true, budget);
    }
}
```

### Design Principles

- **Primary constructors** - Modern C# 12 DI syntax
- **Always localize** - Use `ILocalizationService` for all messages
- **GenericResponse** - Standardized return type for write operations
- **No ASP.NET types** - ⚠️ Current violation: some services use `IFormFile`

---

## 4. Infrastructure Layer

**Project:** `src/Infrastructure/`  
**Dependencies:** Domain, Application.Contracts, EF Core, Npgsql  
**Namespace:** `Infrastructure.*`

### Responsibilities

- Implement repositories
- Configure Entity Framework Core
- Manage database migrations
- Implement `IUnitOfWork`

### Key Contents

```
Infrastructure/
├── Persistance/
│   ├── ApplicationDbContext.cs          # EF Core DbContext
│   ├── UnitOfWork.cs                    # IUnitOfWork implementation
│   ├── EntityConfiguration/             # Fluent API configurations
│   └── Repositories/                    # Repository implementations
├── Migrations/                          # EF Core migrations
└── ApplicationDbContextFactory.cs       # Design-time factory
```

### ApplicationDbContext

```csharp
public class ApplicationDbContext : DbContext
{
    // Domain entities
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<SalesOrderHeader> SalesOrderHeaders { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }

    // Database views (keyless)
    public DbSet<ConsolidatedExpense> ConsolidatedExpenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### Repository Implementation

```csharp
public class BudgetRepository : Repository<Budget, Guid>, IBudgetRepository
{
    public IRepository<BudgetDetail, Guid> Details { get; }

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

### Entity Configuration Pattern

```csharp
public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ConfigureBase();  // Common properties

        builder.Property(b => b.Amount).HasColumnType("decimal(18,4)");
        builder.HasOne(b => b.Customer).WithMany().HasForeignKey(b => b.CustomerId);
    }
}
```

### Design Principles

- **PostgreSQL-specific** - Decimal precision, timestamp types, UUID keys
- **Eager loading** - Use `Include()` for related entities
- **AsNoTracking** - Default for read operations
- **Soft delete** - Query filter on `Disabled` property

---

## 5. Api Layer

**Project:** `src/Api/`  
**Dependencies:** All layers (composition root)  
**Namespace:** `Api.*`

### Responsibilities

- Define HTTP endpoints (controllers)
- Configure dependency injection
- Register middleware pipeline
- Configure authentication/authorization
- Serve Swagger/OpenAPI documentation

### Key Contents

```
Api/
├── Controllers/              # REST API endpoints
├── Middlewares/
│   ├── CorrelationMiddleware.cs
│   ├── CultureMiddleware.cs
│   └── ErrorHandlerMiddleware.cs
├── Setup/
│   ├── ApplicationServicesSetup.cs
│   ├── CorsSetup.cs
│   ├── JwtSetup.cs
│   └── LocalizationSetup.cs
├── Resources/
│   └── LocalizationService/  # ca.json, es.json, en.json
├── Program.cs                # Application startup
└── appsettings.json
```

### Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class BudgetController(IBudgetService service, ILocalizationService localization) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateHeaderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await service.Create(request);

        if (response.Result)
            return Ok(response.Content);
        else
            return BadRequest(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var budget = await service.GetById(id);

        if (budget == null)
            return NotFound(new GenericResponse(false,
                localization.GetLocalizedString("BudgetNotFound")));

        return Ok(budget);
    }
}
```

### Middleware Pipeline

```csharp
app.UseSerilogRequestLogging();           // 1. Log requests
app.UseMiddleware<CorrelationMiddleware>(); // 2. Correlation IDs
app.UseAuthentication();                  // 3. JWT validation
app.UseLocalizationSetup();               // 4. Culture detection
app.UseAuthorization();                   // 5. Authorization
app.UseMiddleware<ErrorHandlerMiddleware>(); // 6. Error handling
```

### Design Principles

- **Thin controllers** - Delegate to service layer
- **Validate ModelState** - Check before calling services
- **HTTP status codes** - 200/201 (success), 400 (validation), 404 (not found), 409 (conflict)
- **Primary constructors** - Inject dependencies cleanly

---

## 6. Verifactu Layer

**Project:** `src/Verifactu/`  
**Dependencies:** System.ServiceModel (SOAP)  
**Namespace:** `Verifactu.*`

### Responsibilities

- Spanish tax authority integration (AEAT)
- SOAP web service client
- Invoice registration and validation
- QR code generation for tax compliance

### Key Contents

```
Verifactu/
├── VerifactuInvoiceService.cs      # Main SOAP client
├── Contracts/                       # Request/response models
├── Factories/                       # Request builders
└── Mappers/                         # Domain to SOAP DTOs
```

### Service Interface

```csharp
public interface IVerifactuInvoiceService
{
    Task<VerifactuResponse> RegisterInvoice(RegisterInvoiceRequest request);
    Task<FindInvoicesResponse> FindInvoices(FindInvoicesRequest request);
    Task<VerifactuResponse> CancelInvoice(CancelInvoiceRequest request);
}
```

### Design Principles

- **X.509 certificate** - Client authentication
- **SOAP protocol** - Legacy integration
- **Hash chaining** - Invoice integrity verification
- **Separate project** - External concern isolated from core business

For detailed Verifactu integration, see [External Integrations](external-integrations.md).

---

## Layer Interaction Summary

```
User Request (HTTP)
      ↓
┌─────────────────────────────────┐
│  Controller (Api)               │
│  • Validates ModelState         │
│  • Calls Service Interface      │
│  • Maps to HTTP Status          │
└─────────────────────────────────┘
      ↓ (only service interface)
┌─────────────────────────────────┐
│  Service (Application)          │
│  • Business Validation          │
│  • Workflow Logic               │
│  • Orchestrates Repositories    │
└─────────────────────────────────┘
      ↓ (uses IUnitOfWork)
┌─────────────────────────────────┐
│  Repository (Infrastructure)    │
│  • Data Access                  │
│  • EF Core Operations           │
└─────────────────────────────────┘
      ↓
  EF Core → PostgreSQL
```

**Key Points:**

1. **Controllers** handle HTTP, validate requests, return responses (NO business logic, NO data access)
2. **Services** implement business logic, manage workflows, orchestrate repositories
3. **Repositories** abstract data access through IUnitOfWork interface
4. **UnitOfWork** ensures transaction boundaries
5. **Domain entities** flow through all layers unchanged

---

## Architectural Benefits

✅ **Testability** - Each layer can be tested independently (mocking interfaces)  
✅ **Maintainability** - Clear separation of concerns  
✅ **Flexibility** - Swap implementations without affecting business logic  
✅ **Scalability** - Add features without breaking existing code

## Current Violations

⚠️ **Application layer references ASP.NET Core** - `IFormFile`, `IHostEnvironment` used in services

See [Architectural Debt Assessment](architectural-debt-assessment.md) for improvement plan.

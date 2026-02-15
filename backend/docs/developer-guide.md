# Developer Guide

This guide provides essential information for developers working on the Lilith Backend solution.

## Prerequisites

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **PostgreSQL 16+** - [Download](https://www.postgresql.org/download/)
- **IDE:** Visual Studio 2022, VS Code, or JetBrains Rider
- **Docker** (optional) - For containerized PostgreSQL

## Initial Setup

### 1. Clone Repository

```bash
git clone <repository-url>
cd lilith-backend
```

### 2. Configure Database

**Option A: Local PostgreSQL**

Update connection string in `src/Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=lilith;Username=postgres;Password=yourpassword"
  }
}
```

**Option B: Docker PostgreSQL**

```bash
docker run -d \
  --name lilith-postgres \
  -e POSTGRES_DB=lilith \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Apply Database Migrations

```bash
dotnet ef database update --project src/Infrastructure/
```

### 5. Run the Application

**Using dotnet CLI:**

```bash
dotnet run --project src/Api/
```

**Using VS Code tasks:**

- Press `Ctrl+Shift+P` → "Run Task" → "watch"

**Using Visual Studio:**

- Set `Api` as startup project
- Press F5

### 6. Access Swagger UI

Navigate to: `https://localhost:5001/swagger`

---

## Common Tasks

### Add New Entity

Follow this workflow across layers:

#### 1. Define Entity (Domain)

```csharp
// src/Domain/Entities/YourModule/YourEntity.cs
namespace Domain.Entities.YourModule;

public class YourEntity : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? StatusId { get; set; }
    public Status? Status { get; set; }
}
```

#### 2. Create Repository Interface (Application.Contracts)

```csharp
// src/Application.Contracts/Contracts/IYourEntityRepository.cs
namespace Application.Contracts;

public interface IYourEntityRepository : IRepository<YourEntity, Guid>
{
    // Add specialized methods if needed
}
```

#### 3. Add to IUnitOfWork (Application.Contracts)

```csharp
// Update src/Application.Contracts/Persistance/IUnitOfWork.cs
public interface IUnitOfWork
{
    // ... existing repositories
    IYourEntityRepository YourEntities { get; }
}
```

#### 4. Implement Repository (Infrastructure)

```csharp
// src/Infrastructure/Persistance/Repositories/YourEntityRepository.cs
namespace Infrastructure.Persistance.Repositories;

public class YourEntityRepository : Repository<YourEntity, Guid>, IYourEntityRepository
{
    public YourEntityRepository(ApplicationDbContext context) : base(context)
    {
    }
}
```

#### 5. Configure Entity (Infrastructure)

```csharp
// src/Infrastructure/Persistance/EntityConfiguration/YourEntityConfiguration.cs
public class YourEntityConfiguration : IEntityTypeConfiguration<YourEntity>
{
    public void Configure(EntityTypeBuilder<YourEntity> builder)
    {
        builder.ConfigureBase();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(e => e.Status)
            .WithMany()
            .HasForeignKey(e => e.StatusId);
    }
}
```

#### 6. Update DbContext (Infrastructure)

```csharp
// src/Infrastructure/Persistance/ApplicationDbContext.cs
public DbSet<YourEntity> YourEntities { get; set; }
```

#### 7. Update UnitOfWork (Infrastructure)

```csharp
// src/Infrastructure/Persistance/UnitOfWork.cs
public IYourEntityRepository YourEntities { get; }

public UnitOfWork(ApplicationDbContext context)
{
    _context = context;
    YourEntities = new YourEntityRepository(context);
    // ...
}
```

#### 8. Create Service Interface (Application.Contracts)

```csharp
// src/Application.Contracts/Services/IYourEntityService.cs
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

#### 9. Implement Service (Application)

```csharp
// src/Application/Services/YourEntityService.cs
namespace Application.Services;

public class YourEntityService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IYourEntityService
{
    public async Task<GenericResponse> Create(YourEntity entity)
    {
        // Validation
        var exists = unitOfWork.YourEntities.Find(e => e.Id == entity.Id).Any();
        if (exists)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityAlreadyExists"));

        // Persist
        await unitOfWork.YourEntities.Add(entity);
        return new GenericResponse(true, entity);
    }

    public async Task<YourEntity?> GetById(Guid id)
    {
        return await unitOfWork.YourEntities.Get(id);
    }
}
```

#### 10. Register Service (Api)

```csharp
// src/Api/Setup/ApplicationServicesSetup.cs
services.AddScoped<IYourEntityService, YourEntityService>();
```

#### 11. Create Controller (Api)

```csharp
// src/Api/Controllers/YourEntityController.cs
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
}
```

#### 12. Create Migration

```bash
dotnet ef migrations add AddYourEntity --project src/Infrastructure/
dotnet ef database update --project src/Infrastructure/
```

---

### Create Migration

```bash
# Create new migration
dotnet ef migrations add MigrationName --project src/Infrastructure/

# Apply to database
dotnet ef database update --project src/Infrastructure/

# Rollback to previous migration
dotnet ef database update PreviousMigrationName --project src/Infrastructure/

# View migration SQL (without applying)
dotnet ef migrations script --project src/Infrastructure/
```

**Best practices:**

- Review generated migration before applying
- Use descriptive migration names (e.g., `AddWorkOrderPhases`, `AddCustomerTypeColumn`)
- Test against dev database first
- Include rollback logic in `Down()` method

---

### Add Localization Key

1. **Add to ca.json:**

```json
{
  "YourNewKey": "El teu missatge en català amb paràmetre {0}"
}
```

2. **Add to es.json:**

```json
{
  "YourNewKey": "Tu mensaje en español con parámetro {0}"
}
```

3. **Add to en.json:**

```json
{
  "YourNewKey": "Your message in English with parameter {0}"
}
```

4. **Use in service:**

```csharp
return new GenericResponse(false,
    localizationService.GetLocalizedString("YourNewKey", someValue));
```

---

## Critical Conventions

### Service Layer Rules

✅ **DO:**

- Inject `IUnitOfWork` for all data access
- Inject `ILocalizationService` for all user-facing messages
- Use `StatusConstants` for lifecycle/status names
- Return `GenericResponse` for write operations
- Return entities directly for read operations
- Validate business rules before persisting

❌ **DON'T:**

- Use hardcoded error strings
- Mix languages in error messages
- Reference ASP.NET Core types (`IFormFile`, `IHostEnvironment`)
- Access database directly (bypass repositories)
- Return `bool` for operations that need error details

### Controller Layer Rules

✅ **DO:**

- Validate `ModelState` before processing
- Call service layer for all business logic
- Map `GenericResponse` to appropriate HTTP status codes
- Use `[ApiController]` and `[Route]` attributes
- Use primary constructors for DI

❌ **DON'T:**

- Inject `IUnitOfWork` directly (use service layer)
- Perform business logic in controllers
- Return inconsistent response formats

### Repository Layer Rules

✅ **DO:**

- Use `AsNoTracking()` for read-only queries
- Include related entities explicitly with `Include()`
- Detach entities after save operations
- Provide specialized repositories for complex queries

❌ **DON'T:**

- Include unnecessary navigation properties (performance)
- Load entire object graphs
- Forget to call `SaveChangesAsync()`

### Naming Conventions

| Type                          | Convention              | Example               |
| ----------------------------- | ----------------------- | --------------------- |
| **Service Interface**         | `I{Entity}Service`      | `IBudgetService`      |
| **Service Implementation**    | `{Entity}Service`       | `BudgetService`       |
| **Repository Interface**      | `I{Entity}Repository`   | `IBudgetRepository`   |
| **Repository Implementation** | `{Entity}Repository`    | `BudgetRepository`    |
| **Controller**                | `{Entity}Controller`    | `BudgetController`    |
| **Entity**                    | PascalCase              | `SalesOrderHeader`    |
| **Entity Configuration**      | `{Entity}Configuration` | `BudgetConfiguration` |

---

## Code Style

### Primary Constructor Pattern (C# 12)

**Use this pattern consistently:**

```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IBudgetService
{
    // Direct usage of parameters
}
```

**Don't use old pattern:**

```csharp
// ❌ Old verbose pattern
public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _unitOfWork;

    public BudgetService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

### Async/Await

✅ **All I/O operations must be async:**

```csharp
public async Task<Budget?> GetById(Guid id)
{
    return await unitOfWork.Budgets.Get(id);
}
```

❌ **Don't block async calls:**

```csharp
// ❌ Don't do this
var budget = unitOfWork.Budgets.Get(id).Result;
```

### Nullable Reference Types

✅ **Use appropriately:**

```csharp
public string Name { get; set; } = string.Empty;  // Non-nullable
public string? Description { get; set; }          // Nullable
public Customer? Customer { get; set; }           // Nullable navigation
```

### Property Initialization

✅ **Initialize collections:**

```csharp
public class Budget : Entity
{
    public ICollection<BudgetDetail> Details { get; set; } = new List<BudgetDetail>();
}
```

---

## Development Workflow

### Build Solution

```bash
dotnet build
```

### Run Tests

```bash
# ⚠️ No tests exist yet (critical debt)
dotnet test
```

### Run with Hot Reload

```bash
dotnet watch run --project src/Api/
```

### Clean Build

```bash
dotnet clean
dotnet build
```

---

## Debugging

### Visual Studio Code

Use provided `.vscode/launch.json`:

```json
{
  "name": ".NET Core Launch (web)",
  "type": "coreclr",
  "request": "launch",
  "preLaunchTask": "build",
  "program": "${workspaceFolder}/src/Api/bin/Debug/net10.0/Api.dll",
  "cwd": "${workspaceFolder}/src/Api",
  "env": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

Press F5 to start debugging.

### Logging

**View logs:**

- Console output during development
- File logs: `src/Api/logs/log-{Date}.txt`

**Log levels:**

- Information (default)
- Warning
- Error
- Debug (verbose)

**Configure in appsettings.json:**

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

---

## Testing

### Manual Testing with Swagger

1. Run the application
2. Navigate to `https://localhost:5001/swagger`
3. Test endpoints interactively
4. Use "Authorize" button for JWT authentication

### PowerShell Testing

```powershell
# Test GET endpoint
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/budget/123e4567-..." -Method GET

# Test POST endpoint
$body = @{
    id = [guid]::NewGuid()
    customerId = "123e4567-..."
    exerciseId = "123e4567-..."
    date = [DateTime]::UtcNow
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/budget" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

### Testing with Culture

```bash
# Test Catalan (default)
curl https://localhost:5001/api/budget/invalid-id

# Test Spanish
curl https://localhost:5001/api/budget/invalid-id?culture=es

# Test English
curl https://localhost:5001/api/budget/invalid-id?culture=en
```

---

## Docker

### Build Image

```bash
docker build -t lilith-backend .
```

### Run Container

```bash
docker run -p 8080:80 \
  -e ConnectionStrings__Default="Host=host.docker.internal;Database=lilith;Username=postgres;Password=postgres" \
  lilith-backend
```

### Docker Compose (Development)

```yaml
version: "3.8"
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: lilith
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"

  api:
    build: .
    ports:
      - "8080:80"
    environment:
      ConnectionStrings__Default: "Host=postgres;Database=lilith;Username=postgres;Password=postgres"
    depends_on:
      - postgres
```

---

## Troubleshooting

### Migration Errors

**Error: "A connection was not established"**

- Check PostgreSQL is running
- Verify connection string in `appsettings.Development.json`

**Error: "No migrations configuration type was found"**

```bash
# Ensure you're specifying the Infrastructure project
dotnet ef database update --project src/Infrastructure/
```

### Build Errors

**Error: "The type or namespace name 'X' could not be found"**

- Run `dotnet restore`
- Check project references
- Ensure correct `using` statements

### Runtime Errors

**Error: "Unable to resolve service for type 'IYourService'"**

- Register service in `ApplicationServicesSetup.cs`
- Ensure interface and implementation are correct

---

## Performance Tips

### Database Queries

✅ **Use eager loading:**

```csharp
var budgets = await context.Budgets
    .Include(b => b.Customer)
    .Include(b => b.Details)
    .ToListAsync();
```

❌ **Avoid N+1 queries:**

```csharp
// ❌ Triggers separate query for each budget's customer
var budgets = await context.Budgets.ToListAsync();
foreach (var budget in budgets)
{
    var customerName = budget.Customer.Name;
}
```

### Use AsNoTracking

For read-only operations:

```csharp
return await dbSet
    .AsNoTracking()  // ← Improves performance
    .FirstOrDefaultAsync(e => e.Id == id);
```

---

## Environment Variables

| Variable                     | Purpose              | Default       |
| ---------------------------- | -------------------- | ------------- |
| `ASPNETCORE_ENVIRONMENT`     | Environment name     | `Development` |
| `ConnectionStrings__Default` | Database connection  | (required)    |
| `JwtConfig__Secret`          | JWT signing key      | (required)    |
| `FileManagement__LimitSize`  | Max upload size (MB) | 50            |

---

## Next Steps

- Read [How to Create Endpoints](how-to-create-endpoints.md) for detailed endpoint creation
- Review [Architectural Patterns](architectural-patterns.md) for implementation patterns
- Check [Architectural Debt Assessment](architectural-debt-assessment.md) for known issues
- Understand [Localization](localization.md) for multilanguage support

---

## Getting Help

- **Documentation:** Review all docs in `/docs` folder
- **Code examples:** Look at existing controllers/services for patterns
- **Architecture questions:** Review [Architecture Layers](architecture-layers.md)
- **Domain questions:** Review [Domain Model](domain-model.md)

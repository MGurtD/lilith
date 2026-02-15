# Request Flow

This document illustrates how HTTP requests flow through the architecture layers, from the client to the database and back.

## High-Level Flow

```
┌─────────┐
│ Client  │
│ (HTTP)  │
└────┬────┘
     │ 1. POST /api/budget
     ↓
┌────────────────────────────────────────────────────────┐
│                   API LAYER                            │
│  ┌──────────────────────────────────────────────┐     │
│  │ Middleware Pipeline                          │     │
│  │ • Logging                                    │     │
│  │ • Correlation ID                             │     │
│  │ • Authentication (JWT)                       │     │
│  │ • Culture Detection                          │     │
│  │ • Error Handling                             │     │
│  └──────────────────────────────────────────────┘     │
│                    ↓                                   │
│  ┌──────────────────────────────────────────────┐     │
│  │ Controller                                   │     │
│  │ • Validate ModelState                        │     │
│  │ • Call Service Layer                         │     │
│  │ • Map to HTTP Status                         │     │
│  └──────────────────────────────────────────────┘     │
└────────────────────┬───────────────────────────────────┘
                     │ 2. service.Create(request)
                     ↓
┌────────────────────────────────────────────────────────┐
│              APPLICATION LAYER                         │
│  ┌──────────────────────────────────────────────┐     │
│  │ Service                                      │     │
│  │ • Business Validation                        │     │
│  │ • Workflow Logic                             │     │
│  │ • Localize Messages                          │     │
│  │ • Orchestrate Repositories                   │     │
│  └──────────────────────────────────────────────┘     │
└────────────────────┬───────────────────────────────────┘
                     │ 3. unitOfWork.Budgets.Add(entity)
                     ↓
┌────────────────────────────────────────────────────────┐
│            INFRASTRUCTURE LAYER                        │
│  ┌──────────────────────────────────────────────┐     │
│  │ UnitOfWork                                   │     │
│  │ • Manage Transaction                         │     │
│  │ • Provide Repository Instances               │     │
│  └──────────────────────────────────────────────┘     │
│                    ↓                                   │
│  ┌──────────────────────────────────────────────┐     │
│  │ Repository                                   │     │
│  │ • Execute EF Core Query                      │     │
│  │ • Include Related Entities                   │     │
│  │ • Track Changes                              │     │
│  └──────────────────────────────────────────────┘     │
│                    ↓                                   │
│  ┌──────────────────────────────────────────────┐     │
│  │ Entity Framework Core                        │     │
│  │ • Generate SQL                               │     │
│  │ • Execute Query                              │     │
│  └──────────────────────────────────────────────┘     │
└────────────────────┬───────────────────────────────────┘
                     │ 4. INSERT INTO budgets ...
                     ↓
               ┌──────────┐
               │PostgreSQL│
               │ Database │
               └──────────┘
```

---

## Detailed Request Flow: Create Budget

### Step-by-Step Walkthrough

**Request:** `POST /api/budget`  
**Body:** `{ "id": "...", "customerId": "...", "exerciseId": "...", "date": "2025-01-15" }`

#### 1. HTTP Request Arrives

```
Client → Kestrel Web Server → Middleware Pipeline
```

**Middleware execution order:**

1. **SerilogRequestLogging** - Logs incoming request
2. **CorrelationMiddleware** - Adds/extracts correlation ID
3. **Authentication** - Validates JWT token, sets `User` principal
4. **CultureMiddleware** - Detects language (query param → JWT → Accept-Language)
5. **Authorization** - Checks permissions (currently minimal)
6. **ErrorHandlerMiddleware** - Wraps request for global error handling

#### 2. Controller Receives Request

```csharp
[ApiController]
[Route("api/[controller]")]
public class BudgetController(IBudgetService service, ILocalizationService localization) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateHeaderRequest request)
    {
        // 2.1 Validate request model
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 2.2 Call service layer
        var response = await service.Create(request);

        // 2.3 Map result to HTTP status
        if (response.Result)
            return Ok(response.Content);
        else
            return BadRequest(response);
    }
}
```

**Controller responsibilities:**

- ✅ Validate `ModelState`
- ✅ Call service method
- ✅ Map `GenericResponse` to HTTP status code
- ❌ No business logic
- ❌ No direct database access

#### 3. Service Executes Business Logic

```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    IExerciseService exerciseService,
    ILocalizationService localizationService) : IBudgetService
{
    public async Task<GenericResponse> Create(CreateHeaderRequest request)
    {
        // 3.1 Validate customer exists
        var customer = await unitOfWork.Customers.Get(request.CustomerId);
        if (customer == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerNotFound"));

        // 3.2 Validate exercise exists
        var exercise = await unitOfWork.Exercises.Get(request.ExerciseId);
        if (exercise == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("ExerciseNotFound"));

        // 3.3 Generate document number
        var counterResponse = await exerciseService.GetNextCounter(
            request.ExerciseId, "budget");
        if (!counterResponse.Result)
            return counterResponse;

        // 3.4 Get initial lifecycle status
        var lifecycle = unitOfWork.Lifecycles
            .Find(l => l.Name == StatusConstants.Lifecycles.Budget)
            .FirstOrDefault();

        if (lifecycle == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("LifecycleNotFound",
                    StatusConstants.Lifecycles.Budget));

        // 3.5 Create entity
        var budget = new Budget
        {
            Id = request.Id,
            Number = counterResponse.Content.ToString()!,
            Date = request.Date,
            CustomerId = request.CustomerId,
            ExerciseId = request.ExerciseId,
            StatusId = lifecycle.InitialStatusId,
            Amount = 0
        };

        // 3.6 Persist via repository
        await unitOfWork.Budgets.Add(budget);

        // 3.7 Return success
        return new GenericResponse(true, budget);
    }
}
```

**Service responsibilities:**

- ✅ Validate business rules
- ✅ Generate document numbers
- ✅ Set lifecycle statuses
- ✅ Orchestrate multiple repositories
- ✅ Localize error messages
- ✅ Return `GenericResponse`

#### 4. Repository Persists Entity

```csharp
public class BudgetRepository : Repository<Budget, Guid>, IBudgetRepository
{
    public BudgetRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Inherited from base Repository<T>
    public async Task Add(Budget entity)
    {
        await context.Set<Budget>().AddAsync(entity);
        await context.SaveChangesAsync();
        context.Entry(entity).State = EntityState.Detached;
    }
}
```

**Repository responsibilities:**

- ✅ Execute EF Core operations
- ✅ Save changes to database
- ✅ Detach entities (prevent tracking issues)
- ❌ No business logic
- ❌ No validation

#### 5. Entity Framework Generates SQL

```sql
INSERT INTO budgets (
    id, number, date, customer_id, exercise_id, status_id,
    amount, created_on, updated_on, disabled
)
VALUES (
    '123e4567-e89b-12d3-a456-426614174000',
    'PRES2025-0042',
    '2025-01-15',
    '...',
    '...',
    '...',
    0,
    NOW(),
    NOW(),
    false
);
```

#### 6. PostgreSQL Executes Query

Database performs:

1. Validates foreign keys (customer_id, exercise_id, status_id)
2. Inserts row
3. Returns success/failure
4. Commits transaction

#### 7. Response Flows Back

```
PostgreSQL → EF Core → Repository → Service → Controller → Middleware → Client
```

**Response stages:**

1. **EF Core** - Returns saved entity with generated timestamps
2. **Repository** - Returns entity to service
3. **Service** - Wraps entity in `GenericResponse(true, budget)`
4. **Controller** - Maps to `200 OK` with entity in body
5. **Middleware** - Adds correlation ID header, logs response
6. **Client** - Receives `{ "id": "...", "number": "PRES2025-0042", ... }`

---

## Error Flow Example

### Scenario: Customer Not Found

#### Request Flow

```
Client → Controller → Service (validation fails) → Controller → Client
```

#### Service Error

```csharp
var customer = await unitOfWork.Customers.Get(request.CustomerId);
if (customer == null)
    return new GenericResponse(false,
        localizationService.GetLocalizedString("CustomerNotFound"));
```

#### Controller Response

```csharp
var response = await service.Create(request);

if (response.Result)
    return Ok(response.Content);
else
    return BadRequest(response);  // ← Returns 400 Bad Request
```

#### Client Receives

```json
{
  "result": false,
  "errors": ["Client no trobat"], // Localized in Catalan
  "content": null
}
```

---

## Exception Flow

### Unhandled Exception in Service

```
Service throws exception → ErrorHandlerMiddleware catches
```

#### ErrorHandlerMiddleware

```csharp
try
{
    await next(context);
}
catch (Exception ex)
{
    logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

    var response = new ProblemDetails
    {
        Status = 500,
        Title = localizationService.GetLocalizedString("InternalServerError"),
        Detail = ex.Message,
        Instance = context.Request.Path
    };

    context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(response);
}
```

#### Client Receives

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Error intern del servidor",
  "status": 500,
  "detail": "NullReferenceException: Object reference not set...",
  "instance": "/api/budget"
}
```

---

## Read Operation Flow: Get Budget by ID

### Simplified Flow

```
GET /api/budget/{id}
     ↓
Controller.GetById(id)
     ↓
Service.GetById(id)
     ↓
Repository.Get(id)
     ↓
EF Core: SELECT * FROM budgets WHERE id = ... (with INCLUDES)
     ↓
PostgreSQL
     ↓
Returns Budget entity (with navigation properties)
     ↓
200 OK { budget object }
```

### Key Differences from Write Operations

- ✅ No `GenericResponse` wrapper (returns entity directly)
- ✅ Uses `AsNoTracking()` for performance
- ✅ Eager loads navigation properties (`Include()`)
- ✅ Returns `404 Not Found` if entity doesn't exist

### Controller Implementation

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var budget = await service.GetById(id);

    if (budget == null)
        return NotFound(new GenericResponse(false,
            localizationService.GetLocalizedString("BudgetNotFound", id)));

    return Ok(budget);
}
```

---

## Performance Considerations

### Database Round-Trips

**Bad: N+1 Query Problem**

```csharp
// Repository without Include()
var budgets = await context.Budgets.ToListAsync();

// Each budget triggers separate query for customer
foreach (var budget in budgets)
{
    var customerName = budget.Customer.Name;  // ← Additional SELECT
}
```

**Good: Eager Loading**

```csharp
var budgets = await context.Budgets
    .Include(b => b.Customer)
    .Include(b => b.Details)
        .ThenInclude(d => d.Reference)
    .ToListAsync();  // ← Single query with JOINs
```

### Tracking vs AsNoTracking

**Use Tracking (default):**

- When you need to update entities
- When EF Core should detect changes automatically

**Use AsNoTracking:**

- For read-only queries
- For reporting endpoints
- Improves performance (no change detection overhead)

```csharp
public override async Task<Budget?> Get(Guid id)
{
    return await dbSet
        .Include(b => b.Customer)
        .AsNoTracking()  // ← No change tracking
        .FirstOrDefaultAsync(b => b.Id == id);
}
```

---

## Summary

### Request Flow Layers

| Layer          | Responsibility            | Input             | Output          |
| -------------- | ------------------------- | ----------------- | --------------- |
| **Controller** | HTTP handling, validation | HTTP Request      | HTTP Response   |
| **Service**    | Business logic, workflows | DTOs, entities    | GenericResponse |
| **Repository** | Data access               | Entity operations | Entities        |
| **EF Core**    | ORM, SQL generation       | LINQ queries      | Entities        |
| **PostgreSQL** | Data persistence          | SQL statements    | Result sets     |

### Key Takeaways

1. **Separation of concerns** - Each layer has specific responsibilities
2. **Dependency flow** - Always inward (Controller → Service → Repository)
3. **Error handling** - Multiple layers (service validation, middleware exceptions)
4. **Localization** - Integrated at service layer
5. **Transactions** - Managed by `UnitOfWork` / `DbContext`
6. **Performance** - Use `AsNoTracking()` and eager loading appropriately

### Related Documentation

- [Architecture Layers](architecture-layers.md) - Detailed layer responsibilities
- [Architectural Patterns](architectural-patterns.md) - Pattern implementations
- [Domain Model](domain-model.md) - Entity relationships
- [How to Create Endpoints](how-to-create-endpoints.md) - Step-by-step guide

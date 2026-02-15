# Lilith Backend - Copilot Instructions

## Solution Overview

Lilith Backend is a comprehensive manufacturing ERP system built with .NET 10 using Clean Architecture principles. The solution manages the complete manufacturing lifecycle including purchases, sales, production planning, inventory, and financial operations for a manufacturing company.

**Architecture Grade: B+** - Solid foundation with clear layer separation, modern patterns, and comprehensive multilanguage support.

## 📚 Comprehensive Documentation System

**Before coding, review the comprehensive documentation in `/docs`:**

**Load only the docs relevant to your task.**

- **[README.md](../README.md)** - Quick start, architecture overview, and documentation index
- **[Architecture Layers](../docs/architecture-layers.md)** - Deep dive into all 6 projects and responsibilities
- **[Domain Model](../docs/domain-model.md)** - Business entities across 4 areas (Sales, Purchase, Production, Warehouse)
- **[Architectural Patterns](../docs/architectural-patterns.md)** - Repository, Service, GenericResponse patterns with examples
- **[Request Flow](../docs/request-flow.md)** - How HTTP requests flow through layers (with ASCII diagrams)
- **[Localization](../docs/localization.md)** - Multilanguage support system (ca/es/en)
- **[Developer Guide](../docs/developer-guide.md)** - Setup, common tasks, conventions, code style
- **[External Integrations](../docs/external-integrations.md)** - Verifactu tax service and future integrations
- **[How to Create Endpoints](../docs/how-to-create-endpoints.md)** - Step-by-step endpoint creation guide
- **[How to Refactor Controllers to Services](../docs/how-to-refactor-controllers-to-services.md)** - Service layer refactoring patterns
- **[Architectural Debt Assessment](../docs/architectural-debt-assessment.md)** - Known issues and improvement roadmap

**Use these docs to understand patterns before implementing new features.**

## Architecture & Structure

**For detailed architecture information, see [Architecture Layers](../docs/architecture-layers.md)**

### Solution Structure

```
lilith-backend/
├── src/                              # All source projects
│   ├── Api/                         # Web API controllers
│   ├── Application/                 # Application services
│   ├── Application.Contracts/       # Service interfaces, DTOs
│   ├── Domain/                      # Domain entities
│   ├── Infrastructure/              # Data access, repositories
│   └── Verifactu/                   # Tax integration service
├── test/                             # Test projects (unit, integration)
├── docs/                             # Comprehensive documentation system
└── Lilith.Backend.slnx              # Solution file (XML format)
```

### Quick Architecture Summary

**Clean Architecture with 6 projects:**

1. **Domain** - Pure entities (no dependencies) - See [Domain Model](../docs/domain-model.md)
2. **Application.Contracts** - All interfaces, DTOs, constants (flat namespace)
3. **Application** - Business logic services - See [Architectural Patterns](../docs/architectural-patterns.md)
4. **Infrastructure** - Repository implementations, EF Core
5. **Api** - Controllers, middleware, startup configuration
6. **Verifactu** - Spanish tax integration - See [External Integrations](../docs/external-integrations.md)

**Dependency flow:** Api → Application/Infrastructure → Application.Contracts → Domain

**For complete layer details, request flow diagrams, and interaction patterns, see the documentation links above.**

## Domain Model

**For complete domain model with entity hierarchies and relationships, see [Domain Model](../docs/domain-model.md)**

### Base Entity

All entities inherit from abstract `Entity` class:

```csharp
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public bool Disabled { get; set; } = false;  // Soft delete
}
```

### Core Business Areas

**Sales:** Customer → Budget → SalesOrder → DeliveryNote → SalesInvoice  
**Purchase:** Supplier → PurchaseOrder → Receipt → PurchaseInvoice  
**Production:** WorkMaster → WorkOrder → Phases → ProductionPart → Cost tracking  
**Warehouse:** Stock → StockMovement → Location management  
**Shared:** Reference (products), Lifecycle/Status (workflows), Exercise (fiscal periods)

**See [Domain Model](../docs/domain-model.md) for complete entity details, relationships, and ASCII diagrams.**

## Common Patterns & Conventions

**For detailed pattern implementations with code examples, see [Architectural Patterns](../docs/architectural-patterns.md)**

### Response Pattern

Use `GenericResponse` from `Application.Contracts` for service layer returns:

```csharp
public class GenericResponse
{
    public bool Result { get; }
    public IList<string> Errors { get; }
    public object? Content { get; }
}
```

**Usage:**

- **Read operations:** Return entities directly (`Task<Budget?>`, `IEnumerable<Budget>`)
- **Write operations:** Return `GenericResponse` for error handling
- **Controllers:** Map `GenericResponse.Result` to HTTP status codes (200/201 success, 400/404/409 errors)

### Primary Constructor Pattern (C# 12)

**Always use primary constructors** for dependency injection:

```csharp
// ✅ Correct - Primary constructor
public class BudgetService(IUnitOfWork unitOfWork, ILocalizationService localization) : IBudgetService
{
    // Direct usage of injected dependencies
}

// ❌ Avoid - Old verbose pattern
public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _unitOfWork;
    public BudgetService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }
}
```

### Namespace Conventions

**Application.Contracts namespace** contains all contracts with flat structure:

- Service interfaces: `Application.Contracts` (e.g., `IBudgetService`, `ISalesOrderService`)
- DTOs and request/response models: `Application.Contracts` (e.g., `GenericResponse`, `CreateHeaderRequest`)
- Repository interfaces: `Application.Contracts` (e.g., `IRepository<T>`, `IUnitOfWork`)
- Constants: `Application.Contracts` (e.g., `StatusConstants`)

**Usage in files:**

```csharp
using Application.Contracts;  // All contracts, interfaces, DTOs, constants
using Domain.Entities.Sales;  // Domain entities
using Infrastructure.Persistance;  // Only for ApplicationDbContext
```

### Controller Conventions

- Use `[ApiController]` and `[Route("api/[controller]")]` attributes
- Implement standard CRUD operations: Create, GetById, GetAll, Update, Delete
- Use DTOs for complex requests (e.g., `CreateHeaderRequest`, `ChangeStatusRequest`)
- Validate `ModelState` before processing requests
- Return appropriate HTTP status codes (200, 201, 400, 404, 409)
- **Primary constructor pattern**: `public class Controller(IDependency dep) : ControllerBase`

### Service Layer Patterns

- Services inject `IUnitOfWork` for data access
- Use lifecycle/status management for workflow control
- Implement business validation before persistence
- Generate document numbers via `IExerciseService.GetNextCounter()`
- Handle related entity updates (e.g., updating stock, changing order statuses)
- **Primary constructor pattern**: `public class Service(IUnitOfWork unitOfWork, ILocalizationService localization)`

### Repository Implementations

- Generic `Repository<TEntity, TId>` for basic CRUD
- Specialized repositories inherit and extend functionality
- Use `Find()` for synchronous queries, `FindAsync()` for async
- Implement `UpdateWithoutSave()` for batch operations with manual `CompleteAsync()`

### Database Configuration

- Entity configurations in `src/Infrastructure/Persistance/EntityConfiguration/`
- Use `EntityBaseConfiguration.ConfigureBase<T>()` for common entity properties
- UUID primary keys with `ValueGeneratedNever()`
- Timestamp columns with PostgreSQL-specific types
- Decimal precision: (18,4) for general amounts, (18,2) for prices

## Business Logic Patterns

### Lifecycle Management

All major business entities use lifecycle/status management:

- Each entity type has a named lifecycle (e.g., "WorkOrder", "SalesOrder", "PurchaseInvoice")
- Status transitions control business workflow
- Use `ILifecycleRepository.GetStatusByName(lifecycleName, statusName)` to get specific statuses
- Initial status set from `Lifecycle.InitialStatusId`

### Document Numbering

- Documents use sequential numbering per exercise/fiscal period
- Generate via `IExerciseService.GetNextCounter(exerciseId, documentType)`
- Document types: "workorder", "salesorder", "purchaseorder", "salesinvoice", etc.

### Cost Calculation

- Work orders track operator, machine, material, and external costs
- Use `IMetricsService.GetWorkmasterMetrics()` for cost calculations
- Costs flow from work orders to references and sales order details
- Production parts capture actual time and costs vs. estimates

### Stock Management

- Stock movements track inventory changes
- Delivery notes consume stock when delivered
- Receipts add to stock when received
- Use `IStockService` for inventory operations

### Background Services

- **BudgetBackgroundService**: Automatically rejects outdated budgets every 8 hours
- Register via `services.AddHostedService<TService>()` in `ApplicationServicesSetup`
- Use `IServiceScopeFactory` to create scoped services within background tasks

## Language & Localization

**For complete localization system documentation, see [Localization](../docs/localization.md)**

The system implements **comprehensive multilanguage support** with full internationalization across all business logic.

### Supported Languages

- **Catalan (ca)** - Default language
- **Spanish (es)** - Secondary language
- **English (en)** - International support

### Localization Architecture

- **JSON-based resource files** in `src/Api/Resources/LocalizationService/`
- **ILocalizationService** for dependency injection and string retrieval
- **CultureMiddleware** for automatic language detection
- **StatusConstants** in `src/Application.Contracts/Constants/StatusConstants.cs` for database-stored values

### Culture Detection Priority

1. Query parameter: `?culture=ca`
2. Accept-Language header
3. Default to Catalan (ca)

### Critical Rules

⚠️ **ALL services MUST:**

1. Inject `ILocalizationService` in constructor
2. Use `StatusConstants` for database values (lifecycles/statuses)
3. Add localization keys to ALL 3 language files (ca.json, es.json, en.json)
4. Use parameterized messages for dynamic content
5. NEVER use hardcoded error strings

### Using Localization in Services

**Example:**

```csharp
public class BudgetService(IUnitOfWork unitOfWork, ILocalizationService localization) : IBudgetService
{
    public async Task<GenericResponse> Create(Budget budget)
    {
        var exists = unitOfWork.Budgets.Find(b => b.Id == budget.Id).Any();
        if (exists)
            return new GenericResponse(false,
                localization.GetLocalizedString("BudgetAlreadyExists"));

        // Use StatusConstants for database values
        var lifecycle = unitOfWork.Lifecycles
            .Find(l => l.Name == StatusConstants.Lifecycles.Budget)
            .FirstOrDefault();

        // ...
    }
}
```

**For complete localization patterns, standard keys, and best practices, see [Localization](../docs/localization.md)**

## Development Guidelines

**For detailed setup instructions and common tasks, see [Developer Guide](../docs/developer-guide.md)**

### When Adding New Features

1. **Review documentation first** - Check [Architecture Layers](../docs/architecture-layers.md) and [Architectural Patterns](../docs/architectural-patterns.md)
2. **Domain First** - Define entities in Domain layer with proper relationships
3. **Follow patterns** - Use existing implementations as reference (see [How to Create Endpoints](../docs/how-to-create-endpoints.md))
4. **Service Layer** - Implement business logic in Application services
5. **Localization** - Add keys to all 3 language files (see [Localization](../docs/localization.md))
6. **Update Documentation** - ⚠️ **MANDATORY** - Keep markdown files in sync with code changes

### Error Handling

- Use `GenericResponse` with multiple error support
- Return appropriate HTTP status codes
- Validate `ModelState` in controllers
- **ALWAYS use localized error messages** via `ILocalizationService`
- **NEVER use hardcoded strings**

### Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName --project src/Infrastructure/

# Apply to database
dotnet ef database update --project src/Infrastructure/
```

### Code Style & Conventions

- **Primary constructors** - Use C# 12 syntax for all services/controllers
- **Async/await** - All I/O operations must be asynchronous
- **Localization** - Inject `ILocalizationService` in all services
- **Constants** - Use `StatusConstants` for lifecycle/status names
- **Nullable types** - Use appropriately (`Type?` for nullable)

**For complete coding conventions, see [Developer Guide](../docs/developer-guide.md)**

---

## 📝 Documentation Maintenance - MANDATORY

**⚠️ CRITICAL: Code and documentation must stay synchronized.**

### When to Update Documentation

**ALWAYS update relevant documentation when:**

1. **Adding new entities** → Update [Domain Model](../docs/domain-model.md)
2. **Adding new patterns** → Update [Architectural Patterns](../docs/architectural-patterns.md)
3. **Changing architecture** → Update [Architecture Layers](../docs/architecture-layers.md)
4. **Adding localization keys** → Update [Localization](../docs/localization.md)
5. **Adding external integrations** → Update [External Integrations](../docs/external-integrations.md)
6. **Changing request flow** → Update [Request Flow](../docs/request-flow.md)
7. **Adding setup steps** → Update [Developer Guide](../docs/developer-guide.md)
8. **Identifying architectural issues** → Update [Architectural Debt Assessment](../docs/architectural-debt-assessment.md)

### Documentation Update Checklist

When making code changes, verify:

- [ ] **README.md** - Updated if quick start or tech stack changed
- [ ] **Architecture Layers** - Updated if project structure or responsibilities changed
- [ ] **Domain Model** - Updated if entities added/modified or relationships changed
- [ ] **Architectural Patterns** - Updated if new patterns introduced or existing ones changed
- [ ] **Request Flow** - Updated if middleware, layers, or flow changed
- [ ] **Localization** - Updated if new keys added or culture detection changed
- [ ] **Developer Guide** - Updated if setup, tasks, or conventions changed
- [ ] **External Integrations** - Updated if integrations added/modified
- [ ] **How-to guides** - Updated if step-by-step processes changed

### Documentation Quality Standards

**All documentation updates must:**

1. ✅ **Use consistent structure** - Follow existing markdown format and headers
2. ✅ **Include ASCII diagrams** - Simple box-and-arrow relationships for architecture
3. ✅ **Provide code examples** - 5-10 line concise snippets showing patterns
4. ✅ **Add cross-references** - Link to related documentation
5. ✅ **Update index** - Ensure README.md index reflects all documents
6. ✅ **Match code reality** - Examples must reflect actual implementation
7. ✅ **Use clear language** - Write for developers joining the team

### Documentation Review Process

Before committing code changes:

1. **Identify affected docs** - Which markdown files relate to your changes?
2. **Update content** - Modify relevant sections with accurate information
3. **Add examples** - Include code snippets if introducing new patterns
4. **Update diagrams** - Modify ASCII diagrams if architecture changed
5. **Test links** - Verify all cross-references work
6. **Review consistency** - Ensure tone and format match existing docs

### Documentation File Ownership

| File                          | When to Update                     | Owner Mindset             |
| ----------------------------- | ---------------------------------- | ------------------------- |
| **README.md**                 | Tech stack, quick start changes    | First impression document |
| **architecture-layers.md**    | Project structure, layer changes   | Architecture authority    |
| **domain-model.md**           | Entity changes, new business areas | Business domain expert    |
| **architectural-patterns.md** | New patterns, pattern changes      | Pattern library           |
| **request-flow.md**           | Middleware, flow, layer changes    | Flow diagram master       |
| **localization.md**           | Localization changes, new keys     | i18n expert               |
| **developer-guide.md**        | Setup, tasks, conventions          | Onboarding guide          |
| **external-integrations.md**  | External service changes           | Integration specialist    |

### Common Documentation Mistakes to Avoid

❌ **DON'T:**

- Leave outdated code examples in documentation
- Document planned features not yet implemented
- Use inconsistent terminology across files
- Forget to update ASCII diagrams when structure changes
- Add documentation without cross-references
- Use overly complex language or unexplained jargon

✅ **DO:**

- Update docs in the same commit as code changes
- Keep examples concise and relevant
- Use established terminology consistently
- Verify all links and references work
- Write for developers with varying experience levels
- Include "why" explanations, not just "what"

---

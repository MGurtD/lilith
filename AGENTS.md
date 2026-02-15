# AGENTS.md - Lilith ERP System

Guidelines for AI coding agents working in this manufacturing ERP monorepo.

## Project Overview

**Monorepo structure**: Backend (.NET 10) + Frontend (Vue 3) as single integrated unit  
**Domain**: Manufacturing ERP covering Sales, Purchase, Production, Warehouse  
**Languages**: Catalan primary (ca), Spanish (es), English (en)  
**Architecture**: Backend uses Clean Architecture (6 layers), Frontend uses domain modules

## Quick Commands

### Backend (lilith-backend/)

```bash
# Build and run
dotnet build
dotnet run --project src/Api/
dotnet watch run --project src/Api/  # Hot reload

# Database migrations
dotnet ef migrations add MigrationName --project src/Infrastructure/
dotnet ef database update --project src/Infrastructure/

# Run single test
⚠️ NO TESTS CONFIGURED (critical architectural debt)
```

**Swagger**: https://localhost:5001/swagger

### Frontend (lilith-frontend/)

```bash
# Setup (requires pnpm v10+)
pnpm install

# Development
pnpm run dev                    # Start dev server at http://localhost:8100
pnpm run typecheck              # Type check without building
pnpm run build                  # Production build
pnpm run build-development      # Dev mode build to dist-test/

# Run single test
⚠️ NO TESTS CONFIGURED
```

**Package manager**: MUST use pnpm (not npm/yarn)

## Code Style Guidelines

### Backend (C# 12)

**Critical conventions:**
- **Primary constructors**: `public class BudgetService(IUnitOfWork unitOfWork, ILocalizationService localization) : IBudgetService`
- **ALWAYS async/await** for all I/O operations
- **ALWAYS inject ILocalizationService** for user-facing messages
- **ALWAYS use StatusConstants** - Never hardcode "Budget", "SalesOrder", "Pendent d'acceptar"
- **ALWAYS return GenericResponse** for write operations (create/update/delete)
- **Nullable reference types** enabled: `string Name { get; set; } = string.Empty;` or `string? Description { get; set; }`

**Layer responsibilities:**
- **Controllers**: HTTP validation only, NO business logic
- **Services**: ALL business logic + localization + workflow orchestration
- **Repositories**: Data access only, EF Core queries with Include() for relations

**Naming conventions:**
- Services: `IBudgetService`, `BudgetService`
- Repositories: `IBudgetRepository`, `BudgetRepository`
- Controllers: `BudgetController`
- Entities: PascalCase (`SalesOrderHeader`, `WorkOrder`)

### Frontend (TypeScript + Vue 3)

**Critical conventions:**
- **ALWAYS use Composition API** with `<script setup>` (never Options API)
- **ALWAYS PascalCase** component files: `WorkOrderDetail.vue`
- **ALWAYS path alias** `@/` for imports: `import { useStore } from "@/store"`
- **ALWAYS lazy-load routes**: `component: () => import('./views/WorkOrder.vue')`
- **ALWAYS normalize dates** before API calls: `convertDateTimeToJSON(date)`
- **ALWAYS Catalan** for all UI messages and labels

**Architecture patterns:**
- Services extend `BaseService<T>` with inherited CRUD methods
- Pinia stores manage state + orchestrate service calls
- Clone objects before editing in dialogs: `const editModel = { ...original }`
- Numeric defaults are `0`, not `undefined`

**Naming conventions:**
- Components: PascalCase files and imports
- Stores: `useWorkOrderStore`, `useBudgetStore`
- Services: `WorkOrderService`, `BudgetService`
- Functions: camelCase

### Shared Conventions (Backend + Frontend)

**Database patterns:**
- **UUID primary keys** (client-generated): Backend `Guid.NewGuid()`, Frontend `getNewUuid()`
- **Soft deletes** via `Disabled` field - never physical deletion
- **Audit timestamps**: `CreatedOn`, `UpdatedOn` auto-managed

**Localization:**
- All lifecycle/status names in database are Catalan
- Error messages via `ILocalizationService` (backend) or Catalan strings (frontend)
- Culture detection: query param → JWT claim → Accept-Language header → default (ca)

## Critical Architectural Patterns

### Backend Request Flow

```
HTTP Request → Controller → Service → Repository → Database
             ↓            ↓          ↓
         Validate    Business   Data Access
         ModelState  Logic +    (EF Core)
                    Localize
```

**Controller pattern:**
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateRequest request)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);
    var response = await service.Create(request);
    return response.Result ? Ok(response.Content) : BadRequest(response);
}
```

**Service pattern:**
```csharp
public async Task<GenericResponse> Create(Budget budget)
{
    var exists = unitOfWork.Budgets.Find(b => b.Id == budget.Id).Any();
    if (exists)
        return new GenericResponse(false, 
            localizationService.GetLocalizedString("EntityAlreadyExists"));
    
    await unitOfWork.Budgets.Add(budget);
    return new GenericResponse(true, budget);
}
```

### Frontend Data Flow

```
Component → Store → Service → API
    ↓        ↓        ↓
   UI     State    HTTP
Rendering  Mgmt   (Axios)
```

**Component → Store:**
```typescript
const store = useWorkOrderStore();
await store.create(model);
```

**Store → Service:**
```typescript
async create(model: WorkOrder) {
  const result = await Services.WorkOrder.create(model);
  if (result) await this.fetchOne(model.id);
  return result;
}
```

**Service (BaseService):**
```typescript
export class WorkOrderService extends BaseService<WorkOrder> {
  constructor() { super("workorder"); }
}
```

### Backend Architecture Layers

```
         API (Composition Root)
           ↓         ↓
    Application  Infrastructure
           ↓         ↓
     Application.Contracts
           ↓
         Domain (Pure Core)
```

**Dependency rule**: Dependencies only flow inward. Inner layers never reference outer layers.

## Anti-Patterns to Avoid

**Backend:**
- ❌ Business logic in controllers
- ❌ Hardcoded error strings (use ILocalizationService)
- ❌ Magic strings for lifecycle/status names (use StatusConstants)
- ❌ Direct `IUnitOfWork` injection in controllers (use service interfaces)
- ❌ Mixing languages in error messages
- ❌ Returning `bool` instead of `GenericResponse` for operations

**Frontend:**
- ❌ Options API (always use Composition API with `<script setup>`)
- ❌ Mutating store state outside actions
- ❌ Hard-coding API URLs (use service layer)
- ❌ Using `any` type
- ❌ Forgetting to normalize dates before API calls
- ❌ Mixing languages in UI (maintain Catalan)

## Documentation References

**Backend deep dives:**
- Architecture: `lilith-backend/docs/architecture-layers.md`
- Patterns: `lilith-backend/docs/architectural-patterns.md`
- Domain model: `lilith-backend/docs/domain-model.md`
- Developer guide: `lilith-backend/docs/developer-guide.md`
- Localization: `lilith-backend/docs/localization.md`
- Request flow: `lilith-backend/docs/request-flow.md`

**Frontend guide:**
- Comprehensive reference: `lilith-frontend/AGENTS.md` (218 lines)

**Skills (task-specific guides):**
- See `.skills/` directory for focused, task-oriented guides
- Use skills for: adding entities, implementing CRUD, understanding workflows

## Environment Setup

**Prerequisites:**
- .NET 10 SDK
- Node.js 18+ with pnpm v10+
- PostgreSQL 16+

**Connection strings:**
- Backend: `src/Api/appsettings.Development.json`
- Frontend: `.env` with `VITE_API_BASE_URL` and `VITE_REPORTS_BASE_URL`

**First time setup:**
```bash
# Backend
cd lilith-backend
dotnet restore
dotnet ef database update --project src/Infrastructure/
dotnet run --project src/Api/

# Frontend
cd lilith-frontend
pnpm install
pnpm run dev
```

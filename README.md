# Lilith ERP - Manufacturing Management System

Monorepo for the Lilith ERP system, a comprehensive manufacturing management solution covering Sales, Purchase, Production, and Warehouse operations.

## 📁 Repository Structure

```
lilith/
├── backend/              # .NET 10 Web API (Clean Architecture)
├── frontend/             # Vue 3 + TypeScript SPA
├── .github/workflows/    # CI/CD pipelines
├── docker-compose.yml    # Full stack orchestration
├── .env.example         # Environment variables template
└── AGENTS.md            # AI coding agent guidelines
```

**Note**: If you see `lilith-backend/` and `lilith-frontend/` directories instead of `backend/` and `frontend/`, these need to be renamed manually due to file locks. See [Setup](#-setup) section.

## 🚀 Quick Start

### Prerequisites

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 18+** with **pnpm v10+** - [Install pnpm](https://pnpm.io/installation)
- **PostgreSQL 16+** - [Download](https://www.postgresql.org/download/)
- **Docker & Docker Compose** (optional, for containerized deployment)

### Backend Setup

```bash
cd backend
dotnet restore
dotnet ef database update --project src/Infrastructure/
dotnet run --project src/Api/
```

Backend runs on: `https://localhost:5001`  
Swagger UI: `https://localhost:5001/swagger`

**Hot reload during development:**
```bash
dotnet watch run --project src/Api/
```

### Frontend Setup

```bash
cd frontend
pnpm install
pnpm run dev
```

Frontend runs on: `http://localhost:8100`

**Production build:**
```bash
pnpm run typecheck    # Type checking
pnpm run build        # Build to dist/
```

### Docker Compose (Full Stack)

```bash
# Copy environment template
cp .env.example .env

# Edit .env with your configuration
# Then start all services
docker-compose up -d
```

Services:
- **Backend API**: `http://localhost:5000`
- **Frontend**: `http://localhost:8080`
- **PostgreSQL**: `localhost:5432`

## 🏗️ Architecture

### Backend (.NET 10)

**Clean Architecture** with 6 layers following the Dependency Rule:

```
API (Composition Root)
  ↓
Application + Infrastructure
  ↓
Application.Contracts
  ↓
Domain (Pure Core)
```

**Key patterns:**
- **Primary constructors** for dependency injection
- **Repository pattern** with `IUnitOfWork`
- **Service layer** for all business logic
- **GenericResponse** for operation results
- **ILocalizationService** for multilingual support (ca/es/en)

**Tech stack:**
- ASP.NET Core 10 Web API
- Entity Framework Core 10
- PostgreSQL 16
- FluentValidation
- AutoMapper

### Frontend (Vue 3)

**Domain-driven modules** with centralized state management:

```
Component → Pinia Store → Service Layer → API
```

**Key patterns:**
- **Composition API** with `<script setup>` (never Options API)
- **BaseService<T>** for inherited CRUD operations
- **Pinia stores** for state + service orchestration
- **Path aliases** with `@/` imports
- **Lazy-loaded routes** for code splitting

**Tech stack:**
- Vue 3 (Composition API)
- TypeScript 5
- Pinia (state management)
- Ionic Framework (UI components)
- Axios (HTTP client)
- Vite (build tool)

## 🌍 Localization

**Primary language**: Catalan (ca)  
**Supported languages**: Spanish (es), English (en)

- All database lifecycle/status names are in Catalan
- Backend uses `ILocalizationService` for error messages
- Frontend UI is in Catalan
- Culture detection priority: query param → JWT claim → Accept-Language → default (ca)

## 📊 Database

**PostgreSQL 16** with Entity Framework Core migrations

**Key conventions:**
- **UUID primary keys** (client-generated via `Guid.NewGuid()` / `getNewUuid()`)
- **Soft deletes** via `Disabled` field (never physical deletion)
- **Audit timestamps**: `CreatedOn`, `UpdatedOn` (auto-managed)

**Migrations:**
```bash
cd backend

# Create migration
dotnet ef migrations add MigrationName --project src/Infrastructure/

# Apply migrations
dotnet ef database update --project src/Infrastructure/

# Rollback migration
dotnet ef database update PreviousMigrationName --project src/Infrastructure/
```

## 🧪 Testing

⚠️ **Critical architectural debt**: No automated tests are currently configured for either backend or frontend.

**Planned testing stack:**
- Backend: xUnit + FluentAssertions
- Frontend: Vitest + Vue Test Utils

## 🔄 CI/CD

GitHub Actions workflows with path-based triggers:

- **`.github/workflows/backend-ci.yml`** - Triggers on `backend/**` or `lilith-backend/**` changes
- **`.github/workflows/frontend-ci.yml`** - Triggers on `frontend/**` or `lilith-frontend/**` changes

**Workflows:**
1. Build and test (on push/PR)
2. Deploy to production (on push to `main`/`master`)
3. Docker image build and push

## 🛠️ Development

### Code Style

**Backend (C#):**
- Primary constructors for DI
- Always async/await for I/O
- Always inject ILocalizationService
- Use StatusConstants (never hardcode status strings)
- Always return GenericResponse for write operations
- Nullable reference types enabled

**Frontend (TypeScript):**
- Always use Composition API with `<script setup>`
- PascalCase for component files
- Always use `@/` path alias
- Lazy-load routes
- Normalize dates before API calls with `convertDateTimeToJSON()`
- All UI text in Catalan

### VS Code Workspace

Open the workspace file for optimal multi-root setup:
```bash
code lilith.code-workspace
```

This provides:
- Separate settings for backend and frontend
- Proper TypeScript/C# tooling
- Recommended extensions

## 📚 Documentation

- **`AGENTS.md`** - Guidelines for AI coding agents
- **`backend/docs/`** - Backend architecture deep-dives
  - `architecture-layers.md`
  - `architectural-patterns.md`
  - `domain-model.md`
  - `developer-guide.md`
  - `localization.md`
  - `request-flow.md`
- **`frontend/AGENTS.md`** - Frontend development guide
- **`.opencode/skills/`** - Task-specific OpenCode skills

## 🐛 Known Issues

1. **Directory naming**: If directories are still named `lilith-backend/` and `lilith-frontend/`, they need manual rename:
   ```bash
   # Close VS Code and stop all dev servers first
   mv lilith-backend backend
   mv lilith-frontend frontend
   git add -A
   git commit -m "refactor: rename directories to backend/ and frontend/"
   ```

2. **No automated tests**: Critical debt - test infrastructure needs implementation

3. **Line endings**: Git may warn about LF/CRLF conversions on Windows

## 🤝 Contributing

This is a monorepo managed with standard Git workflows:

1. Create feature branch: `git checkout -b feature/my-feature`
2. Make changes in `backend/` or `frontend/`
3. Commit with conventional commits format: `feat:`, `fix:`, `chore:`, etc.
4. Push and create pull request
5. CI/CD will automatically test/build affected parts

## 📄 License

[Your license here]

## 🔗 Links

- **Backend API Docs**: https://localhost:5001/swagger
- **Frontend Dev Server**: http://localhost:8100
- **Production**: [Your production URL]

---

**Monorepo structure note**: This project was converted from two separate repositories (`lilith-backend` and `lilith-frontend`) into a unified monorepo for better semantic coupling and streamlined development.

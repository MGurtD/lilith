# Lilith Backend

> Part of the [Lilith monorepo](../README.md). Backend-only docs — assume you already cloned and have prerequisites installed (see root README).

.NET 10 Web API implementing Clean Architecture with **6 projects** following the Dependency Rule. All business logic lives in the Service layer (51 controllers, refactored as of Dec 2025).

**Architecture grade: A (9.5/10)** — complete service-layer separation, consistent error handling, full localization. Remaining work: test coverage and authorization framework.

## Project layout

```
backend/
├── src/
│   ├── Domain/                  # Pure entities. Zero dependencies.
│   ├── Application.Contracts/   # Service & Repository interfaces, DTOs, StatusConstants.
│   ├── Application/             # Business logic. Implements contracts.
│   ├── Infrastructure/          # EF Core + PostgreSQL, repositories, UnitOfWork.
│   ├── Api/                     # Controllers, middleware, Program.cs (composition root).
│   └── Verifactu/               # Spanish AEAT invoice registry integration.
├── docs/                        # Architecture deep-dives (see index below).
├── tests/                       # ⚠️ Not yet implemented.
└── README.md
```

### Dependency flow

```
              ┌─────────────────────────────────────────┐
              │                  API                    │  composition root
              │  (Controllers → service interfaces only)│
              └─────────────┬───────────────────────────┘
                            │ depends on
              ┌─────────────▼───────────────────────────┐
              │          Application.Contracts          │  interfaces, DTOs
              └─────────────┬───────────────────────────┘
                            │ implemented by
        ┌───────────────────┴───────────────────┐
        │                                       │
┌───────▼──────────┐                  ┌─────────▼────────┐
│   Application    │──── uses IUoW ──▶│  Infrastructure  │
│   (Services)     │                  │  (Repos, EF Core)│
└──────────────────┘                  └─────────┬────────┘
                                               │
                                    ┌──────────▼────────┐
                                    │      Domain       │  pure core
                                    └───────────────────┘
```

External integrations (`Verifactu`) are isolated in their own project and called from `Application` services.

## Run locally

```bash
cd backend

# 1. Restore
dotnet restore

# 2. Configure connection string
#    Edit src/Api/appsettings.Development.json
#    OR: dotnet user-secrets set "ConnectionStrings:Default" "Host=...;..."

# 3. Apply migrations
dotnet ef database update --project src/Infrastructure/

# 4. Run with hot reload
dotnet watch run --project src/Api/

# Swagger UI: https://localhost:5001/swagger
```

## Migrations

```bash
# Create
dotnet ef migrations add <Name> --project src/Infrastructure/

# Apply
dotnet ef database update --project src/Infrastructure/

# Rollback
dotnet ef database update <PreviousMigrationName> --project src/Infrastructure/
```

## Critical conventions

⚠️ Non-negotiable — break these and the architecture degrades.

| Rule | Why |
|------|-----|
| Use `StatusConstants` for lifecycle/status names | Catalan DB values + type safety |
| Inject `ILocalizationService` in every service | Multilingual error messages (ca/es/en) |
| Return `GenericResponse` from all write operations | Standardized result + error contract |
| Use primary constructors for DI (`public class X(IY y) : IX`) | Modern C# 12, testability |
| `async/await` for all I/O | Scalability, no thread blocking |
| No business logic in controllers | All 51 already follow this — keep the pattern |
| Never inject `IUnitOfWork` in controllers | Use service interfaces; repos stay in Infrastructure |

## Documentation index

### Architecture
- [architecture-layers.md](docs/architecture-layers.md) — the 6 projects and their responsibilities
- [architectural-patterns.md](docs/architectural-patterns.md) — Repository, Service, GenericResponse, FluentAPI
- [request-flow.md](docs/request-flow.md) — HTTP request lifecycle with sequence diagrams
- [domain-model.md](docs/domain-model.md) — entities across Sales, Purchase, Production, Warehouse

### Development
- [developer-guide.md](docs/developer-guide.md) — setup, common tasks, conventions
- [localization.md](docs/localization.md) — ILocalizationService, JSON resources, culture detection
- [external-integrations.md](docs/external-integrations.md) — Verifactu and other external systems

### Health & debt
- [architectural-debt-assessment.md](docs/architectural-debt-assessment.md) — tests, authz, known issues

## Verifactu (Spanish AEAT)

`src/Verifactu/` is the Spanish tax authority invoice registry integration. Isolated as its own project; called from `Application` services via interface. See [external-integrations.md](docs/external-integrations.md) for activation and usage.

## Docker

```bash
# Build
docker build -t lilith-backend .

# Run
docker run -p 8080:80 \
  -e ConnectionStrings__Default="Host=postgres;Database=lilith;Username=...;Password=..." \
  lilith-backend
```

For full-stack orchestration see the root [docker-compose.yml](../docker-compose.yml).

## Contributing

1. Read [developer-guide.md](docs/developer-guide.md) for the full conventions list.
2. Check [architectural-debt-assessment.md](docs/architectural-debt-assessment.md) before adding scope.
3. Mirror existing patterns in [architectural-patterns.md](docs/architectural-patterns.md).
4. Add localization keys for every new user-facing string.
5. Update relevant `docs/` files when behavior or contracts change.

## License

Internal project — all rights reserved.
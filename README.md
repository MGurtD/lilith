# Lilith ERP

A manufacturing ERP for small and mid-sized manufacturers in Catalonia and Spain, covering Sales, Purchase, Production, and Warehouse operations end to end.

**Monorepo**: .NET 10 backend + Vue 3 frontend, deployed via Docker Compose.

## Who is this for

| You are... | Start here |
|------------|------------|
| New engineer on the project | [Quick start](#quick-start) below, then [backend](backend/README.md) or [frontend](frontend/README.md) |
| Backend developer | [backend/README.md](backend/README.md) |
| Frontend developer | [frontend/README.md](frontend/README.md) |
| AI coding agent | [AGENTS.md](AGENTS.md) |
| Looking for architecture | [backend/docs/architecture-layers.md](backend/docs/architecture-layers.md) |
| Tracking known gaps | [backend/docs/architectural-debt-assessment.md](backend/docs/architectural-debt-assessment.md) |

## Quick start

Prerequisites: **.NET 10 SDK**, **Node.js 18+**, **pnpm v10+**, **PostgreSQL 16+**, Docker (optional).

```bash
git clone <repo-url> lilith
cd lilith

# Option A — full stack via Docker
cp .env.example .env
docker compose up -d

# Option B — local dev (two terminals)
cd backend && dotnet ef database update --project src/Infrastructure/ && dotnet run --project src/Api/
cd frontend && pnpm install && pnpm run dev
```

| Service | URL |
|---------|-----|
| Backend API + Swagger | https://localhost:5001/swagger |
| Frontend dev server | http://localhost:8100 |
| Backend (Docker) | http://localhost:5000 |
| Frontend (Docker) | http://localhost:8080 |
| PostgreSQL | localhost:5432 |

## Stack at a glance

| Layer | Tech |
|-------|------|
| Backend | .NET 10, EF Core 10, PostgreSQL 16, Clean Architecture (6 projects) |
| Frontend | Vue 3 Composition API, TypeScript 5, Pinia, PrimeVue 4, Vite |
| Auth | JWT bearer |
| Localization | Catalan (primary), Spanish, English |
| CI/CD | GitHub Actions (path-based: `backend/**`, `frontend/**`) |
| Deploy | Docker Compose + Nginx static SPA |

## Repository layout

```
.
├── backend/           # .NET 10 Web API → backend/README.md
│   └── docs/          # Architecture deep-dives
├── frontend/          # Vue 3 SPA → frontend/README.md
├── .github/workflows/ # CI/CD per app
├── docker-compose.yml # Full stack
├── AGENTS.md          # AI coding agent guidelines
└── .env.example
```

## Conventions in one line

- **IDs**: client-generated UUIDs (`Guid.NewGuid()` / `getNewUuid()`). No autoincrement.
- **Deletes**: soft via `Disabled` field. Never physical delete.
- **Audit**: `CreatedOn` / `UpdatedOn` auto-managed on every entity.
- **Errors**: backend returns `GenericResponse` with localized messages; frontend shows toast in Catalan.
- **Status names**: stored in Catalan, referenced via `StatusConstants` — never hardcoded.
- **Commits**: conventional (`feat:`, `fix:`, `chore:`, `docs:`, `refactor:`).

## Domain coverage

| Area | Entities |
|------|----------|
| Sales | Customer, Budget, SalesOrder, DeliveryNote, SalesInvoice |
| Purchase | Supplier, PurchaseOrder, Receipt, PurchaseInvoice |
| Production | WorkMaster, WorkOrder, ProductionPart, Workcenter |
| Warehouse | Stock, Location, StockMovement, Reference |
| Tax | Verifactu (Spanish AEAT invoice registry) |

## Known gaps

- ⚠️ **No automated tests** in either app. Track via [architectural-debt-assessment](backend/docs/architectural-debt-assessment.md).
- ⚠️ **No authorization framework** yet (auth works, authz is open).
- On Windows: Git may warn about LF/CRLF — harmless, ignore.

## Contributing

1. Branch from `dev`: `git checkout -b feature/my-feature`.
2. Touch only `backend/` or `frontend/` — CI runs the affected pipeline.
3. Use conventional commits.
4. Open PR → review → merge to `dev` → promote to `main`.

## License

Internal project — all rights reserved.
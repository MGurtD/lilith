# Lilith ERP Agent Guide

Project instructions for OpenCode sessions in this monorepo. Keep this file limited to durable, project-wide rules. Task procedures belong in `.opencode/skills/`.

## Repository

- `backend/`: .NET 10 backend with Domain, Application.Contracts, Application, Infrastructure, Api, and Verifactu projects.
- `frontend/`: Vue 3.5, TypeScript 6, Vite 8, Pinia 2, PrimeVue 4, and Axios.
- Business domains include Sales, Purchase, Production, Warehouse, Plant, and System.
- Supported cultures are Catalan (`ca`), Spanish (`es`), and English (`en`). Catalan is the default/source culture.

Before changing frontend code, read `frontend/AGENTS.md`. Treat it as the canonical frontend policy.

## Prerequisites

- .NET SDK `10.0.100` (see `backend/global.json`).
- Node `^20.19.0` or `>=22.12.0`.
- pnpm `10.28.0`; never use npm or yarn in `frontend/`.
- PostgreSQL for database-backed runtime work.

## Verification Commands

Run backend commands from `backend/`:

```bash
dotnet build
dotnet test
dotnet test tests/Application.Tests/Application.Tests.csproj --filter "FullyQualifiedName~TypeOrMethod"
dotnet run --project src/Api
```

Run frontend commands from `frontend/`:

```bash
pnpm run i18n:check
pnpm run typecheck
pnpm run build
pnpm run smoke
pnpm run smoke:e2e
```

The frontend has no unit/component test framework. It does have Playwright smoke checks. Run only the checks relevant to the change; use the production build for broad frontend changes.

Local launch-profile Swagger is `https://localhost:7284/swagger`. Docker exposes the API separately on port `5000`.

## Backend Rules

- New business workflow logic belongs in application services, not controllers. Existing controllers contain legacy exceptions; do not copy them.
- Controllers handle HTTP concerns and delegate through service interfaces. Do not inject `IUnitOfWork` into new controller code.
- Keep data access and query shape in repositories. Use asynchronous APIs for database and other I/O.
- Use `ILocalizationService` and resource keys for new user-facing backend messages.
- Read the current `StatusConstants.cs` before using lifecycle or status identifiers. Never copy status catalogues into documentation.
- Use `GenericResponse` where the analogous write service contract uses it. Do not change established public contracts solely for uniformity.
- Nullable reference types are enabled. Model optionality explicitly and avoid suppressing nullability without evidence.
- Keep dependency direction inward for new code. Treat current cross-project exceptions as legacy constraints, not examples.
- Prefer the established style in the nearest current module over generic templates.

## Shared Data Rules

- Entity IDs are application-generated GUIDs. The frontend or backend may assign them; the database does not.
- Deletion behavior is entity-specific. Inspect the analogous service and repository before choosing physical deletion, `Disabled`, or another lifecycle transition.
- Most entities use `CreatedOn`, `UpdatedOn`, and `Disabled`, but not every entity uses the standard timestamp configuration.
- Lifecycle identifiers and persisted statuses are domain values, not frontend translation strings.

## Localization

- Backend: localize user-facing responses through `ILocalizationService` and keep placeholders consistent across all resource files.
- Frontend: use Vue i18n keys with parity across `ca`, `es`, and `en`; do not add hardcoded Catalan as the desired end state.
- Culture selection is query parameter, authenticated locale claim, `Accept-Language`, then configured default.
- Do not mix backend resource keys with frontend Vue i18n keys.

## Safety

- Inspect the current implementation and a close analogue before editing. Documentation describes intent; current source defines the active contract.
- Do not generate or remove EF migrations, update a database, install dependencies, run servers, commit, or push unless the user explicitly requests it.
- Preserve unrelated worktree changes.
- Do not add compatibility layers without a concrete persisted or external consumer requirement.

## Task Skills

Load the matching skill for specialized workflows:

- `adding-backend-entity`
- `frontend-crud`
- `backend-localization`
- `audit-frontend-localization`
- `translate-frontend-view`
- `migrate-datatable-to-table`
- `contextual-help`

Skills provide decision procedures, not substitute source code. If a skill conflicts with current code, current code and verified project configuration win.

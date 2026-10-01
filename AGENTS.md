# Lilith ERP Agent Guide

Project instructions for AI coding agent sessions (Claude Code, OpenCode) in this monorepo. Keep this file limited to durable, project-wide rules. Task procedures belong in `.claude/skills/`.

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
dotnet ef migrations has-pending-model-changes --project src/Infrastructure
dotnet run --project src/Api
```

`has-pending-model-changes` compares the EF model with the migrations snapshot. It needs `ConnectionStrings:Default` configured (user secrets `Lilith.Backend` or an environment variable) but does not connect to the database.

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

### Master data deletion

`Repository.Remove` and `RemoveRange` refuse to delete master data that is still in use: they throw `EntityInUseException`, which the API returns as 409 with the reason. What counts as in use is derived from the EF model plus `Infrastructure/Persistance/MasterData/MasterDataCatalog.cs`. See `docs/adr/0001-master-data-delete-guard.md`.

- Mark every new configuration entity that documents refer to (customers, suppliers, references, taxes, statuses…) with `IMasterData`, and give it a name in `MasterDataCatalog.Names`. Add it to `MasterDataCatalog.CanBeDisabled` once its screen lets the user deactivate it, so a refused delete suggests that instead. Nothing detects a master entity left unmarked, and it stays unprotected.
- In `MasterDataCatalog`, declare the records that are deleted together with a master (`OwnedParts`) and every reference to a master without a foreign key (`ExtraReferences`). Prefer adding a real foreign key over an extra reference.
- Foreign keys from master data are `Restrict` by convention (`MasterDataForeignKeys`), whatever the entity configuration says, so PostgreSQL also refuses a delete that bypasses the guard. Only foreign keys to declared owned parts without a filter cascade; the repository deletes filtered parts, such as empty stock, explicitly. A new reference to master data, or a new owned part, therefore changes the model and needs a migration.
- When a new entity refers to master data, add it to `MasterDataCatalog.DocumentKinds` with a `DocumentKind.*` key in ca, es and en. `MasterDataDeleteGuardTests` fails and lists what is missing.
- Delete master data only through `Repository.Remove` or `RemoveRange`, never with `context.Remove`, `ExecuteDelete` or SQL. Do not add in-use checks to services or repositories.
- In a delete that has other effects, remove the master data first, so a refusal happens before anything else changes.
- Most entities use `CreatedOn`, `UpdatedOn`, and `Disabled`, but not every entity uses the standard timestamp configuration.
- Lifecycle identifiers and persisted statuses are domain values, not frontend translation strings.

## Dates and Time

Every date is stored in Europe/Madrid local time. Mixing conventions stores the same date as different instants per environment, and the model then drifts from its migrations so that `dotnet ef database update` refuses to run.

- Every date column is `timestamp without time zone` holding Europe/Madrid wall-clock time. Never add `timestamp with time zone` (timestamptz) columns, `HasColumnType("timestamp with time zone")`, or `DateTimeOffset` properties on entities.
- Use `DateTime` and `DateTime.Now` for any value stored in or compared with the database. Keep `DateTime.UtcNow` for values that never reach the database, such as JWT expiry, log timestamps and caches.
- The API runs with `TZ=Europe/Madrid`, and every connection sets `Timezone=Europe/Madrid` through `DatabaseConnectionString.WithSessionTimeZone`. Connect only through it, and do not remove it or the `Npgsql.EnableLegacyTimestampBehavior` switch from `DatabaseSetup` or `ApplicationDbContextFactory`. `dotnet ef` builds the model through that factory.
- After any change to entities or EF configuration, run `dotnet ef migrations has-pending-model-changes --project src/Infrastructure` from `backend/`. It must report no changes, or a migration for those changes must exist. If it reports changes you did not intend, stop and report them instead of generating a migration.
- Never apply a scaffolded migration that changes the type of an existing date column. Converting between timestamp types needs a hand-written migration that uses `USING "<Column>" AT TIME ZONE 'Europe/Madrid'` and drops and recreates the views that read the column (PostgreSQL refuses to alter a column used by a view). See `UnifyDateColumnsToMadridLocalTime`.

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

Load the matching skill from `.claude/skills/` for specialized workflows:

- `adding-backend-entity`
- `frontend-crud`
- `frontend-form`
- `backend-localization`
- `audit-frontend-localization`
- `translate-frontend-view`
- `migrate-datatable-to-table`
- `contextual-help`

Skills provide decision procedures, not substitute source code. If a skill conflicts with current code, current code and verified project configuration win.

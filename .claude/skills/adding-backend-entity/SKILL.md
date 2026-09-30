---
name: adding-backend-entity
description: Add or extend a Lilith backend entity and its API flow. Use when creating domain entities, relationships, repositories, services, controllers, EF Core configuration, or an explicitly requested migration in the .NET backend.
compatibility: Requires .NET 10; PostgreSQL is required only for database updates.
---

# Add A Backend Entity

Build from current source, not copied templates.

## Discover

1. Read root `AGENTS.md` and inspect the target domain.
2. Find the closest current entity with similar ownership, relationships, and lifecycle behavior.
3. Trace its Domain, Application.Contracts, Application, Infrastructure, and Api flow.
4. Record which files and registrations are actually required. Do not assume every entity needs a custom repository or every write uses the same response contract.

## Implement

1. Add or update the Domain entity with explicit nullability and relationship ownership.
2. Add contracts only where consumers need them: repository, service, request/response model, or constants.
3. Put business rules and orchestration in the application service. Use async APIs for I/O.
4. Use `ILocalizationService` for user-facing responses; load `backend-localization` when adding keys or statuses.
5. Add repository queries only for entity-specific data access. Keep includes and tracking behavior intentional.
6. Add EF configuration using the naming and base configuration used by neighboring current entities.
7. Register the entity in Unit of Work, dependency injection, and model configuration only where current architecture requires explicit registration.
8. Add a thin controller that validates HTTP input and delegates to the service.

## Data Decisions

- IDs are application-generated GUIDs.
- Inspect the analogous entity before choosing physical deletion, `Disabled`, or a lifecycle transition.
- Read the live `StatusConstants.cs`; never reproduce its values in the skill or new documentation.
- Define cascade behavior deliberately for each relationship.
- Decide whether the entity is master data (see Master data deletion in `AGENTS.md`):
  - Configuration that documents refer to: implement `IMasterData` and add its name to `MasterDataCatalog.Names`.
  - Records deleted together with a master: declare them in `MasterDataCatalog.OwnedParts`.
  - A property that refers to master data without a foreign key: declare it in `MasterDataCatalog.ExtraReferences`, or better, add the foreign key.
  - A new entity that refers to master data: give it a `DocumentKind.*` entry in `MasterDataCatalog.DocumentKinds`, with the key in every backend culture.
  - `MasterDataDeleteGuardTests` must pass. Never write an in-use check in the service or repository.
- Date properties are `DateTime` in Europe/Madrid local time, stored as `timestamp without time zone`, and set with `DateTime.Now`. Never use `DateTimeOffset` or `timestamp with time zone` (see Dates and Time in `AGENTS.md`).
- Do not add backward compatibility unless a persisted or external consumer requires it.

## Migrations

Create or remove an EF migration only when the user explicitly requests it. Review generated `Up` and `Down` code before any database update. Never run `database update` without explicit approval.

A scaffolded migration must contain only the changes you made. If it also alters columns you did not touch, especially date column types, the model had already drifted: stop and report it instead of keeping those operations.

## Verify

From `backend/` run:

```bash
dotnet build
dotnet test
dotnet ef migrations has-pending-model-changes --project src/Infrastructure
```

The last command must report no changes, or a migration for those changes must exist.

Add focused tests for business rules when the existing test project can exercise them. Report separately if runtime or database verification was not requested.

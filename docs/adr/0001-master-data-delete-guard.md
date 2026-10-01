---
status: accepted
---

# Master data delete guard lives in the generic repository and is derived from the EF model

Most foreign keys that point at master data delete in cascade, so deleting a customer, tax or reference that documents still use would silently delete those documents. Each master used to guard itself with a hand-written `IsInUse` query in its repository, called from its service. A new document that referred to master data had to remember to extend every one of those queries, and forgetting one reopened the data loss.

We replaced them with one module. `Repository.Remove` and `RemoveRange` run `MasterDataDeleteGuard` for every entity marked `IMasterData`. The guard reads from the EF model which records refer to the entity and adds the declarations in `MasterDataCatalog`: owned parts deleted together with it, references without a foreign key, and the name of each kind of referring record. When anything outside the deleted set still refers to it, the guard throws `EntityInUseException`, and the API error middleware answers 409 with the localized reason.

## Considered options

- **An explicit list per entity, kept in one place.** Rejected: it keeps today's failure mode, because every new document still has to be added to every list by hand.
- **Letting the database refuse** (`Restrict` foreign keys, translating PostgreSQL error 23503). Kept as a safety net (#166), not as the guard. It stops only at the first violated constraint, so it can name one kind of document at most, and only after the delete reached the database.
- **An `EnsureNotInUse` call in every service before `Remove`.** Rejected: it depends on remembering the call, which is how guards went missing before.

## Consequences

- `Repository.Remove` can throw. A delete with other effects (files, related records) must remove the master data first, so a refusal happens before anything else changes. `EnterpriseService` and `WorkcenterService` are ordered this way.
- Only `Remove` and `RemoveRange` are guarded. `context.Remove`, `ExecuteDelete` and SQL bypass the guard, so master data must not be deleted that way.
- As the safety net for those paths, `MasterDataForeignKeys` turns every cascading foreign key from master data into `Restrict`, except those to owned parts, which keep cascading (migration `RestrictMasterDataForeignKeys`). A filtered owned part, such as empty stock, is restricted too, because a foreign key cannot tell empty stock from stock on hand, so `Repository.Remove` loads and deletes the owned parts itself. When `SaveChanges` hits one of them, `ApplicationDbContext` rethrows the PostgreSQL refusal as `EntityInUseException` naming the referring document kind; `ExecuteDelete` and SQL get the raw database error.
- Nothing detects a configuration entity that should implement `IMasterData` but does not. `AGENTS.md` and the `adding-backend-entity` skill carry that rule.
- `MasterDataDeleteGuardTests` fails when a record that can keep master data in use has no document kind, when an owned part does not match exactly one foreign key, or when a master has no name.

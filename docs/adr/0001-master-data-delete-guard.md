---
status: accepted
---

# Master data delete guard lives in the generic repository and is derived from the EF model

Most foreign keys that point at master data delete in cascade, so deleting a customer, tax or reference that documents still use would silently delete those documents. Each master used to guard itself with a hand-written `IsInUse` query in its repository, called from its service. A new document that referred to master data had to remember to extend every one of those queries, and forgetting one reopened the data loss.

We replaced them with one module. `Repository.Remove` and `RemoveRange` run `MasterDataDeleteGuard` for every entity marked `IMasterData`. The guard reads from the EF model which records refer to the entity and adds the declarations in `MasterDataCatalog`: owned parts deleted together with it, references without a foreign key, and the name of each kind of referring record. When anything outside the deleted set still refers to it, the guard throws `EntityInUseException`, and the API error middleware answers 409 with the localized reason.

## Considered options

- **An explicit list per entity, kept in one place.** Rejected: it keeps today's failure mode, because every new document still has to be added to every list by hand.
- **Letting the database refuse** (`Restrict` foreign keys, translating PostgreSQL error 23503). Kept as a later safety net (#166), not as the guard. It needs a migration over dozens of foreign keys, and it can only name a constraint, not the documents that use the record.
- **An `EnsureNotInUse` call in every service before `Remove`.** Rejected: it depends on remembering the call, which is how guards went missing before.

## Consequences

- `Repository.Remove` can throw. A delete with other effects (files, related records) must remove the master data first, so a refusal happens before anything else changes. `EnterpriseService` and `WorkcenterService` are ordered this way.
- Only `Remove` and `RemoveRange` are guarded. `context.Remove`, `ExecuteDelete` and SQL bypass the guard, so master data must not be deleted that way.
- Nothing detects a configuration entity that should implement `IMasterData` but does not. `AGENTS.md` and the `adding-backend-entity` skill carry that rule.
- `MasterDataDeleteGuardTests` fails when a record that can keep master data in use has no document kind, when an owned part does not match exactly one foreign key, or when a master has no name.

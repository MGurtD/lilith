# User Table View — Specification

## Purpose

Define the persistence and management contract for per-user named table view configurations in Lilith ERP. A view bundles a page identifier, column visibility/order/totals, optional filter values, and an optional sort configuration into a single JSON document that can be loaded, saved, shared (future), and made default for a user on a given page.

---

## Requirements

### Requirement: Per-User, Per-Page View CRUD

The system SHALL expose CRUD operations on `UserTableView` rows scoped to `(userId, page)`. Each view SHALL have a unique `Name` within that scope. The view SHALL be soft-deleted via the existing `Disabled` field (no physical deletion in normal flow).

#### Scenario: Create a new named view

- GIVEN the user is on page `"Budgets"`
- WHEN `Create({ UserId, Page: "Budgets", Name: "Vista Q1", ViewConfig: "..." })` is called
- THEN a new `UserTableView` row is persisted with `IsDefault=false`
- AND the view is returned in `GetByUserAndPage(userId, "Budgets")`

#### Scenario: Duplicate name on same page is rejected

- GIVEN a view named `"Vista Q1"` already exists for `(userId, "Budgets")`
- WHEN `Create(...)` is called with the same name
- THEN the operation returns a localized "TableViewNameExists" error
- AND no row is inserted

#### Scenario: Update preserves identity

- GIVEN an existing view with id `v1`
- WHEN `Update(v1, { Name: "Vista Q1 revised", ViewConfig: "..." })` is called
- THEN the row's `Name` and `ViewConfig` change but `Id`, `UserId`, `Page`, and `IsDefault` are preserved
- AND name uniqueness within the `(userId, page)` scope is re-validated

### Requirement: Default View Invariant

The system SHALL guarantee that for every `(userId, page)` tuple, AT MOST ONE `UserTableView` row has `IsDefault=true`. Setting a new default MUST atomically unset any existing default in the same scope.

#### Scenario: SetDefault unsets siblings

- GIVEN views `v1` (default) and `v2` (not default) exist for `(userId, "Budgets")`
- WHEN `SetDefault(v2.id, true)` is called
- THEN `v1.IsDefault` becomes `false`
- AND `v2.IsDefault` becomes `true`
- AND both updates are persisted

#### Scenario: Unsetting the only default leaves no default

- GIVEN `v1` is the only default for `(userId, "Budgets")`
- WHEN `SetDefault(v1.id, false)` is called
- THEN `v1.IsDefault` becomes `false`
- AND no other view is promoted

### Requirement: EnsureDefault — Idempotent Get-or-Create

The system SHALL expose `EnsureDefault(Guid userId, string page)` that returns the existing default view for the `(userId, page)` scope, or creates a new one if none exists. The contract MUST be idempotent: concurrent calls with the same `(userId, page)` MUST NOT produce duplicates.

#### Scenario: First call creates default

- GIVEN no view exists for `(userId, "Budgets")` with `IsDefault=true`
- WHEN `EnsureDefault(userId, "Budgets")` is called
- THEN a new `UserTableView` is created with `Name="Per defecte"`, `IsDefault=true`, `ViewConfig='{"columns":[]}'`
- AND the new view is returned

#### Scenario: Subsequent call returns existing

- GIVEN a default view `v1` already exists for `(userId, "Budgets")`
- WHEN `EnsureDefault(userId, "Budgets")` is called
- THEN `v1` is returned unchanged
- AND no new row is created

#### Scenario: Concurrent calls do not duplicate

- GIVEN no default exists yet for `(userId, "Budgets")`
- WHEN two concurrent `EnsureDefault` calls fire from the same client
- THEN the frontend in-flight promise cache returns the SAME promise to both callers
- AND on the backend, the first call creates the row and the second call returns it
- AND only ONE row exists in the database with `IsDefault=true` for that scope

### Requirement: ViewConfig Document Structure

The `ViewConfig` field SHALL store a JSON document with three top-level keys, each optional: `columns` (array of `{ field, visible?, order?, total? }`), `filters` (object of `{ [key: string]: unknown }`), `sort` (`{ field, order }` where order is `1` or `-1`). The system MUST preserve all three sections when updating one of them.

#### Scenario: Saving filters preserves columns and sort

- GIVEN an existing view with `ViewConfig = '{"columns":[{...}],"sort":{"field":"date","order":1}}'`
- WHEN the user triggers a row-click save with `{ customerId: "abc", search: "x" }`
- THEN the updated `ViewConfig` is `'{"columns":[{...}],"sort":{"field":"date","order":1},"filters":{"customerId":"abc","search":"x"}}'`
- AND the `columns` and `sort` sections are byte-for-byte identical to the previous document
# Filters Integration — UserTableView Spec

## Purpose

Extender `UserTableView` amb `FilterConfig` perqu cada vista guardi columnes + filtres en un sol concepte. Quan l'usuari carrega una vista, obt display + dades automticament. `UserFilter` queda deprecat per funcional per a vistes que encara no usen `UserTableView`.

---

## Functional Requirements

### FR-01: UserTableView Entity — FilterConfig Property

**What**: Add `FilterConfig?` (nullable string) to `UserTableView` entity to store filter state as JSON.

The system **MUST** support a new nullable `FilterConfig` property on `UserTableView` that stores serialized filter values (e.g., `{"dates":["2026-01-01","2026-12-31"],"customerId":"abc"}`).

| Field | Type | Nullable | Max Length | Description |
|-------|------|----------|------------|-------------|
| FilterConfig | string | Yes | 4000 | JSON-serialized filter values |

#### Scenario: Entity accepts null FilterConfig

- GIVEN a `UserTableView` instance
- WHEN `FilterConfig` is not set (null or empty)
- THEN the entity is valid and behaves as before (columns-only)

#### Scenario: Entity stores valid JSON filter config

- GIVEN a `UserTableView` instance
- WHEN `FilterConfig` contains valid JSON `{"dates":["2026-01-01"],"customerId":"xyz"}`
- THEN the entity persists and returns the JSON string on retrieval

---

### FR-02: Backend — Database Migration

**What**: Create EF Core migration to add `FilterConfig` column to `UserTableViews` table.

The system **MUST** add a nullable `varchar(4000)` column `FilterConfig` to `UserTableViews` without altering existing data.

#### Scenario: Migration runs without data loss

- GIVEN existing `UserTableViews` rows with `ColumnConfig` populated
- WHEN migration is applied
- THEN all existing rows remain intact with `FilterConfig` = NULL

#### Scenario: New rows support FilterConfig

- GIVEN the migration is applied
- WHEN a new `UserTableView` is inserted with `FilterConfig` set
- THEN the row is persisted with both `ColumnConfig` and `FilterConfig`

---

### FR-03: Backend — Service Layer Updates

**What**: Update `UserTableViewService` to handle `FilterConfig` in Create and Update operations.

The system **MUST** persist `FilterConfig` when creating or updating a view. The Update method **MUST** copy `FilterConfig` from the request to the existing entity.

| Operation | Behavior |
|-----------|----------|
| Create | Accept and persist `FilterConfig` from request |
| Update | Copy `FilterConfig` from request to existing entity |
| GetByUserAndPage | Return entity with `FilterConfig` |
| GetById | Return entity with `FilterConfig` |

#### Scenario: Create saves FilterConfig

- GIVEN a POST request to `/api/UserTableView` with `FilterConfig` set
- WHEN the service creates the view
- THEN the saved entity includes `FilterConfig`

#### Scenario: Update overwrites FilterConfig

- GIVEN an existing view with `FilterConfig = {"dates":["2025-01-01"]}`
- WHEN a PUT request updates with `FilterConfig = {"customerId":"abc"}`
- THEN the entity's `FilterConfig` is replaced with the new value

---

### FR-04: Frontend — UserTableView Type Extension

**What**: Add `filterConfig?: string` to the `UserTableView` TypeScript interface.

The system **MUST** extend the `UserTableView` interface to include an optional `filterConfig` field.

```typescript
export interface UserTableView {
  id: string;
  userId: string;
  page: string;
  name: string;
  isDefault: boolean;
  columnConfig: string;
  filterConfig?: string;  // NEW
}
```

#### Scenario: Type accepts undefined filterConfig

- GIVEN a `UserTableView` object from the API without `filterConfig`
- WHEN the frontend deserializes it
- THEN `filterConfig` is `undefined` and no type error occurs

---

### FR-05: Frontend — TableViewConfig Save Includes Filter Values

**What**: When saving or updating a view, `TableViewConfig` **MUST** serialize the current `filterValues` prop into `filterConfig`.

The component **MUST** accept a new `filterValues` prop and include it in both `saveView()` and `saveAsNewView()` operations.

#### Scenario: Save existing view includes current filters

- GIVEN a user has applied filters `{customerId: "abc", dates: [...]}`
- WHEN they open TableViewConfig and click "Desar canvis"
- THEN the saved view includes `filterConfig` with the current filter values

#### Scenario: Create new view includes current filters

- GIVEN a user has applied filters
- WHEN they open TableViewConfig and click "Crear nova"
- THEN the new view is created with both `columnConfig` and `filterConfig`

---

### FR-06: Frontend — TableViewConfig Load Applies Filters

**What**: When a view with `filterConfig` is selected, `TableViewConfig` **MUST** deserialize and emit `update:filterValues` with the stored filters.

The component **MUST** emit a new event `apply-filters` (or extend `apply-config`) when the selected view contains `filterConfig`.

#### Scenario: Selecting view with filterConfig restores filters

- GIVEN a saved view has `filterConfig = {"customerId":"abc"}`
- WHEN the user selects this view in the dropdown
- THEN `TableViewConfig` emits the filter values to the parent

#### Scenario: Selecting view without filterConfig does nothing

- GIVEN a saved view has `filterConfig` = null or empty
- WHEN the user selects this view
- THEN no filter event is emitted (columns-only behavior unchanged)

---

### FR-07: Frontend — Table.vue Coordinates Default View Filter Loading

**What**: When `loadDefaultView()` finds a default view with `filterConfig`, `Table.vue` **MUST** emit `update:filterValues` + `filter` to trigger data fetch.

The component **MUST** check `filterConfig` on the default view and, if present, deserialize and emit filter restoration after column application.

#### Scenario: Default view with filters auto-applies on mount

- GIVEN the user has a default view with `filterConfig` set
- WHEN `Table.vue` mounts and calls `loadDefaultView()`
- THEN the component emits `update:filterValues` with stored filters AND emits `filter` event

#### Scenario: Default view without filters loads columns only

- GIVEN the default view has no `filterConfig`
- WHEN `Table.vue` mounts
- THEN only columns are applied (current behavior preserved)

---

### FR-08: Frontend — Table.vue Passes Current Filters to TableViewConfig

**What**: `Table.vue` **MUST** pass the current `filterValues` to `TableViewConfig` so save operations capture active filters.

The component **MUST** bind `filterValues` to the `TableViewConfig` dialog via a new prop.

#### Scenario: Dialog receives current filter state

- GIVEN the user has active filters on the table
- WHEN they open the TableViewConfig dialog
- THEN the dialog receives `filterValues` as a prop for serialization on save

---

### FR-09: Frontend — Budgets.vue Removes UserFilterStore Logic

**What**: `Budgets.vue` **MUST** remove `useUserFilterStore` import, `onUnmounted` save, and `onMounted` restore logic. Filter persistence is now handled by `UserTableView` via `Table.vue`.

The component **MUST** rely on `Table.vue`'s default view loading for filter restoration.

#### Scenario: Budgets loads without UserFilterStore

- GIVEN `Budgets.vue` no longer imports or uses `useUserFilterStore`
- WHEN the page mounts
- THEN filters are restored via `Table.vue`'s default view mechanism

#### Scenario: Budgets unmount does not trigger auto-save

- GIVEN `Budgets.vue` has no `onUnmounted` filter save
- WHEN the user navigates away
- THEN no `UserFilter` API call is made (filters saved explicitly via view save)

---

### FR-10: Backward Compatibility — UserFilter Continues Working

**What**: `UserFilter` entity, API, and store **MUST** remain functional for views that do not yet use `UserTableView`.

The system **MUST NOT** break existing `UserFilter` consumers (Workorders, SalesOrders, PurchaseInvoices, etc.) during this change.

#### Scenario: Non-TableView views still save/load filters

- GIVEN a view like `Workorders.vue` that uses `UserFilterStore`
- WHEN the user navigates to that page
- THEN filters are still saved on unmount and restored on mount via `UserFilter`

---

### FR-11: Backward Compatibility — Views Without FilterConfig Load Gracefully

**What**: Views created before this feature (no `filterConfig` column data) **MUST** load without errors, applying only column configuration.

The system **MUST** handle null/empty/missing `filterConfig` gracefully at every layer (backend entity, frontend type, store `applyView`, component load).

#### Scenario: Old view loads without filterConfig

- GIVEN a `UserTableView` created before this feature with only `ColumnConfig`
- WHEN the view is loaded by `Table.vue`
- THEN columns are applied and no filter event is emitted (no errors)

#### Scenario: Invalid JSON in filterConfig is ignored

- GIVEN a view has `filterConfig` with malformed JSON
- WHEN the view is loaded
- THEN the error is caught silently and columns are applied without filters

---

## Non-Functional Requirements

| ID | Requirement |
|----|-------------|
| NFR-01 | **Performance**: Filter deserialization on view load **MUST NOT** add measurable latency (< 5ms for JSON parse of typical filter payload) |
| NFR-02 | **Type Safety**: Frontend `pnpm run typecheck` **MUST** pass with zero errors after all changes |
| NFR-03 | **Data Integrity**: Migration **MUST NOT** modify or delete existing `UserTableViews` or `UserFilters` data |
| NFR-04 | **API Contract**: Existing API responses **MUST** remain backward compatible — `filterConfig` is optional, clients ignoring it continue working |
| NFR-05 | **Size Budget**: Total spec artifact under 650 words (excluding tables and code blocks) |

---

## User Scenarios

### SC-01: Save View With Filters → Restore on Reload

- GIVEN a user is on Budgets with filters `{dates: [Jan-Dec 2026], customerId: "ABC Corp"}`
- AND they have a saved view "Q1 Review"
- WHEN they open TableViewConfig, verify filters are active, and click "Desar canvis"
- THEN the view saves with both column config and filter config
- AND on page reload, the default view restores columns AND filters
- AND the table automatically fetches data with those filters

### SC-02: Change Filters Without Saving → Default View Restores Saved Filters

- GIVEN a user has a default view "My View" with saved filters `{customerId: "XYZ"}`
- WHEN they change filters to `{customerId: "ABC"}` without saving the view
- AND they refresh the page
- THEN the default view loads with `{customerId: "XYZ"}` (the saved filters, not the unsaved changes)

### SC-03: Manually Load View With Filters → Table Updates

- GIVEN a user has a saved view "All Clients" with filters `{dates: [2025-01-01, 2025-12-31]}`
- WHEN they open TableViewConfig and select "All Clients" from the dropdown
- THEN columns update to the view's configuration
- AND filters are restored to `{dates: [2025-01-01, 2025-12-31]}`
- AND the table fetches data with the new filters

### SC-04: Old View Without FilterConfig → Loads Columns Only

- GIVEN a view "Compact" was created before this feature (no `filterConfig`)
- WHEN the user selects "Compact" in TableViewConfig
- THEN only column configuration is applied
- AND no filter changes occur
- AND no errors are thrown

### SC-05: Create New View With Current Filters

- GIVEN a user has filters `{statusId: "pending", customerId: "ABC"}` applied
- WHEN they open TableViewConfig, enter a name "Pending ABC", and click "Crear nova"
- THEN a new view is created with both column config and filter config
- AND the view appears in the dropdown list

### SC-06: Update Existing View Overwrites Both Columns and Filters

- GIVEN a view "Monthly" has saved filters `{dates: [Jan 2025, Jan 2025]}`
- WHEN the user changes filters to `{dates: [Feb 2025, Feb 2025]}` and clicks "Desar canvis"
- THEN the view's `filterConfig` is overwritten with the new dates
- AND the old filter values are permanently replaced

---

## Acceptance Criteria

| ID | Criterion | Verification |
|----|-----------|-------------|
| AC-01 | Backend API returns `filterConfig` in GET responses for `UserTableView` | Swagger test: POST view with `filterConfig`, GET returns it |
| AC-02 | Backend accepts `filterConfig` in POST and PUT requests | Swagger test: create/update with `filterConfig`, verify DB |
| AC-03 | EF Core migration adds `FilterConfig` column without data loss | `dotnet ef database update` succeeds, existing rows intact |
| AC-04 | Frontend TypeScript typecheck passes with zero errors | `pnpm run typecheck` exits 0 |
| AC-05 | `Budgets.vue` works without `UserFilterStore` imports or calls | Grep: no `useUserFilterStore` in Budgets.vue; page loads correctly |
| AC-06 | Page reload restores default view with saved filters | Manual test: save view with filters, reload, verify filters + data |
| AC-07 | Old views without `filterConfig` continue working | Manual test: load pre-existing view, verify columns apply, no errors |
| AC-08 | `UserFilter` API still works for non-TableView views | Manual test: navigate to Workorders, verify filter save/load works |

---

## Exclusions

The following are explicitly **OUT OF SCOPE** for this change:

1. **Data migration** from `UserFilter` to `UserTableView` — existing `UserFilter` data stays as-is
2. **Physical deletion** of `UserFilter` table, entity, or API — deprecation only
3. **Multi-key filter support** — each view stores one flat filter object (same structure as current `UserFilter.filter`)
4. **Auto-save filters on navigation** — filters are saved explicitly when user saves/updates a view (no `onUnmounted` auto-save for TableView pages)
5. **Filter config validation** beyond JSON parse — no schema validation of filter keys/values
6. **Migration of other views** (Workorders, SalesOrders, etc.) from `UserFilter` to `UserTableView` — those remain on `UserFilter` for now

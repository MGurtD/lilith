# Delta for table-component

## ADDED Requirements

### Requirement: Auto-Provision Default View on Mount

The `Table.vue` component MUST, when its `page` prop is set and the authenticated user is available, call `useUserTableViewStore.ensureDefault(userId, page)` BEFORE `loadDefaultView()`. The call SHALL be awaited so the default view is guaranteed to exist before any view-merging logic runs.

#### Scenario: First visit to a Table page creates a default view

- GIVEN a user navigates to a page containing `<Table :page="Budgets">`
- AND no `UserTableView` row exists for `(userId, "Budgets")`
- WHEN the component mounts
- THEN the system creates a `UserTableView` row with `Name="Per defecte"`, `IsDefault=true`, `ViewConfig='{"columns":[]}'`
- AND `loadDefaultView()` then loads it as the active view

#### Scenario: Subsequent visit reuses existing default view

- GIVEN a `UserTableView` default row already exists for `(userId, "Budgets")`
- WHEN the component mounts on a subsequent visit
- THEN `ensureDefault` returns the existing row WITHOUT creating a duplicate
- AND the row count for `(userId, "Budgets", IsDefault=true)` stays at 1

#### Scenario: No auto-provision when `page` prop is absent

- GIVEN a `<Table>` is rendered WITHOUT the `page` prop
- WHEN the component mounts
- THEN `ensureDefault` is NOT called
- AND no `UserTableView` row is created or fetched

### Requirement: Row-Click Event Passthrough

The `Table.vue` component MUST forward PrimeVue's `rowClick` event as a new `@row-click` emit when the `page` prop is set. The event payload SHALL be the raw `DataTableRowClickEvent` so consumers retain full access to `event.data`, `event.originalEvent`, etc.

#### Scenario: Consumer wires navigation on row click

- GIVEN a view uses `<Table :page="Budgets" @row-click="editRow">`
- WHEN the user clicks any data row
- THEN `editRow(event)` fires with the full `DataTableRowClickEvent`
- AND the consumer's navigation handler runs as before

#### Scenario: Without `page` prop, row-click is not auto-emitted

- GIVEN a `<Table>` without `page` prop
- WHEN the user clicks a row
- THEN the component does NOT emit its own `@row-click`
- AND PrimeVue's native event flows through `v-bind="$attrs"` as today

### Requirement: Filter Dirty Tracking

The `Table.vue` component MUST maintain a `filtersDirty: boolean` ref. The flag SHALL be set to `true` when the embedded `TableFilter` emits `@filter` or `@clear`. The flag SHALL be set to `false` after a successful `saveFiltersToDefault` call AND on initial mount (before any user interaction).

#### Scenario: User applies a filter marks dirty

- GIVEN the user types a search term and clicks the "Filtrar" button
- WHEN the embedded TableFilter emits `@filter`
- THEN `filtersDirty.value` becomes `true`

#### Scenario: Successful save clears dirty

- GIVEN `filtersDirty.value === true`
- WHEN `saveFiltersToDefault()` resolves successfully
- THEN `filtersDirty.value` becomes `false` in `.finally()`

#### Scenario: Failed save also clears dirty

- GIVEN `filtersDirty.value === true`
- WHEN `saveFiltersToDefault()` rejects (network error)
- THEN `filtersDirty.value` becomes `false` in `.finally()`
- AND the error is logged but NOT surfaced to the user (non-critical operation)

### Requirement: Save Filters on Row Click When Dirty

The `Table.vue` component MUST, when `@row-click` fires, invoke `useUserTableViewStore.saveFiltersToDefault(userId, page, filterValues)` IF AND ONLY IF `filtersDirty.value === true` AND a default view exists (`activeViewId.value !== ""`). The save SHALL be fire-and-forget (no `await`) so navigation is not blocked.

#### Scenario: Dirty row click saves filters to default view

- GIVEN the user has applied a filter (dirty=true)
- AND the default view is active
- WHEN the user clicks a row
- THEN `saveFiltersToDefault` is called with the current `filterValues`
- AND the navigation triggered by the consumer's handler proceeds without delay

#### Scenario: Clean row click does NOT save

- GIVEN the user has NOT applied any filter (dirty=false)
- WHEN the user clicks a row
- THEN `saveFiltersToDefault` is NOT called
- AND no PUT request is sent to the backend

#### Scenario: No default view skips save

- GIVEN `activeViewId.value === ""` (e.g., user deleted the default)
- WHEN the user clicks a row with dirty filters
- THEN `saveFiltersToDefault` is NOT called
- AND the row click navigation still works
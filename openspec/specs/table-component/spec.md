# Table Component — Specification

## Purpose

Define the enhanced `Table.vue` component as the universal data-presentation layer for Lilith ERP frontend. The component must support preset configurations that eliminate duplication of PrimeVue DataTable props across ~115 views, while remaining backward-compatible with the single existing consumer (`Budgets.vue`).

---

## 1. Functional Requirements

### Requirement: Preset System

The system SHALL provide a `preset` prop on `Table.vue` that applies a named set of default DataTable properties. Each preset encapsulates a common usage pattern discovered in the codebase.

#### Scenario: Developer applies `preset="crud-list"`

- GIVEN a developer is building a standard CRUD list view (paginator, scrollable, striped rows)
- WHEN they use `<Table preset="crud-list" ...>`
- THEN the DataTable renders with `selectionMode="single"`, `paginator=true`, `rows=20`, `scrollable=true`, `scrollHeight="flex"`, `stripedRows=true`, `rowHover=true`
- AND the developer MAY override any preset default by passing the prop explicitly (e.g., `:rows="50"` overrides the preset's `rows=20`)

#### Scenario: Developer applies `preset="read-only"`

- GIVEN a developer is building a read-only reference table (no paginator, compact)
- WHEN they use `<Table preset="read-only" ...>`
- THEN the DataTable renders with `paginator=false`, `stripedRows=true`, `rowHover=true`, no scrollHeight

#### Scenario: Developer applies `preset="detail-lines"`

- GIVEN a developer is building a nested detail-lines table inside a detail view
- WHEN they use `<Table preset="detail-lines" ...>`
- THEN the DataTable renders with `paginator=false`, `scrollable=true`, `scrollHeight="40vh"`, `stripedRows=true`, `rowHover=true`

#### Scenario: Developer applies `preset="selector"`

- GIVEN a developer is building a row-selection dialog (pick one item from a list)
- WHEN they use `<Table preset="selector" ...>`
- THEN the DataTable renders with `selectionMode="single"`, `paginator=true`, `rows=10`, `scrollable=true`, `scrollHeight="50vh"`

#### Scenario: Preset defaults are overridable

- GIVEN a developer uses `<Table preset="crud-list" :rows="50" stripedRows>`
- WHEN the component renders
- THEN `rows=50` (explicit override wins) and `stripedRows=true` (explicit prop wins over preset)

### Requirement: Passthrough Props

The system SHALL expose the following props on `Table.vue` that pass through to the underlying PrimeVue DataTable, enabling features currently only available via `v-bind="attrs"`:

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `loading` | `boolean` | `false` | Shows DataTable loading overlay |
| `dataKey` | `string` | `"id"` | Key field for row identification |
| `stripedRows` | `boolean` | `false` | Alternating row background colors |
| `rowHover` | `boolean` | `false` | Row hover highlight effect |
| `selectionMode` | `"single" \| "multiple"` | `undefined` | Row selection mode (passes to DataTable) |
| `rowGroupMode` | `"rowspan" \| "subheader" \| "subfooter"` | `undefined` | Row grouping mode |
| `expandedRows` | `any[] \| null` | `null` | Controlled expanded rows for row expansion |

#### Scenario: Loading state displays overlay

- GIVEN a list view is fetching data from the API
- WHEN the developer binds `:loading="store.isLoading"` to `<Table>`
- THEN the DataTable shows its native loading overlay
- AND when loading completes, the overlay disappears

#### Scenario: dataKey enables row operations

- GIVEN a table needs row expansion or selection
- WHEN the developer sets `dataKey="id"` (or accepts default)
- THEN PrimeVue can correctly identify rows for selection/expansion operations

#### Scenario: stripedRows and rowHover improve readability

- GIVEN a developer passes `stripedRows` and `rowHover` to `<Table>`
- WHEN the table renders with multiple rows
- THEN alternating rows have different background colors AND rows highlight on hover

### Requirement: Backward Compatibility

The system SHALL maintain identical behavior for all existing `Table.vue` consumers after the change. New props MUST have defaults that match current behavior. The `preset` prop MUST be optional and MUST NOT affect existing consumers that do not use it.

#### Scenario: Budgets.vue works unchanged

- GIVEN `Budgets.vue` currently uses `<Table>` with `selectionMode="single"`, `scrollable`, `scrollHeight="flex"`, `paginator`, `:rows="20"`, `sort-field`, `sort-mode`, `sort-order`
- WHEN the change is applied and no modifications are made to `Budgets.vue`
- THEN the table renders identically: same columns, same sorting, same pagination, same delete behavior
- AND all existing slots (`#prepend`, `#body-date`, `#body-_customer`, etc.) continue to work

#### Scenario: Existing emits continue to fire

- GIVEN a consumer listens to `@filter`, `@clear`, `@create`, `@delete`, `@update:filterValues`
- WHEN the corresponding user action occurs
- THEN the emit fires with the same payload shape as before

### Requirement: Column Interface Stability

The system SHALL NOT remove or rename any existing field on the `Column` interface. New fields MAY be added if required by preset capabilities.

#### Scenario: Existing column definitions remain valid

- GIVEN a view defines `columns` as `ref<Column[]>([{ field: "number", header: "Número" }, ...])`
- WHEN the change is applied
- THEN the TypeScript type checker accepts the definition without modification
- AND `visible` and `order` fields continue to function for user table views

---

## 2. Interface Definitions

### Updated `Table.vue` Props

```typescript
export type TablePreset = "crud-list" | "read-only" | "detail-lines" | "selector";

const props = withDefaults(
  defineProps<{
    // Existing props
    columns: Column[];
    items: readonly any[];
    filterConfig?: FilterConfig[];
    filterValues?: any;
    filterBodyWidth?: FilterBodyWidth;
    showFilters?: boolean;
    page?: string;
    showDeleteColumn?: boolean;
    canDelete?: (item: any) => boolean;
    // New props
    preset?: TablePreset;
    loading?: boolean;
    dataKey?: string;
    stripedRows?: boolean;
    rowHover?: boolean;
    selectionMode?: "single" | "multiple";
    rowGroupMode?: "rowspan" | "subheader" | "subfooter";
    expandedRows?: any[] | null;
  }>(),
  {
    showFilters: true,
    loading: false,
    dataKey: "id",
    stripedRows: false,
    rowHover: false,
    expandedRows: null,
  },
);
```

### Preset Defaults Map

| Property | `crud-list` | `read-only` | `detail-lines` | `selector` |
|----------|-------------|-------------|----------------|------------|
| `paginator` | `true` | `false` | `false` | `true` |
| `rows` | `20` | — | — | `10` |
| `scrollable` | `true` | `false` | `true` | `true` |
| `scrollHeight` | `"flex"` | — | `"40vh"` | `"50vh"` |
| `stripedRows` | `true` | `true` | `true` | `false` |
| `rowHover` | `true` | `true` | `true` | `true` |
| `selectionMode` | — | — | — | `"single"` |

Properties marked `—` are not set by the preset (DataTable default applies).

### Emits Contract (Unchanged)

```typescript
const emit = defineEmits<{
  (e: "update:filterValues", value: any): void;
  (e: "filter"): void;
  (e: "clear"): void;
  (e: "create"): void;
  (e: "delete", item: any): void;
}>();
```

No new emits are added in this change.

---

## 3. Pilot Migration

### Requirement: Single Pilot View Migration

The system SHALL migrate exactly one pilot view from raw `<DataTable>` to the enhanced `<Table>` component, validating that the preset system and new props work correctly in production code.

#### Scenario: Pilot view uses `preset="crud-list"`

- GIVEN a medium-complexity CRUD list view currently uses raw `<DataTable>` with `paginator`, `scrollable`, `scrollHeight="flex"`, `:rows="20"`, `stripedRows`, `rowHover`
- WHEN the view is migrated to `<Table preset="crud-list">`
- THEN the view renders with identical columns, filters, sorting, and delete actions
- AND the view code is reduced by eliminating redundant DataTable prop declarations

#### Scenario: Pilot view delete action works identically

- GIVEN the pilot view has a delete column with conditional delete logic
- WHEN the developer uses `<Table>` with `showDeleteColumn` and `:canDelete="..."`
- THEN the delete icon renders, the click handler fires, and the confirm dialog appears identically

---

## 4. Acceptance Criteria

### Type Safety
- [ ] `pnpm run typecheck` passes with zero errors
- [ ] `TablePreset` type is exported and usable by consumers
- [ ] All new props have correct TypeScript types and defaults
- [ ] `Column` interface is backward-compatible (no breaking changes)

### Existing Consumer Regression
- [ ] `Budgets.vue` renders identically without any code changes
- [ ] Budgets pagination, sorting, delete column, and filter bar all work as before
- [ ] User table view config button (cog icon) still appears when `page` prop is set

### Pilot View
- [ ] Pilot view migrated from raw `<DataTable>` to `<Table preset="crud-list">`
- [ ] Pilot view columns, filters, and sorting match pre-migration behavior
- [ ] Pilot view delete action works with `showDeleteColumn` + `canDelete`
- [ ] Pilot view slot passthrough works (body slots, prepend, append)

### Preset System
- [ ] `preset="crud-list"` applies paginator, scrollHeight="flex", rows=20, stripedRows, rowHover
- [ ] `preset="read-only"` applies stripedRows, rowHover, no paginator
- [ ] `preset="detail-lines"` applies scrollable, scrollHeight="40vh", stripedRows, rowHover
- [ ] `preset="selector"` applies selectionMode="single", paginator, rows=10, scrollHeight="50vh"
- [ ] Explicit props override preset defaults

### New Props
- [ ] `loading` prop shows/hides DataTable loading overlay
- [ ] `dataKey` defaults to `"id"` and passes through to DataTable
- [ ] `stripedRows` and `rowHover` pass through to DataTable
- [ ] `selectionMode` passes through and enables row selection
- [ ] `expandedRows` passes through for row expansion support

---

## 5. Exclusions (Out of Scope)

- Bulk migration of the remaining ~115 DataTable usages (follow-up changes)
- Changes to `UserTableView` entity, `TableViewConfig.vue`, or `useUserTableViewStore`
- Any backend modifications or new API endpoints
- Changes to `TableFilter.vue` component
- Changes to views that already use `Table.vue` (only `Budgets.vue` exists)
- New slot additions beyond what the pilot migration requires
- Column resize persistence or column width presets
- Dashboard or analytics table patterns (separate preset if needed later)

---

## 6. Dependencies

### Frontend
- PrimeVue 4 DataTable API (already in use)
- Existing `Table.vue`, `TableFilter.vue`, `TableViewConfig.vue` components
- No new npm dependencies required

### Cross-cutting
- Preset defaults must not conflict with props passed through `v-bind="attrs"`
- Preset application order: preset defaults → explicit props → attrs passthrough

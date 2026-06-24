# Design: Table Component Evolution

## Technical Approach

Enhance `Table.vue` with a preset-driven prop resolution system. A `preset` prop injects DataTable defaults for common patterns; explicit props override preset defaults; `attrs` passthrough provides the final override layer. One pilot view (`SalesOrders.vue`) migrates from raw `<DataTable>` to `<Table preset="crud-list">`, validating the system end-to-end while `Budgets.vue` continues working unchanged.

## Architecture Decisions

### Decision: Prop defaults as `undefined` to enable preset override detection

| Option | Tradeoff | Decision |
|--------|----------|----------|
| `withDefaults` with `false`/`0` defaults | Can't distinguish "user passed false" from "not passed" — preset always overridden | Rejected |
| All new props default `undefined`; resolve via computed | Requires `??` chain per prop but clean override semantics | **Chosen** |

**Rationale**: Vue's `withDefaults` merges user-supplied and default values before the component sees them. For `preset="crud-list"` to set `stripedRows=true`, we must detect when the user didn't explicitly pass `stripedRows`. Only `undefined` defaults make this detectable. Final resolved values fall back to spec-stated defaults (`false`, `"id"`, `null`).

### Decision: Make `paginator`, `rows`, `scrollable`, `scrollHeight` explicit props

| Option | Tradeoff | Decision |
|--------|----------|----------|
| Keep as attrs passthrough only | Presets can't control them — breaks the core feature | Rejected |
| Add as explicit props with `undefined` defaults | 4 more props, but presets work correctly; attrs no longer carries them | **Chosen** |
| Inject preset values into attrs object | Hacky, fragile, violates Vue's prop extraction contract | Rejected |

**Rationale**: The preset defaults map in the spec controls these 4 properties. If they stay in `attrs`, we can't distinguish user intent from absence. Declaring them as props extracts them from `attrs` automatically (Vue 3 behavior), giving us clean resolution: preset → explicit prop → attrs (for remaining passthrough like `sortField`, `class`, `tableStyle`).

### Decision: Add `style` field to `Column` interface

| Option | Tradeoff | Decision |
|--------|----------|----------|
| Don't add — use attrs for column widths | No way to set per-column widths via the `columns` prop; pilot can't migrate width styles | Rejected |
| Add `style?: string` to `Column` | One new optional field; backward-compatible; enables width control per column | **Chosen** |

**Rationale**: All raw DataTable views set column widths via `<Column style="width: 10%">`. Without a `style` field, migrated views lose column widths. The field is optional and additive — no existing consumer breaks.

### Decision: Pilot view is `SalesOrders.vue`

| Candidate | Filters | Delete col | Body slots | Complexity | Match |
|-----------|---------|------------|------------|------------|-------|
| `SalesOrders.vue` | 3 (period, customer, status) | Yes (conditional) | 3 (date, expectedDate, status) | Medium | Perfect `crud-list` |
| `DeliveryNotes.vue` | 2 (period, customer) | Yes | 3 | Lower | Good but simpler |
| `PurchaseInvoices.vue` | 5 (period, supplier, payment, account, due) | Yes | 4 + footer | High | Too complex for pilot |

**Rationale**: SalesOrders uses exactly the `crud-list` pattern (`paginator`, `scrollable`, `scrollHeight="flex"`, `:rows="20"`), has the same filter structure as Budgets.vue (the reference consumer), and is in the same domain module (sales) for easy pattern comparison.

## Data Flow

```
Developer writes: <Table preset="crud-list" :rows="50" sort-field="num" @row-click="edit">
                                     │                │            │           │
                                     ▼                ▼            ▼           ▼
                              Preset defaults   Explicit props   Attrs     Events
                              (crud-list map)   (rows=50)     (sortField) (onRowClick)
                                     │                │            │           │
                                     ▼                ▼            ▼           ▼
                            resolvedDataTableProps (computed: preset → explicit → attrs)
                                     │
                                     ▼
                            <DataTable showGridlines v-bind="resolvedDataTableProps" :value="items">
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `frontend/src/components/tables/Table.vue` | Modify | Add 12 new props, preset map, resolution computed, `style` on Column render, merged `v-bind` |
| `frontend/src/modules/sales/views/SalesOrders.vue` | Modify | Migrate from raw `<DataTable>` to `<Table preset="crud-list">` with columns array |
| `frontend/src/components/tables/Table.vue` (Column interface) | Modify | Add `style?: string` field |

**No new files. No deleted files. No backend changes.**

### Estimated Line Impact

| File | Current | After | Delta |
|------|---------|-------|-------|
| `Table.vue` | 319 | ~370 | +51 (props, preset map, computed, Column style) |
| `SalesOrders.vue` | 318 | ~300 | -18 (template shrinks; columns array + canDelete offset removed DataTable boilerplate) |
| **Total** | | | **+33 lines** |

## Interfaces / Contracts

### Updated `Column` interface

```typescript
export interface Column {
  field: string;
  header: string;
  sortable?: boolean;
  total?: Aggregation;
  totalFormat?: (value: number) => string;
  visible?: boolean;
  order?: number;
  style?: string;  // NEW — CSS style string for column width, e.g. "width: 10%"
}
```

### `TablePreset` type

```typescript
export type TablePreset = "crud-list" | "read-only" | "detail-lines" | "selector";
```

### New props (all default `undefined` for override detection)

```typescript
// Added to defineProps<{...}>():
preset?: TablePreset;
loading?: boolean;
dataKey?: string;
stripedRows?: boolean;
rowHover?: boolean;
selectionMode?: "single" | "multiple";
rowGroupMode?: "rowspan" | "subheader" | "subfooter";
expandedRows?: any[] | null;
paginator?: boolean;
rows?: number;
scrollable?: boolean;
scrollHeight?: string;
```

No `withDefaults` entries for new props — all default to `undefined`.

### Preset defaults map

```typescript
const PRESET_DEFAULTS: Record<TablePreset, Record<string, unknown>> = {
  "crud-list": {
    paginator: true, rows: 20, scrollable: true,
    scrollHeight: "flex", stripedRows: true, rowHover: true,
  },
  "read-only": {
    paginator: false, stripedRows: true, rowHover: true,
  },
  "detail-lines": {
    paginator: false, scrollable: true,
    scrollHeight: "40vh", stripedRows: true, rowHover: true,
  },
  "selector": {
    selectionMode: "single", paginator: true, rows: 10,
    scrollable: true, scrollHeight: "50vh",
  },
};
```

### Resolution computed

```typescript
const resolvedDataTableProps = computed(() => {
  const preset = props.preset ? PRESET_DEFAULTS[props.preset] : {};
  const explicit: Record<string, unknown> = {};
  // Only include explicitly-set props (undefined = not set by user)
  if (props.loading !== undefined) explicit.loading = props.loading;
  if (props.dataKey !== undefined) explicit.dataKey = props.dataKey;
  if (props.stripedRows !== undefined) explicit.stripedRows = props.stripedRows;
  if (props.rowHover !== undefined) explicit.rowHover = props.rowHover;
  if (props.selectionMode !== undefined) explicit.selectionMode = props.selectionMode;
  if (props.rowGroupMode !== undefined) explicit.rowGroupMode = props.rowGroupMode;
  if (props.expandedRows !== undefined) explicit.expandedRows = props.expandedRows;
  if (props.paginator !== undefined) explicit.paginator = props.paginator;
  if (props.rows !== undefined) explicit.rows = props.rows;
  if (props.scrollable !== undefined) explicit.scrollable = props.scrollable;
  if (props.scrollHeight !== undefined) explicit.scrollHeight = props.scrollHeight;
  return { ...preset, ...explicit, ...attrs };
});
```

Final defaults (when no preset and no explicit prop): `loading=false`, `dataKey="id"`, `expandedRows=null`, all others defer to PrimeVue DataTable's built-in defaults.

## Slot Strategy

### Currently forwarded slots (unchanged)

| Slot | Target | Status |
|------|--------|--------|
| `prepend` | TableFilter `#prepend` | Keep |
| `append` | TableFilter `#append` | Keep |
| `filter-{name}` | TableFilter dynamic filter slots | Keep |
| `body-{field}` | DataTable Column `#body` | Keep |
| `footer-{field}` | DataTable ColumnGroup `#footer` | Keep |
| `empty` | DataTable `#empty` | Keep |
| `loading` | DataTable `#loading` | Keep |
| `paginatorstart` | DataTable `#paginatorstart` | Keep |
| `paginatorend` | DataTable `#paginatorend` | Keep |

### No new slots needed for pilot

SalesOrders uses `#prepend` (filters), `#body-date`, `#body-expectedDate`, and `#body-statusId` — all already supported. Delete handled by `showDeleteColumn` + `canDelete` (no slot needed).

### Custom slots continue working

`body-{field}`, `footer-{field}`, and `filter-{name}` use dynamic slot forwarding via `v-for` over `slots`. Adding new props does not affect this mechanism.

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| TypeScript | All new types compile; `TablePreset` exported; Column interface backward-compatible | `pnpm run typecheck` — zero errors |
| Visual (Budgets.vue) | Pagination, sorting, delete column, filter bar, body slots, view config cog | Manual browser verification — must be identical to pre-change |
| Visual (SalesOrders.vue) | Table renders with correct columns, widths, row click navigation, delete, filters | Manual browser verification — compare side-by-side with git stash |
| Preset resolution | `crud-list` applies correct defaults; explicit `:rows="50"` overrides preset | Browser dev tools — inspect rendered DataTable props |

### Manual Verification Checklist for Pilot

1. **Budgets.vue unchanged**: Navigate to `/budgets`. Verify: filters work, period picker works, rows paginate at 20, sorting works, delete icon appears for initial-status rows, cog icon opens view config, clicking row navigates to detail.
2. **SalesOrders.vue after migration**: Navigate to `/salesorders`. Verify: same columns in same order, same widths, period+customer+status filters work, rows paginate at 20, scrolling works, row click navigates to detail, delete icon appears for initial-status rows, confirm dialog fires, sort by column headers.
3. **Preset override test**: Temporarily add `:rows="50"` to SalesOrders' `<Table>`. Verify table paginates at 50 instead of 20.

## Migration / Rollout

### Pilot Migration Steps (SalesOrders.vue)

1. Add `columns` ref with `Column[]` array (field, header, sortable, style)
2. Replace `<DataTable>` opening tag with `<Table preset="crud-list">`
3. Remove redundant DataTable props (`scrollable`, `scrollHeight`, `paginator`, `:rows="20"`) — handled by preset
4. Move filter fields from nested `<TableFilter #prepend>` to `<Table #prepend>` — same template content
5. Add `:filter-config="[]"`, `v-model:filter-values="filter"`, `:filter-body-width="filterBodyWidth"`
6. Convert inline `<Column>` elements to `columns` array entries; move body slots to `#body-{field}` pattern
7. Add `showDeleteColumn` and `:canDelete="(item) => item.statusId === lifecycleStore.lifecycle?.initialStatusId"`
8. Add `@delete="deleteSalesInvoice"` emit handler (Table's built-in delete, not custom column)
9. Remove `TableFilter` import (Table embeds it); add `Table` and `Column` type imports
10. Keep `@row-click="editRow"` via attrs passthrough
11. Keep `class`, `tableStyle`, `sort-field`, `sort-mode`, `sort-order` via attrs passthrough

### Rollback Plan

1. `git revert` the SalesOrders.vue change — single file, no dependencies
2. Remove new props from Table.vue — additive only, no breaking change to Budgets.vue
3. Budgets.vue was never modified — zero risk

## Open Questions

- [ ] Delete icon visual change: SalesOrders currently uses `PrimeIcons.TIMES` (×); Table uses `pi-trash`. Accept this visual difference in the pilot, or make the delete icon configurable?

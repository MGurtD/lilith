# Tasks: Table Component Evolution

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~33 net (+51 Table.vue, -18 SalesOrders.vue) |
| 800-line budget risk | Low |
| Chained PRs recommended | No |
| Suggested split | Single PR |
| Delivery strategy | single-pr |
| Chain strategy | pending |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: pending
800-line budget risk: Low

### Suggested Work Units

| Unit | Goal | Likely PR | Notes |
|------|------|-----------|-------|
| 1 | Enhance Table.vue + migrate SalesOrders.vue | Single PR | Small cohesive change, well under budget |

## Phase 1: Foundation — Table.vue Props & Preset System

- [x] 1.1 Add `style?: string` field to the `Column` interface (line 11-19 in Table.vue)
- [x] 1.2 Add `TablePreset` type: `"crud-list" | "read-only" | "detail-lines" | "selector"` and export it
- [x] 1.3 Add 12 new props to `defineProps`: `preset`, `loading`, `dataKey`, `stripedRows`, `rowHover`, `selectionMode`, `rowGroupMode`, `expandedRows`, `paginator`, `rows`, `scrollable`, `scrollHeight` — all default `undefined` (no `withDefaults` entries for new props)
- [x] 1.4 Add `PRESET_DEFAULTS` constant map with the 4 preset configurations per the spec defaults table
- [x] 1.5 Add `resolvedDataTableProps` computed that merges preset defaults → explicit props → attrs (per design.md resolution computed)
- [x] 1.6 Update the `<DataTable>` tag to use `v-bind="resolvedDataTableProps"` instead of `v-bind="attrs"`
- [x] 1.7 Apply `col.style` to the dynamic `<Column>` element via `:style="col.style"` attribute

## Phase 2: Pilot Migration — SalesOrders.vue

- [x] 2.1 Add `columns` ref as `ref<Column[]>` with entries for number, date, expectedDate, customerComercialName, customerNumber, status — include `sortable` and `style` per current inline Column widths
- [x] 2.2 Add `filterConfig` (empty array), `v-model:filter-values="filter"`, and `:filter-body-width="filterBodyWidth"` props to the new `<Table>` component
- [x] 2.3 Replace `<DataTable>` with `<Table preset="crud-list" :columns="columns" :items="salesOrderStore.salesOrders" @row-click="editRow">` — keep `class`, `tableStyle`, `sort-field`, `sort-mode`, `sort-order` as attrs passthrough
- [x] 2.4 Move the filter template from `<DataTable #header>` to `<Table #prepend>` — same DatePicker, DropdownCustomers, DropdownLifecycle content
- [x] 2.5 Add `showDeleteColumn` and `:canDelete="(item) => item.statusId === lifecycleStore.lifecycle?.initialStatusId"` to `<Table>`
- [x] 2.6 Add `@delete="deleteSalesInvoice"` emit handler — adapt existing `deleteSalesInvoice` to accept `(item: any)` instead of `(event, order)` signature
- [x] 2.7 Move body slots (`#body-date`, `#body-expectedDate`, `#body-statusId`) to the `#body-{field}` pattern expected by Table.vue
- [x] 2.8 Update imports: remove `TableFilter`, add `Table` and `Column` type from `@/components/tables/Table.vue`

## Phase 3: Verification

- [x] 3.1 Run `pnpm run typecheck` in `frontend/` — must pass with zero errors
- [ ] 3.2 Manual regression check: navigate to `/budgets`, verify pagination, sorting, delete column, filter bar, and view config cog work identically
- [ ] 3.3 Manual pilot check: navigate to `/salesorders`, verify columns, widths, filters, pagination, row-click navigation, and delete action match pre-migration behavior
- [ ] 3.4 Preset override test: temporarily add `:rows="50"` to SalesOrders `<Table>`, verify pagination changes from 20 to 50 rows, then remove the override

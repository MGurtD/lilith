# Tasks: Review Table CRUD List User Views

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~30 net (+10 Table.vue, ~-20 across four sales consumers) |
| 400-line budget risk | Low |
| Chained PRs recommended | No |
| Suggested split | Single PR |
| Delivery strategy | exception-ok |
| Chain strategy | size-exception |
| Harness runtime | None (no automated frontend tests configured) |
| Rollback boundary | `git revert` of frontend files; any persisted `SalesOrders` default view row stays unused |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: size-exception
400-line budget risk: Low

### Suggested Work Units

| Unit | Goal | Likely PR | Test command | Rollback boundary |
|------|------|-----------|--------------|-------------------|
| 1 | Extend `crud-list` preset, activate `SalesOrders` page, strip exact duplicates, verify invariants | Single PR | `pnpm run typecheck && pnpm run build` | Frontend revert only; no DB or backend impact |

## Phase 1: Extend the `crud-list` Preset

- [x] 1.1 In `frontend/src/components/tables/Table.vue`, add `sortMode: "single"` to `PRESET_DEFAULTS["crud-list"]` (next to `scrollHeight`)
- [x] 1.2 Confirm `resolvedDataTableProps` merge order still allows `$attrs` to override the preset (`preset -> explicit -> attrs`)

## Phase 2: Activate `SalesOrders` UserTableViews

- [x] 2.1 In `frontend/src/modules/sales/views/SalesOrders.vue`, add `page="SalesOrders"` to the `<Table>` tag
- [x] 2.2 In the same `<Table>` tag, drop the redundant `sort-mode="single"` (now supplied by the preset)

## Phase 3: Strip Exact Duplicate Preset Props

- [x] 3.1 `Customers.vue` - remove the two `:scroll-height="..."` bindings that exactly resolve to `"flex"` on both `<Table preset="crud-list">` instances (the `Customers` and `CustomerTypes` tables); delete the now-unused local height constants
- [x] 3.2 `SalesInvoicesByDates.vue` - remove redundant `scrollable` and `scroll-height="flex"`; KEEP `selection-mode="multiple"` and `sort-mode="multiple"` (intentional overrides)
- [x] 3.3 `DeliveryNotes.vue` - remove the redundant `sort-mode="single"` declaration
- [x] 3.4 `SalesOrders.vue` - confirm Phase 2 already covered all exact duplicates; no further removals expected

## Phase 4: Verify Invariants of Other Consumers

- [x] 4.1 `Budgets.vue` - keep its explicit `page="Budgets"` namespace; confirm no preset-equivalent props remain
- [x] 4.2 `SalesInvoices.vue` - confirm `sortMode="multiple"` remains explicit so the preset MUST NOT override it
- [x] 4.3 `components/TableReferences.vue` - confirm `page` namespace and any non-preset attrs are untouched
- [x] 4.4 Sanity-check that all eight `crud-list` instances still declare their `page` namespace (grep `preset="crud-list"`)

## Phase 5: Typecheck, Build, and Diff Review

- [x] 5.1 From `frontend/`, run `pnpm run typecheck` - must pass with zero errors
- [x] 5.2 From `frontend/`, run `pnpm run build` - production bundle must succeed
- [x] 5.3 Inspect `git diff` - every removed prop must exactly equal the preset value; intentional overrides (multiple sorting, multiple selection, non-flex heights, deletion policy, filters, navigation) must remain
- [x] 5.4 Search the eight reviewed `crud-list` instances for `scrollable`, `scrollHeight="flex"`, equivalent bindings, and `sort-mode="single"` - no redundant declarations remain


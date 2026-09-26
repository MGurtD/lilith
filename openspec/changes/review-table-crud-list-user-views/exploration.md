# Exploration: Review Table CRUD List User Views

## Current State

`frontend/src/components/tables/Table.vue` already provides four presets. `crud-list` supplies `selectionMode="single"`, automatic pagination with 20 rows, scrollable layout, `scrollHeight="flex"`, striped rows, and row hover. User table view management is intentionally gated by the independent `page` prop; it provisions, loads, and saves a `UserTableView` only when that namespace is present.

Eight `crud-list` instances were reviewed across seven sales files. Seven already provide a page namespace. `SalesOrders.vue` is the only instance without one, so its view configuration button, default view loading, filter persistence, and row-click state save remain disabled.

Redundant preset overrides were found in `SalesInvoicesByDates.vue` (`scrollable`, `scrollHeight="flex"`) and both tables in `Customers.vue` (`:scroll-height` values that are exactly `"flex"`). `sort-mode="single"` is also repeated in two standard CRUD lists, while two invoice lists intentionally override the proposed preset default with `sortMode="multiple"`.

## Affected Areas

- `frontend/src/components/tables/Table.vue` — extend `crud-list` with `sortMode: "single"` while preserving explicit attribute overrides.
- `frontend/src/modules/sales/views/SalesOrders.vue` — add `page="SalesOrders"` to activate UserTableViews.
- `frontend/src/modules/sales/views/Customers.vue` — remove redundant flex scroll-height bindings and their now-unused constants.
- `frontend/src/modules/sales/views/SalesInvoicesByDates.vue` — remove redundant scroll props; retain multiple selection and multiple sorting.
- `frontend/src/modules/sales/views/DeliveryNotes.vue` — remove the explicit single sort mode covered by the preset.
- `frontend/src/modules/sales/views/SalesOrders.vue` — remove the explicit single sort mode covered by the preset.
- Other reviewed consumers (`Budgets.vue`, `SalesInvoices.vue`, `TableReferences.vue`) — no redundant preset props identified.

## Approaches

1. **Couple page management to crud-list** — rejected because page is a variable persistence namespace, not a table presentation default.
2. **Add delete behavior to crud-list** — rejected because `showDeleteColumn` is Table-specific and not used by the multi-selection invoice list.
3. **Activate namespaces and centralize only stable table defaults** — selected. Add `sortMode="single"` to the preset, allow attributes to override it, add the missing SalesOrders page, and delete only exact redundant consumer props.

## Recommendation

Apply the narrow refactor. It enables UserTableViews for all eight CRUD-list instances, removes exact duplicate declarations, and preserves intentional differences such as multiple sorting, multiple selection, deletion policy, custom scroll heights, filters, and navigation handlers.

## Risks

- `sortMode="multiple"` must continue to override the preset for both invoice views.
- Enabling `SalesOrders` creates or loads its per-user default view and may trigger the existing fire-and-forget save on row navigation.
- No automated frontend tests are configured; typecheck and production build are the available verification gates.

## Ready for Proposal

Yes. The user approved this scope: activate the missing page namespace, remove exact preset duplicates, and add the stable single-sort default without introducing a wrapper component.

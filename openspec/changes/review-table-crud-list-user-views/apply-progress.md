# Apply Progress: Review Table CRUD List User Views

## Mode

Standard mode. No frontend test runner is configured; the proving gates are the repository typecheck and production build. Delivery strategy: `exception-ok`, single PR, `size-exception`.

## Completed Tasks

- [x] Phase 1: Added `sortMode: "single"` to `crud-list`; preserved `preset → explicit → attrs` precedence.
- [x] Phase 2: Added `page="SalesOrders"` and removed its redundant single-sort declaration.
- [x] Phase 3: Removed exact duplicate scroll and single-sort declarations from `Customers`, `SalesInvoicesByDates`, `DeliveryNotes`, and `SalesOrders`; retained intentional multiple-sort and multiple-selection overrides.
- [x] Phase 4: Confirmed all eight CRUD-list instances have explicit page namespaces and untouched intentional overrides.
- [x] Phase 5: Typecheck, build, targeted diff review, and consumer search completed.

## Work Unit Evidence

| Evidence | Result |
|---|---|
| Focused test command | `frontend: pnpm run typecheck` — passed with zero errors. |
| Build command | `frontend: pnpm run build` — passed; only existing chunk-size and Browserslist freshness warnings. |
| Runtime harness | N/A — no automated frontend test/runtime harness is configured; this change has no new endpoint or process boundary. |
| Scope review | Eight `preset="crud-list"` instances found; all have `page`; invoice multiple-sort and multi-selection overrides remain. |
| Rollback boundary | Revert `Table.vue`, `SalesOrders.vue`, `Customers.vue`, `SalesInvoicesByDates.vue`, and `DeliveryNotes.vue`; no backend/schema rollback required. |

## Deviations

None from the approved design. The working tree contains unrelated pre-existing changes; they were not modified or included in this change's implementation scope.

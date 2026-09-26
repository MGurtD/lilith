# Proposal: Review CRUD List Table Views

## Intent

Make the `Table.vue` CRUD-list contract consistent across all current consumers. `SalesOrders.vue` uses the preset without a `page` namespace, so its per-user `UserTableView` management is silently disabled. Several consumers also repeat exact DataTable defaults already supplied by `crud-list`, creating configuration noise and making the source of truth unclear.

## Scope

### In Scope
- Add `page="SalesOrders"` so all eight current `crud-list` instances participate in UserTableViews.
- Add `sortMode: "single"` to `crud-list`, preserving consumer attributes as higher-priority overrides.
- Remove exact redundant `scrollable`, `scrollHeight="flex"`, and `sortMode="single"` declarations from consumers.
- Remove constants made unused by the cleanup.

### Out of Scope
- Do not couple `page` or `showDeleteColumn` to the preset.
- Do not change delete policies, filters, sorting fields, selection modes, or navigation behavior.
- Do not introduce a wrapper component or migrate raw PrimeVue DataTables.
- No backend or database changes.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `table-component`: `crud-list` owns the stable single-sort default, while explicit attributes continue to override preset values. All current `crud-list` consumers provide an explicit page namespace when they require UserTableViews.

## Approach

Keep `page` as an explicit, consumer-specific identity because it varies per table and gates persistence behavior. Extend `PRESET_DEFAULTS["crud-list"]` with `sortMode: "single"`; the existing `preset → explicit props → attrs` merge keeps the two invoice views on multiple sorting. Remove only declarations whose values exactly match the preset, including the two customer scroll-height constants.

## Affected Areas

| Area | Impact | Description |
|---|---|---|
| `frontend/src/components/tables/Table.vue` | Modified | Add the stable `sortMode` preset default. |
| Sales sales views/components | Modified | Activate `SalesOrders` and remove exact duplicate props. |
| `openspec/changes/review-table-crud-list-user-views/` | New | Proposal, delta specs, design, tasks, verification report. |

## Risks

| Risk | Likelihood | Mitigation |
|---|---|---|
| Multiple invoice sorting is overwritten | Low | Attributes override preset defaults; retain `sortMode="multiple"` and verify typecheck/build. |
| SalesOrders creates a new default view on first visit | Expected | This is the requested behavior; no schema change is needed. |
| Hidden visual difference from removed props | Low | Remove only exact preset-equivalent values and inspect the final diff. |

## Rollback Plan

Revert the frontend changes and leave any created `SalesOrders` default view harmlessly unused; no migration or backend rollback is required.

## Dependencies

Existing `UserTableView` endpoint/store behavior and the current `Table.vue` preset merge.

## Success Criteria

- [ ] All eight `crud-list` instances have an explicit `page` namespace.
- [ ] No consumer repeats an exact `crud-list` default for the reviewed props.
- [ ] `sortMode="multiple"` remains active for both invoice tables.
- [ ] `pnpm run typecheck` and `pnpm run build` pass.

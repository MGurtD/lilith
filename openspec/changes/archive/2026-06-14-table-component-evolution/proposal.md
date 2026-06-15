# Proposal: Table Component Evolution

## Intent

Only 1 of 116 DataTable files uses the `Table.vue` component. The remaining 115 inline raw PrimeVue `<DataTable>` usages duplicate filters, delete columns, scroll heights, and paginator settings. We need to enrich `Table.vue` so it can serve as the universal data-presentation layer, then validate with one pilot migration before scaling.

## Scope

### In Scope
- Add missing props to `Table.vue`: `loading`, `dataKey`, `stripedRows`, `rowHover`, `selectionMode`, `rowGroupMode`, `expandedRows`, and preset configurations (CRUD list, read-only, detail lines, selector)
- Migrate **one pilot view** from raw `<DataTable>` to the enhanced `Table.vue`
- Update `Column` interface if needed for new capabilities
- Verify typecheck passes and the pilot view works identically

### Out of Scope
- Migrating the remaining ~115 views (follow-up changes)
- Changing `UserTableView` or `TableViewConfig` (separate change)
- Adding new backend endpoints
- Touching any view that already uses `Table.vue`

## Capabilities

### New Capabilities
- `table-component`: Spec for the enhanced `Table.vue` — its props, presets, slot contract, and event surface

### Modified Capabilities
- None (no existing specs to modify; this is the first spec)

## Approach

1. **Audit patterns**: catalog the top 5 DataTable usage patterns (CRUD list, detail lines, read-only, selector, dashboard) and extract the common props they set
2. **Enhance Table.vue**: add missing passthrough props (`loading`, `dataKey`, `stripedRows`, `rowHover`) and new feature props (`selectionMode`, `rowGroupMode`, `expandedRows`). Introduce a `preset` prop that applies sane defaults per pattern (e.g., `preset="crud-list"` sets paginator, scrollHeight="70vh", stripedRows)
3. **Pilot migration**: pick one medium-complexity CRUD list view, replace its raw `<DataTable>` with `<Table preset="crud-list">`, verify identical behavior

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `frontend/src/components/tables/Table.vue` | Modified | New props, presets, extended Column interface |
| `frontend/src/modules/*/views/*.vue` (1 file) | Modified | Pilot view migrated to Table component |
| `frontend/src/types/index.ts` | Modified | Possibly Column type updates |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Preset defaults conflict with existing Budgets.vue usage | Low | Budgets.vue already uses Table — test it still works after changes |
| Pilot view needs slots Table.vue doesn't expose yet | Medium | Add only the slots the pilot needs; defer exotic slots |
| Scope creep (temptation to migrate more views) | Medium | Strict scope: exactly one pilot, tracked by task |

## Rollback Plan

1. Revert the pilot view to raw `<DataTable>` (single file)
2. Remove new props from `Table.vue` — they're additive, no breaking change to existing consumers
3. Budgets.vue is unaffected because new props have defaults matching current behavior

## Dependencies

- PrimeVue 4 DataTable API (already in use)
- No backend changes required

## Success Criteria

- [ ] `Table.vue` exposes `loading`, `dataKey`, `stripedRows`, `rowHover`, `selectionMode`, `rowGroupMode`, `expandedRows`, and `preset` props
- [ ] At least one preset ("crud-list") applies correct defaults for the common CRUD pattern
- [ ] One pilot view replaced raw `<DataTable>` with `<Table>` and shows identical behavior
- [ ] Budgets.vue (existing Table consumer) works unchanged
- [ ] `pnpm run typecheck` passes with zero errors
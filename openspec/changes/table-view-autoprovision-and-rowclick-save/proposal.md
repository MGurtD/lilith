# Proposal: Table View Autoprovision + Row-Click Filter Save

## Intent

Two coupled UX gaps in `frontend/src/components/tables/Table.vue`:

1. **No default view exists on first visit.** Today, the dialog `TableViewConfig.vue` only shows the "Vista actual" section when saved views exist (improvement #311). New users hitting a `<Table>` page see the gear icon open a dialog with no manageable view, and the full-feature surface (filter persistence, default toggle, nameable views) is silently unavailable until the user manually creates one via the "Nova vista" input.
2. **Filters are lost when the user leaves the list.** `onUnmounted` saves filters to `localStorage` only if no default view is active — but the common flow is click-row → navigate to detail → come back, and `localStorage` is fragile across sessions/devices/browsers.

Both gaps stem from the same root cause: **the per-user default `UserTableView` row is treated as something the user must explicitly create, when it should be a system-managed invariant** (every (user, page) pair has at most one default view).

## Scope

### In Scope
- Backend: `EnsureDefault(Guid userId, string page)` contract + service implementation — idempotent get-or-create.
- Backend: `UserTableViewController` route for the new endpoint.
- Frontend: `UserTableViewService.EnsureDefault(userId, page)` HTTP method.
- Frontend: `useUserTableViewStore.ensureDefault(userId, page)` action with in-flight promise cache to dedupe concurrent calls.
- Frontend: `useUserTableViewStore.saveFiltersToDefault(userId, page, filterValues)` action that merges `filters` into the existing `viewConfig` JSON and PUTs.
- Frontend: `Table.vue` — on mount (with `page` set) call `ensureDefault` BEFORE `loadDefaultView`. Track a `filtersDirty` flag on `@filter`/`@clear` events. Expose new `@row-click` emit that forwards `DataTableRowClickEvent`. On row click, if dirty, fire-and-forget `saveFiltersToDefault` (no await, no UI block).
- Tests: `pnpm run typecheck` clean; manual smoke on a pilot page (e.g. `Budgets.vue`).

### Out of Scope
- Multi-default views (one per page per user stays as the only invariant).
- Sharing views across users/roles.
- Debouncing the row-click save (one click = one PUT; the dirty flag already filters the common case).
- Auto-provisioning when the `page` prop is absent (Table.vue without page = no view management at all).
- Migration of existing users' data.

## Capabilities

### New Capabilities
None — both behaviors extend existing capabilities.

### Modified Capabilities
- **`user-table-view`** — adds the `EnsureDefault` contract and the frontend `ensureDefault` + `saveFiltersToDefault` actions.
- **`table-component`** — extends `Table.vue` with autoprovision on mount, `@row-click` passthrough, and conditional save-on-row-click via dirty flag.

## Approach

### Backend (`EnsureDefault`)

Idempotent contract: `EnsureDefault(userId, page) → UserTableView`. Query existing default; if none, insert a new row with `Name="Per defecte"`, `IsDefault=true`, `ViewConfig='{"columns":[]}'`. Use the existing `SetDefault` transactional pattern (unset other defaults first) to keep the invariant.

Frontend dedupe: in-flight promise stored in `Map<string, Promise<UserTableView>>` keyed by `${userId}:${page}`. Concurrent callers receive the same promise. Cleared on resolve/reject.

### Frontend (`Table.vue` orchestration)

```ts
onMounted → ensureDefault → loadDefaultView → restoreFiltersIfNeeded
@filter/@clear → filtersDirty = true
@row-click (DataTableRowClickEvent) →
  emit("row-click", event)  // consumer handles navigation
  if (filtersDirty && defaultViewId)
    saveFiltersToDefault(userId, page, filterValues).finally(() => filtersDirty = false)
```

The save is fire-and-forget (no `await`) to avoid blocking the row-click navigation. Errors are logged but never surfaced — the worst case is "filters not saved this once", which the next click will retry.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `backend/src/Application.Contracts/Services/System/IUserTableViewService.cs` | Modified | Add `EnsureDefault` signature |
| `backend/src/Application/Services/System/UserTableViewService.cs` | Modified | Implement `EnsureDefault` |
| `backend/src/Api/Controllers/System/UserTableViewController.cs` | Modified | Add POST `/ensure-default` route |
| `frontend/src/services/usertableview.service.ts` | Modified | Add `EnsureDefault` HTTP method |
| `frontend/src/store/usertableview.ts` | Modified | Add `ensureDefault` + `saveFiltersToDefault` actions + in-flight promise map |
| `frontend/src/components/tables/Table.vue` | Modified | Call `ensureDefault` on mount; add `filtersDirty` ref; expose `@row-click`; save on row-click if dirty |
| `openspec/specs/table-component/spec.md` | Modified (delta) | Add requirements for autoprovision, `@row-click`, save-on-row-click |
| `openspec/specs/user-table-view/spec.md` | New full spec | Define EnsureDefault contract |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Race: two parallel `ensureDefault` calls create duplicates | Med | Backend uses `IsDefault=true` filter + `SetDefault` pattern (unsets siblings before insert) so DB-level uniqueness on `(UserId, Page, IsDefault=true)` is the source of truth; frontend dedupes via in-flight promise map. |
| Row-click save throws → unhandled promise rejection | Low | Wrap save in try/catch inside the store action; log and swallow (no toast — non-critical). |
| Auto-save corrupts existing `viewConfig.columns` or `viewConfig.sort` | Low | `saveFiltersToDefault` does a read-modify-write: GET existing → merge `filters` only into the JSON object → PUT. Never blindly overwrite. |
| `@row-click` breaking change for existing consumers (e.g. `Budgets.vue` already wires `@row-click="editRow"`) | Med | New emit only fires when `page` prop is set; existing consumers without `page` continue to use PrimeVue's event directly via `v-bind="$attrs"`. Verify with smoke test on Budgets.vue. |
| Dirty flag never resets on failed save → repeated PUTs | Low | `.finally(() => filtersDirty = false)` so a failed save clears the flag and the next user-driven filter change re-arms it. |

## Rollback Plan

1. **Backend**: remove `EnsureDefault` route + method + interface entry. Existing CRUD endpoints untouched → no other behavior change.
2. **Frontend**: revert `Table.vue` mount call, `@row-click` handler, and dirty flag. Remove `ensureDefault` / `saveFiltersToDefault` actions from store. Consumers lose the autoprovisioned default (revert to today's "empty dialog" state) and row-click saves (revert to `localStorage` only).
3. **No DB migration needed**: nothing changes schema-wise.
4. **One PR per capability** so a partial revert is possible (backend alone, or frontend alone).

## Dependencies

- Existing `UserTableView` entity, service, controller, store, frontend service — all already in place.
- No new npm or NuGet packages.
- Existing localization keys (`TableViewNameExists`, `EntityNotFound`) reused.

## Success Criteria

- [ ] `pnpm run typecheck` passes
- [ ] Backend `dotnet build` passes
- [ ] First visit to a `<Table :page="X">` page creates exactly one default `UserTableView` (verify via DB row count)
- [ ] Click-row on a Budgets list with filters applied → row navigates AND `UserTableView.viewConfig.filters` in DB contains the current filter values within 1s
- [ ] Click-row with no dirty filters → NO PUT call (verify network tab)
- [ ] Reload the page after a save → filters restore from DB (not localStorage)
- [ ] Budgets.vue still works without any code changes (backward compatibility smoke)
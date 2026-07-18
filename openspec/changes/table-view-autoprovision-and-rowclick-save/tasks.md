# Tasks: Table View Autoprovision + Row-Click Filter Save

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~150-200 across 7 files |
| 400-line budget risk | Low |
| Chained PRs recommended | No |
| Suggested split | Single PR (fits comfortably under 400 lines) |
| Delivery strategy | exception-ok |
| Chain strategy | size-exception |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: size-exception
400-line budget risk: Low

## Phase 1: Backend Foundation

- [ ] 1.1 Create `backend/src/Application.Contracts/Services/System/EnsureDefaultRequest.cs` with record `EnsureDefaultRequest(Guid UserId, string Page)`
- [ ] 1.2 Modify `IUserTableViewService.cs` — add `Task<GenericResponse> EnsureDefault(EnsureDefaultRequest request)` after `SetDefault`

## Phase 2: Backend Service Implementation

- [ ] 2.1 Modify `UserTableViewService.cs` — implement `EnsureDefault`: query existing default via `Find(v => v.UserId == userId && v.Page == page && v.IsDefault)`; if found return it wrapped in `GenericResponse(true, existing)`; if not found, instantiate `UserTableView { UserId, Page, Name="Per defecte", IsDefault=true, ViewConfig="{\"columns\":[]}" }` with new `Guid`, call `unitOfWork.UserTableViews.Add(...)`, return `GenericResponse(true, created)`
- [ ] 2.2 Modify `UserTableViewController.cs` — add `[HttpPost("ensure-default")]` route taking `[FromBody] EnsureDefaultRequest request`, return 200 on success (NOT 201 — endpoint is idempotent, not "create")

## Phase 3: Frontend Service Layer

- [ ] 3.1 Modify `frontend/src/services/usertableview.service.ts` — add `public async EnsureDefault(userId: string, page: string): Promise<UserTableView | undefined>` that POSTs to `${resource}/ensure-default` with `{ userId, page }` body

## Phase 4: Frontend Store Layer

- [ ] 4.1 Modify `frontend/src/store/usertableview.ts` — add `ensureInFlight: Map<string, Promise<UserTableView>> = new Map()` as module-level non-reactive state (or store state — pick whichever matches existing patterns)
- [ ] 4.2 Add `async ensureDefault(userId, page): Promise<UserTableView>` action — check `ensureInFlight` cache, return cached promise if exists; else create promise from `AppServices.UserTableView.EnsureDefault`, cache it, clear cache on settle (success or error), fetch full view list to keep store consistent
- [ ] 4.3 Add `async saveFiltersToDefault(userId, page, filterValues): Promise<boolean>` action — call `ensureDefault` first, then GET existing view, parse `viewConfig`, merge `filters` into the JSON object (preserve `columns` and `sort`), PUT via `AppServices.UserTableView.Update`, refresh `fetchViews` on success, swallow + log errors

## Phase 5: Frontend Component (Table.vue)

- [ ] 5.1 Add `filtersDirty: ref(false)` to script setup state
- [ ] 5.2 Modify `onMounted` flow — when `props.page` is set, `await viewStore.ensureDefault(userId, page)` BEFORE the existing `loadDefaultView()` call
- [ ] 5.3 Wrap existing `@filter` and `@clear` emitters to also set `filtersDirty.value = true` (do NOT remove original emits — consumers still need them)
- [ ] 5.4 Add `@row-click` event to `defineEmits` typing — `(e: "row-click", event: DataTableRowClickEvent): void` (import `DataTableRowClickEvent` from `primevue/datatable`)
- [ ] 5.5 Add `@row-click="onRowClick"` binding to the `<DataTable>` element
- [ ] 5.6 Implement `onRowClick(event)` function — call `emit("row-click", event)` first (consumer navigates), then if `filtersDirty.value && activeViewId.value !== "" && store.user?.id && props.page`, call `viewStore.saveFiltersToDefault(store.user.id, props.page, props.filterValues).catch(err => console.warn("[Table] auto-save filters failed", err)).finally(() => { filtersDirty.value = false })`
- [ ] 5.7 Reset `filtersDirty.value = false` at the start of `loadDefaultView` (loading a saved view = clean slate)

## Phase 6: Verification

- [ ] 6.1 Run `pnpm run typecheck` in `frontend/` — must pass with zero errors
- [ ] 6.2 Run `dotnet build` in `backend/` — must pass with zero errors
- [ ] 6.3 Manual smoke: open `/budgets` as a fresh user → verify 1 row created in `UserTableViews` table with `Name="Per defecte"`, `IsDefault=true`
- [ ] 6.4 Manual smoke: apply a filter on `/budgets`, click a row → verify `viewConfig.filters` JSON contains the filter values within 1s in DB
- [ ] 6.5 Manual smoke: click a row WITHOUT any filter applied → verify NO PUT request in network tab
- [ ] 6.6 Manual smoke: refresh `/budgets` after smoke 6.4 → verify filters are restored (not from localStorage)
- [ ] 6.7 Regression: open `/budgets` and verify navigation (row click → detail page) still works as before
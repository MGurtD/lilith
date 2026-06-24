# Tasks: Integrate Filters into UserTableView

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~250-350 |
| 400-line budget risk | Low |
| Chained PRs recommended | No |
| Suggested split | Single PR |
| Delivery strategy | single-pr |
| Chain strategy | pending |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: pending
400-line budget risk: Low

### Suggested Work Units

| Unit | Goal | Likely PR | Notes |
|------|------|-----------|-------|
| 1 | Full filter integration (backend + frontend + cleanup) | Single PR | Under 400 lines, cohesive change |

## Phase 1: Backend — Entity & Migration

- [ ] 1.1 Add `public string? FilterConfig { get; set; }` to `UserTableView` entity (after `ColumnConfig`)
  - **Files**: `backend/src/Domain/Entities/Auth/UserTableView.cs`
  - **Est**: +2 lines
  - **Acceptance**: Entity compiles, nullable string property exists

- [ ] 1.2 Add EF Core configuration for `FilterConfig` — nullable varchar(4000) in `UserTableViewBuilder`
  - **Files**: `backend/src/Infrastructure/Persistance\EntityConfiguration\Auth\UserTableViewBuilder.cs`
  - **Est**: +5 lines
  - **Acceptance**: Builder config matches `ColumnConfig` pattern but `.IsRequired(false)`

- [ ] 1.3 Generate EF Core migration `AddFilterConfigToUserTableView`
  - **Command**: `dotnet ef migrations add AddFilterConfigToUserTableView --project src/Infrastructure/`
  - **Est**: +20 lines (generated)
  - **Acceptance**: Migration adds nullable `FilterConfig` column to `UserTableViews` table

## Phase 2: Backend — Service Layer

- [ ] 2.1 Update `UserTableViewService.Update()` to also copy `FilterConfig` from incoming model to existing entity
  - **Files**: `backend/src/Application/Services/System/UserTableViewService.cs`
  - **Est**: +1 line (add `existing.FilterConfig = userTableView.FilterConfig;` after `ColumnConfig` line)
  - **Acceptance**: Update persists filter config changes

- [ ] 2.2 Verify `Create()` works without changes — entity property is nullable, serializes automatically
  - **Files**: `backend/src/Application/Services/System/UserTableViewService.cs`
  - **Est**: 0 lines (verification only)
  - **Acceptance**: Create accepts `FilterConfig` via entity binding, no code change needed

- [ ] 2.3 Verify controller works without changes — `UserTableView` entity is the DTO, model binding handles new property
  - **Files**: `backend/src/Api/Controllers/System/UserTableViewController.cs`
  - **Est**: 0 lines (verification only)
  - **Acceptance**: POST/PUT accept `filterConfig` in JSON body automatically

## Phase 3: Frontend — Types & Store

- [ ] 3.1 Add `filterConfig?: string` to `UserTableView` interface in `types/index.ts`
  - **Files**: `frontend/src/types/index.ts`
  - **Est**: +1 line
  - **Acceptance**: TypeScript interface includes optional `filterConfig`

- [ ] 3.2 Extract `hydrateDates()` and helpers from `userfilter.ts` into shared utility `utils/filter-hydrate.ts`
  - **Files**: `frontend/src/utils/filter-hydrate.ts` (new), `frontend/src/store/userfilter.ts`
  - **Est**: +30 new, -15 removed (refactor)
  - **Acceptance**: `hydrateFilter(obj)` exported, `userfilter.ts` imports from shared util, all existing consumers still work

- [ ] 3.3 Update `usertableview.ts` store: add `applyFilters(view, filterValues)` method that deserializes `filterConfig` via `hydrateFilter` and returns parsed filter object
  - **Files**: `frontend/src/store/usertableview.ts`
  - **Est**: +15 lines
  - **Acceptance**: Given a view with `filterConfig`, returns hydrated filter object; returns `null` if no `filterConfig`

- [ ] 3.4 Update `usertableview.ts` store: modify `createNewView()` to accept optional `filterConfig` parameter and include it in returned model
  - **Files**: `frontend/src/store/usertableview.ts`
  - **Est**: +3 lines (add param, add to return object)
  - **Acceptance**: New view model includes `filterConfig` when provided

## Phase 4: UI Integration — TableViewConfig

- [ ] 4.1 Add `filterValues?: any` prop to `TableViewConfig.vue`
  - **Files**: `frontend/src/components/tables/TableViewConfig.vue`
  - **Est**: +1 line in props definition
  - **Acceptance**: Component accepts filter values from parent

- [ ] 4.2 Update `buildColumnConfig()` → `buildSavePayload()` that returns `{ columnConfig, filterConfig }` — serialize `props.filterValues` as `filterConfig` (JSON.stringify)
  - **Files**: `frontend/src/components/tables/TableViewConfig.vue`
  - **Est**: +10 lines (rename function, add filterConfig serialization)
  - **Acceptance**: Save payload includes both columnConfig and filterConfig strings

- [ ] 4.3 Update `saveView()` and `saveAsNewView()` to pass `filterConfig` in the model sent to store
  - **Files**: `frontend/src/components/tables/TableViewConfig.vue`
  - **Est**: +6 lines (merge filterConfig into update/create payloads)
  - **Acceptance**: Saving view persists filter values alongside column config

- [ ] 4.4 Add logic: when selected view changes (watch `selectedViewId`), if view has `filterConfig`, deserialize and emit `update:filterValues` event with hydrated filters
  - **Files**: `frontend/src/components/tables/TableViewConfig.vue`
  - **Est**: +10 lines (add emit, call hydrateFilter)
  - **Acceptance**: Loading a saved view emits filter values back to parent

## Phase 5: UI Integration — Table.vue

- [ ] 5.1 Update `TableViewConfig` usage in `Table.vue` template to pass `:filter-values="props.filterValues"`
  - **Files**: `frontend/src/components/tables/Table.vue`
  - **Est**: +1 line (add prop binding)
  - **Acceptance**: TableViewConfig receives current filter values

- [ ] 5.2 Update `loadDefaultView()`: if default view has `filterConfig`, deserialize via store method, emit `update:filterValues` and `filter` events
  - **Files**: `frontend/src/components/tables/Table.vue`
  - **Est**: +10 lines
  - **Acceptance**: Loading default view with filters applies them to the table

- [ ] 5.3 Update `onApplyViewConfig()`: when view is applied manually, also handle `filterConfig` — deserialize and emit `update:filterValues` + `filter`
  - **Files**: `frontend/src/components/tables/Table.vue`
  - **Est**: +8 lines
  - **Acceptance**: Manual view selection applies both columns and filters

## Phase 6: View Cleanup — Budgets.vue

- [ ] 6.1 Remove `import { useUserFilterStore }` and `const userFilterStore = useUserFilterStore()`
  - **Files**: `frontend/src/modules/sales/views/Budgets.vue`
  - **Est**: -2 lines
  - **Acceptance**: No UserFilterStore references remain

- [ ] 6.2 Remove `getUserFilter()` function and its call in `onMounted`
  - **Files**: `frontend/src/modules/sales/views/Budgets.vue`
  - **Est**: -12 lines
  - **Acceptance**: `onMounted` no longer calls `getUserFilter()`

- [ ] 6.3 Remove `userFilterStore.addFilter()` call from `onUnmounted`
  - **Files**: `frontend/src/modules/sales/views/Budgets.vue`
  - **Est**: -1 line
  - **Acceptance**: `onUnmounted` only resets `budgetStore.budgets`

- [ ] 6.4 Verify `Budgets.vue` filter behavior: filter state is now managed by `Table.vue` via `filterConfig` on default view, not by `UserFilterStore`
  - **Files**: `frontend/src/modules/sales/views/Budgets.vue`
  - **Est**: 0 lines (manual verification)
  - **Acceptance**: Period picker, customer dropdown, status dropdown still work; default view loads saved filters

## Phase 7: Verification

- [ ] 7.1 `dotnet build` — backend compiles without errors
  - **Est**: 0 lines
  - **Acceptance**: Build succeeds

- [ ] 7.2 `dotnet ef database update` — migration applies cleanly
  - **Est**: 0 lines
  - **Acceptance**: `UserTableViews` table has `FilterConfig` column

- [ ] 7.3 `pnpm run typecheck` — frontend types pass
  - **Est**: 0 lines
  - **Acceptance**: No TypeScript errors

- [ ] 7.4 Manual test SC-01: Save view with filters → reload page → verify columns AND filters restored
  - **Acceptance**: Both column visibility/order and filter values persist

- [ ] 7.5 Manual test SC-04: Load view without `filterConfig` (legacy view) → verify only columns apply, no filter errors
  - **Acceptance**: Graceful degradation, table loads with default filters

- [ ] 7.6 Manual test SC-02: Change filters without saving → switch view → verify filters revert to saved state
  - **Acceptance**: Unsaved filter changes are discarded when loading a saved view

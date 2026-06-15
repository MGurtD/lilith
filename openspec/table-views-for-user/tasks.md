# Tasks: Table Views per User

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~550-650 |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | PR 1 (Backend API) → PR 2 (Frontend Foundation) → PR 3 (UI Dialog + Integration) |
| Delivery strategy | ask-on-risk |
| Chain strategy | stacked-to-main |

Decision needed before apply: Yes
Chained PRs recommended: Yes
Chain strategy: stacked-to-main
400-line budget risk: High

### Suggested Work Units

| Unit | Goal | Likely PR | Notes |
|------|------|-----------|-------|
| 1 | Backend CRUD API + migration + localization | PR 1 | Base = main; independent, Swagger-verifiable |
| 2 | Frontend types, service, store, Table.vue extension | PR 2 | Base = main; depends on PR 1 API being available |
| 3 | TableViewConfig dialog + list view integration + default load | PR 3 | Base = main; depends on PR 2 store/components |

## Phase 1: Backend Foundation

- [ ] 1.1 Create `UserTableView.cs` entity in `backend/src/Domain/Entities/Auth/` with Name, Page, ColumnConfig, IsDefault, UserId, User navigation (skill: adding-backend-entity)
- [ ] 1.2 Create `UserTableViewBuilder.cs` in `backend/src/Infrastructure/Persistance/EntityConfiguration/Auth/` with FluentAPI config (varchar limits, FK Restrict)
- [ ] 1.3 Add `IRepository<UserTableView, Guid> UserTableViews` to `IUnitOfWork.cs` and instantiate in `UnitOfWork.cs`
- [ ] 1.4 Create `IUserTableViewService.cs` in `backend/src/Application.Contracts/Services/System/` with GetAllByUserId, GetByUserIdAndPage, Create, Update, Delete, SetDefault
- [ ] 1.5 Implement `UserTableViewService.cs` in `backend/src/Application/Services/System/` with duplicate name validation, default-swap logic, ILocalizationService
- [ ] 1.6 Register `IUserTableViewService` in `backend/src/Api/Setup/ApplicationServicesSetup.cs`
- [ ] 1.7 Create `UserTableViewController.cs` in `backend/src/Api/Controllers/System/` with GET by userId/page, POST, PUT, DELETE endpoints
- [ ] 1.8 Add localization keys "TableViewNameExists" and "CannotDeleteDefaultView" to `ca.json`, `es.json`, `en.json`
- [ ] 1.9 Create EF migration: `dotnet ef migrations add AddUserTableView --project src/Infrastructure/`

## Phase 2: Frontend Foundation

- [ ] 2.1 Add `UserTableView` interface to `frontend/src/types/index.ts` matching backend entity (skill: adding-frontend-entity)
- [ ] 2.2 Create `usertableview.service.ts` in `frontend/src/services/` extending `BaseService<UserTableView>` with endpoint "usertableview"
- [ ] 2.3 Register service in `frontend/src/services/index.ts`
- [ ] 2.4 Create `usertableview.ts` Pinia store in `frontend/src/store/` with loadViews, activateView, saveView, deleteView, setDefault, applyView actions
- [ ] 2.5 Extend `Column` interface in `frontend/src/components/tables/Table.vue` with `visible?: boolean` and `order?: number`
- [ ] 2.6 Modify Table.vue template to filter by `visible` and sort by `order` when present on columns

## Phase 3: UI Dialog & Integration

- [ ] 3.1 Create `TableViewConfig.vue` in `frontend/src/components/tables/` with PrimeVue Dialog, column list with drag-reorder, visibility/total toggles (skill: frontend-patterns)
- [ ] 3.2 Implement view selector dropdown, Save/SaveAs/Delete/SetDefault buttons in TableViewConfig.vue
- [ ] 3.3 Wire TableViewConfig.vue store actions: load current page views, save new/updated view, delete, set default
- [ ] 3.4 Implement `applyView(page, baseColumns)` in store: merge ColumnConfig onto base, drop unknown fields, append missing base columns
- [ ] 3.5 Integrate auto-load of default view: call `applyView()` in list view `onMounted` after `fetchAll()`
- [ ] 3.6 Add toolbar icon to one list view (e.g., `Budgets.vue`) to open TableViewConfig dialog as integration proof
- [ ] 3.7 Verify UserFilter state is NOT modified when activating/changing a table view (FR-10)
- [ ] 3.8 Run `pnpm run typecheck` — must pass with 0 errors; run `dotnet build` — must pass with 0 errors

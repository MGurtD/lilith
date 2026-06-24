# Proposal: Table Views per User

## Intent

Users cannot save table configurations. Every time they open a list view (Budgets, SalesOrders, etc.), they see the same hardcoded columns in the same order. This forces different user profiles (e.g., administration vs. accounting) to work with identical table layouts, reducing efficiency.

We need persistent, named table views that save **which columns are visible, their order, and which columns have totals** — so each user can configure tables to match their workflow, like Excel/SharePoint named views.

## Scope

### In Scope
- New `UserTableView` entity (backend): name, page, column config (JSON), user FK, isDefault flag
- `UserTableViewService` + `UserTableViewController` (backend): CRUD + get by user/page + set default
- New `UserTableView` TypeScript interface (frontend)
- `UserTableViewService` extending `BaseService<T>` (frontend)
- `useUserTableViewStore` (Pinia): load/save/activate views
- `TableViewConfig.vue` dialog component: column reorder, toggle visibility, toggle totals, name/save/load/delete views
- Extend `Table.vue` to accept view-driven column config
- `Column` interface extended with `visible?: boolean` and `order?: number` properties
- EF Core migration for `UserTableViews` table

### Out of Scope
- Shared/role-level views (future: views shared across roles)
- View export/import (future)
- Modifying `UserFilter` entity — filters remain independent
- Column resize persistence (not part of column config)
- Column sort persistence (already handled by PrimeVue multiSort)
- Mobile-specific view adaptations

## Capabilities

### New Capabilities
- `user-table-view`: CRUD API and persistence for named per-user table view configurations including column visibility, order, and totals

### Modified Capabilities
- `table-component`: Table.vue accepts an optional `UserTableView`-driven column configuration that overrides the hardcoded `columns` prop

## Approach

### Backend: New `UserTableView` entity (Pattern A — generic repository)

```
UserTableView : Entity
  Name        : string (required, max 200)
  Page        : string (required, max 250) — matches UserFilter.Page convention
  ColumnConfig: string (required, max 8000) — JSON-serialized ColumnConfig[]
  IsDefault   : bool (default false)
  UserId      : Guid (FK → User)
  User        : User? (navigation)
```

`ColumnConfig` JSON structure:
```json
[
  { "field": "number", "visible": true, "order": 0, "total": null },
  { "field": "date",   "visible": true, "order": 1, "total": "sum" },
  { "field": "_customer", "visible": false, "order": 2, "total": null }
]
```

Uses `IRepository<UserTableView, Guid>` — no custom queries needed beyond `Find(userId, page)` which generic repo handles.

**Controller endpoints**:
- `GET /api/usertableview/{userId}` — all views for user
- `GET /api/usertableview/{userId}/{page}` — views for user+page (includes default)
- `POST /api/usertableview` — create or update (upsert pattern, matches UserFilter convention)
- `DELETE /api/usertableview/{id}` — delete

### Frontend: View management

1. **`Column` interface** — add `visible?: boolean` and `order?: number` to existing `Column` type in `Table.vue`
2. **`TableViewConfig.vue`** — PrimeVue Dialog with:
   - Drag-to-reorder list (OrderList component)
   - Checkboxes for visibility toggle
   - Checkboxes for totals (where applicable)
   - Dropdown to switch between saved views
   - Save/Delete/SaveAs buttons
   - "Set as default" toggle
3. **`useUserTableViewStore`** — loads views for current page, activates a view by merging its `columnConfig` into the view's `columns` prop
4. **Integration pattern** in list views:
   ```ts
   // Before (Budgets.vue):
   const columns: Column[] = [ ... ]; // hardcoded
   
   // After:
   const baseColumns: Column[] = [ ... ]; // base definition
   const viewStore = useUserTableViewStore();
   const columns = computed(() => viewStore.applyView("Budgets", baseColumns));
   ```
5. **View activation merges** view config onto base columns: hidden columns removed, order rearranged, totals applied. Columns not in the view config keep their base definition.

### Relationship with UserFilter

UserFilter and UserTableView are **independent** entities:
- `UserFilter` persists the **filter values** (what data to show)
- `UserTableView` persists the **column layout** (how to display it)

When a user activates a saved view, no filter values are changed. The view only controls columns. A future enhancement could optionally snapshot filter values into a view, but that's out of scope.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `backend/src/Domain/Entities/Auth/` | New | `UserTableView.cs` entity |
| `backend/src/Application.Contracts/` | New | `IUserTableViewService` interface |
| `backend/src/Application/Services/System/` | New | `UserTableViewService.cs` |
| `backend/src/Infrastructure/Persistance/` | New | `UserTableViewBuilder.cs` + UnitOfWork registration |
| `backend/src/Api/Controllers/System/` | New | `UserTableViewController.cs` |
| `backend/src/Api/Setup/` | Modified | Register `IUserTableViewService` in DI |
| `frontend/src/types/index.ts` | Modified | Add `UserTableView` interface |
| `frontend/src/services/` | New | `usertableview.service.ts` |
| `frontend/src/store/` | New | `usertableview.ts` Pinia store |
| `frontend/src/components/tables/Table.vue` | Modified | `Column` type extended with `visible`, `order` |
| `frontend/src/components/tables/` | New | `TableViewConfig.vue` dialog component |
| `frontend/src/modules/*/views/*s.vue` | Modified | List views use `viewStore.applyView()` for columns |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| JSON column config drifts from actual table columns (fields renamed/removed) | Medium | `applyView()` silently drops unknown fields and adds missing ones from base config |
| Large column config JSON exceeding varchar(8000) | Low | Even 50 columns ≈ 3KB JSON; 8000 chars is generous. Monitor and increase if needed |
| Breaking existing list views during migration | Low | `applyView()` returns `baseColumns` unchanged when no saved view exists — zero-impact fallback |
| Users confused by view UI | Low | Implícit "default view" = current behavior; view dialog is opt-in via toolbar icon |

## Rollback Plan

1. Remove `UserTableView` entity and migration — no data dependency (views are cosmetic)
2. Remove frontend store, service, dialog — Table.vue falls back to `columns` prop
3. No UserFilter changes needed — fully independent

## Dependencies

- EF Core migration infrastructure (existing)
- PrimeVue OrderList component (available in PrimeVue 4)
- No external dependencies

## Success Criteria

- [ ] User can save a named table view with custom column visibility, order, and totals
- [ ] User can switch between saved views on the same page
- [ ] User can set one view as default, auto-loaded on page visit
- [ ] Views with stale column references (field removed from code) degrade gracefully
- [ ] Existing list views without saved views work identically to current behavior
- [ ] `pnpm run typecheck` and `dotnet build` pass with zero errors
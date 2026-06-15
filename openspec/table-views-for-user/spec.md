# Table Views per User — Specification

## Purpose

Enable users to save, load, and switch between named table view configurations per page. Each view controls column visibility, column order, and optional column totals — independently from UserFilter (which controls data filtering).

---

## 1. Functional Requirements

### FR-01: Create Table View
The system SHALL allow an authenticated user to create a named table view for a specific page, storing column visibility, order, and totals as a JSON configuration.

#### Scenario: FR-01-S1 — User creates a new view

- GIVEN user is on the Budgets page with default columns visible
- WHEN user opens the Table View dialog, enters name "Administració", and clicks Save
- THEN a new UserTableView record is created with UserId, Page="Budgets", Name="Administració", IsDefault=false, and ColumnConfig reflecting current column state

#### Scenario: FR-01-S2 — Duplicate name on same page

- GIVEN user already has a view named "Administració" for page "Budgets"
- WHEN user attempts to create another view with the same name on the same page
- THEN the system SHALL reject the request with a localized error message "TableViewNameExists"

### FR-02: Read Table Views by User and Page
The system SHALL return all saved table views for a given user and page, including which view is marked as default.

#### Scenario: FR-02-S1 — User loads views for a page

- GIVEN user has 3 saved views for page "SalesInvoice"
- WHEN user navigates to the SalesInvoice page
- THEN the system fetches and returns all 3 views with their ColumnConfig and IsDefault flags

#### Scenario: FR-02-S2 — No saved views for page

- GIVEN user has no saved views for page "WorkOrder"
- WHEN the system queries views for that user+page combination
- THEN the system returns an empty list (no error)

### FR-03: Apply Default View Automatically
The system SHALL automatically apply the default view (IsDefault=true) when a user navigates to a page that has one.

#### Scenario: FR-03-S1 — Default view exists

- GIVEN user has a default view "Comptabilitat" for page "SalesInvoice"
- WHEN user navigates to the SalesInvoice page
- THEN the table renders with columns ordered and filtered per the "Comptabilitat" view config

#### Scenario: FR-03-S2 — No default view

- GIVEN user has no saved views for page "Budgets"
- WHEN user navigates to the Budgets page
- THEN the table renders with the hardcoded base column definition (zero-impact fallback)

### FR-04: Update Table View
The system SHALL allow a user to update an existing view's name, ColumnConfig, and IsDefault flag.

#### Scenario: FR-04-S1 — User modifies column order and saves

- GIVEN user has a saved view "General" for page "Budgets"
- WHEN user reorders columns via drag-and-drop, toggles a column invisible, and clicks Save on the "General" view
- THEN the view's ColumnConfig is updated with the new order and visibility

#### Scenario: FR-04-S2 — User renames a view

- GIVEN user has a view named "Old Name" for page "Budgets"
- WHEN user changes the name to "New Name" and saves
- THEN the view is updated with the new name

### FR-05: Delete Table View
The system SHALL allow a user to delete a saved table view. Deleting the default view SHALL NOT be permitted without first assigning a new default.

#### Scenario: FR-05-S1 — Delete non-default view

- GIVEN user has views "View A" (default) and "View B" for page "Budgets"
- WHEN user deletes "View B"
- THEN "View B" is removed and "View A" remains as default

#### Scenario: FR-05-S2 — Delete default view blocked

- GIVEN user has only one view "Default View" with IsDefault=true
- WHEN user attempts to delete it
- THEN the system SHALL reject the deletion with a localized error "CannotDeleteDefaultView"

### FR-06: Set Default View
The system SHALL allow a user to designate exactly one view per page as the default.

#### Scenario: FR-06-S1 — Set new default

- GIVEN user has views "A" (default) and "B" (not default) for page "Budgets"
- WHEN user marks "B" as default
- THEN "B" becomes IsDefault=true and "A" becomes IsDefault=false

#### Scenario: FR-06-S2 — Only one default per user+page

- GIVEN user sets view "C" as default for page "Budgets"
- THEN no other view for that user+page combination SHALL have IsDefault=true

### FR-07: Column Config JSON Structure
The ColumnConfig field SHALL store a JSON array of objects, each containing: `field` (string), `visible` (boolean), `order` (integer), and `total` (string | null).

#### Scenario: FR-07-S1 — Valid config saved

- GIVEN user configures columns with visibility, order, and one total
- WHEN the view is saved
- THEN ColumnConfig contains: `[{"field":"number","visible":true,"order":0,"total":null},{"field":"amount","visible":true,"order":1,"total":"sum"}]`

### FR-08: View Activation Merges with Base Columns
When a view is activated, its ColumnConfig SHALL be merged onto the page's base column definition. Unknown fields are silently dropped; base columns not in the config retain their default state.

#### Scenario: FR-08-S1 — Stale field reference

- GIVEN a saved view references field "oldColumn" that no longer exists in the base definition
- WHEN the view is activated
- THEN "oldColumn" is silently ignored and remaining fields are applied

#### Scenario: FR-08-S2 — New base column not in view

- GIVEN base definition has column "newField" not present in the saved view's ColumnConfig
- WHEN the view is activated
- THEN "newField" appears with its base default (visible, appended in base order)

### FR-09: Table View Dialog (TableViewConfig.vue)
The system SHALL provide a PrimeVue Dialog component that allows users to: reorder columns via drag-and-drop, toggle column visibility, toggle column totals, switch between saved views, save/save-as/delete views, and set a view as default.

#### Scenario: FR-09-S1 — Dialog opens with current state

- GIVEN user is on Budgets page with current column configuration
- WHEN user clicks the Table View toolbar icon
- THEN the dialog opens showing all columns with current visibility/order/total state

#### Scenario: FR-09-S2 — Drag reorder

- GIVEN the dialog is open with columns listed
- WHEN user drags "Amount" above "Date"
- THEN the column order updates visually in the list

### FR-10: Coexistence with UserFilter
Table views SHALL NOT affect, modify, or depend on UserFilter state. Filters control which data rows appear; views control which columns are displayed and in what order.

#### Scenario: FR-10-S1 — View change does not alter filters

- GIVEN user has active filters on Budgets page (status=Pendent, search="ABC")
- WHEN user switches from view "General" to view "Comptabilitat"
- THEN the filter values remain unchanged; only column display changes

---

## 2. Non-Functional Requirements

### NFR-01: Performance
View activation (merge of ColumnConfig onto base columns) SHALL complete in under 50ms for tables with up to 50 columns.

### NFR-02: Security
All UserTableView CRUD operations SHALL be scoped to the authenticated user's UserId. Users SHALL NOT be able to read, modify, or delete views belonging to other users.

### NFR-03: Column Config Size
The ColumnConfig field SHALL support up to 50 columns with full metadata. The varchar(8000) limit provides sufficient margin (~3KB for 50 columns).

### NFR-04: Graceful Degradation
If a saved view references columns that no longer exist in the base definition, the view SHALL activate without error, silently dropping unknown fields.

### NFR-05: UI Language
All dialog labels, error messages, and toast notifications SHALL be in Catalan, consistent with the existing frontend convention.

---

## 3. User Scenarios

### SC-01: User creates a new view "Administració" for SalesInvoice

- GIVEN user is on the SalesInvoice list page
- AND the table shows all default columns
- WHEN user opens the Table View dialog
- AND enters name "Administració"
- AND hides columns "Referència" and "Notes"
- AND reorders "Data" to position 1
- AND clicks Save
- THEN a new UserTableView is persisted with UserId, Page="SalesInvoice", Name="Administració", IsDefault=false
- AND ColumnConfig reflects the hidden columns and new order
- AND the table immediately reflects the new configuration

### SC-02: User loads a saved view and columns reorder

- GIVEN user has a saved view "Comptabilitat" for page "SalesInvoice" with columns ordered: Number, Date, Amount, Status
- AND the current table shows columns in default order: Number, Status, Date, Amount
- WHEN user opens the Table View dialog
- AND selects "Comptabilitat" from the view dropdown
- AND clicks Apply
- THEN the table columns reorder to: Number, Date, Amount, Status
- AND column visibility and totals match the saved config

### SC-03: User marks a view as default

- GIVEN user has views "General" and "Comptabilitat" for page "SalesInvoice"
- AND "General" is currently the default
- WHEN user opens the Table View dialog
- AND selects "Comptabilitat"
- AND toggles "Set as default"
- AND clicks Save
- THEN "Comptabilitat" IsDefault becomes true
- AND "General" IsDefault becomes false
- AND the next time user visits SalesInvoice, "Comptabilitat" loads automatically

### SC-04: User deletes a non-default view

- GIVEN user has views "General" (default) and "Temp View" for page "Budgets"
- WHEN user opens the Table View dialog
- AND selects "Temp View"
- AND clicks Delete
- AND confirms the deletion
- THEN "Temp View" is removed from the database
- AND "General" remains as the default
- AND the view dropdown no longer shows "Temp View"

### SC-05: User enters page and default view loads automatically

- GIVEN user has a default view "Admin" for page "Budgets"
- WHEN user navigates to /budgets (from any route)
- THEN the useUserTableViewStore loads views for user+page
- AND detects "Admin" as the default
- AND merges its ColumnConfig onto the base columns
- AND the table renders with "Admin" configuration without user interaction

### SC-06: User modifies columns without saving — changes do not persist

- GIVEN user is on the Budgets page with default columns
- WHEN user opens the Table View dialog
- AND hides column "Status"
- AND reorders columns
- AND closes the dialog WITHOUT clicking Save
- THEN the table reflects the changes for the current session only
- AND on page reload, columns return to the saved (or default) configuration
- AND no UserTableView record is created or modified

### SC-07: User with multiple views switches between them

- GIVEN user has 3 views for page "SalesInvoice": "General" (default), "Comptabilitat", "Logística"
- WHEN user opens the Table View dialog
- AND selects "Comptabilitat" and clicks Apply
- THEN columns update to "Comptabilitat" config
- WHEN user then selects "Logística" and clicks Apply
- THEN columns update to "Logística" config
- AND "General" remains the default for the next page visit

---

## 4. Acceptance Criteria

### Backend

- [ ] `UserTableView` entity created in `Domain/Entities/Auth/` with fields: Name (string, max 200), Page (string, max 250), ColumnConfig (string, max 8000), IsDefault (bool, default false), UserId (Guid FK), User (navigation)
- [ ] EF Core migration created and applies cleanly to PostgreSQL
- [ ] `IUserTableViewService` interface defines: GetAllByUserId, GetByUserIdAndPage, Create, Update, Delete, SetDefault
- [ ] `UserTableViewService` implements all operations with ILocalizationService for error messages
- [ ] `UserTableViewController` exposes: GET /api/usertableview/{userId}, GET /api/usertableview/{userId}/{page}, POST /api/usertableview, PUT /api/usertableview/{id}, DELETE /api/usertableview/{id}
- [ ] POST validates: Name required, Page required, ColumnConfig valid JSON, no duplicate Name+Page per user
- [ ] DELETE blocks deletion of default view with localized error
- [ ] SetDefault ensures exactly one default per user+page (transactional)
- [ ] All write operations return GenericResponse
- [ ] Service registered in ApplicationServicesSetup.cs
- [ ] Swagger shows all endpoints with correct request/response schemas

### Frontend

- [ ] `UserTableView` TypeScript interface defined in `src/types/index.ts` matching backend entity
- [ ] `UserTableViewService` extends BaseService<UserTableView> with endpoint "usertableview"
- [ ] `useUserTableViewStore` Pinia store with actions: loadViews(userId, page), activateView(view), saveView(view), deleteView(id), setDefault(viewId), applyView(page, baseColumns)
- [ ] `TableViewConfig.vue` dialog component renders with PrimeVue Dialog
- [ ] Dialog includes: column list with drag-reorder (PrimeVue OrderList or similar), visibility checkboxes, total checkboxes, view selector dropdown, Save/Delete/Set Default buttons
- [ ] `Column` interface in Table.vue extended with `visible?: boolean` and `order?: number`
- [ ] Table.vue filters columns by `visible` and sorts by `order` when present
- [ ] `applyView()` returns baseColumns when no saved view exists (zero-impact fallback)
- [ ] `applyView()` silently drops unknown fields from ColumnConfig
- [ ] `applyView()` appends base columns not present in ColumnConfig
- [ ] `pnpm run typecheck` passes with zero errors

### Integration

- [ ] Existing list views without saved views render identically to current behavior
- [ ] UserFilter state is NOT modified when activating/changing a table view
- [ ] View dialog is accessible via toolbar icon in list views (opt-in, does not alter default UX)
- [ ] `dotnet build` passes with zero errors
- [ ] All UI strings in Catalan

---

## 5. Exclusions (Out of Scope)

- Shared or role-level views (views accessible by multiple users or roles)
- View export/import functionality
- Any modification to UserFilter entity or its behavior
- Column width/resize persistence
- Column sort state persistence (already handled by PrimeVue multiSort)
- Mobile-specific view adaptations
- Snapshotting filter values into a view
- View templates or preset views provided by the system

---

## 6. Dependencies

### Backend
- EF Core migration: `dotnet ef migrations add AddUserTableView --project src/Infrastructure/`
- New entity follows Pattern A (generic repository) — no custom repository needed
- Requires `ILocalizationService` entries: "TableViewNameExists", "CannotDeleteDefaultView"

### Frontend
- PrimeVue OrderList component (available in PrimeVue 4) — for drag-reorder in TableViewConfig dialog
- No new external npm dependencies required
- Existing `BaseService<T>`, Pinia, and Composition API patterns reused

### Cross-cutting
- ColumnConfig JSON schema must be consistent between backend validation and frontend serialization
- `Page` string values must match existing UserFilter.Page convention (e.g., "Budgets", "SalesInvoice")

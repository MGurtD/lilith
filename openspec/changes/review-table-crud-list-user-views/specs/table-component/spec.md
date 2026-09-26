# Delta for table-component

## MODIFIED Requirements

### Requirement: Preset System

The `Table.vue` component MUST apply `sortMode="single"` as the default whenever `preset="crud-list"` is used. Preset defaults MUST be merged before explicit component props and `$attrs`; an explicit consumer value MUST take precedence. All other existing `crud-list` defaults remain unchanged.

#### Scenario: CRUD list uses single sorting by default

- GIVEN a consumer uses `<Table preset="crud-list">` without an explicit sort mode
- WHEN the component renders
- THEN the underlying DataTable uses `sortMode="single"`
- AND the existing CRUD-list defaults remain applied

#### Scenario: Multiple sorting overrides the preset

- GIVEN an invoice view uses `<Table preset="crud-list" sortMode="multiple">`
- WHEN the component renders
- THEN the underlying DataTable uses `sortMode="multiple"`
- AND the `crud-list` default MUST NOT replace the explicit multiple-sort value

## ADDED Requirements

### Requirement: Explicit UserTableView Page Namespaces

Every current `preset="crud-list"` consumer participating in UserTableViews MUST pass an explicit `page` prop. The `page` value MUST remain consumer-specific and MUST NOT be inferred from the preset. The eight current instances are `Budgets`, `SalesInvoices`, `SalesInvoicesByDates`, `SalesOrders`, `DeliveryNotes`, both tables in `Customers` (`Customers` and `CustomerTypes`), and `TableReferences`.

#### Scenario: All current CRUD-list views participate in UserTableViews

- GIVEN the current eight CRUD-list instances are reviewed
- WHEN the change is applied
- THEN each instance passes its explicit page namespace
- AND UserTableView behavior remains scoped to that namespace

#### Scenario: SalesOrders receives its missing namespace

- GIVEN `SalesOrders.vue` uses `preset="crud-list"` without `page`
- WHEN the consumer is updated
- THEN it passes `page="SalesOrders"`
- AND its existing filters, sorting field, delete policy, and row navigation remain unchanged

#### Scenario: A page-less consumer is not assigned a namespace

- GIVEN a non-view consumer intentionally omits `page`
- WHEN the component renders
- THEN presentation defaults still apply
- AND the component MUST NOT infer or manage UserTableViews

### Requirement: Safe Removal of Redundant CRUD-list Props

Consumers MAY remove declarations whose values exactly match `crud-list` defaults, including `scrollable`, `scrollHeight="flex"` or an equivalent binding, and `sortMode="single"`. Removing those declarations MUST NOT change behavior. Non-equivalent values and intentional overrides MUST remain explicit.

#### Scenario: Exact redundant props are removed

- GIVEN a consumer repeats only exact preset values
- WHEN those declarations are removed
- THEN the DataTable retains the same scrolling, height, and single-sort behavior
- AND filters, selection, deletion, and navigation remain unchanged

#### Scenario: Custom table behavior is preserved

- GIVEN a consumer sets `sortMode="multiple"` or a non-`"flex"` scroll height
- WHEN redundant defaults are cleaned up
- THEN the explicit custom value remains active

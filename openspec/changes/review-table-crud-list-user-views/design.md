# Design: Review Table CRUD List User Views

## Technical Approach

Centralize `sortMode="single"` inside `Table.vue`'s `crud-list` preset. Reuse the existing `preset → explicit props → attrs` merge so consumers that need multiple sorting or custom scroll heights continue to override cleanly. Activate `page="SalesOrders"` so all eight `crud-list` instances participate in UserTableViews, and remove only exact preset-equivalent declarations from the affected consumers.

## Architecture Decisions

### Decision: Reuse the existing merge order

**Choice**: Add `sortMode` to `PRESET_DEFAULTS["crud-list"]`; keep `{ ...presetRest, ...explicit, ...attrs }` unchanged.

**Alternatives considered**: Add a new resolver or bind `sortMode` explicitly.

**Rationale**: The existing merge already guarantees that `sortMode="multiple"` remains an override while the preset supplies the default.

### Decision: Keep `sortMode` in `$attrs`

**Choice**: Do not promote `sortMode` to a declared prop.

**Alternatives considered**: Add a typed prop and explicit merge branch.

**Rationale**: `sortMode` is currently passed through `$attrs`; changing its declaration is unnecessary and would expand the public API without improving this refactor.

### Decision: Keep `page` consumer-supplied

**Choice**: Add the namespace only to `SalesOrders.vue`.

**Alternatives considered**: Infer or assign `page` from `crud-list`.

**Rationale**: `page` identifies a variable persistence namespace and is not a presentation default. The component must continue allowing presentation-only consumers without UserTableViews.

### Decision: Remove only exact duplicates

**Choice**: Remove `sort-mode="single"`, `scrollable`, `scrollHeight="flex"`, and equivalent bindings only where they exactly match the preset; delete the two now-unused customer height constants.

**Rationale**: Intentional overrides such as multiple sorting, multiple selection, and non-flex heights remain explicit and preserve behavior.

## Data Flow

```text
<Table preset="crud-list" ...>
  → PRESET_DEFAULTS["crud-list"] includes sortMode: "single"
  → resolvedDataTableProps merges preset, explicit props, then attrs
  → sortMode="multiple" attrs override the preset when supplied
  → DataTable receives the effective props

Table with page="SalesOrders"
  → ensureDefault/loadDefaultView use the SalesOrders namespace
  → existing UserTableView configuration and persistence become active
```

## File Changes

| File | Action | Description |
|---|---|---|
| `frontend/src/components/tables/Table.vue` | Modify | Add `sortMode: "single"` to the `crud-list` defaults. |
| `frontend/src/modules/sales/views/SalesOrders.vue` | Modify | Add `page="SalesOrders"`; remove explicit single sort mode. |
| `frontend/src/modules/sales/views/SalesInvoicesByDates.vue` | Modify | Remove redundant `scrollable` and flex scroll height; keep multiple sort and selection. |
| `frontend/src/modules/sales/views/Customers.vue` | Modify | Remove redundant flex height bindings and unused constants. |
| `frontend/src/modules/sales/views/DeliveryNotes.vue` | Modify | Remove explicit single sort mode. |

`Budgets.vue`, `SalesInvoices.vue`, and `TableReferences.vue` require no changes.

## Interfaces / Contracts

```typescript
"crud-list": {
  selectionMode: "single",
  paginator: "auto",
  rows: 20,
  scrollable: true,
  scrollHeight: "flex",
  stripedRows: true,
  rowHover: true,
  sortMode: "single",
}
```

The existing attribute precedence remains: preset defaults → declared explicit props → `$attrs`.

## Testing Strategy

| Layer | What to verify | Approach |
|---|---|---|
| Type safety | Consumer and preset changes compile | `pnpm run typecheck` |
| Build | Production bundle remains valid | `pnpm run build` |
| Override behavior | Invoice tables retain multiple sorting and selected rows | Inspect diff and manual smoke if available |
| UserTableViews | SalesOrders shows configuration and uses its namespace | Manual navigation with an authenticated user |
| Cleanup | No exact preset duplicates remain in reviewed consumers | Search and `git diff` review |

## Threat Matrix

N/A — frontend-only refactor; no routing, shell, subprocess, VCS/PR automation, executable classification, or process-integration boundary is introduced.

## Migration / Rollout

No schema or backend migration is required. The first visit to SalesOrders may create its existing per-user default view through `ensureDefault`; rollback is a frontend revert and leaves that row harmlessly unused.

## Open Questions

None blocking.

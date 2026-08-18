---
name: migrate-datatable-to-table
description: Audit or migrate Lilith Vue code from raw PrimeVue DataTable to the internal Table.vue. Use when evaluating compatibility, preserving table behavior, implementing a migration, or planning support for a missing Table.vue feature.
compatibility: OpenCode with Node 20.19+ and pnpm 10.
---

# Migrate DataTable To Table.vue

Never rely on a static feature matrix. Inspect the current component and current table implementation on every invocation.

## Audit

1. Read the target component completely and inspect the current `Table.vue`, table types, presets, filters, and persisted-view support that affect it.
2. Inventory columns, body/footer slots, filters and header actions, row actions, totals, sorting, pagination/scroll, selection, editing, attachments, and persisted configuration.
3. Inspect tightly owned children only when they render the table for the target; keep delegated child tables out of scope unless explicitly included.
4. Classify each behavior as directly supported, adaptable, or unsupported. Explain a required unsupported behavior before editing.

## Decide

- Migrate when all required behavior can be preserved directly or through a supported adaptation.
- If support is missing, ask whether to extend `Table.vue`, remove obsolete behavior, write a plan/issue, or stop. Do not silently drop behavior or invent a workaround.
- For iterative migrations, change one coherent view, run the relevant automated checks, then stop for the user's manual validation before continuing.

## Implement

- Use `crud-list` for a CRUD listing when its current behavior matches that purpose. Use another existing preset when it is a closer fit; override only real differences.
- Define typed, locale-reactive column and filter configuration using current APIs.
- Keep list titles and create actions in the table's integrated header when the current API supports them. Prefer the standard create event over a separate floating button.
- Do not use a filter slot merely as layout without checking its side effects. A title-only header must not accidentally display apply or clear actions; extend the base API rather than hiding unintended controls locally when necessary.
- Use the table's delete event and `canDelete` predicate instead of inspecting click-target class names when those APIs preserve the behavior.
- Base lifecycle permissions on status identifiers such as `initialStatusId`, never on localized status names.
- Check system-column geometry. Use the current configurable delete/attachment width API when available and keep widths consistent in body and footer.
- Remove dead PrimeVue imports and event types.

## Columns And Totals

- Prefer declarative `field` paths such as `workOrder.code` over body slots that only read nested data. Apply the same approach to totals when the current aggregation implementation supports nested paths.
- Prefer a column `resolver(value, row)` for a calculated scalar display. Return the raw value expected by `columnType` when possible, for example a date from a date resolver, and let `Table.vue` format it.
- Use a synthetic field with a row-aware resolver when the displayed value is derived from several fields or a collection. Ensure the resolver handles missing data without throwing.
- Keep a forwarded body slot for interactive controls, components, rich markup, or behavior not supported by a column type. Do not replace an editable cell with a display resolver.
- Confirm which specialized column types invoke resolvers. Boolean and progress columns currently have dedicated rendering paths; extend the shared component or keep a slot if they need calculated values.
- Replace manual footer reductions and footer slots with `total` and `totalFormat` when the total is a supported aggregation over the table's actual `items`. Preserve labels and currency/date formatting through `totalFormat`.
- Preserve numeric alignment in both body and footer. If the shared footer does not inherit column styles, fix that globally rather than adding repeated local footer CSS.
- Keep lookup resolvers distinct from filter-value resolvers. Calculated display resolvers should not automatically format persisted filter values unless that behavior is explicitly supported.

## Shared Fixes

- Fix layout defects in the owning shared component when the expected behavior is global; do not add per-view CSS for a base-component defect.
- For integrated-header alignment, preserve filter field wrappers at their intended bottom alignment while vertically centering simple `prepend` content such as titles.
- Keep shared API defaults backward compatible for existing consumers. New sizing or layout options should retain the previous visual value unless explicitly overridden.
- When broadening a shared callback such as `resolver`, preserve existing lookup call sites and narrow `unknown` values before formatting.

## Verify

Run `pnpm run typecheck` from `frontend/`. Run `pnpm run build` after changing shared table components, table types, presets, or routes.

For each migrated view, manually verify:

- The integrated header contains the intended title and actions only.
- Header fields and actions align correctly on desktop and mobile.
- Create, row click, and delete do not trigger one another.
- Nested and calculated cells, lookup labels, date/currency formatting, and totals match the original values.
- Conditional delete visibility, system-column widths, filtering, sorting, and pagination match the original behavior.

Create a GitHub issue only when the user explicitly asks. Otherwise return the proposed gap, API impact, affected files, and acceptance criteria in the response.

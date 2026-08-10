---
name: migrate-datatable-to-table
description: Audit or migrate Lilith Vue code from raw PrimeVue DataTable to the internal Table.vue. Use when evaluating compatibility, preserving table behavior, implementing a migration, or planning support for a missing Table.vue feature.
compatibility: OpenCode with Node 20.19+ and pnpm 10.
---

# Migrate DataTable To Table.vue

Never rely on a static feature matrix. Inspect the current component and current table implementation on every invocation.

## Audit

1. Read the target component completely.
2. Inspect `Table.vue`, `TableFilter.vue`, `TableViewConfig.vue`, presets, and table types.
3. Inventory columns, custom body slots, filters, header actions, row actions, sorting, selection, expansion, totals, pagination, scrolling, lazy/server data, editing, context menu, frozen columns, attachments, and persisted view configuration.
4. Map each behavior as direct support, supported adaptation, or unsupported.
5. Present the compatibility result before editing when required behavior is unsupported or user intent is ambiguous.

## Decide

- Migrate when all required behavior can be preserved.
- Adapt when the internal component offers an equivalent supported pattern.
- If support is missing, ask whether to extend `Table.vue`, remove obsolete behavior, write a plan/issue, or stop. Do not silently drop behavior or invent a workaround.
- A coherent view plus its tightly owned child may be migrated together when needed.

## Implement

- Define typed column and filter configuration using current table APIs.
- Preserve unsupported-by-type rendering with the current forwarded slot convention.
- Use an existing preset that matches the table purpose; override only real differences.
- Map create, delete, row click, totals, attachments, and persisted views through current APIs.
- Keep i18n keys reactive and preserve existing authorization checks.
- Remove dead PrimeVue imports and event types.

## Verify

Run `pnpm run typecheck` from `frontend/`. Run the build when shared table components or routes change. Manually compare filtering, sorting, pagination/scroll, navigation, row actions, slots, totals, attachments, and persisted view behavior that apply to the target.

Create a GitHub issue only when the user explicitly asks. Otherwise return the proposed gap, API impact, affected files, and acceptance criteria in the response.

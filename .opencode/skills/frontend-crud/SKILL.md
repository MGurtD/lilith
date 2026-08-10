---
name: frontend-crud
description: Add or extend a Lilith Vue frontend CRUD flow. Use when creating entity types, services, Pinia stores, list/detail views, forms, dialogs, tables, or lazy routes under frontend/src/modules.
compatibility: OpenCode with Node 20.19+ and pnpm 10.
---

# Frontend CRUD

Use current domain code as the template. Do not paste generic screens.

## Discover

1. Read root and `frontend/AGENTS.md`.
2. Inspect the backend contract and the closest frontend feature with similar lifecycle, relationships, and UI behavior.
3. Trace route, view, owned components, store, service, and TypeScript types before editing.
4. Identify date fields, localization namespaces, permissions, filters, and required table behavior.

## Implement

1. Define types that match the actual API nullability and value shapes. Use `Date` where a DatePicker owns the value.
2. Extend `BaseService<T>` only for conventional CRUD. Use `apiClient` directly for custom endpoints following a current service analogue.
3. Add a Pinia store only for state shared across the flow. Preserve the module's naming and refresh behavior.
4. Build list/detail/forms with Composition API and typed props/emits.
5. Prefer internal `Table.vue` for list views only when it supports every required behavior. Load `migrate-datatable-to-table` for an existing raw DataTable.
6. Reuse the feature's validation framework and PrimeVue 4 component APIs.
7. Add lazy routes and required route metadata using the domain's current pattern.
8. Add every user-facing key to `ca`, `es`, and `en`. Do not introduce literal Catalan UI.

## Dates And State

- Convert API date strings to native `Date` at the established feature boundary.
- Send native Date values directly for ordinary request bodies; existing serialization handles them.
- Use `formatDateForQueryParameter` for query filters.
- Clone dialog records when cancel must not mutate source state.
- Generate a frontend GUID only when the current API flow expects the client to assign it.

## Verify

From `frontend/` run:

```bash
pnpm run i18n:check
pnpm run typecheck
```

Run `pnpm run build` for routes, shared components, configuration, or broad changes. Use a relevant smoke check when the behavior is covered.

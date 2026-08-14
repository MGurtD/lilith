# Lilith Frontend Agent Guide

Canonical rules for work under `frontend/`. When OpenCode starts from this directory, also read `../AGENTS.md` for repository-wide safety and backend/frontend contracts.

## Stack And Commands

- Vue 3.5 with Composition API and `<script setup>`.
- TypeScript 6 in strict mode.
- Vite 8, Pinia 2, PrimeVue 4, Axios, and Vue i18n 11.
- pnpm `10.28.0` is required.

Run from `frontend/`:

```bash
pnpm run dev              # localhost:8100
pnpm run i18n:check       # locale parity and placeholder validation
pnpm run i18n:audit       # full localization audit
pnpm run typecheck
pnpm run build
pnpm run build-development
pnpm run build-preprod
pnpm run smoke
pnpm run smoke:e2e
```

There is no frontend unit/component test framework and no lint script. Smoke and Playwright E2E checks are available.

## Structure

- `src/api/`: API client and generic `BaseService<T>`.
- `src/components/`: shared application components.
- `src/modules/<domain>/`: routes, views, components, services, stores, and types owned by a domain.
- `src/services/` and `src/store/`: cross-domain services and stores.
- `src/i18n/`: Vue i18n setup and `ca`, `es`, `en` dictionaries.
- `src/utils/`: shared utilities; inspect before adding another helper.

## Vue And TypeScript

- Use Composition API with `<script setup>` for Vue components.
- Keep component filenames and imports in PascalCase.
- Type props, emits, service responses, and event payloads. Prefer `unknown` plus narrowing over new `any` usage.
- Use `@/` for shared or cross-module imports. Relative imports are acceptable for tightly owned neighboring files.
- Extract components when it improves ownership or readability; do not enforce an arbitrary line limit.
- Preserve established direct `storeToRefs` form editing where the surrounding feature uses it. Do not introduce a new state architecture incidentally.

## Services And Stores

- Use `BaseService<T>` for conventional entity CRUD when it fits the endpoint contract.
- Specialized APIs such as authentication, reports, language, and custom operations may use dedicated clients.
- Do not invent BaseService helpers. Inspect `src/api/base.service.ts` and a current service in the same domain.
- Stores own shared state and orchestrate service calls. Refresh state after writes only when the screen contract requires it.
- Clone records before editing in a dialog when cancelling must leave source state unchanged.
- Keep numeric defaults aligned with the backend model and the nearest analogous form.

## Localization

- Add user-facing text through Vue i18n. Every new key must exist in `ca.ts`, `es.ts`, and `en.ts` with matching placeholders.
- Write every new translation key in English, with camelCase for each namespace segment.
- Catalan is the source/default locale, not a reason to hardcode visible Catalan text.
- Use stable semantic keys in the existing feature namespace. Reuse `common.*` only when meaning matches exactly.
- Keep translated titles and option labels reactive to locale changes; use template calls or computed values.
- PrimeVue built-in strings come from the configured PrimeVue locale.
- Use the localization audit skill for screen-level audits and translations.

## Dates

- PrimeVue DatePicker models must be native `Date` values, not display-formatted strings.
- Convert API date strings to `Date` at the store or feature boundary used by the analogous flow.
- Native `Date` values in request bodies are serialized by the existing `Date.prototype.toJSON` override. Do not mutate reactive date fields solely to serialize them.
- Use `formatDate()` only for display.
- Use `formatDateForQueryParameter()` for date query parameters.
- `convertDateTimeToJSON()` remains for explicit non-reactive payload construction; do not apply it mechanically.

## UI And Routing

- Prefer shared components such as `Table.vue` when they preserve required behavior. Audit unsupported table features before replacing raw PrimeVue `DataTable`.
- PrimeVue components are globally registered unless the local code demonstrates otherwise.
- Use PrimeVue components rather than raw controls when an established equivalent exists.
- Lazy-load route views. Follow the route naming, metadata, and authorization pattern of the current domain.
- Keep HTTP URLs in services and environment configuration, never in components.
- Validate forms with the established Yup/FormValidation or VeeValidate pattern used by that feature; do not mix validation frameworks inside one flow.

## Verification

- Translation-only change: scoped i18n audit, `pnpm run i18n:check`, and `pnpm run typecheck`.
- Component/store/service change: `pnpm run typecheck` and the relevant smoke check when practical.
- Route, build configuration, shared component, or broad refactor: `pnpm run build`.
- Review warnings from the localization auditor manually; heuristic hardcoded-text findings may include domain or technical values.

Environment variables are documented in `.env.example`. Never commit secrets or local `.env` values.

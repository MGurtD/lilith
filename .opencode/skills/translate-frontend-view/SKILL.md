---
name: translate-frontend-view
description: Translate a complete Lilith Vue screen to ca/es/en. Use when replacing hardcoded user-facing text, completing a partially localized view, fixing literal translation fallbacks, or validating reactive language changes in a view and its owned children.
compatibility: OpenCode with frontend dependencies installed by pnpm.
---

# Translate A Frontend Screen

Translate the visible screen boundary, not only the named file. Preserve unrelated work and do not broaden to shared components without validating every consumer.

## Workflow

1. Read `frontend/AGENTS.md`, inspect worktree status, and resolve route, view, and screen-owned children.
2. Load `audit-frontend-localization` and capture a scoped baseline.
3. Inventory template labels, visible props, toasts, confirmations, validation, titles, menus, and accessible names.
4. Exclude identifiers, routes, CSS, payload values, business data, and persisted lifecycle/status values.
5. Reuse a key only when meaning matches. Otherwise extend the current feature namespace with semantic English names. Every segment of a new key must use camelCase; do not copy localized, kebab-case, snake_case, or sentence-shaped legacy keys.
6. Add each key to `ca.ts`, `es.ts`, and `en.ts` with identical placeholders. Catalan is the source; Spanish and English must be natural translations.
7. Use `t(...)` in touched script/template code and interpolation for dynamic values.
8. Keep translations reactive with direct template calls or computed values. Do not store a one-time translated title when locale changes must update it.
9. Let the PrimeVue locale handle built-in component text.
10. Run the scoped strict audit again and manually resolve or explain every remaining hardcoded-text warning.

## Verify

From `frontend/` run:

```bash
pnpm run i18n:check
pnpm run typecheck
```

Report the translated boundary, namespaces, validation results, and justified remaining warnings. Do not claim complete localization while unresolved user-facing literals remain.

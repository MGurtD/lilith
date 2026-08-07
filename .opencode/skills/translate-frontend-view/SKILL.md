---
name: translate-frontend-view
description: Translate a complete Lilith Vue 3 frontend screen with the existing Vue i18n, Pinia language, and PrimeVue locale utilities. Use when converting hardcoded user-facing text in a requested view and its screen-owned child components to Catalan, Spanish, and English; completing a partially localized screen; replacing literal translation fallbacks; or validating that a screen changes language consistently without altering backend localization resources.
---

# Translate Frontend View

Translate the complete visible screen, not only the named `.vue` file. Preserve unrelated work and do not broaden the change to a whole module unless the user asks.

## 1. Establish the boundary

1. Resolve the repository root and inspect `git status --short` before editing.
2. Follow the repository CodeGraph instructions: locate the route, requested view, imported components, and callers before filesystem-wide searches. If graph results conflict, verify the concrete route and import declarations with targeted source reads and treat current source as authoritative.
3. Include components owned exclusively by the screen when they render its labels, dialogs, tables, or messages.
4. Exclude shared components unless the same translation is correct for every consumer. Record excluded shared components in the completion summary when they leave visible debt.

## 2. Inventory user-facing text

Inspect template text and visible props such as `label`, `header`, `placeholder`, `title`, `aria-label`, `tooltip`, and empty-state messages. Inspect script values used by:

- Toasts and global toasts.
- Confirmation prompts.
- Dialog and menu titles.
- Client-side validation.
- Computed labels and dynamic messages.
- Existing accessible names for icon-only controls. If an icon-only action has no accessible name, add a localized aria-label or tooltip without changing its behavior.

Do not translate identifiers, routes, URLs, CSS values, API payload fields, business data, file names, or lifecycle/status values stored in the database.

Run the audit skill over every file in scope before editing to capture the baseline:

    node .opencode/skills/audit-frontend-localization/scripts/audit-i18n.mjs --scope <view> --scope <owned-child>

## 3. Design keys

1. Reuse a `common.*` key only when wording and meaning match exactly in every context.
2. Extend the existing feature namespace when one exists.
3. Otherwise create a lowerCamel namespace from the view name, for example `phaseTemplates`.
4. Group keys by behavior when useful: `title`, `filters`, `columns`, `actions`, `dialogs`, `messages`, and `validation`.
5. Prefer stable semantic names such as `actions.create`; do not encode Catalan wording or component position in a key.
6. Keep frontend keys separate from backend JSON localization keys. The systems share only the culture code and `Accept-Language` contract.

## 4. Apply translations

1. Add every new key in the same logical location in:
   - `frontend/src/i18n/ca.ts`
   - `frontend/src/i18n/es.ts`
   - `frontend/src/i18n/en.ts`
2. Use Catalan as the functional source and produce natural Spanish and English translations.
3. Preserve the same named placeholders in all locales. Prefer named Vue i18n placeholders such as `{name}`.
4. In newly touched `<script setup>` code, import `useI18n` and declare `const { t } = useI18n()`.
5. Use `t(...)` from both script and template for new replacements. Do not mechanically rewrite existing `$t(...)` outside touched code.
6. Pass dynamic values through interpolation, for example `t("orders.messages.deleted", { number })`.
7. Remove literal `||` or `??` fallbacks only when the referenced key is added to all locales.
8. Keep translated values reactive when the locale changes. Prefer direct template bindings or computed translation values; for titles persisted into stores, update them from a locale-aware watcher instead of storing a one-time translation result.
9. Let `applyPrimeVueLocale` translate PrimeVue built-ins; do not duplicate PrimeVue locale strings in application dictionaries.
10. Preserve UTF-8, BOM state, and existing newlines. Never broadly recode locale files to fix an isolated string.

## 5. Validate and report

Run strict validation over the complete screen boundary:

    node .opencode/skills/audit-frontend-localization/scripts/audit-i18n.mjs --strict --scope <view> --scope <owned-child>

Then run:

    cd frontend
    pnpm run typecheck

Finally:

1. Review `git diff` and confirm all new keys exist in `ca`, `es`, and `en`.
2. Verify interpolation and placeholders in each locale.
3. Distinguish new findings from pre-existing global localization debt.
4. Summarize the translated screen, owned children, reused/new namespaces, validation results, and any intentionally excluded shared text.
5. Do not commit, push, launch the runtime, or modify backend localization unless explicitly requested.

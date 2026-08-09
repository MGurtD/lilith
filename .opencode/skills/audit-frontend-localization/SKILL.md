---
name: audit-frontend-localization
description: Audit Lilith frontend localization without editing source. Use when checking ca/es/en parity, placeholders, missing static keys, literal fallbacks, unused keys, or hardcoded user-facing text globally or for a Vue screen boundary.
compatibility: OpenCode with frontend dependencies installed by pnpm.
---

# Audit Frontend Localization

This workflow is read-only. Report evidence; do not fix findings unless the user separately requests changes.

## Run

From the repository root:

```bash
node frontend/scripts/audit-i18n.mjs --strict
node frontend/scripts/audit-i18n.mjs --scope frontend/src/path --strict
node frontend/scripts/audit-i18n.mjs --scope frontend/src/path --format json
```

Repeat `--scope` for a view and each screen-owned child. Default scope is `frontend/src`. Exit code `1` means strict localization errors; `2` means invalid arguments or execution failure. Warnings do not fail strict mode.

## Classify

Errors:

- A key is missing from any of `ca`, `es`, or `en`.
- Placeholders differ between locales.
- A static translation reference has no dictionary key.
- A locale dictionary has duplicate, unsupported, or non-string values.

Warnings requiring review:

- Literal translation fallback.
- Likely hardcoded template text, visible attribute, toast, dialog, or message property.
- Globally unused static key; dynamic key construction can be a false positive.

Do not classify identifiers, routes, URLs, CSS values, API payloads, business data, file names, or persisted lifecycle/status values as UI translations.

## Screen Audit

1. Resolve the route, view, and screen-owned children.
2. Run a baseline audit over the complete boundary.
3. Review every warning manually; use JSON if text output is truncated.
4. Report findings by severity with file and line, distinguishing scoped findings from global debt.

After changing the auditor itself, run from `frontend/`:

```bash
pnpm run i18n:audit:test
```

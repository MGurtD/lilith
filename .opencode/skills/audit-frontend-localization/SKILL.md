---
name: audit-frontend-localization
description: Audit Lilith frontend localization without changing source files. Use when checking Vue i18n key parity across Catalan, Spanish, and English; finding missing static translation references, placeholder mismatches, literal translation fallbacks, unused keys, or likely hardcoded user-facing text; validating a translated view and its owned child components; or producing a global localization-debt report.
---

# Audit Frontend Localization

Audit first and report evidence. Do not edit application files unless the user separately asks for fixes.

## Run the audit

Run from the repository root:

    node .opencode/skills/audit-frontend-localization/scripts/audit-i18n.mjs

Options:

- `--scope <file-or-directory>`: limit source scanning; repeat for a view and each owned child component. Default: `frontend/src`.
- `--format text|json`: select human-readable or machine-readable output. Default: `text`.
- `--strict`: exit with code `1` when localization errors exist. Warnings never fail strict mode.
- `--repo-root <path>`: override repository discovery, primarily for fixtures and isolated validation.

Exit codes: `0` for a completed report, `1` for strict-mode localization errors, and `2` for invalid arguments or execution failures.

## Interpret findings

Treat these as errors:

- A key is not present in every one of `ca.ts`, `es.ts`, and `en.ts`.
- The named or positional placeholders for the same key differ by locale.
- A statically referenced `t(...)`, `$t(...)`, or `i18n.global.t(...)` key is missing.
- A locale dictionary contains a duplicate or unsupported property shape.

Treat these as warnings requiring human review:

- A translation call falls back to a literal with `||` or `??`.
- Template text, a visible component attribute, or a toast/dialog property appears hardcoded.
- A global audit finds a key with no static reference. Dynamic key construction can make this a false positive.

Do not classify domain data, identifiers, routes, URLs, CSS values, status values persisted in the database, or API payload values as UI translations.

## Validate a translated screen

1. Resolve the screen boundary before auditing: include the requested view and its screen-owned children, but not unrelated shared components.
2. Pass one `--scope` for every file or common owning directory.
3. Run with `--strict --format text`.
4. Review hardcoded-text warnings manually; heuristics deliberately prefer false positives over missed visible text.
5. Run `pnpm run typecheck` from `frontend/` after the localization audit passes.

## Maintain the auditor

Run its deterministic tests after changing the script:

    node .opencode/skills/audit-frontend-localization/scripts/audit-i18n.test.mjs

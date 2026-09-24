# Form Migration Record

Use this template as the working audit for a legacy form before changing its
implementation. Keep factual observations separate from proposed changes.
Retain a completed record only while it contains unresolved gaps or verification
work. Once validated, promote reusable findings to `README.md` or the
`frontend-form` skill and remove campaign-specific history.

## Scope

- Legacy component:
- Parent consumers:
- Domain entity or DTO:
- Migration owner:
- Status: `inventory | ready | in progress | migrated | blocked | excluded`

## Current Contract

- Props:
- Emits:
- Exposed methods used by parents:
- State owner: `local | prop | Pinia | parent`
- Does the parent consume the submit payload?:
- Does editing mutate the source before submit?:
- What does cancel or dialog close do today?:

## Field Map

| Model field | Legacy control | `FormFieldType` | Default | Validation | Disabled/visible condition | Side effects | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| | | | | | | | |

Record hidden fields that must survive submit, such as IDs, foreign keys,
timestamps, lifecycle state, or nested metadata.

## Behavior Inventory

- Watchers and computed setters:
- Fields updated by another field:
- Asynchronous loads:
- Asynchronous calculations:
- Can asynchronous responses overlap, and how are stale responses bounded?:
- Can programmatic setters retrigger source callbacks?:
- Custom controls and emitted values:
- Nested forms, tables, or collections:
- Does a parent-owned collection change while scalar edits are unsaved?:
- Create/edit action differences:
- Toasts or validation summaries:
- Navigation after submit:
- Loading and disabled behavior:

## Date Contract

| Field | API type | UI type before migration | Required UI type | Request behavior |
| --- | --- | --- | --- | --- |
| | | | | |

- Boundary that converts API strings to `Date`:
- Display-only formatting:
- Legacy submit-time mutations to remove or preserve:

## Compatibility Assessment

- Fields supported directly:
- Fields requiring slots:
- Required cross-field updates:
- Required dynamic visibility or disabled state:
- Required external validation:
- Required request sequencing or callback suppression:
- State that must remain outside the form and reset with the input entity:
- Missing `Form.vue` capability:
- Can the migration proceed without changing `Form.vue`?:
- Difficulty: `low | medium | high | very high`

Do not extend `Form.vue` with feature-specific rules. Record a missing generic
capability here and validate it against another real use case first.

## Intended Contract

- `initialValues` source:
- Typed payload produced from `FormValues`:
- Shared value helpers and explicit fallbacks:
- Domain-specific guards or normalization kept in the feature:
- Parent handler changes:
- Custom field slots:
- Action strategy: `default | actions slot | external submit`
- Cancel strategy:
- Behavior changes accepted explicitly:

## Acceptance Matrix

| Scenario | Expected result | Verified |
| --- | --- | --- |
| Create with valid values | | [ ] |
| Edit with valid values | | [ ] |
| Submit invalid values | Inline errors and no submit | [ ] |
| Correct values after invalid submit | Errors update and submit succeeds | [ ] |
| Cancel | Internal edits are discarded | [ ] |
| Close dialog without saving | Source data remains unchanged | [ ] |
| Hidden fields | Preserved in emitted payload | [ ] |
| Loading/disabled | No duplicate or forbidden submit | [ ] |
| Change locale while open | Labels, options, and errors update | [ ] |
| Date fields | Use native `Date` values | [ ] |
| Custom fields | Value, disabled state, and label are correct | [ ] |

Add feature-specific scenarios for calculations, lifecycle restrictions,
asynchronous failures, or nested records.

## Verification

- [ ] `pnpm run typecheck`
- [ ] `pnpm run i18n:check` when translations or translated configuration change
- [ ] Relevant production build for a shared or broad change
- [ ] Manual or focused E2E verification of the affected screen
- [ ] Parent consumers reviewed together with the form
- [ ] Migration findings added to the campaign record

## Findings

- What mapped directly:
- What required a custom slot:
- What required a parent change:
- What changed in cancel/edit semantics:
- Which `value-utils.ts` helpers and fallback contracts were used:
- Reusable pattern discovered:
- Anti-pattern discovered:
- Candidate improvement to `Form.vue`:
- Evidence for or against adding it to the future skill:

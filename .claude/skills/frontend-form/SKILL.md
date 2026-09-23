---
name: frontend-form
description: Create or migrate Lilith Vue forms with frontend/src/components/forms/Form.vue. Use when building a new declarative form, replacing legacy PrimeVue form markup, FormValidation, direct prop or Pinia editing, adding Yup validation, custom field slots, responsive form rows, or auditing whether Form.vue supports a workflow.
compatibility: Requires Vue 3, PrimeVue 4, Node 20.19+, and pnpm 10.
---

# Frontend Form

Use the current shared component contract, not remembered examples. Read these
files before editing:

- `frontend/src/components/forms/Form.vue`
- `frontend/src/components/forms/types.ts`
- `frontend/src/components/forms/value-utils.ts`
- `frontend/src/components/forms/README.md`

For a migration, also use
`frontend/src/components/forms/MIGRATION_TEMPLATE.md`. Use it as a working audit
for non-trivial forms. After validation, promote reusable findings to this skill
or the stable README and remove feature-campaign history that no longer carries
open decisions or verification work.

## Applicability Gate

1. Inventory the model, fields, validation, owner of state, parent consumers,
   exposed methods, actions, dates, custom controls, watchers, side effects,
   asynchronous work, and cancel behavior.
2. Classify each behavior as directly supported, adaptable through a field or
   actions slot, or unsupported by the current `Form.vue` contract.
3. Proceed directly for mostly flat forms whose fields are independent.
4. Treat dynamic visibility, asynchronous calculations, and nested collections
   as compatibility gaps that require an explicit ownership design. Async work
   can proceed when feature code can build payloads from callback snapshots,
   sequence overlapping requests, suppress recursive programmatic updates, and
   reset hidden state with the input entity. A reusable control with multiple
   flat outputs may use a section row when each output can be registered
   explicitly.
5. Extend `Form.vue` only for a generic capability demonstrated by real forms.
   Never add feature-specific business logic to the shared component.

## Create A New Form

1. Define typed props and emits. Pass the entity or DTO through
   `initialValues`; it is a deep-cloned snapshot, not `v-model`.
2. Define `computed<FormRowConfig[]>` rows whenever labels, options, or Yup
   messages use `t()` or reactive data.
3. Map controls to `FormFieldType` and carry over the PrimeVue props that affect
   behavior, such as options, option keys, date format, precision, locale, and
   currency.
4. Configure responsive rows and spans for the layout's actual needs. The
   supported ranges are mobile below `768px`, tablet from `768px`, and desktop
   from `1024px`. Use `columns: { mobile, tablet, desktop }` on rows and
   `span: { mobile, tablet, desktop }` on fields. Omitted tablet values inherit
   desktop values; do not repeat them unless the tablet layout differs.
5. Add defaults only for values absent from `initialValues`. Keep numeric and
   boolean defaults aligned with the backend and nearby feature code.
6. Use `disabled: (values) => boolean` when a field's enabled state depends only
   on current form values. Keep the predicate side-effect free; it controls
   presentation and must not clear or update sibling fields.
7. Add field-level Yup schemas for independent rules. Use `Yup.ref` when a
   simple cross-field rule fits the generated object schema. Pass a form
   resolver only when field schemas cannot express the rule; a resolver
   replaces, rather than merges with, field validation.
8. Implement a custom control with `#field-<name>` and connect its `value` and
   `setValue`. Pass an empty label when the legacy control renders its own, and
   propagate `disabled` to the actual interactive control.
9. For synchronous derived sibling values, define `onChange` only on source
   fields and call the exposed `setFieldValue`. Keep formulas feature-owned and
   do not attach callbacks to derived fields unless recursion is guarded.
10. For a reusable multi-field component, define a row `section`, list every flat
    field for registration/validation, and connect the controlled component to
    section `values`, `errors`, and `setValues`. Never mutate slot values.
    A section may remain registered while its slot conditionally renders from
    current `values`.
11. Use default actions when their labels and behavior match. Use the actions
   slot or an external template ref only for a real create/edit or workflow
   difference. On a full screen pass `page-actions`: Save moves to the header
   and Cancel is dropped (the header's back button leaves). Keep the default
   footer actions inside dialogs.
12. Narrow every `FormValues` field with the shared helpers from
   `frontend/src/components/forms/value-utils.ts` and construct the typed entity
   or DTO at the submit boundary. Choose explicit fallbacks for required,
   optional, nullable, and preserved values. Keep trimming, enum guards, and
   domain normalization in the feature. Preserve required hidden fields through
   `initialValues` and an explicit typed merge.
13. Make the parent persist the emitted payload. Do not expect the source prop or
   Pinia ref to contain internal edits.

## Migrate A Legacy Form

1. Read the form and every direct consumer. Complete the migration record before
   changing the implementation.
2. Identify whether the legacy form mutates a prop, selected table row, or Pinia
   state. Check whether each parent consumes the submit argument or silently
   rereads mutated state.
3. Map every visible and hidden field. Preserve IDs, foreign keys, lifecycle
   values, disabled flags, metadata, and fields omitted from the UI.
4. Preserve existing validation unless the user explicitly approves a behavior
   improvement. Replace manual `FormValidation`, error refs, invalid classes,
   and aggregated validation toasts with the shared inline validation when that
   change is accepted.
5. Preserve native `Date` values. Convert API strings at the established feature
   boundary, never with display formatting, and do not mutate reactive dates
   merely to serialize a request.
6. Reproduce conditional fields in computed rows only when the condition is
   stable while the form is open. Do not use row reconstruction as an unproven
   substitute for dynamic visibility when it depends on current form values.
7. Use a field-level disabled predicate for enabled state derived from current
   values. Do not use it for side effects or as a substitute for conditional
   validation.
8. Implement synchronous cross-field calculations through source-field
   `onChange` callbacks and the exposed field setter only when formulas remain
   in the feature component. Avoid callbacks on derived fields to prevent loops.
9. For asynchronous enrichment or calculations, build requests from the cloned
   callback snapshot, not the source prop. Keep a feature-owned request sequence
   and ignore stale responses. If programmatic resets, normalization, or remote
   results update fields that have `onChange`, suppress feature callbacks while
   applying those updates. Retain the latest dependent values needed after an
   `await` and reset isolated hidden state when the input entity changes.
10. When parent-owned collections can change independently, pass a stable scalar
   initial snapshot rather than the whole entity. Merge the latest collections
   at submit and exchange only narrowly required derived state through feature
   events or methods. Do not add generic form-state access solely for a parent.
11. For dialogs, clone selected rows when closing without save must leave the
   source unchanged. Treat the snapshot's discard-on-cancel behavior as an
   observable change and verify it.
12. Adapt custom selectors through named field slots when each selector owns one
    value. Use a section row for a controlled reusable component that owns
    several explicitly registered flat fields and granular errors. Keep section
    fields registered when conditional rendering depends on current form values.
13. Change the parent and child together when the state contract changes. A
    parameterless event handler may compile while ignoring the new payload and
    persisting stale data.
14. Replace local primitive `FormValues` conversion helpers with the shared
   `value-utils.ts` functions when their fallback semantics match. Do not move
   feature-specific enum guards, calculations, or normalization into the shared
   module.
15. Remove dead imports, validation state, toast dependencies, handlers, and
   bespoke layout only after equivalent behavior is wired through `Form.vue`.
16. Complete the working audit with parent changes, custom slots, behavior
    changes, gaps, and verification. Retain it only while unresolved information
    remains; otherwise promote durable guidance and remove the temporary record.

## Field Mapping

| Legacy control | Preferred field type |
| --- | --- |
| `BaseInput` / `InputText` | `Text` |
| Password input | `Password` |
| Numeric `BaseInput` / `InputNumber` | `Number` |
| Monetary `InputNumber` | `Currency` with explicit currency props |
| `Textarea` | `Textarea` |
| Standard `Select` | `Select` |
| Standard `MultiSelect` | `MultiSelect` |
| Binary `Checkbox` | `Checkbox` |
| `DatePicker` | `Date` |
| Feature selector or rich control | `Custom` plus a named slot |

## Proven Patterns

- `FormLifecycleTag.vue`: reactive rows, validation, custom slot, typed submit,
  and cancel.
- `FormTransportRate.vue`: flat fields, native dates, field Yup schemas, and a
  date range rule with `Yup.ref`.
- `FormExpenseType.vue`: replacing direct Pinia mutation and adapting the parent
  to consume the submitted snapshot.
- `FormSupplierReference.vue`: context-dependent custom selectors and a reusable
  parent serving two feature screens.
- `FormReceiptDetail.vue`: conditional registered section, async enrichment,
  remote calculations, callback suppression, stale-response sequencing, and
  hidden derived state.
- `FormPurchaseInvoice.vue`: stable scalar snapshot with independently owned
  collections, derived summary section, narrow parent events, and typed external
  workflow methods.

## Anti-Patterns

- Treating `initialValues` as `v-model`.
- Ignoring the submit payload and saving a stale prop or store ref.
- Defining local primitive fallback helpers already provided by
  `value-utils.ts`, or coercing arbitrary `unknown` values with `String`,
  `Number`, or `Boolean` at the submit boundary.
- Building translated rows once outside `computed`.
- Passing formatted strings to `DatePicker`.
- Mixing field validation and a resolver expecting them to merge.
- Using `Custom` without a matching slot.
- Mutating `initialValues` from a custom slot to update another field.
- Mutating section `values` or a controlled section component's `modelValue`.
- Attaching `onChange` to derived fields without guarding recursion.
- Applying programmatic values to source fields without suppressing feature
  callbacks, or accepting stale asynchronous responses.
- Including independently changing parent collections in `initialValues` and
  resetting unrelated unsaved scalar edits.
- Applying only visual disabled styling to a custom control.
- Rendering duplicate labels inside and outside a custom control.
- Dropping hidden entity fields during payload construction.
- Adding validation, action, or cancel behavior without recording the change.
- Extending `Form.vue` to hide one feature's unsupported behavior.

## Verify

From `frontend/`, run:

```bash
pnpm run i18n:check
pnpm run typecheck
```

Run `pnpm run build` after changing `Form.vue`, its shared types, routes, or a
broad set of consumers. Manually verify create, edit, invalid submit, correction
after invalid submit, valid submit, cancel, dialog close, hidden fields,
loading/disabled behavior, dates, custom controls, responsive layout, and locale
changes. Test every caller of a reusable form because current smoke tests do not
exercise these interactions.

# Form

`Form.vue` is the shared declarative wrapper around PrimeVue Forms. Use it for
forms whose fields can be described as rows and whose edits can remain isolated
until a valid submit.

The source contract is defined by [`Form.vue`](./Form.vue),
[`types.ts`](./types.ts), and [`value-utils.ts`](./value-utils.ts). This document
describes the current behavior; it is not a roadmap for capabilities the
component does not yet provide.

## When To Use It

Use `Form.vue` when:

- the model is mostly flat;
- fields are independent or a custom field only updates its own value;
- editing should not mutate the source object before submit;
- Yup field validation or a PrimeVue Forms resolver covers the rules;
- the layout can be represented as responsive rows and columns.

Audit the form before using it when it contains cross-field calculations,
asynchronous effects, conditional sections, nested collections, or custom
controls that update several properties. See
[`MIGRATION_TEMPLATE.md`](./MIGRATION_TEMPLATE.md).

## Public Contract

### Props

| Prop | Type | Default | Behavior |
| --- | --- | --- | --- |
| `rows` | `FormRowConfig[]` | Required | Defines fields and responsive layout. |
| `initialValues` | `object` | `{}` | Source for the internal initial snapshot. |
| `resolver` | `FormResolver` | `undefined` | Replaces the resolver generated from field validations. |
| `loading` | `boolean` | `false` | Sets loading/disabled state on the default actions only. |
| `disabled` | `boolean` | `false` | Disables native fields and default actions. |
| `showSubmit` | `boolean` | `true` | Shows the default Save action. |
| `showCancel` | `boolean` | `true` | Shows the default Cancel action. |

`loading` does not disable fields. A custom field must implement the supplied
`disabled` state itself.

### Events

```ts
(event: "submit", values: FormValues): void;
(event: "cancel"): void;
```

- `submit` is emitted only when the form is valid.
- `cancel` resets the internal form before it is emitted.
- `FormValues` is `Record<string, unknown>` and must be narrowed before it is
  passed to a typed store or service.

### Exposed Methods

```ts
submit(): void;
reset(): Promise<void>;
cancel(): Promise<void>;
setFieldValue(name: string, value: unknown): void;
setValues(values: FormValues): void;
getValues(): FormValues;
```

Use these methods through a template ref when an action outside the component
submits the form or when feature-owned change logic updates derived sibling
fields.

`getValues()` returns a detached copy of the current values, without
validating them. Use it only when a feature workflow must persist in-progress
edits together with another save, and expose a typed feature method rather
than the raw values to the parent. Example: the production phase screens save
unsaved header edits when a step or material is saved, because the reload
that follows would otherwise discard them (`FormWorkmasterPhase.currentPhase`).
Do not use it to replace the submit payload.

### Section Rows

Use a section row when one reusable component renders several flat form fields:

```ts
{
  section: "location",
  fields: [
    { name: "country", label: "Country", type: FormFieldType.Custom },
    {
      name: "address",
      label: "Address",
      type: FormFieldType.Custom,
      validation: Yup.string().required("Address is required"),
    },
  ],
}
```

`Form.vue` registers every field but delegates the complete row rendering to
`#section-<section>`. The section slot receives:

```ts
{
  values;
  states;
  errors;
  setFieldValue;
  setValues;
  disabled;
}
```

`errors` contains the first visible error for each section field after an
invalid submit. `setValues` updates registered flat fields through the same
snapshot/change pipeline as native controls. The section component must remain
controlled and must not mutate `values` or its `modelValue` prop.

## State Model

`initialValues` is an input snapshot, not `v-model`.

The initial snapshot is a deep clone of:

```ts
{
  ...fieldDefaults,
  ...initialValues,
}
```

Consequences:

- `initialValues` takes precedence over `defaultValue`;
- properties not rendered as fields, such as `id`, are retained on submit;
- editing does not mutate the source prop or Pinia object;
- the parent must save or apply the emitted payload;
- cancel discards internal edits;
- a deep external change to `initialValues` remounts and resets the form;
- changing only `rows` does not rebuild the initial snapshot.

Do not ignore the submit payload while expecting a directly edited prop or
store to contain the changes.

### Independently Owned State

Do not include a nested collection in `initialValues` only to preserve it at
submit when a parent table or dialog can change that collection independently.
The deep `initialValues` watcher correctly resets the form when its input
changes, so replacing the collection could discard unsaved scalar edits.

For these workflows:

- pass a stable scalar snapshot containing only form-owned values;
- keep collections with their parent owner;
- merge the latest collections into the typed payload at submit;
- exchange only the derived values needed by external UI through narrow events
  or feature-specific exposed methods;
- when the parent must persist in-progress edits with another save, wrap
  `getValues()` in a typed feature method instead of reading raw form values.

## Value Narrowing

`FormValues` is intentionally `Record<string, unknown>`. Use the helpers from
`value-utils.ts` at the submit boundary instead of defining primitive conversion
functions in each feature form:

| Helper | Accepted value | Fallback behavior |
| --- | --- | --- |
| `stringValue` | `string` | Returns the required string fallback without coercion. |
| `nullableStringValue` | `string \| null` | Preserves explicit `null`; otherwise returns the nullable fallback. |
| `optionalStringValue` | `string \| null` | Converts explicit `null` to `undefined`; otherwise returns the optional fallback. |
| `finiteNumberValue` | Finite `number` | Returns the supplied `number`, `null`, or `undefined` fallback. |
| `integerValue` | Integer `number` | Returns the numeric fallback. |
| `booleanValue` | `boolean` | Returns the boolean fallback. |
| `dateValue` | `Date` | Returns the supplied `Date`, `null`, or `undefined` fallback. |

```ts
import {
  booleanValue,
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.entity,
    name: stringValue(values.name, "").trim(),
    amount: finiteNumberValue(values.amount, props.entity.amount),
    disabled: booleanValue(values.disabled, false),
  });
};
```

Fallbacks are explicit because required, optional, nullable, and preserved
values have different domain meanings. These helpers narrow values; they do not
validate the form or perform arbitrary coercion. Keep feature normalization
such as `.trim()`, enum guards, calculations, and other business rules in the
owning form.

## Fields And Layout

Supported `FormFieldType` values are:

- `Text`
- `Password`
- `Number`
- `Currency`
- `Textarea`
- `Select`
- `MultiSelect`
- `Checkbox`
- `Date`
- `Custom`

`Currency` forces PrimeVue `InputNumber` currency mode. `Checkbox` forces
binary mode. `Custom` has no default control and requires a matching field
slot.

Each row can configure mobile, tablet, and desktop columns:

```ts
{
  columns: { mobile: 1, tablet: 2, desktop: 4 },
  fields: [/* ... */],
}
```

Each field can span columns:

```ts
{
  name: "description",
  span: { mobile: 1, tablet: 2, desktop: 3 },
  // ...
}
```

A field can be disabled statically or from the current form snapshot:

```ts
{
  name: "frequency",
  disabled: (values) => values.recurring !== true,
  // ...
}
```

The predicate is presentation-only: it does not clear or alter field values.
It is re-evaluated when form values change. Custom fields receive the resolved
boolean through their slot's `disabled` property.

Synchronous feature-owned derived values can use an opt-in field callback:

```ts
{
  name: "quantity",
  onChange: (value, values) => updateDerivedValues(value, values),
  // ...
}
```

Only fields defining `onChange` receive a cloned current snapshot, avoiding
change-processing overhead for all other fields. Keep callbacks side-effect
free except for deliberate calls to the form's exposed `setFieldValue`, and do
not attach callbacks to derived fields unless recursion is explicitly guarded.

### Async Enrichment And Calculations

Remote enrichment and calculations remain feature-owned. A custom scalar field
can update its own value, start an asynchronous request, and apply sibling
results through the exposed setters. Source-field `onChange` can dispatch a
remote calculation using its complete cloned snapshot.

Use these safeguards whenever requests can overlap:

- sequence requests and ignore responses older than the latest user edit;
- build request payloads from the callback snapshot, not the source prop;
- retain the latest dependent values needed after an `await`;
- suppress feature callbacks while applying programmatic resets, normalized
  source values, or remote results through `setFieldValue` or `setValues`;
- reset isolated hidden derived state whenever the input entity changes.

Programmatic setters intentionally use the same `onChange` pipeline as user
edits. Suppression belongs to the feature because only it knows which updates
are derived and which should trigger another calculation.

A conditionally visible multi-field component can keep its fields registered in
a section row while the section slot decides whether to render from current
`values`. This preserves field state without requiring a generic row-visibility
option.

The layout is mobile-first and uses these viewport ranges:

| Mode | Viewport width |
| --- | --- |
| Mobile | Less than 768 px |
| Tablet | 768 px to 1023 px |
| Desktop | 1024 px and wider |

Mobile defaults to one column. Desktop defaults to one column per field. When
`tablet` is omitted, it inherits the resolved desktop value for both row
columns and field spans. This preserves the behavior of configurations created
before tablet support while allowing new forms to adapt the intermediate
range explicitly.

## Validation

A field can provide a Yup schema in `validation`. When at least one field does,
`Form.vue` builds a Yup object schema and adapts it with PrimeVue's Yup
resolver.

Alternatively, pass `resolver` for form-level or cross-field validation. A
provided resolver replaces the generated field resolver; the two mechanisms
are not merged.

Errors are hidden until the first invalid submit. After that attempt, fields
validate on value updates until reset, cancel, or a change to `initialValues`.
The default UI displays one error message per field.

Build translated labels, options, and Yup messages in `computed` values so they
react to locale changes:

```ts
const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "name",
        label: t("feature.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("feature.form.validation.nameRequired"),
        ),
      },
    ],
  },
]);
```

## Custom Fields

Define a custom field and a `field-<name>` slot:

```vue
<Form :rows="rows" :initial-values="entity" @submit="submit">
  <template #field-color="{ value, setValue, disabled, inputId }">
    <ColorPicker
      :input-id="inputId"
      :model-value="typeof value === 'string' ? value : undefined"
      :disabled="disabled"
      @update:model-value="setValue"
    />
  </template>
</Form>
```

Every field slot receives:

```ts
{
  value;
  setValue;
  config;
  state;
  errors;
  disabled;
  inputId;
}
```

`inputId` is the native ID the field label points to. Bind it to the focusable
element of the custom control (`input-id` on PrimeVue inputs) so the label and
error description stay connected. Never hardcode it: IDs are generated per form
instance so that simultaneous forms, such as a dialog over a screen, never
collide.

`setValue` updates only that field. Do not mutate `initialValues` from a slot to
simulate updates to other fields. Avoid duplicate labels when a custom control
also renders its own label.

## Actions

The default actions use `common.save` and `common.cancel`. Replace them with the
`actions` slot when the feature needs different labels or behavior.

The slot receives:

```ts
{
  submit;
  reset;
  values;
  states;
  valid;
  loading;
  disabled;
}
```

It does not currently receive `cancel`. Use the exposed `cancel()` method or
handle custom cancellation outside the slot when reset-before-cancel semantics
are required.

## Dates

`FormFieldType.Date` expects the native values supported by PrimeVue
`DatePicker`, normally `Date | null` for a single date.

- Convert API date strings to `Date` at the store or feature boundary.
- Never pass display-formatted strings to a `DatePicker`.
- Use `formatDate()` only for display.
- Do not mutate reactive dates solely to serialize a request.

`Form.vue` neither parses nor serializes dates.

## Current Limitations

- No `modelValue` or `update:modelValue` contract.
- Field visibility is not configurable.
- A custom field can update only its own form value through the supported API.
- No built-in nested collection or array-field support.
- No invalid-submit event or error summary.
- The actions slot does not receive `cancel`.
- A custom field without its matching slot renders no control.

Add shared capabilities only when a real migration demonstrates the need and
the behavior can be expressed without feature-specific logic in `Form.vue`.

## Reference Implementation

[`FormLifecycleTag.vue`](../../modules/shared/components/FormLifecycleTag.vue)
is the first production consumer. It demonstrates reactive rows, Yup field
validation, a custom field slot, value narrowing, submit, and cancel.

## Verification

For a component migration, run from `frontend/`:

```bash
pnpm run typecheck
pnpm run i18n:check
```

Also verify create, edit, invalid submit, valid submit, cancel, dialog close,
loading/disabled behavior, and locale changes on the affected screen. Current
smoke tests do not exercise `Form.vue` interactions.

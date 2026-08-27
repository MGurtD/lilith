<script setup lang="ts">
import { yupResolver } from "@primevue/forms/resolvers/yup";
import PrimeForm, {
  type FormFieldState,
  type FormInstance,
  type FormSubmitEvent,
} from "@primevue/forms/form";
import PrimeFormField from "@primevue/forms/formfield";
import { cloneDeep } from "lodash";
import PrimeButton from "primevue/button";
import PrimeCheckbox from "primevue/checkbox";
import PrimeDatePicker from "primevue/datepicker";
import PrimeInputNumber from "primevue/inputnumber";
import PrimeInputText from "primevue/inputtext";
import PrimeMultiSelect from "primevue/multiselect";
import PrimePassword from "primevue/password";
import PrimeSelect from "primevue/select";
import PrimeTextarea from "primevue/textarea";
import {
  computed,
  mergeProps,
  nextTick,
  ref,
  shallowRef,
  useSlots,
  watch,
  type CSSProperties,
} from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import {
  FormFieldType,
  type FormFieldConfig,
  type FormResolver,
  type FormRowConfig,
  type FormValues,
} from "./types";

interface Props {
  rows: FormRowConfig[];
  initialValues?: object;
  resolver?: FormResolver;
  loading?: boolean;
  disabled?: boolean;
  showSubmit?: boolean;
  showCancel?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  initialValues: () => ({}),
  resolver: undefined,
  loading: false,
  disabled: false,
  showSubmit: true,
  showCancel: true,
});

const emit = defineEmits<{
  (event: "submit", values: FormValues): void;
  (event: "cancel"): void;
}>();

const slots = useSlots();
const { t } = useI18n();
const formRef = ref<FormInstance | null>(null);
const formRevision = ref(0);
const invalidSubmitAttempted = ref(false);

const fields = computed(() => props.rows.flatMap((row) => row.fields));
const defaultValues = computed<FormValues>(() =>
  Object.fromEntries(
    fields.value
      .filter((field) => field.defaultValue !== undefined)
      .map((field) => [field.name, cloneDeep(field.defaultValue)]),
  ),
);

const createInitialSnapshot = (): FormValues =>
  cloneDeep({
    ...defaultValues.value,
    ...props.initialValues,
  });

const initialSnapshot = shallowRef<FormValues>(createInitialSnapshot());

const validationSchema = computed(() => {
  const shape: Record<string, Yup.AnySchema> = {};
  fields.value.forEach((field) => {
    if (field.validation) shape[field.name] = field.validation;
  });
  return Object.keys(shape).length ? Yup.object().shape(shape) : undefined;
});

const resolvedResolver = computed<FormResolver | undefined>(() => {
  if (props.resolver) return props.resolver;
  if (!validationSchema.value) return undefined;
  return yupResolver(validationSchema.value) as FormResolver;
});

const formStates = computed<Record<string, FormFieldState>>(
  () => formRef.value?.states ?? {},
);
const formValues = computed<FormValues>(() =>
  cloneDeep({
    ...initialSnapshot.value,
    ...Object.fromEntries(
      Object.entries(formStates.value).map(([name, state]) => [
        name,
        state.value,
      ]),
    ),
  }),
);
const formValid = computed(() => formRef.value?.valid ?? true);
const hasActions = computed(
  () => Boolean(slots.actions) || props.showSubmit || props.showCancel,
);

watch(
  () => props.initialValues,
  () => {
    initialSnapshot.value = createInitialSnapshot();
    invalidSubmitAttempted.value = false;
    formRevision.value += 1;
  },
  { deep: true },
);

const positiveInteger = (
  value: number | undefined,
  fallback: number,
): number =>
  Number.isInteger(value) && (value ?? 0) > 0 ? (value as number) : fallback;

type GridStyle = CSSProperties & Record<`--form-${string}`, string>;

const rowStyle = (row: FormRowConfig): GridStyle => ({
  "--form-columns-mobile": String(positiveInteger(row.columns?.mobile, 1)),
  "--form-columns-desktop": String(
    positiveInteger(row.columns?.desktop, Math.max(row.fields.length, 1)),
  ),
});

const fieldStyle = (field: FormFieldConfig): GridStyle => ({
  "--form-span-mobile": String(positiveInteger(field.span?.mobile, 1)),
  "--form-span-desktop": String(positiveInteger(field.span?.desktop, 1)),
});

const fieldId = (name: string): string =>
  `form-field-${name.replace(/[^a-zA-Z0-9_-]/g, "-")}`;
const errorId = (name: string): string => `${fieldId(name)}-error`;

const showFieldError = (state: FormFieldState): boolean =>
  invalidSubmitAttempted.value && state.invalid;

const errorMessage = (state: FormFieldState): string => {
  const error: unknown = state.error;
  if (typeof error === "string") return error;
  if (error && typeof error === "object" && "message" in error) {
    const message = (error as { message?: unknown }).message;
    return typeof message === "string" ? message : "";
  }
  return error == null ? "" : String(error);
};

const isFieldDisabled = (field: FormFieldConfig): boolean =>
  props.disabled || field.disabled === true;

const controlProps = (
  field: FormFieldConfig,
  state: FormFieldState,
  formControlProps?: Record<string, unknown>,
): Record<string, unknown> => {
  const invalid = showFieldError(state);
  const widthClass =
    field.type === FormFieldType.Checkbox ? undefined : "w-full";

  return mergeProps(formControlProps ?? {}, field.props ?? {}, {
    id: fieldId(field.name),
    class: [widthClass, { "p-invalid": invalid }],
    disabled: isFieldDisabled(field),
    "aria-invalid": invalid ? "true" : undefined,
    "aria-describedby": invalid ? errorId(field.name) : undefined,
  });
};

const checkboxProps = (
  field: FormFieldConfig,
  state: FormFieldState,
  formControlProps?: Record<string, unknown>,
): Record<string, unknown> => ({
  ...controlProps(field, state, formControlProps),
  binary: true,
});

const currencyProps = (
  field: FormFieldConfig,
  state: FormFieldState,
  formControlProps?: Record<string, unknown>,
): Record<string, unknown> => ({
  ...controlProps(field, state, formControlProps),
  mode: "currency",
});

const setFieldValue = (name: string, value: unknown): void => {
  formRef.value?.setFieldValue(name, cloneDeep(value));
};

const fieldSetter =
  (name: string) =>
  (value: unknown): void => {
    setFieldValue(name, value);
  };

const submit = (): void => formRef.value?.submit();

const reset = async (): Promise<void> => {
  formRef.value?.reset();
  await nextTick();
  invalidSubmitAttempted.value = false;
};

const cancel = async (): Promise<void> => {
  await reset();
  emit("cancel");
};

const handleSubmit = (event: FormSubmitEvent): void => {
  if (!event.valid) {
    invalidSubmitAttempted.value = true;
    return;
  }

  emit(
    "submit",
    cloneDeep({
      ...initialSnapshot.value,
      ...event.values,
    }),
  );
};

defineExpose({ submit, reset, cancel });
</script>

<template>
  <PrimeForm
    :key="formRevision"
    ref="formRef"
    class="generic-form"
    :initial-values="initialSnapshot"
    :resolver="resolvedResolver"
    :validate-on-value-update="invalidSubmitAttempted"
    :validate-on-submit="true"
    @submit="handleSubmit"
  >
    <div
      v-for="(row, rowIndex) in rows"
      :key="rowIndex"
      class="generic-form__row"
      :style="rowStyle(row)"
    >
      <PrimeFormField
        v-for="field in row.fields"
        v-slot="$field"
        :key="field.name"
        :name="field.name"
        as-child
      >
        <div class="generic-form__field" :style="fieldStyle(field)">
          <label
            v-if="field.type !== FormFieldType.Checkbox"
            class="generic-form__label"
            :for="fieldId(field.name)"
          >
            {{ field.label }}
          </label>

          <slot
            :name="`field-${field.name}`"
            :value="$field.value"
            :set-value="fieldSetter(field.name)"
            :config="field"
            :state="$field"
            :errors="$field.errors"
            :disabled="isFieldDisabled(field)"
          >
            <PrimeInputText
              v-if="field.type === FormFieldType.Text"
              :model-value="$field.value as string | undefined"
              v-bind="controlProps(field, $field, $field.props)"
              @update:model-value="setFieldValue(field.name, $event)"
            />
            <PrimePassword
              v-else-if="field.type === FormFieldType.Password"
              :model-value="$field.value as string | undefined"
              v-bind="controlProps(field, $field, $field.props)"
              @update:model-value="setFieldValue(field.name, $event)"
            />
            <PrimeInputNumber
              v-else-if="field.type === FormFieldType.Number"
              :model-value="$field.value as number | null | undefined"
              v-bind="controlProps(field, $field, $field.props)"
              @update:model-value="setFieldValue(field.name, $event)"
            />
            <PrimeInputNumber
              v-else-if="field.type === FormFieldType.Currency"
              :model-value="$field.value as number | null | undefined"
              v-bind="currencyProps(field, $field, $field.props)"
              @update:model-value="setFieldValue(field.name, $event)"
            />
            <PrimeTextarea
              v-else-if="field.type === FormFieldType.Textarea"
              :model-value="$field.value as string | undefined"
              v-bind="controlProps(field, $field, $field.props)"
              @update:model-value="setFieldValue(field.name, $event)"
            />
            <PrimeSelect
              v-else-if="field.type === FormFieldType.Select"
              :model-value="$field.value"
              v-bind="controlProps(field, $field, $field.props)"
              @update:model-value="setFieldValue(field.name, $event)"
            />
            <PrimeMultiSelect
              v-else-if="field.type === FormFieldType.MultiSelect"
              :model-value="$field.value"
              v-bind="controlProps(field, $field, $field.props)"
              @update:model-value="setFieldValue(field.name, $event)"
            />
            <div
              v-else-if="field.type === FormFieldType.Checkbox"
              class="generic-form__checkbox"
            >
              <PrimeCheckbox
                :model-value="$field.value as boolean | undefined"
                v-bind="checkboxProps(field, $field, $field.props)"
                @update:model-value="setFieldValue(field.name, $event)"
              />
              <label
                class="generic-form__checkbox-label"
                :for="fieldId(field.name)"
              >
                {{ field.label }}
              </label>
            </div>
            <PrimeDatePicker
              v-else-if="field.type === FormFieldType.Date"
              :model-value="
                $field.value as
                  Date | Date[] | (Date | null)[] | null | undefined
              "
              v-bind="controlProps(field, $field, $field.props)"
              @update:model-value="setFieldValue(field.name, $event)"
            />
          </slot>

          <small
            v-if="showFieldError($field)"
            :id="errorId(field.name)"
            class="generic-form__error"
            role="alert"
          >
            <i class="pi pi-exclamation-circle" aria-hidden="true" />
            <span>{{ errorMessage($field) }}</span>
          </small>
        </div>
      </PrimeFormField>
    </div>

    <div v-if="hasActions" class="generic-form__actions">
      <slot
        name="actions"
        :submit="submit"
        :reset="reset"
        :values="formValues"
        :states="formStates"
        :valid="formValid"
        :loading="loading"
        :disabled="disabled"
      >
        <PrimeButton
          v-if="showCancel"
          type="button"
          severity="secondary"
          icon="pi pi-times"
          :label="t('common.cancel')"
          :disabled="disabled || loading"
          @click="cancel"
        />
        <PrimeButton
          v-if="showSubmit"
          type="submit"
          icon="pi pi-save"
          :label="t('common.save')"
          :loading="loading"
          :disabled="disabled"
        />
      </slot>
    </div>
  </PrimeForm>
</template>

<style scoped>
.generic-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  min-width: 0;
}

.generic-form__row {
  display: grid;
  grid-template-columns: repeat(var(--form-columns-mobile), minmax(0, 1fr));
  gap: 1rem;
  min-width: 0;
}

.generic-form__field {
  grid-column: span var(--form-span-mobile);
  min-width: 0;
}

.generic-form__label {
  display: block;
  margin-bottom: 0.5rem;
  color: var(--p-text-color);
}

.generic-form__checkbox {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-height: 2.5rem;
}

.generic-form__checkbox-label {
  cursor: pointer;
}

.generic-form__error {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  margin-top: 0.35rem;
  color: var(--p-orange-600);
  line-height: 1.25;
  overflow-wrap: anywhere;
}

.generic-form__error .pi {
  flex: 0 0 auto;
  font-size: 1em;
  line-height: inherit;
}

.generic-form__actions {
  display: flex;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.5rem;
}

@media (min-width: 768px) {
  .generic-form__row {
    grid-template-columns: repeat(var(--form-columns-desktop), minmax(0, 1fr));
  }

  .generic-form__field {
    grid-column: span var(--form-span-desktop);
  }
}
</style>

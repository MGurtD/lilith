<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  dateValue,
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";
import { PrimeIcons } from "@primevue/core/api";
import { isEqual } from "lodash";
import { computed, ref, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { useCustomersStore } from "../store/customers";
import type { Budget } from "../types";

const props = defineProps<{
  budget: Budget;
  /** Number of the sales order created from this budget, if any. */
  orderNumber?: string;
}>();

const emit = defineEmits<{
  (event: "submit", budget: Budget): void;
  (event: "download"): void;
  (event: "printPdf"): void;
  (event: "createOrder"): void;
  (event: "clone"): void;
}>();

const { t } = useI18n();
const customerStore = useCustomersStore();
const form = shallowRef<{ getValues: () => FormValues } | null>(null);

// The parent reloads the budget (with its details, transports and external
// services) after every line change, so the form receives a stable scalar
// snapshot and only resets when a form-owned value changes. Collections and
// the automatic notes are merged from the latest prop at submit.
type BudgetScalars = Pick<
  Budget,
  | "id"
  | "number"
  | "date"
  | "acceptanceDate"
  | "statusId"
  | "customerId"
  | "deliveryDays"
  | "exerciseId"
  | "userNotes"
>;

const toDate = (value: unknown): Date | null => {
  if (value instanceof Date) return value;
  if (typeof value === "string" && value !== "") return new Date(value);
  return null;
};

const scalarSnapshot = (model: Budget): BudgetScalars => ({
  id: model.id,
  number: model.number,
  date: toDate(model.date),
  acceptanceDate: toDate(model.acceptanceDate),
  statusId: model.statusId,
  customerId: model.customerId,
  deliveryDays: model.deliveryDays,
  exerciseId: model.exerciseId,
  userNotes: model.userNotes,
});

const initialValues = ref(scalarSnapshot(props.budget));

watch(
  () => scalarSnapshot(props.budget),
  (next) => {
    if (!isEqual(next, initialValues.value)) initialValues.value = next;
  },
);

const items = computed(() => [
  {
    label: t("sales.detail.actions.download"),
    icon: PrimeIcons.FILE_WORD,
    command: () => emit("download"),
  },
  {
    label: t("sales.detail.actions.printPdf"),
    icon: PrimeIcons.FILE_PDF,
    command: () => emit("printPdf"),
  },
  {
    label: t("sales.detail.actions.createOrder"),
    icon: PrimeIcons.FLAG_FILL,
    command: () => emit("createOrder"),
  },
  {
    label: t("sales.detail.actions.cloneBudget"),
    icon: PrimeIcons.COPY,
    command: () => emit("clone"),
  },
]);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "number",
        label: t("sales.components.pressupost"),
        type: FormFieldType.Text,
        disabled: true,
      },
      {
        name: "date",
        label: t("sales.components.dataAlta"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("sales.validation.dateRequired"))
          .required(t("sales.validation.dateRequired")),
      },
      {
        name: "acceptanceDate",
        label: t("sales.components.dataAcceptacio"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
      },
      {
        name: "orderNumber",
        label: t("sales.components.comanda"),
        type: FormFieldType.Custom,
        disabled: true,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "statusId",
        label: t("sales.components.estat"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(t("sales.validation.statusRequired")),
      },
      {
        name: "customerId",
        label: t("sales.components.client"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("sales.validation.customerRequired"),
        ),
      },
      {
        name: "deliveryDays",
        label: t("sales.components.diesNaturalsEntrega"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 0 },
      },
    ],
  },
  {
    fields: [
      {
        name: "userNotes",
        label: t("sales.detail.labels.internalNotes"),
        type: FormFieldType.Textarea,
        props: { rows: 3, placeholder: t("sales.detail.labels.internalNotes") },
      },
    ],
  },
  {
    fields: [
      {
        name: "notes",
        label: t("sales.detail.labels.automaticNotes"),
        type: FormFieldType.Custom,
        disabled: true,
      },
    ],
  },
  {
    // The fiscal year is not editable here, but the legacy form required it.
    // The field stays registered and only its error is rendered.
    section: "exercise",
    fields: [
      {
        name: "exerciseId",
        label: "",
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("sales.validation.exerciseRequired"),
        ),
      },
    ],
  },
]);

const budgetValues = (values: Readonly<FormValues>): Budget => ({
  ...props.budget,
  date: dateValue(values.date, toDate(props.budget.date)),
  acceptanceDate: dateValue(values.acceptanceDate, null),
  statusId: stringValue(values.statusId, props.budget.statusId),
  customerId: stringValue(values.customerId, props.budget.customerId),
  deliveryDays: finiteNumberValue(
    values.deliveryDays,
    props.budget.deliveryDays,
  ),
  userNotes: stringValue(values.userNotes, props.budget.userNotes),
});

const submit = (values: FormValues): void => {
  emit("submit", budgetValues(values));
};

/**
 * The budget including unsaved header edits. Creating a sales order sends the
 * budget body (customer, delivery days) to the API, as the legacy form did
 * when it edited the store directly.
 */
const currentBudget = (): Budget => {
  const values = form.value?.getValues();
  return values ? budgetValues(values) : { ...props.budget };
};

defineExpose({ currentBudget });
</script>

<template>
  <Form
    ref="form"
    page-actions
    :rows="rows"
    :initial-values="initialValues"
    @submit="submit"
  >
    <template #field-orderNumber="{ disabled, inputId }">
      <InputText
        :id="inputId"
        :model-value="orderNumber ?? ''"
        :disabled="disabled"
        class="w-full"
      />
    </template>

    <template #field-statusId="{ value, setValue, disabled, inputId }">
      <DropdownLifecycleStatusTransitions
        :input-id="inputId"
        label=""
        :status-id="budget.statusId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #field-customerId="{ value, setValue, disabled, inputId }">
      <div class="flex align-items-center gap-2">
        <Select
          :input-id="inputId"
          :model-value="typeof value === 'string' ? value : null"
          :options="customerStore.customers ?? []"
          option-value="id"
          option-label="comercialName"
          class="w-full"
          :disabled="disabled"
          @update:model-value="setValue"
        />
        <router-link
          v-if="typeof value === 'string' && value !== ''"
          :to="`/customers/${value}`"
          style="color: inherit"
        >
          <i class="pi pi-search" aria-hidden="true" />
        </router-link>
      </div>
    </template>

    <template #field-notes="{ disabled, inputId }">
      <InputText
        :id="inputId"
        :model-value="budget.notes ?? ''"
        :disabled="disabled"
        class="w-full"
      />
    </template>

    <template #section-exercise="{ errors }">
      <small v-if="errors.exerciseId" class="budget-form__error" role="alert">
        <i class="pi pi-exclamation-circle" aria-hidden="true" />
        <span>{{ errors.exerciseId }}</span>
      </small>
    </template>

    <template #actions="{ submit: submitBudget, loading, disabled }">
      <SplitButton
        icon="pi pi-save"
        :label="t('sales.detail.actions.save')"
        :model="items"
        :loading="loading"
        :disabled="disabled"
        @click="submitBudget"
      />
    </template>
  </Form>
</template>

<style scoped>
.budget-form__error {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: var(--p-orange-600);
  line-height: 1.25;
}
</style>

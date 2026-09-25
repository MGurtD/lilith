<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { dateValue, stringValue } from "@/components/forms/value-utils";
import { PrimeIcons } from "@primevue/core/api";
import { isEqual } from "lodash";
import { useToast } from "primevue/usetoast";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { useCustomersStore } from "../store/customers";
import type { SalesOrderHeader } from "../types";

const props = defineProps<{
  salesOrder: SalesOrderHeader;
  budgetNumber: string;
  deliveryNoteNumber: string;
}>();

const emit = defineEmits<{
  (event: "submit", salesOrder: SalesOrderHeader): void;
  (event: "download", showPrices: boolean): void;
  (event: "downloadPdf"): void;
  (event: "createDeliveryNote"): void;
}>();

const { t } = useI18n();
const toast = useToast();
const customerStore = useCustomersStore();

const items = computed(() => [
  {
    label: t("sales.detail.actions.download"),
    icon: PrimeIcons.FILE_WORD,
    command: () => emit("download", true),
  },
  {
    label: t("sales.detail.actions.printPdf"),
    icon: PrimeIcons.FILE_PDF,
    command: () => emit("downloadPdf"),
  },
  {
    label: t("sales.detail.actions.downloadWithoutPrice"),
    icon: PrimeIcons.FILE_WORD,
    command: () => emit("download", false),
  },
  {
    label: t("sales.detail.actions.createDeliveryNote"),
    icon: PrimeIcons.TRUCK,
    command: () => emit("createDeliveryNote"),
  },
]);

// The screen reloads the order (details, transports, external services) after
// every child change, so the form receives a stable scalar snapshot; it only
// resets when a form-owned value changes. Collections are merged from the
// latest prop at submit.
type SalesOrderScalars = Pick<
  SalesOrderHeader,
  | "id"
  | "number"
  | "customerId"
  | "customerNumber"
  | "statusId"
  | "date"
  | "expectedDate"
>;

const scalarSnapshot = (model: SalesOrderHeader): SalesOrderScalars => ({
  id: model.id,
  number: model.number,
  customerId: model.customerId,
  customerNumber: model.customerNumber,
  statusId: model.statusId,
  date: model.date,
  expectedDate: model.expectedDate,
});

const initialValues = ref(scalarSnapshot(props.salesOrder));

watch(
  () => scalarSnapshot(props.salesOrder),
  (next) => {
    if (!isEqual(next, initialValues.value)) initialValues.value = next;
  },
);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "number",
        label: t("sales.components.numComanda"),
        type: FormFieldType.Text,
        disabled: true,
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
        name: "customerNumber",
        label: t("sales.components.comandaClient"),
        type: FormFieldType.Text,
      },
      {
        name: "statusId",
        label: t("sales.components.estat"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(t("sales.validation.statusRequired")),
      },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "date",
        label: t("sales.components.dataAlta"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
      },
      {
        name: "expectedDate",
        label: t("sales.components.dataEntrega"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
      },
      {
        name: "budgetNumber",
        label: t("sales.components.numPressupost"),
        type: FormFieldType.Custom,
        disabled: true,
      },
      {
        name: "deliveryNoteNumber",
        label: t("sales.components.albaraEntrega"),
        type: FormFieldType.Custom,
        disabled: true,
      },
    ],
  },
]);

// siteId and exerciseId are required but not editable here; the legacy form
// validated them with the visible fields and reported them in a toast.
const validateHiddenFields = (): boolean => {
  const errors: string[] = [];
  if (!props.salesOrder.siteId) errors.push(t("sales.validation.siteRequired"));
  if (!props.salesOrder.exerciseId)
    errors.push(t("sales.validation.exerciseRequired"));
  if (errors.length === 0) return true;

  toast.add({
    severity: "warn",
    summary: t("sales.components.formulariInvalid"),
    detail: errors.join(". "),
    life: 5000,
  });
  return false;
};

// Changing the customer copies its identification into the order header.
const customerFields = (
  customerId: string,
): Partial<SalesOrderHeader> => {
  if (customerId === props.salesOrder.customerId) return {};
  const customer = customerStore.customers?.find(
    (item) => item.id === customerId,
  );
  if (!customer) return {};
  return {
    customerCode: customer.code,
    customerComercialName: customer.comercialName,
    customerTaxName: customer.taxName,
    customerVatNumber: customer.vatNumber,
    customerAccountNumber: customer.accountNumber,
  };
};

const submit = (values: FormValues): void => {
  if (!validateHiddenFields()) return;

  const customerId = stringValue(
    values.customerId,
    props.salesOrder.customerId,
  );
  emit("submit", {
    ...props.salesOrder,
    ...customerFields(customerId),
    customerId,
    customerNumber: stringValue(
      values.customerNumber,
      props.salesOrder.customerNumber,
    ),
    statusId: stringValue(values.statusId, props.salesOrder.statusId),
    date: dateValue(values.date, null),
    expectedDate: dateValue(values.expectedDate, null),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="initialValues"
    @submit="submit"
  >
    <template #field-customerId="{ value, setValue, disabled, inputId }">
      <div class="flex align-items-center gap-2">
        <Select
          :input-id="inputId"
          :model-value="typeof value === 'string' ? value : undefined"
          :options="customerStore.customers ?? []"
          option-value="id"
          option-label="comercialName"
          class="w-full"
          :disabled="disabled"
          @update:model-value="setValue"
        />
        <router-link
          v-if="typeof value === 'string' && value"
          :to="`/customers/${value}`"
          style="color: inherit"
        >
          <i class="pi pi-search" />
        </router-link>
      </div>
    </template>

    <template #field-statusId="{ value, setValue, disabled, inputId }">
      <DropdownLifecycleStatusTransitions
        :input-id="inputId"
        label=""
        :status-id="salesOrder.statusId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #field-budgetNumber="{ inputId }">
      <InputText
        :id="inputId"
        :model-value="budgetNumber"
        class="w-full"
        disabled
      />
    </template>

    <template #field-deliveryNoteNumber="{ inputId }">
      <InputText
        :id="inputId"
        :model-value="deliveryNoteNumber"
        class="w-full"
        disabled
      />
    </template>

    <template #actions="{ submit: submitOrder, loading, disabled }">
      <SplitButton
        icon="pi pi-save"
        :label="t('sales.detail.actions.save')"
        :model="items"
        :loading="loading"
        :disabled="disabled"
        @click="submitOrder"
      />
    </template>
  </Form>
</template>

<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { dateValue, stringValue } from "@/components/forms/value-utils";
import { PrimeIcons } from "@primevue/core/api";
import type { MenuItem } from "primevue/menuitem";
import { computed, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { useSharedDataStore } from "../../shared/store/masterData";
import type { SalesInvoice } from "../types";

const props = defineProps<{
  invoice: SalesInvoice;
  saving?: boolean;
}>();

const emit = defineEmits<{
  (event: "submit", invoice: SalesInvoice): void;
  (event: "download"): void;
  (event: "download-pdf"): void;
  (event: "rectificative"): void;
}>();

const { t } = useI18n();
const sharedData = useSharedDataStore();

// The screen reloads the invoice after its details, delivery notes or fiscal
// data change. Only the header scalars are form-owned, so the snapshot is
// rebuilt when they change and unsaved header edits survive those reloads.
const invoiceDate = (invoice: SalesInvoice): Date | null =>
  invoice.invoiceDate instanceof Date ? invoice.invoiceDate : null;

const createInitialValues = (invoice: SalesInvoice): FormValues => ({
  id: invoice.id,
  invoiceNumber: invoice.invoiceNumber,
  invoiceDate: invoiceDate(invoice),
  statusId: invoice.statusId,
  paymentMethodId: invoice.paymentMethodId,
});

const snapshotKey = (invoice: SalesInvoice): string =>
  JSON.stringify([
    invoice.id,
    invoice.invoiceNumber,
    invoiceDate(invoice)?.getTime() ?? null,
    invoice.statusId,
    invoice.paymentMethodId,
  ]);

const initialValues = shallowRef<FormValues>(
  createInitialValues(props.invoice),
);

watch(
  () => snapshotKey(props.invoice),
  () => {
    initialValues.value = createInitialValues(props.invoice);
  },
);

const menuItems = computed<MenuItem[]>(() => [
  {
    label: t("sales.detail.actions.download"),
    icon: PrimeIcons.FILE_WORD,
    command: () => emit("download"),
  },
  {
    label: t("sales.detail.actions.printPdf"),
    icon: PrimeIcons.FILE_PDF,
    command: () => emit("download-pdf"),
  },
  ...(props.invoice.parentSalesInvoiceId
    ? []
    : [
        {
          label: t("sales.detail.messages.rectificativeInvoice"),
          icon: PrimeIcons.FILE_IMPORT,
          command: () => emit("rectificative"),
        },
      ]),
]);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "invoiceNumber",
        label: t("sales.components.numero"),
        type: FormFieldType.Text,
        disabled: true,
      },
      {
        name: "invoiceDate",
        label: t("sales.components.dataFactura"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("sales.validation.invoiceDateRequired"))
          .required(t("sales.validation.invoiceDateRequired")),
      },
      {
        name: "statusId",
        label: t("sales.components.estat"),
        type: FormFieldType.Custom,
      },
      {
        name: "paymentMethodId",
        label: t("sales.components.metodePagament"),
        type: FormFieldType.Select,
        props: {
          options: sharedData.paymentMethods ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("sales.validation.paymentMethodRequired"),
        ),
      },
    ],
  },
]);

// Collections, amounts and fiscal data come from the latest invoice at submit.
const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.invoice,
    invoiceDate: dateValue(values.invoiceDate, props.invoice.invoiceDate),
    statusId: stringValue(values.statusId, props.invoice.statusId),
    paymentMethodId: stringValue(
      values.paymentMethodId,
      props.invoice.paymentMethodId,
    ),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="initialValues"
    :loading="saving"
    @submit="submit"
  >
    <template #field-statusId="{ value, setValue, disabled, inputId }">
      <DropdownLifecycleStatusTransitions
        :input-id="inputId"
        label=""
        :status-id="invoice.statusId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #actions="{ submit: submitInvoice, loading, disabled }">
      <SplitButton
        icon="pi pi-save"
        :label="t('sales.detail.actions.save')"
        :model="menuItems"
        :loading="loading"
        :disabled="disabled"
        @click="submitInvoice"
      />
    </template>
  </Form>
</template>

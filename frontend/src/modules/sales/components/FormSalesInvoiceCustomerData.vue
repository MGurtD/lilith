<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { stringValue } from "@/components/forms/value-utils";
import { computed, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import DropdownCountry from "../../shared/components/DropdownCountry.vue";
import type { SalesInvoice, SalesInvoiceCustomerDataUpdate } from "../types";

const props = withDefaults(
  defineProps<{
    invoice: SalesInvoice;
    saving?: boolean;
  }>(),
  { saving: false },
);

const emit = defineEmits<{
  (event: "submit", customerData: SalesInvoiceCustomerDataUpdate): void;
}>();

const { t } = useI18n();

const persistedCustomerData = (
  invoice: SalesInvoice,
): SalesInvoiceCustomerDataUpdate => ({
  customerComercialName: invoice.customerComercialName ?? "",
  customerTaxName: invoice.customerTaxName ?? "",
  customerVatNumber: invoice.customerVatNumber ?? "",
  customerAccountNumber: invoice.customerAccountNumber ?? "",
  customerAddress: invoice.customerAddress ?? "",
  customerCity: invoice.customerCity ?? "",
  customerPostalCode: invoice.customerPostalCode ?? "",
  customerRegion: invoice.customerRegion ?? "",
  customerCountry: invoice.customerCountry ?? "",
});

// Rebuilt only when the persisted fiscal data changes, so reloads caused by
// detail or delivery note changes keep unsaved fiscal edits.
const initialValues = shallowRef<FormValues>({
  ...persistedCustomerData(props.invoice),
});

watch(
  () => JSON.stringify(persistedCustomerData(props.invoice)),
  () => {
    initialValues.value = { ...persistedCustomerData(props.invoice) };
  },
);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "customerComercialName",
        label: t("salesInvoice.customerData.labels.comercialName"),
        type: FormFieldType.Text,
      },
      {
        name: "customerTaxName",
        label: t("salesInvoice.customerData.labels.taxName"),
        type: FormFieldType.Text,
      },
      {
        name: "customerVatNumber",
        label: t("salesInvoice.customerData.labels.vatNumber"),
        type: FormFieldType.Text,
      },
      {
        name: "customerAccountNumber",
        label: t("salesInvoice.customerData.labels.accountNumber"),
        type: FormFieldType.Text,
      },
    ],
  },
  {
    fields: [
      {
        name: "customerAddress",
        label: t("salesInvoice.customerData.labels.address"),
        type: FormFieldType.Text,
      },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "customerCity",
        label: t("salesInvoice.customerData.labels.city"),
        type: FormFieldType.Text,
      },
      {
        name: "customerPostalCode",
        label: t("salesInvoice.customerData.labels.postalCode"),
        type: FormFieldType.Text,
      },
      {
        name: "customerRegion",
        label: t("salesInvoice.customerData.labels.region"),
        type: FormFieldType.Text,
      },
      {
        name: "customerCountry",
        label: t("salesInvoice.customerData.labels.country"),
        type: FormFieldType.Custom,
      },
    ],
  },
]);

const customerDataValues = (
  values: Readonly<FormValues>,
): SalesInvoiceCustomerDataUpdate => {
  const persisted = persistedCustomerData(props.invoice);
  return {
    customerComercialName: stringValue(
      values.customerComercialName,
      persisted.customerComercialName,
    ),
    customerTaxName: stringValue(
      values.customerTaxName,
      persisted.customerTaxName,
    ),
    customerVatNumber: stringValue(
      values.customerVatNumber,
      persisted.customerVatNumber,
    ),
    customerAccountNumber: stringValue(
      values.customerAccountNumber,
      persisted.customerAccountNumber,
    ),
    customerAddress: stringValue(
      values.customerAddress,
      persisted.customerAddress,
    ),
    customerCity: stringValue(values.customerCity, persisted.customerCity),
    customerPostalCode: stringValue(
      values.customerPostalCode,
      persisted.customerPostalCode,
    ),
    customerRegion: stringValue(
      values.customerRegion,
      persisted.customerRegion,
    ),
    // The country selector emits null when cleared; send an empty string.
    customerCountry: stringValue(values.customerCountry, ""),
  };
};

// The fiscal data has its own endpoint and its own Save inside the tab; the
// parent runs the propagation check and persists it.
const submit = (values: FormValues): void => {
  emit("submit", customerDataValues(values));
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="initialValues"
    :loading="saving"
    :show-cancel="false"
    @submit="submit"
  >
    <template #field-customerCountry="{ value, setValue, disabled, inputId }">
      <DropdownCountry
        :input-id="inputId"
        label=""
        :model-value="typeof value === 'string' ? value : null"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #actions="{ submit: submitCustomerData, loading, disabled }">
      <Button
        type="button"
        icon="pi pi-save"
        size="small"
        :label="t('salesInvoice.customerData.saveButton')"
        :loading="loading"
        :disabled="disabled || loading"
        @click="submitCustomerData"
      />
    </template>
  </Form>
</template>

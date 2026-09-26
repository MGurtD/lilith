<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { stringValue } from "@/components/forms/value-utils";
import LanguageSwitcher from "@/components/LanguageSwitcher.vue";
import { isEqual } from "lodash";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { useSharedDataStore } from "../../shared/store/masterData";
import { useCustomersStore } from "../store/customers";
import type { Customer } from "../types";

const props = defineProps<{
  customer: Customer;
}>();

const emit = defineEmits<{
  (event: "submit", customer: Customer): void;
}>();

const { t } = useI18n();
const customerStore = useCustomersStore();
const sharedData = useSharedDataStore();

// The parent refetches the customer (with its contacts and addresses) whenever
// a contact or address changes, so the form receives a stable scalar snapshot
// instead of the whole entity; it only resets when a form-owned value changes.
type CustomerScalars = Pick<
  Customer,
  | "id"
  | "comercialName"
  | "taxName"
  | "customerTypeId"
  | "vatNumber"
  | "web"
  | "preferredLanguage"
  | "accountNumber"
  | "paymentMethodId"
  | "observations"
  | "invoiceNotes"
>;

const scalarSnapshot = (model: Customer): CustomerScalars => ({
  id: model.id,
  comercialName: model.comercialName,
  taxName: model.taxName,
  customerTypeId: model.customerTypeId,
  vatNumber: model.vatNumber,
  web: model.web,
  preferredLanguage: model.preferredLanguage,
  accountNumber: model.accountNumber,
  paymentMethodId: model.paymentMethodId,
  observations: model.observations,
  invoiceNotes: model.invoiceNotes,
});

const initialValues = ref(scalarSnapshot(props.customer));

watch(
  () => scalarSnapshot(props.customer),
  (next) => {
    if (!isEqual(next, initialValues.value)) initialValues.value = next;
  },
);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "comercialName",
        label: t("sales.customers.commercialName"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.commercialNameRequired"))
          .max(
            250,
            t("sales.validation.commercialNameMaxLength"),
          ),
      },
      {
        name: "taxName",
        label: t("sales.customers.taxName"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("sales.validation.taxNameRequired"),
        ),
      },
      {
        name: "customerTypeId",
        label: t("sales.components.tipusClient"),
        type: FormFieldType.Select,
        props: {
          options: customerStore.customerTypes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string()
          .nullable()
          .required(t("sales.validation.customerTypeRequired")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "vatNumber",
        label: t("sales.customers.vatNumber"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("sales.validation.vatNumberRequired"),
        ),
      },
      {
        name: "web",
        label: t("sales.components.web"),
        type: FormFieldType.Text,
      },
      {
        name: "preferredLanguage",
        label: t("forms.user.languageLabel"),
        type: FormFieldType.Custom,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "accountNumber",
        label: t("sales.components.numeroDeCompte"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("sales.validation.accountNumberRequired"),
        ),
      },
      {
        name: "paymentMethodId",
        label: t("sales.components.formaDePagament"),
        type: FormFieldType.Select,
        props: {
          options: sharedData.paymentMethods ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
      },
    ],
  },
  {
    fields: [
      {
        name: "observations",
        label: t("sales.components.observacions"),
        type: FormFieldType.Textarea,
      },
    ],
  },
  {
    fields: [
      {
        name: "invoiceNotes",
        label: t("sales.components.notesDeFactura"),
        type: FormFieldType.Textarea,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.customer,
    comercialName: stringValue(values.comercialName, ""),
    taxName: stringValue(values.taxName, ""),
    customerTypeId: stringValue(
      values.customerTypeId,
      props.customer.customerTypeId,
    ),
    vatNumber: stringValue(values.vatNumber, ""),
    web: stringValue(values.web, props.customer.web),
    preferredLanguage: stringValue(
      values.preferredLanguage,
      props.customer.preferredLanguage,
    ),
    accountNumber: stringValue(values.accountNumber, ""),
    paymentMethodId: stringValue(
      values.paymentMethodId,
      props.customer.paymentMethodId,
    ),
    observations: stringValue(values.observations, props.customer.observations),
    invoiceNotes: stringValue(values.invoiceNotes, props.customer.invoiceNotes),
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
    <template #field-preferredLanguage="{ value, setValue, disabled, inputId }">
      <LanguageSwitcher
        :model-value="typeof value === 'string' ? value : undefined"
        :change-app-language="false"
        :input-id="inputId"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>

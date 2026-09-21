<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";
import { round } from "lodash";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { FormActionMode } from "../../../types/component";
import { usePurchaseMasterDataStore } from "../store/purchase";
import type { PurchaseInvoiceImport } from "../types";

const props = defineProps<{
  formAction: FormActionMode;
  invoiceImport: PurchaseInvoiceImport;
}>();

const emit = defineEmits<{
  (event: "submit", invoiceImport: PurchaseInvoiceImport): void;
}>();

const purchaseMasterData = usePurchaseMasterDataStore();
const { t } = useI18n();
const form = ref<{
  setFieldValue: (name: string, value: unknown) => void;
} | null>(null);

const textActionButton = computed(() =>
  props.formAction === FormActionMode.CREATE
    ? t("purchase.purchaseInvoiceImport.actions.add")
    : t("purchase.purchaseInvoiceImport.actions.update"),
);

const amountProps = {
  locale: "en-US",
  minFractionDigits: 2,
  suffix: " €",
};

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "baseAmount",
        label: t("purchase.purchaseInvoiceImport.fields.baseAmount"),
        type: FormFieldType.Number,
        props: amountProps,
        validation: Yup.number()
          .typeError(
            t(
              "purchase.purchaseInvoiceImport.validation.baseAmountRequired",
            ),
          )
          .required(
            t(
              "purchase.purchaseInvoiceImport.validation.baseAmountRequired",
            ),
          ),
        onChange: (_value, values) => calculateAmounts(values),
      },
      {
        name: "taxId",
        label: t("purchase.purchaseInvoiceImport.fields.tax"),
        type: FormFieldType.Select,
        props: {
          options: purchaseMasterData.masterData.taxes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        onChange: (_value, values) => calculateAmounts(values),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "taxAmount",
        label: t("purchase.purchaseInvoiceImport.fields.taxAmount"),
        type: FormFieldType.Number,
        props: amountProps,
        disabled: true,
      },
      {
        name: "netAmount",
        label: t("common.total"),
        type: FormFieldType.Number,
        props: amountProps,
        disabled: true,
      },
    ],
  },
]);

const calculateAmounts = (values: Readonly<FormValues>): void => {
  const tax = purchaseMasterData.masterData.taxes?.find(
    (item) => item.id === values.taxId,
  );
  if (!tax || typeof values.baseAmount !== "number") return;

  const taxAmount = tax.isReverseCharge
    ? 0
    : (values.baseAmount / 100) * tax.percentatge;

  form.value?.setFieldValue("taxAmount", round(taxAmount, 2));
  form.value?.setFieldValue(
    "netAmount",
    round(values.baseAmount + taxAmount, 2),
  );
};

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.invoiceImport,
    taxId: stringValue(values.taxId, ""),
    baseAmount: finiteNumberValue(
      values.baseAmount,
      props.invoiceImport.baseAmount,
    ),
    taxAmount: finiteNumberValue(
      values.taxAmount,
      props.invoiceImport.taxAmount,
    ),
    netAmount: finiteNumberValue(
      values.netAmount,
      props.invoiceImport.netAmount,
    ),
  });
};
</script>

<template>
  <Form
    ref="form"
    :rows="rows"
    :initial-values="invoiceImport"
    @submit="submit"
  >
    <template #actions="{ submit: submitForm, loading, disabled }">
      <Button
        type="button"
        :label="textActionButton"
        :loading="loading"
        :disabled="disabled"
        @click="submitForm"
      />
    </template>
  </Form>
</template>

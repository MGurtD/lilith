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
import { computed, ref, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { useSharedDataStore } from "../../shared/store/masterData";
import type { SalesInvoiceDetail } from "../types";

const props = defineProps<{
  invoiceDetail: SalesInvoiceDetail;
}>();

const emit = defineEmits<{
  (event: "submit", invoiceDetail: SalesInvoiceDetail): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();
const sharedData = useSharedDataStore();
const form = shallowRef<{
  setFieldValue: (name: string, value: unknown) => void;
} | null>(null);

// Hidden costs derived from the last valid quantity and unit price.
const derivedCosts = ref({
  unitCost: props.invoiceDetail.unitCost,
  totalCost: props.invoiceDetail.totalCost,
});

watch(
  () => props.invoiceDetail,
  (detail) => {
    derivedCosts.value = {
      unitCost: detail.unitCost,
      totalCost: detail.totalCost,
    };
  },
);

const numberProps = { locale: "en-US", minFractionDigits: 0 } as const;
const currencyProps = {
  locale: "en-US",
  minFractionDigits: 2,
  suffix: " €",
} as const;

// amount has no onChange, so setting it here cannot recurse.
const calcAmount = (_value: unknown, values: Readonly<FormValues>): void => {
  const quantity = finiteNumberValue(values.quantity, 0);
  const unitPrice = finiteNumberValue(values.unitPrice, 0);
  if (quantity <= 0 || !unitPrice) return;

  const amount = round(quantity * unitPrice, 2);
  derivedCosts.value = { unitCost: unitPrice, totalCost: amount };
  form.value?.setFieldValue("amount", amount);
};

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "description",
        label: t("sales.components.descripcio"),
        type: FormFieldType.Text,
        span: { desktop: 3 },
      },
      {
        name: "taxId",
        label: t("sales.components.impost"),
        type: FormFieldType.Select,
        props: {
          options: sharedData.taxes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "quantity",
        label: t("sales.components.quantitat"),
        type: FormFieldType.Number,
        props: numberProps,
        onChange: calcAmount,
        validation: Yup.number()
          .min(1, t("sales.validation.quantityGreaterThanOneRequired"))
          .required(t("sales.validation.quantityGreaterThanOneRequired")),
      },
      {
        name: "unitPrice",
        label: t("sales.components.preuUnitat"),
        type: FormFieldType.Number,
        props: currencyProps,
        onChange: calcAmount,
        validation: Yup.number().required(
          t("sales.validation.unitPriceRequired"),
        ),
      },
      {
        name: "amount",
        label: t("sales.components.total"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: true,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.invoiceDetail,
    description: stringValue(values.description, ""),
    taxId: stringValue(values.taxId, props.invoiceDetail.taxId),
    quantity: finiteNumberValue(values.quantity, props.invoiceDetail.quantity),
    unitPrice: finiteNumberValue(
      values.unitPrice,
      props.invoiceDetail.unitPrice,
    ),
    amount: finiteNumberValue(values.amount, props.invoiceDetail.amount),
    unitCost: derivedCosts.value.unitCost,
    totalCost: derivedCosts.value.totalCost,
  });
};
</script>

<template>
  <Form
    ref="form"
    :rows="rows"
    :initial-values="invoiceDetail"
    @submit="submit"
    @cancel="emit('cancel')"
  />
</template>

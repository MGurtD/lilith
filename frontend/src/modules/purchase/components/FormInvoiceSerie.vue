<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  booleanValue,
  integerValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { InvoiceSerie } from "../types";

const props = defineProps<{
  purchaseInvoiceSerie: InvoiceSerie;
}>();

const emit = defineEmits<{
  (event: "submit", purchaseInvoiceSerie: InvoiceSerie): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "name",
        label: t("purchase.invoiceSeries.fields.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.invoiceSeries.validation.nameRequired"))
          .max(50, t("purchase.invoiceSeries.validation.nameMaxLength")),
      },
      {
        name: "description",
        label: t("purchase.invoiceSeries.fields.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(
            t("purchase.invoiceSeries.validation.descriptionRequired"),
          )
          .max(
            250,
            t("purchase.invoiceSeries.validation.descriptionMaxLength"),
          ),
      },
      {
        name: "disabled",
        label: t("purchase.invoiceSeries.fields.disabled"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "prefix",
        label: t("purchase.invoiceSeries.fields.prefix"),
        type: FormFieldType.Text,
        defaultValue: "",
        validation: Yup.string().max(
          10,
          t("purchase.invoiceSeries.validation.prefixMaxLength"),
        ),
      },
      {
        name: "suffix",
        label: t("purchase.invoiceSeries.fields.suffix"),
        type: FormFieldType.Text,
        defaultValue: "",
        validation: Yup.string().max(
          10,
          t("purchase.invoiceSeries.validation.suffixMaxLength"),
        ),
      },
      {
        name: "nextNumber",
        label: t("purchase.invoiceSeries.fields.nextNumber"),
        type: FormFieldType.Number,
        defaultValue: 1,
        props: { minFractionDigits: 0, maxFractionDigits: 0 },
        validation: Yup.number()
          .typeError(
            t("purchase.invoiceSeries.validation.nextNumberRequired"),
          )
          .positive(t("purchase.invoiceSeries.validation.nextNumberPositive"))
          .integer(t("purchase.invoiceSeries.validation.nextNumberInteger"))
          .required(
            t("purchase.invoiceSeries.validation.nextNumberRequired"),
          ),
      },
      {
        name: "length",
        label: t("purchase.invoiceSeries.fields.length"),
        type: FormFieldType.Number,
        defaultValue: 1,
        props: { minFractionDigits: 0, maxFractionDigits: 0 },
        validation: Yup.number()
          .typeError(t("purchase.invoiceSeries.validation.lengthRequired"))
          .positive(t("purchase.invoiceSeries.validation.lengthPositive"))
          .integer(t("purchase.invoiceSeries.validation.lengthInteger"))
          .min(1, t("purchase.invoiceSeries.validation.lengthMinimum"))
          .max(20, t("purchase.invoiceSeries.validation.lengthMaxLength"))
          .required(t("purchase.invoiceSeries.validation.lengthRequired")),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.purchaseInvoiceSerie,
    name: stringValue(values.name, "").trim(),
    description: stringValue(values.description, "").trim(),
    disabled: booleanValue(values.disabled, false),
    prefix: stringValue(values.prefix, "").trim(),
    suffix: stringValue(values.suffix, "").trim(),
    nextNumber: integerValue(
      values.nextNumber,
      props.purchaseInvoiceSerie.nextNumber,
    ),
    length: integerValue(values.length, props.purchaseInvoiceSerie.length),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="purchaseInvoiceSerie"
    :show-cancel="false"
    @submit="submit"
  />
</template>

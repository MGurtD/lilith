<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  booleanValue,
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { Tax } from "../types";

const props = defineProps<{
  tax: Tax;
}>();

const emit = defineEmits<{
  (event: "submit", tax: Tax): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "name",
        label: t("shared.taxes.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.taxes.validation.nameRequired"))
          .max(250, t("shared.taxes.validation.nameMax")),
      },
      {
        name: "percentatge",
        label: t("shared.taxes.form.percentage"),
        type: FormFieldType.Number,
        props: { locale: "en-US" },
        validation: Yup.number()
          .typeError(t("shared.taxes.validation.percentageRequired"))
          .required(t("shared.taxes.validation.percentageRequired")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "isReverseCharge",
        label: t("shared.taxes.form.reverseCharge"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
      {
        name: "disabled",
        label: t("shared.taxes.form.disabled"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.tax,
    name: stringValue(values.name, ""),
    percentatge: finiteNumberValue(values.percentatge, props.tax.percentatge),
    isReverseCharge: booleanValue(values.isReverseCharge, false),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form page-actions :rows="rows" :initial-values="tax" @submit="submit" />
</template>

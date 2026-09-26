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
import type { PaymentMethod } from "../types";

const props = defineProps<{
  paymentMethod: PaymentMethod;
}>();

const emit = defineEmits<{
  (event: "submit", paymentMethod: PaymentMethod): void;
}>();

const { t } = useI18n();

const requiredNumber = (message: string) =>
  Yup.number().typeError(message).required(message);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "name",
        label: t("shared.paymentMethods.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.paymentMethods.validation.nameRequired"))
          .max(250, t("shared.paymentMethods.validation.nameMax")),
      },
      {
        name: "description",
        label: t("shared.paymentMethods.form.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.paymentMethods.validation.descriptionRequired"))
          .max(250, t("shared.paymentMethods.validation.descriptionMax")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "dueDays",
        label: t("shared.paymentMethods.form.dueDays"),
        type: FormFieldType.Number,
        validation: requiredNumber(
          t("shared.paymentMethods.validation.dueDaysRequired"),
        ),
      },
      {
        name: "paymentDay",
        label: t("shared.paymentMethods.form.paymentDay"),
        type: FormFieldType.Number,
        validation: requiredNumber(
          t("shared.paymentMethods.validation.paymentDayRequired"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "numberOfPayments",
        label: t("shared.paymentMethods.form.numberOfPayments"),
        type: FormFieldType.Number,
        validation: requiredNumber(
          t("shared.paymentMethods.validation.numberOfPaymentsRequired"),
        ),
      },
      {
        name: "frequency",
        label: t("shared.paymentMethods.form.frequency"),
        type: FormFieldType.Number,
        validation: requiredNumber(
          t("shared.paymentMethods.validation.frequencyRequired"),
        ),
      },
    ],
  },
  {
    fields: [
      {
        name: "disabled",
        label: t("shared.paymentMethods.form.disabled"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.paymentMethod,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    dueDays: finiteNumberValue(values.dueDays, props.paymentMethod.dueDays),
    paymentDay: finiteNumberValue(
      values.paymentDay,
      props.paymentMethod.paymentDay,
    ),
    numberOfPayments: finiteNumberValue(
      values.numberOfPayments,
      props.paymentMethod.numberOfPayments,
    ),
    frequency: finiteNumberValue(
      values.frequency,
      props.paymentMethod.frequency,
    ),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="paymentMethod"
    @submit="submit"
  />
</template>

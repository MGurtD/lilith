<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormFieldConfig,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  booleanValue,
  dateValue,
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { Exercise } from "../types";

const props = defineProps<{
  exercise: Exercise;
}>();

const emit = defineEmits<{
  (event: "submit", exercise: Exercise): void;
}>();

const { t } = useI18n();

const counterFields = [
  "budgetCounter",
  "salesOrderCounter",
  "deliveryNoteCounter",
  "salesInvoiceCounter",
  "purchaseOrderCounter",
  "receiptCounter",
  "purchaseInvoiceCounter",
] as const;

const counterField = (
  name: (typeof counterFields)[number],
  label: string,
): FormFieldConfig => ({ name, label, type: FormFieldType.Text });

const profitProps = { locale: "en-US", minFractionDigits: 2 };

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "name",
        label: t("shared.exercises.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.exercises.validation.nameRequired"))
          .max(250, t("shared.exercises.validation.nameMax")),
      },
      {
        name: "description",
        label: t("shared.exercises.form.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.exercises.validation.descriptionRequired"))
          .max(250, t("shared.exercises.validation.descriptionMax")),
      },
      {
        name: "startDate",
        label: t("shared.exercises.form.startDate"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("shared.exercises.validation.startDateRequired"))
          .required(t("shared.exercises.validation.startDateRequired")),
      },
      {
        name: "endDate",
        label: t("shared.exercises.form.endDate"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("shared.exercises.validation.endDateRequired"))
          .required(t("shared.exercises.validation.endDateRequired"))
          .min(
            Yup.ref("startDate"),
            t("shared.exercises.validation.endDateAfterStart"),
          ),
      },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      counterField(
        "budgetCounter",
        t("shared.exercises.form.budgetCounter"),
      ),
      counterField(
        "salesOrderCounter",
        t("shared.exercises.form.salesOrderCounter"),
      ),
      counterField(
        "deliveryNoteCounter",
        t("shared.exercises.form.deliveryNoteCounter"),
      ),
      counterField(
        "salesInvoiceCounter",
        t("shared.exercises.form.salesInvoiceCounter"),
      ),
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      counterField(
        "purchaseOrderCounter",
        t("shared.exercises.form.purchaseOrderCounter"),
      ),
      counterField(
        "receiptCounter",
        t("shared.exercises.form.receiptCounter"),
      ),
      counterField(
        "purchaseInvoiceCounter",
        t("shared.exercises.form.purchaseInvoiceCounter"),
      ),
      {
        name: "disabled",
        label: t("shared.exercises.form.disabled"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "materialProfit",
        label: t("shared.exercises.form.materialProfit"),
        type: FormFieldType.Number,
        props: profitProps,
      },
      {
        name: "externalProfit",
        label: t("shared.exercises.form.externalProfit"),
        type: FormFieldType.Number,
        props: profitProps,
      },
    ],
  },
]);

// Dates stay native Date values; Date.prototype.toJSON serializes them.
const submit = (values: FormValues): void => {
  const counters = Object.fromEntries(
    counterFields.map((name) => [
      name,
      stringValue(values[name], props.exercise[name]),
    ]),
  ) as Pick<Exercise, (typeof counterFields)[number]>;

  emit("submit", {
    ...props.exercise,
    ...counters,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    startDate: dateValue(values.startDate, props.exercise.startDate),
    endDate: dateValue(values.endDate, props.exercise.endDate),
    materialProfit: finiteNumberValue(
      values.materialProfit,
      props.exercise.materialProfit,
    ),
    externalProfit: finiteNumberValue(
      values.externalProfit,
      props.exercise.externalProfit,
    ),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="exercise"
    @submit="submit"
  />
</template>

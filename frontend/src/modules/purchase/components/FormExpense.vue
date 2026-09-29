<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
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
import { useExpenseStore } from "../store/expense";
import type { Expense } from "../types";

const props = defineProps<{
  expense: Expense;
}>();

const emit = defineEmits<{
  (event: "submit", expense: Expense): void;
}>();

const expenseStore = useExpenseStore();
const { t } = useI18n();

const recurringFieldDisabled = (values: Readonly<FormValues>): boolean =>
  values.recurring !== true;

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "expenseTypeId",
        label: t("purchase.fields.type"),
        type: FormFieldType.Select,
        props: {
          options: expenseStore.expenseTypes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("purchase.validation.expenseTypeRequired"),
        ),
      },
      {
        name: "creationDate",
        label: t("purchase.fields.creationDate"),
        type: FormFieldType.Date,
        validation: Yup.date()
          .typeError(t("purchase.validation.creationDateRequired"))
          .required(t("purchase.validation.creationDateRequired")),
      },
      {
        name: "paymentDate",
        label: t("purchase.fields.paymentDate"),
        type: FormFieldType.Date,
        validation: Yup.date()
          .typeError(t("purchase.validation.paymentDateRequired"))
          .required(t("purchase.validation.paymentDateRequired")),
      },
      {
        name: "amount",
        label: t("purchase.fields.amount"),
        type: FormFieldType.Number,
        props: {
          locale: "en-US",
          minFractionDigits: 2,
          suffix: " €",
        },
        validation: Yup.number()
          .typeError(t("purchase.validation.amountRequired"))
          .required(t("purchase.validation.amountRequired")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "recurring",
        label: t("purchase.fields.recurring"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
      {
        name: "frecuency",
        label: t("purchase.fields.frequency"),
        type: FormFieldType.Select,
        props: {
          options: [
            { id: 1, name: t("purchase.frequency.monthly") },
            { id: 2, name: t("purchase.frequency.bimonthly") },
            { id: 3, name: t("purchase.frequency.quarterly") },
            { id: 6, name: t("purchase.frequency.halfYearly") },
            { id: 12, name: t("purchase.frequency.yearly") },
          ],
          optionValue: "id",
          optionLabel: "name",
        },
        disabled: recurringFieldDisabled,
        validation: Yup.number()
          .nullable()
          .when("recurring", {
            is: true,
            then: (schema) =>
              schema
                .typeError(t("purchase.validation.frequencyRequired"))
                .required(t("purchase.validation.frequencyRequired"))
                .min(1, t("purchase.validation.frequencyRequired")),
          }),
      },
      {
        name: "paymentDay",
        label: t("purchase.fields.paymentDay"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 0 },
        disabled: recurringFieldDisabled,
        validation: Yup.number()
          .nullable()
          .when("recurring", {
            is: true,
            then: (schema) =>
              schema
                .typeError(t("purchase.validation.paymentDayRange"))
                .required(t("purchase.validation.paymentDayRange"))
                .min(1, t("purchase.validation.paymentDayRange"))
                .max(31, t("purchase.validation.paymentDayRange")),
          }),
      },
      {
        name: "endDate",
        label: t("purchase.fields.endDate"),
        type: FormFieldType.Date,
        disabled: recurringFieldDisabled,
        validation: Yup.date()
          .nullable()
          .when("recurring", {
            is: true,
            then: (schema) =>
              schema
                .typeError(t("purchase.validation.endDateRequired"))
                .required(t("purchase.validation.endDateRequired"))
                .when("paymentDate", ([paymentDate], dateSchema) =>
                  paymentDate instanceof Date
                    ? dateSchema.min(
                        paymentDate,
                        t("purchase.validation.endDateAfterPaymentDate"),
                      )
                    : dateSchema,
                ),
          }),
      },
    ],
  },
  {
    fields: [
      {
        name: "description",
        label: t("purchase.fields.description"),
        type: FormFieldType.Textarea,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.expense,
    expenseTypeId: stringValue(values.expenseTypeId, ""),
    creationDate: dateValue(values.creationDate, props.expense.creationDate),
    paymentDate: dateValue(values.paymentDate, props.expense.paymentDate),
    amount: finiteNumberValue(values.amount, props.expense.amount),
    recurring: booleanValue(values.recurring, false),
    frecuency: finiteNumberValue(values.frecuency, props.expense.frecuency),
    paymentDay: finiteNumberValue(
      values.paymentDay,
      props.expense.paymentDay,
    ),
    endDate: dateValue(values.endDate, props.expense.endDate),
    description: stringValue(values.description, ""),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="expense"
    @submit="submit"
  />
</template>

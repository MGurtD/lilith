<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  booleanValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { ExpenseType } from "../types";

const props = defineProps<{
  expenseType: ExpenseType;
}>();

const emit = defineEmits<{
  (event: "submit", expenseType: ExpenseType): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "name",
        label: t("purchase.fields.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.validation.nameRequired"))
          .max(250, t("purchase.validation.nameMaxLength")),
      },
      {
        name: "description",
        label: t("purchase.fields.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.validation.descriptionRequired"))
          .max(250, t("purchase.validation.descriptionMaxLength")),
      },
      {
        name: "disabled",
        label: t("purchase.fields.disabled"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.expenseType,
    name: stringValue(values.name, "").trim(),
    description: stringValue(values.description, "").trim(),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="expenseType"
    :show-cancel="false"
    @submit="submit"
  />
</template>

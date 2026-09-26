<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { stringValue } from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { CustomerType } from "../types";

const props = defineProps<{
  customerType: CustomerType;
}>();

const emit = defineEmits<{
  (event: "submit", customerType: CustomerType): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 1 },
    fields: [
      {
        name: "name",
        label: t("sales.customers.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.nameRequired"))
          .max(250, t("sales.validation.nameMaxLength")),
      },
      {
        name: "description",
        label: t("sales.customers.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.descriptionRequired"))
          .max(250, t("sales.validation.descriptionMaxLength")),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.customerType,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="customerType"
    @submit="submit"
  />
</template>

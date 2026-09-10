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
import type { SupplierType } from "../types";

const props = withDefaults(defineProps<{
  supplierType: SupplierType;
  loading?: boolean;
}>(), {
  loading: false,
});

const emit = defineEmits<{
  (event: "submit", supplierType: SupplierType): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
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
    ],
  },
  {
    fields: [
      {
        name: "description",
        label: t("purchase.fields.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.validation.descriptionRequired"))
          .max(250, t("purchase.validation.descriptionMaxLength")),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.supplierType,
    name: stringValue(values.name, "").trim(),
    description: stringValue(values.description, "").trim(),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="supplierType"
    :loading="loading"
    :disabled="loading"
    :show-cancel="false"
    @submit="submit"
  />
</template>

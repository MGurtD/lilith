<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  optionalStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { Lifecycle } from "../types";

const props = defineProps<{
  lifecycle: Lifecycle;
}>();

const emit = defineEmits<{
  (event: "submit", lifecycle: Lifecycle): void;
}>();

const { t } = useI18n();

// Statuses are edited by the parent screen, so they stay out of the snapshot
// and are merged back from the latest prop at submit.
const initialValues = computed(() => ({
  name: props.lifecycle.name,
  description: props.lifecycle.description,
  initialStatusId: props.lifecycle.initialStatusId,
}));

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "name",
        label: t("shared.lifecycle.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.lifecycle.validation.nameRequired"))
          .max(250, t("shared.lifecycle.validation.nameMax")),
      },
      {
        name: "description",
        label: t("shared.lifecycle.form.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.lifecycle.validation.descriptionRequired"))
          .max(250, t("shared.lifecycle.validation.descriptionMax")),
      },
      {
        name: "initialStatusId",
        label: t("shared.lifecycle.form.initialStatus"),
        type: FormFieldType.Select,
        props: {
          options: props.lifecycle.statuses,
          optionLabel: "name",
          optionValue: "id",
        },
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.lifecycle,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    initialStatusId: optionalStringValue(
      values.initialStatusId,
      props.lifecycle.initialStatusId,
    ),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="initialValues"
    @submit="submit"
  />
</template>

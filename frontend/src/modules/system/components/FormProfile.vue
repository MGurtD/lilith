<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  booleanValue,
  optionalStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import type { ProfileDetail } from "@/modules/system/types/profile";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";

const props = defineProps<{
  initialData: Partial<ProfileDetail>;
  submitting?: boolean;
  readonlySystem?: boolean;
}>();

const emit = defineEmits<{
  (e: "submit", data: Partial<ProfileDetail>): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "name",
        label: t("profiles.form.name"),
        type: FormFieldType.Text,
        disabled: props.readonlySystem === true,
        validation: Yup.string().required(
          t("profiles.form.validation.nameRequired"),
        ),
      },
      {
        name: "description",
        label: t("profiles.form.description"),
        type: FormFieldType.Text,
      },
      {
        name: "isSystem",
        label: t("profiles.system"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
        disabled: props.readonlySystem === true,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.initialData,
    name: stringValue(values.name, ""),
    description: optionalStringValue(
      values.description,
      props.initialData.description,
    ),
    isSystem: booleanValue(values.isSystem, props.initialData.isSystem ?? false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="initialData"
    :loading="submitting"
    @submit="submit"
  />
</template>

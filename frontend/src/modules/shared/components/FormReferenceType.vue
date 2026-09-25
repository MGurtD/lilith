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
import type { ReferenceType } from "../types";

const props = defineProps<{
  referenceType: ReferenceType;
}>();

const emit = defineEmits<{
  (event: "submit", referenceType: ReferenceType): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "name",
        label: t("shared.referenceTypes.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.referenceTypes.validation.nameRequired"))
          .max(250, t("shared.referenceTypes.validation.nameMax")),
      },
      {
        name: "description",
        label: t("shared.referenceTypes.form.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.referenceTypes.validation.descriptionRequired"))
          .max(250, t("shared.referenceTypes.validation.descriptionMax")),
      },
      {
        name: "density",
        label: t("shared.referenceTypes.form.density"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 2 },
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "primaryColor",
        label: t("shared.referenceTypes.form.primaryColor"),
        type: FormFieldType.Custom,
      },
      {
        name: "secondaryColor",
        label: t("shared.referenceTypes.form.secondaryColor"),
        type: FormFieldType.Custom,
      },
      {
        name: "disabled",
        label: t("shared.referenceTypes.form.disabled"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

// New reference types start without colors or density; keep them unset
// unless the user picks a value, as the legacy form did.
const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.referenceType,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    density: finiteNumberValue(values.density, props.referenceType.density),
    primaryColor: stringValue(
      values.primaryColor,
      props.referenceType.primaryColor,
    ),
    secondaryColor: stringValue(
      values.secondaryColor,
      props.referenceType.secondaryColor,
    ),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="referenceType"
    @submit="submit"
  >
    <template #field-primaryColor="{ value, setValue, disabled, inputId }">
      <ColorPicker
        :input-id="inputId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
    <template #field-secondaryColor="{ value, setValue, disabled, inputId }">
      <ColorPicker
        :input-id="inputId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>

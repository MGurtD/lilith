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
import type { RejectionReason } from "../types";

const props = defineProps<{
  rejectionReason: RejectionReason;
}>();

const emit = defineEmits<{
  (event: "submit", rejectionReason: RejectionReason): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "code",
        label: t("production.components.codi"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("production.validation.elCodiEsObligatori"))
          .max(20, t("production.validation.elCodiNoPotSuperarEls20Caracters")),
      },
      {
        name: "name",
        label: t("production.components.nom"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("production.validation.elNomEsObligatori"))
          .max(100, t("production.validation.elNomNoPotSuperarEls100Caracters")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 1 },
    fields: [
      {
        name: "description",
        label: t("production.components.descripcio"),
        type: FormFieldType.Text,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "color",
        label: t("production.components.color"),
        type: FormFieldType.Custom,
      },
      {
        name: "disabled",
        label: t("production.components.desactivat"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.rejectionReason,
    code: stringValue(values.code, "").trim(),
    name: stringValue(values.name, "").trim(),
    description: stringValue(values.description, "").trim(),
    color: stringValue(values.color, ""),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="rejectionReason"
    @submit="submit"
  >
    <template #field-color="{ value, setValue, disabled }">
      <ColorPicker
        input-id="form-field-color"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>

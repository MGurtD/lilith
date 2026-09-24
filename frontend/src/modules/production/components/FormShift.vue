<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { booleanValue, stringValue } from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { Shift } from "../types";

const props = defineProps<{
  shift: Shift;
}>();

const emit = defineEmits<{
  (event: "submit", shift: Shift): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "name",
        label: t("production.components.nom"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("production.validation.elNomEsObligatori"))
          .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
      },
      {
        name: "disabled",
        label: t("production.components.deshabilitat"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.shift,
    name: stringValue(values.name, ""),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="shift"
    @submit="submit"
    @cancel="emit('cancel')"
  />
</template>

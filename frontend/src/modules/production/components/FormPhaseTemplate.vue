<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { booleanValue, stringValue } from "@/components/forms/value-utils";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { PhaseTemplate } from "../types";

const props = defineProps<{
  phaseTemplate: PhaseTemplate;
}>();

const emit = defineEmits<{
  (event: "submit", phaseTemplate: PhaseTemplate): void;
}>();

const { t } = useI18n();

// The parent refetches the template (and its details collection) whenever a
// detail changes, so the form receives a stable scalar snapshot instead of
// the whole entity; it only resets when a form-owned value really changes.
type PhaseTemplateScalars = Pick<
  PhaseTemplate,
  "id" | "name" | "description" | "disabled"
>;

const scalarSnapshot = (model: PhaseTemplate): PhaseTemplateScalars => ({
  id: model.id,
  name: model.name,
  description: model.description,
  disabled: model.disabled,
});

const initialValues = ref(scalarSnapshot(props.phaseTemplate));

watch(
  () => scalarSnapshot(props.phaseTemplate),
  (next) => {
    const current = initialValues.value;
    if (
      next.id !== current.id ||
      next.name !== current.name ||
      next.description !== current.description ||
      next.disabled !== current.disabled
    ) {
      initialValues.value = next;
    }
  },
);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "name",
        label: t("phaseTemplates.fields.name"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("phaseTemplates.validation.nameRequired"),
        ),
      },
      {
        name: "description",
        label: t("common.description"),
        type: FormFieldType.Text,
      },
      {
        name: "disabled",
        label: t("phaseTemplates.fields.disabled"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.phaseTemplate,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    disabled: booleanValue(values.disabled, false),
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

<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormFieldConfig,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { booleanValue, stringValue } from "@/components/forms/value-utils";
import IconPicker from "@/components/IconPicker.vue";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { MachineStatus } from "../types";

const props = defineProps<{
  machineStatus: MachineStatus;
}>();

const emit = defineEmits<{
  (event: "submit", machineStatus: MachineStatus): void;
}>();

const { t } = useI18n();

const checkbox = (name: string, label: string): FormFieldConfig => ({
  name,
  label,
  type: FormFieldType.Checkbox,
  defaultValue: false,
});

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
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
        name: "description",
        label: t("production.components.descripcio"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("production.validation.laDescripcioEsObligatoria"))
          .max(
            250,
            t("production.validation.laDescripcioNoPotSuperarEls250Caracters"),
          ),
      },
      {
        name: "color",
        label: t("production.components.color"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("production.validation.elColorEsObligatori"),
        ),
      },
      {
        name: "icon",
        label: t("production.components.icona"),
        type: FormFieldType.Custom,
      },
    ],
  },
  {
    columns: { mobile: 2, tablet: 3, desktop: 6 },
    fields: [
      checkbox("stopped", t("production.components.aturada")),
      checkbox("operatorsAllowed", t("production.components.operaris")),
      checkbox("closed", t("production.components.tancada")),
      checkbox("preferred", t("production.components.preferida")),
      checkbox("workOrderAllowed", t("production.components.permetOf")),
      checkbox("disabled", t("production.components.desactivat")),
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.machineStatus,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    color: stringValue(values.color, ""),
    icon: stringValue(values.icon, ""),
    stopped: booleanValue(values.stopped, false),
    operatorsAllowed: booleanValue(values.operatorsAllowed, false),
    closed: booleanValue(values.closed, false),
    preferred: booleanValue(values.preferred, false),
    workOrderAllowed: booleanValue(values.workOrderAllowed, false),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="machineStatus"
    @submit="submit"
  >
    <template #field-color="{ value, setValue, disabled, inputId }">
      <ColorPicker
        :input-id="inputId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
    <template #field-icon="{ value, setValue, disabled }">
      <IconPicker
        :model-value="typeof value === 'string' ? value : null"
        :placeholder="t('production.components.seleccionaUnaIcona')"
        :class="{ 'pointer-events-none opacity-60': disabled }"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>

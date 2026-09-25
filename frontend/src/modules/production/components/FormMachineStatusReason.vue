<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { stringValue } from "@/components/forms/value-utils";
import IconPicker from "@/components/IconPicker.vue";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { MachineStatusReason } from "../types";

const props = defineProps<{
  reason: MachineStatusReason;
  existingReasons: Array<MachineStatusReason>;
}>();

const emit = defineEmits<{
  (event: "submit", reason: MachineStatusReason): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const isDuplicateCode = (code: string): boolean =>
  props.existingReasons.some(
    (r) =>
      r.code.toLowerCase() === code.toLowerCase() && r.id !== props.reason.id,
  );

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "code",
        label: t("production.components.codi"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("production.validation.elCodiEsObligatori"))
          .max(20, t("production.validation.elCodiNoPotSuperarEls20Caracters"))
          .test(
            "unique-code",
            t("production.validation.machineStatusReasonCodeAlreadyExists"),
            (value) => !value || !isDuplicateCode(value),
          ),
      },
      {
        name: "name",
        label: t("production.components.nom"),
        type: FormFieldType.Text,
        validation: Yup.string()
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
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.reason,
    code: stringValue(values.code, ""),
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    color: stringValue(values.color, ""),
    icon: stringValue(values.icon, ""),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="reason"
    @submit="submit"
    @cancel="emit('cancel')"
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

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
import { FormActionMode } from "../../../types/component";
import type { Status, StatusTransition } from "../types";

const props = defineProps<{
  formAction: FormActionMode;
  transition: StatusTransition;
  statuses: Array<Status>;
}>();

const emit = defineEmits<{
  (event: "submit", transition: StatusTransition): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const statusProps = computed(() => ({
  options: props.statuses,
  optionLabel: "name",
  optionValue: "id",
}));

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "name",
        label: t("shared.statusTransitions.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("shared.lifecycle.validation.nameRequired"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "statusId",
        label: t("shared.statusTransitions.form.origin"),
        type: FormFieldType.Select,
        props: statusProps.value,
      },
      {
        name: "statusToId",
        label: t("shared.statusTransitions.form.destination"),
        type: FormFieldType.Select,
        props: statusProps.value,
        // Legacy rule: origin and destination must differ (two empty
        // selections count as equal, as before).
        validation: Yup.mixed().test(
          "different-status",
          t("shared.statusTransitions.form.sameStatusError"),
          (value, context) => value !== context.parent.statusId,
        ),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.transition,
    name: stringValue(values.name, ""),
    statusId: stringValue(values.statusId, props.transition.statusId),
    statusToId: stringValue(values.statusToId, props.transition.statusToId),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="transition"
    @submit="submit"
    @cancel="emit('cancel')"
  />
</template>

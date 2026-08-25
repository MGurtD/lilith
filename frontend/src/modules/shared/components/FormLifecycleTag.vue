<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import IconPicker from "@/components/IconPicker.vue";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { FormActionMode } from "../../../types/component";
import { LifecycleTag } from "../types";

const props = defineProps<{
  tag: LifecycleTag;
  formAction: FormActionMode;
}>();

const emit = defineEmits<{
  (event: "submit", tag: LifecycleTag): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

interface ColorOption {
  id: string;
  value: string;
}

const colors = computed<ColorOption[]>(() => [
  { id: "", value: t("shared.lifecycleTags.form.colors.none") },
  { id: "info", value: t("shared.lifecycleTags.form.colors.info") },
  {
    id: "secondary",
    value: t("shared.lifecycleTags.form.colors.secondary"),
  },
  { id: "help", value: t("shared.lifecycleTags.form.colors.help") },
  { id: "contrast", value: t("shared.lifecycleTags.form.colors.contrast") },
  { id: "warn", value: t("shared.lifecycleTags.form.colors.warn") },
  { id: "success", value: t("shared.lifecycleTags.form.colors.success") },
  { id: "danger", value: t("shared.lifecycleTags.form.colors.danger") },
]);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "name",
        label: t("shared.lifecycleTags.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("shared.lifecycleTags.form.validation.nameRequired"))
          .max(250, t("shared.lifecycleTags.form.validation.nameMax")),
      },
      {
        name: "description",
        label: t("shared.lifecycleTags.form.description"),
        type: FormFieldType.Text,
        validation: Yup.string().max(
          250,
          t("shared.lifecycleTags.form.validation.descriptionMax"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "color",
        label: t("shared.lifecycleTags.form.color"),
        type: FormFieldType.Select,
        defaultValue: "",
        props: {
          options: colors.value,
          optionLabel: "value",
          optionValue: "id",
        },
      },
      {
        name: "icon",
        label: t("shared.lifecycleTags.form.icon"),
        type: FormFieldType.Custom,
        defaultValue: "",
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.tag,
    name: String(values.name ?? ""),
    description: String(values.description ?? ""),
    color: String(values.color ?? ""),
    icon: String(values.icon ?? ""),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="tag"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #field-icon="{ value, setValue, disabled }">
      <IconPicker
        :model-value="typeof value === 'string' ? value : null"
        :class="{ 'pointer-events-none opacity-60': disabled }"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>

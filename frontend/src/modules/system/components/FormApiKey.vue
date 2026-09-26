<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  dateValue,
  optionalStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";

// The expiry date stays a native Date inside the form; the parent converts it
// to the API string when it builds the create request.
export interface ApiKeyFormData {
  id: string;
  name: string;
  description?: string;
  scopes?: string;
  expiresOn: Date | null;
}

const props = defineProps<{
  initialData: ApiKeyFormData;
  submitting?: boolean;
}>();

const emit = defineEmits<{
  (e: "submit", data: ApiKeyFormData): void;
  (e: "cancel"): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "name",
        label: t("apiKeys.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string().required(t("apiKeys.form.nameRequired")),
      },
      {
        name: "description",
        label: t("apiKeys.form.description"),
        type: FormFieldType.Text,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "scopes",
        label: t("apiKeys.form.scopes"),
        type: FormFieldType.Custom,
      },
      {
        name: "expiresOn",
        label: t("apiKeys.form.expiresOn"),
        type: FormFieldType.Date,
        defaultValue: null,
        props: { dateFormat: "dd/mm/yy", showButtonBar: true },
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
    scopes: optionalStringValue(values.scopes, props.initialData.scopes),
    expiresOn: dateValue(values.expiresOn, null),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="initialData"
    :loading="submitting"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #field-scopes="{ value, setValue, disabled, inputId }">
      <InputText
        :id="inputId"
        :model-value="typeof value === 'string' ? value : undefined"
        :placeholder="t('apiKeys.form.scopesPlaceholder')"
        :disabled="disabled"
        class="w-full"
        @update:model-value="setValue"
      />
      <small class="text-color-secondary">{{
        t("apiKeys.form.scopesHelp")
      }}</small>
    </template>
  </Form>
</template>

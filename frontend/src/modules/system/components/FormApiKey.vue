<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import * as yup from "yup";
import { FormValidation } from "@/utils/form-validator";
import { convertDateTimeToJSON } from "@/utils/functions";

interface ApiKeyFormData {
  id: string;
  name: string;
  description?: string;
  scopes?: string;
  expiresOn?: string | null;
}

interface ApiKeyFormState {
  id: string;
  name: string;
  description?: string;
  scopes?: string;
  expiresOn?: Date | null;
}

const { t } = useI18n();

const props = defineProps<{
  initialData: Partial<ApiKeyFormData>;
  submitting?: boolean;
}>();

const emit = defineEmits<{
  (e: "submit", data: Partial<ApiKeyFormData>): void;
}>();

const schema = yup.object({
  name: yup.string().required(() => t("apiKeys.form.nameRequired")),
});
const validator = new FormValidation(schema as any);

const form = ref<Partial<ApiKeyFormState>>({});
const errors = ref<Record<string, string[]>>({});

onMounted(() => {
  form.value = {
    ...props.initialData,
    expiresOn: props.initialData.expiresOn
      ? new Date(props.initialData.expiresOn)
      : null,
  };
});

const validate = () => {
  const r = validator.validate(form.value);
  errors.value = r.errors;
  return r.result;
};

const submit = () => {
  if (!validate()) return;
  emit("submit", {
    ...form.value,
    expiresOn: form.value.expiresOn
      ? convertDateTimeToJSON(form.value.expiresOn)
      : null,
  });
};
</script>

<template>
  <div class="form-apikey">
    <div class="formgrid grid">
      <div class="field col-12 md:col-6">
        <label class="block mb-2"
          >{{ t("apiKeys.form.name") }} <span class="p-error">*</span></label
        >
        <InputText v-model="form.name" class="w-full" />
        <small class="p-error" v-if="errors.name">{{ errors.name[0] }}</small>
      </div>
      <div class="field col-12 md:col-6">
        <label class="block mb-2">{{ t("apiKeys.form.description") }}</label>
        <InputText v-model="form.description" class="w-full" />
      </div>
      <div class="field col-12 md:col-6">
        <label class="block mb-2">{{ t("apiKeys.form.scopes") }}</label>
        <InputText
          v-model="form.scopes"
          class="w-full"
          :placeholder="t('apiKeys.form.scopesPlaceholder')"
        />
        <small class="text-color-secondary">{{
          t("apiKeys.form.scopesHelp")
        }}</small>
      </div>
      <div class="field col-12 md:col-6">
        <label class="block mb-2">{{ t("apiKeys.form.expiresOn") }}</label>
        <DatePicker
          v-model="form.expiresOn"
          dateFormat="dd/mm/yy"
          class="w-full"
          :showButtonBar="true"
        />
      </div>
    </div>
    <div class="flex justify-content-end mt-4">
      <Button
        size="small"
        :label="t('apiKeys.form.submitButton')"
        icon="pi pi-key"
        :loading="submitting"
        @click="submit"
      />
    </div>
  </div>
</template>

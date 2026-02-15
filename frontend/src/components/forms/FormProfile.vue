<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import * as yup from "yup";
import { FormValidation } from "../../utils/form-validator";
import type { ProfileDetail } from "../../types/profile";

const props = defineProps<{
  initialData: Partial<ProfileDetail>;
  submitting?: boolean;
  readonlySystem?: boolean;
}>();

const emit = defineEmits<{
  (e: "submit", data: Partial<ProfileDetail>): void;
}>();

const { t } = useI18n();

const schema = yup.object({
  name: yup.string().required(() => t("profiles.form.validation.nameRequired")),
});
const validator = new FormValidation(schema as any);

// Estado local independiente - copia única al montar
const form = ref<Partial<ProfileDetail>>({});
const errors = ref<Record<string, string[]>>({});

onMounted(() => {
  // Copia snapshot de los datos iniciales (sin reactividad al padre)
  form.value = { ...props.initialData };
});

const validate = () => {
  const r = validator.validate(form.value);
  errors.value = r.errors;
  return r.result;
};

const submit = () => {
  if (!validate()) return;
  // Emitir copia de los datos al guardar
  emit("submit", { ...form.value });
};
</script>
<template>
  <div class="form-profile">
    <div class="flex justify-content-end mr-3">
      <Button
        size="small"
        :label="t('common.save')"
        icon="pi pi-save"
        :loading="submitting"
        @click="submit"
      />
    </div>
    <div class="formgrid grid">
      <div class="field col-12 md:col-4">
        <label class="block mb-2">{{ t("profiles.form.name") }}</label>
        <InputText
          v-model="form.name"
          :disabled="props.readonlySystem"
          class="w-full"
        />
        <small class="p-error" v-if="errors.name">{{ errors.name[0] }}</small>
      </div>
      <div class="field col-12 md:col-4">
        <label class="block mb-2">{{ t("profiles.form.description") }}</label>
        <InputText v-model="form.description" class="w-full" />
      </div>
      <div class="field col-12 md:col-4">
        <label class="block mb-2">{{ t("profiles.system") }}</label>
        <Checkbox
          v-model="form.isSystem"
          :disabled="props.readonlySystem"
          :binary="true"
          class="mt-1"
        />
      </div>
    </div>
  </div>
</template>

<template>
  <form v-if="enterprise" @submit.prevent="submitForm">
    <div class="three-columns">
      <BaseInput
        class="mb-2"
        :label="t('production.components.nom')"
        id="name"
        v-model="enterprise.name"
        :class="{ 'p-invalid': validation.errors.name }"
      />
      <BaseInput
        class="mb-2"
        :label="t('production.components.descripcio')"
        id="description"
        v-model="enterprise.description"
        :class="{ 'p-invalid': validation.errors.description }"
      />
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.seuPerDefecte") }}</label>
        <Select
          v-model="enterprise.defaultSiteId"
          :options="filteredSites"
          optionLabel="name"
          optionValue="id"
          class="w-full"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.desactivat") }}</label>
        <Checkbox v-model="enterprise.disabled" class="w-full" :binary="true" />
      </div>
    </div>

    <div class="mt-2 flex justify-content-end">
      <Button type="submit" :label="t('production.components.guardar')" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import { computed, onMounted, ref } from "vue";
import { storeToRefs } from "pinia";
import * as Yup from "yup";
import { useToast } from "primevue/usetoast";

import BaseInput from "../../../components/BaseInput.vue";
import { Enterprise } from "../types";
import { FormValidation, FormValidationResult } from "../../../utils/form-validator";
import { usePlantModelStore } from "../store/plantmodel";

const props = defineProps<{ enterprise: Enterprise }>();

const emit = defineEmits<{
  (e: "submit", enterprise: Enterprise): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantStore = usePlantModelStore();
const { sites } = storeToRefs(plantStore);

const filteredSites = computed(() =>
  (sites.value || []).filter((site) => site.enterpriseId === props.enterprise.id),
);

onMounted(async () => {
  if (!sites.value) await plantStore.fetchSites();
});

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("production.validation.elNomEsObligatori"))
    .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
  description: Yup.string()
    .required(t("production.validation.laDescripcioEsObligatoria"))
    .max(250, t("production.validation.laDescripcioNoPotSuperarEls250Caracters")),
});

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const submitForm = () => {
  validation.value = new FormValidation(schema).validate(props.enterprise);
  if (validation.value.result) {
    emit("submit", props.enterprise);
    return;
  }

  const errors = Object.values(validation.value.errors)
    .flat()
    .join(". ");
  toast.add({
    severity: "warn",
    summary: t("production.components.formulariInvalid"),
    detail: errors,
    life: 5000,
  });
};
</script>

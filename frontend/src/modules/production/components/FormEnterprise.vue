<template>
  <form v-if="enterprise" @submit.prevent="submitForm">
    <div class="three-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="enterprise.name"
        :class="{ 'p-invalid': validation.errors.name }"
      />
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="enterprise.description"
        :class="{ 'p-invalid': validation.errors.description }"
      />
      <div>
        <label class="block text-900 mb-2">Seu per defecte</label>
        <Select
          v-model="enterprise.defaultSiteId"
          :options="filteredSites"
          optionLabel="name"
          optionValue="id"
          class="w-full"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox v-model="enterprise.disabled" class="w-full" :binary="true" />
      </div>
    </div>

    <div class="mt-2 flex justify-content-end">
      <Button type="submit" label="Guardar" />
    </div>
  </form>
</template>

<script setup lang="ts">
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
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 caràcters"),
  description: Yup.string()
    .required("La descripció és obligatòria")
    .max(250, "La descripció no pot superar els 250 caràcters"),
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
    summary: "Formulari invàlid",
    detail: errors,
    life: 5000,
  });
};
</script>

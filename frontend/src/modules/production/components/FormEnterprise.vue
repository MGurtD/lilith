<template>
  <form v-if="enterprise">
    <div class="three-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="enterprise.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="enterprise.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
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

    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { Enterprise } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { onMounted, computed } from "vue";
import { usePlantModelStore } from "../store/plantmodel";
import { storeToRefs } from "pinia";

const props = defineProps<{
  enterprise: Enterprise;
}>();

const emit = defineEmits<{
  (e: "submit", enterprise: Enterprise): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantStore = usePlantModelStore();
const { sites } = storeToRefs(plantStore);
const filteredSites = computed(() =>
  (sites.value || []).filter((s) => s.enterpriseId === props.enterprise.id),
);

onMounted(async () => {
  if (!sites.value) await plantStore.fetchSites();
});

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 carácters"),
  description: Yup.string()
    .required("La descripció és obligatori")
    .max(250, "La descripció pot superar els 250 carácters"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.enterprise);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.enterprise);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: "Formulari inválid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>

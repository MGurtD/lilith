<template>
  <form v-if="location">
    <section class="three-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="location.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="location.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Desactivada</label>
        <Checkbox v-model="location.disabled" class="w-full" :binary="true" />
      </div>
    </section>
    <div class="pt-4">
      <Button
        label="Guardar"
        size="small"
        style="float: right"
        @click="submitForm"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { Location } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { usePlantModelStore } from "../../production/store/plantmodel";

const toast = useToast();
const plantmodelStore = usePlantModelStore();

const props = defineProps<{
  location: Location;
}>();

onMounted(async () => {
  await plantmodelStore.fetchSites();
});

const emit = defineEmits<{
  (e: "submit", location: Location): void;
  (e: "cancel"): void;
}>();

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
  validation.value = formValidation.validate(props.location);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.location);
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

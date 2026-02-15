<template>
  <form v-if="lifecycle">
    <section class="three-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="lifecycle.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="lifecycle.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Estat inicial</label>
        <Select
          class="w-full"
          v-model="lifecycle.initialStatusId"
          :options="lifecycle.statuses"
          optionValue="id"
          optionLabel="name"
        />
      </div>
    </section>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { Lifecycle } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const props = defineProps<{
  lifecycle: Lifecycle;
}>();

const emit = defineEmits<{
  (e: "submit", exercise: Lifecycle): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

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
  validation.value = formValidation.validate(props.lifecycle);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.lifecycle);
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

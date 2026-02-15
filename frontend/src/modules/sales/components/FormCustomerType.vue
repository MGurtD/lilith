<template>
  <form v-if="customerType">
    <BaseInput
      class="mb-2"
      label="Nom"
      id="name"
      v-model="customerType.name"
      :class="{
        'p-invalid': validation.errors.name,
      }"
    ></BaseInput>
    <BaseInput
      class="mb-2"
      label="Descripció"
      id="description"
      v-model="customerType.description"
      :class="{
        'p-invalid': validation.errors.description,
      }"
    ></BaseInput>

    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { CustomerType } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const props = defineProps<{
  customerType: CustomerType;
}>();

const emit = defineEmits<{
  (e: "submit", customerType: CustomerType): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom comercial no pot superar els 250 carácters"),
  description: Yup.string()
    .required("La descripció és obligatori")
    .max(250, "La descripció no pot superar els 250 carácters"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.customerType);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.customerType);
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

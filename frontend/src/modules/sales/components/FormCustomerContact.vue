<template>
  <form v-if="contact">
    <section class="three-columns">
      <BaseInput
        label="Nom"
        id="firstName"
        v-model="contact.firstName"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.firstName,
        }"
      ></BaseInput>
      <BaseInput
        label="Cognoms"
        id="lastName"
        v-model="contact.lastName"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.lastName,
        }"
      ></BaseInput>
      <BaseInput
        label="Càrrec"
        id="charge"
        v-model="contact.charge"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.charge,
        }"
      ></BaseInput>
    </section>
    <section class="four-columns">
      <BaseInput
        class="mb-2"
        label="Correu electrònic"
        id="email"
        v-model="contact.email"
        :class="{
          'p-invalid': validation.errors.email,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Extensió"
        id="extension"
        v-model="contact.extension"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Telèfon"
        id="phone"
        v-model="contact.phoneNumber"
        :class="{
          'p-invalid': validation.errors.phone,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Predeterminat</label>
        <Checkbox v-model="contact.main" class="w-full" :binary="true" />
      </div>
    </section>

    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
      <Button
        label="Cancelar"
        severity="secondary"
        class="mr-2"
        @click="cancel"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { CustomerContact } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const props = defineProps<{
  contact: CustomerContact;
}>();

const emit = defineEmits<{
  (e: "submit", contact: CustomerContact): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  firstName: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 carácters"),
  lastName: Yup.string()
    .required("Els cognoms són obligatoris")
    .max(250, "Els cognoms no poden superar els 250 carácters"),
  charge: Yup.string(),
  email: Yup.string()
    .required("El correu és obligatori")
    .email("Correu electrònic invàlid"),
  phoneNumber: Yup.string()
    .required("El telèfon és obligatori")
    .max(15, "Ha superat la longitud màxima del telèfon"),

  disabled: Yup.boolean().required(),
  main: Yup.boolean().required(),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.contact);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.contact);
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

const cancel = () => {
  emit("cancel");
};
</script>

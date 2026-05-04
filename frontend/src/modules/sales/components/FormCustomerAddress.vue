<template>
  <form v-if="address">
    <section class="three-columns mb-2">
      <BaseInput
        id="name"
        label="Nom"
        v-model="address.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Principal</label>
        <Checkbox v-model="address.main" class="w-full" :binary="true" />
      </div>
      <div>
        <label class="block text-900 mb-2">Desactivada</label>
        <Checkbox v-model="address.disabled" class="w-full" :binary="true" />
      </div>
    </section>

    <LocationFields
      :model-value="address"
      :show-distance="true"
      :validation-errors="validation.errors"
    />

    <div>
      <label class="block text-900 mb-2">Observacions</label>
      <Textarea v-model="address.observations" class="w-full" />
    </div>

    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import LocationFields from "@/components/LocationFields.vue";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { CustomerAddress } from "../types";

const props = defineProps<{
  address: CustomerAddress;
}>();

const emit = defineEmits<{
  (e: "submit", address: CustomerAddress): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 caràcters"),
  country: Yup.string().required("El país és obligatori"),
  region: Yup.string().required("La província és obligatòria"),
  city: Yup.string().required("El municipi és obligatori"),
  postalCode: Yup.string().required("El codi postal és obligatori"),
  address: Yup.string().required("La direcció és obligatòria"),
  main: Yup.boolean().required(),
  disabled: Yup.boolean().required(),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.address);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.address);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: "Formulari invàlid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>


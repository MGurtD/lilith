<template>
  <form v-if="referenceType">
    <section class="three-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="referenceType.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="referenceType.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>

      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Densitat g/cm^3"
          id="density"
          v-model="referenceType.density"
        />
      </div>
    </section>

    <section class="three-columns">
      <div>
        <label class="block text-900 mb-2">Color Primari</label>
        <ColorPicker
          v-model="referenceType.primaryColor"
          class="mb-2"
          label="Color"
          id="color"
          :class="{
            'p-invalid': validation.errors.color,
          }"
        ></ColorPicker>
      </div>
      <div>
        <label class="block text-900 mb-2">Color Secundari</label>
        <ColorPicker
          v-model="referenceType.secondaryColor"
          class="mb-2"
          label="Color"
          id="color"
          :class="{
            'p-invalid': validation.errors.color,
          }"
        ></ColorPicker>
      </div>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox
          v-model="referenceType.disabled"
          class="w-full"
          :binary="true"
        />
      </div>
    </section>

    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { ReferenceType } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const toast = useToast();

const props = defineProps<{
  referenceType: ReferenceType;
}>();

const emit = defineEmits<{
  (e: "submit", rawmaterialtype: ReferenceType): void;
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
  validation.value = formValidation.validate(props.referenceType);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.referenceType);
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

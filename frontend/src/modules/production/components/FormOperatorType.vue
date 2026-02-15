<template>
  <form v-if="operatorType">
    <section class="four-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="operatorType.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="operatorType.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Cost/hora"
        id="cost"
        :type="BaseInputType.CURRENCY"
        v-model="operatorType.cost"
        :class="{
          'p-invalid': validation.errors.cost,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox
          v-model="operatorType.disabled"
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
import { OperatorType } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";
import { BaseInputType } from "@/types/component";

const props = defineProps<{
  operatortype: OperatorType;
}>();

onMounted(async () => {
  await plantModelStore.fetchOperatorTypes();
});

const emit = defineEmits<{
  (e: "submit", operatorType: OperatorType): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const { operatorType } = storeToRefs(plantModelStore);

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 carácters"),
  description: Yup.string()
    .required("La descripció és obligatori")
    .max(250, "La descripció pot superar els 250 carácters"),
  cost: Yup.number().required("El cost es obligatori").min(0),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.operatortype);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.operatortype);
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

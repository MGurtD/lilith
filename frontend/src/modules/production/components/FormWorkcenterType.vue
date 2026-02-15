<template>
  <form v-if="workcenterType">
    <section class="four-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="workcenterType.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="workcenterType.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <BaseInput
        :type="BaseInputType.NUMERIC"
        :minFractionDigits="2"
        class="mb-2"
        label="Marge de benefici"
        id="profitPercentage"
        v-model="workcenterType.profitPercentage"
        suffix="%"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox
          v-model="workcenterType.disabled"
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
import { Area, WorkcenterType } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";
import { BaseInputType } from "../../../types/component";

const props = defineProps<{
  workcentertype: WorkcenterType;
}>();

onMounted(async () => {
  await plantModelStore.fetchWorkcenterTypes();
});

const emit = defineEmits<{
  (e: "submit", workcentertype: WorkcenterType): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const { workcenterType } = storeToRefs(plantModelStore);

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
  validation.value = formValidation.validate(props.workcentertype);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.workcentertype);
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

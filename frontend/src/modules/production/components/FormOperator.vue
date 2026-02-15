<template>
  <form v-if="operator">
    <section class="three-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="operator.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Cognom"
        id="surname"
        v-model="operator.surname"
        :class="{
          'p-invalid': validation.errors.surname,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Codi"
        id="code"
        v-model="operator.code"
        :class="{
          'p-invalid': validation.errors.surname,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns">
      <BaseInput
        class="mb-2"
        label="NIF"
        id="vatNumber"
        v-model="operator.vatNumber"
        :class="{
          'p-invalid': validation.errors.vatNumber,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Tipus d'operari</label>
        <Select
          v-model="operator.operatorTypeId"
          :options="plantModelStore.operatorTypes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.operatorTypeId,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox v-model="operator.disabled" class="w-full" :binary="true" />
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
import { Operator } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";

const props = defineProps<{
  operator: Operator;
}>();

onMounted(async () => {
  await plantModelStore.fetchOperators();
  await plantModelStore.fetchOperatorTypes();
});

const emit = defineEmits<{
  (e: "submit", operator: Operator): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const { workcenterType } = storeToRefs(plantModelStore);

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 carácters"),
  surname: Yup.string()
    .required("El cognom és obligatori")
    .max(250, "El cognom no pot superar els 250 carácters"),
  code: Yup.string()
    .required("El codi és obligatori")
    .max(10, "El codi no pot superar els 10 carácters"),
  vatNumber: Yup.string()
    .required("El nif és obligatori")
    .max(20, "El nif no pot superar els 20 carácters"),
  operatorTypeId: Yup.string().required("El tipus d'operari es obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.operator);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.operator);
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

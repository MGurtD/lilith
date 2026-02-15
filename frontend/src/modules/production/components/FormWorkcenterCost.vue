<template>
  <form v-if="workcentercost">
    <section class="four-columns">
      <div>
        <label class="block text-900 mb-2">Màquina</label>
        <Select
          v-model="workcentercost.workcenterId"
          :options="plantModelStore.workcenters"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.WorkcenterId,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Estat de màquina</label>
        <Select
          v-model="workcentercost.machineStatusId"
          :options="plantModelStore.machineStatuses"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.machineStatusId,
          }"
        />
      </div>
      <BaseInput
        class="mb-2 w-full"
        label="Preu hora"
        v-model="workcentercost.cost"
        :type="BaseInputType.CURRENCY"
        :class="{
          'p-invalid': validation.errors.cost,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox
          v-model="workcentercost.disabled"
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
import { WorkcenterCost } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";
import { BaseInputType, FormActionMode } from "../../../types/component";

const props = defineProps<{
  workcentercost: WorkcenterCost;
}>();

onMounted(async () => {
  await plantModelStore.fetchOperatorTypes();
  await plantModelStore.fetchMachineStatuses();
});

const emit = defineEmits<{
  (e: "submit", workcentercost: WorkcenterCost): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const { workcentercost } = storeToRefs(plantModelStore);

const schema = Yup.object().shape({
  workcenterId: Yup.string().required("La màquina és obligatoria"),
  machineStatusId: Yup.string().required("L'estat de màquina és obligatori"),
  cost: Yup.number().required("El cost es obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.workcentercost);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.workcentercost);
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

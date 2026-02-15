<template>
  <form v-if="machineStatus">
    <div class="flex justify-content-end">
      <Button label="Guardar" @click="submitForm" />
    </div>

    <!-- Fila 1: Camps de text -->
    <div class="grid">
      <div class="col-12 md:col-6 lg:col-3">
        <BaseInput
          class="mb-2"
          label="Nom"
          id="name"
          v-model="machineStatus.name"
          :class="{ 'p-invalid': validation.errors.name }"
        />
      </div>
      <div class="col-12 md:col-6 lg:col-3">
        <BaseInput
          class="mb-2"
          label="Descripció"
          id="description"
          v-model="machineStatus.description"
          :class="{ 'p-invalid': validation.errors.description }"
        />
      </div>
      <div class="col-12 md:col-6 lg:col-3">
        <label class="block text-900 mb-2">Color</label>
        <ColorPicker
          v-model="machineStatus.color"
          class="mb-2"
          :class="{ 'p-invalid': validation.errors.color }"
        />
      </div>
      <div class="col-12 md:col-6 lg:col-3">
        <label class="block text-900 mb-2">Icona</label>
        <IconPicker
          v-model="machineStatus.icon"
          placeholder="Selecciona una icona"
        />
      </div>
    </div>

    <!-- Fila 2: Checkboxes -->
    <div class="grid mt-3">
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">Aturada</label>
        <Checkbox v-model="machineStatus.stopped" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">Operaris</label>
        <Checkbox v-model="machineStatus.operatorsAllowed" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">Tancada</label>
        <Checkbox v-model="machineStatus.closed" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">Preferida</label>
        <Checkbox v-model="machineStatus.preferred" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">Permet OF</label>
        <Checkbox v-model="machineStatus.workOrderAllowed" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox v-model="machineStatus.disabled" :binary="true" />
      </div>
    </div>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import IconPicker from "../../../components/IconPicker.vue";
import { MachineStatus } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";

const props = defineProps<{
  machineStatus: MachineStatus;
}>();

onMounted(async () => {
  await plantModelStore.fetchMachineStatuses();
});

const emit = defineEmits<{
  (e: "submit", machineStatus: MachineStatus): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const { machineStatus } = storeToRefs(plantModelStore);

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 carácters"),
  description: Yup.string()
    .required("La descripció és obligatoria")
    .max(250, "La descripció pot superar els 250 carácters"),
  color: Yup.string().required("El color es obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.machineStatus);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.machineStatus);
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

<template>
  <form v-if="machineStatus">
    <div class="flex justify-content-end">
      <Button :label="t('production.components.guardar')" @click="submitForm" />
    </div>

    <!-- Fila 1: Camps de text -->
    <div class="grid">
      <div class="col-12 md:col-6 lg:col-3">
        <BaseInput
          class="mb-2"
          :label="t('production.components.nom')"
          id="name"
          v-model="machineStatus.name"
          :class="{ 'p-invalid': validation.errors.name }"
        />
      </div>
      <div class="col-12 md:col-6 lg:col-3">
        <BaseInput
          class="mb-2"
          :label="t('production.components.descripcio')"
          id="description"
          v-model="machineStatus.description"
          :class="{ 'p-invalid': validation.errors.description }"
        />
      </div>
      <div class="col-12 md:col-6 lg:col-3">
        <label class="block text-900 mb-2">{{ t("production.components.color") }}</label>
        <ColorPicker
          v-model="machineStatus.color"
          class="mb-2"
          :class="{ 'p-invalid': validation.errors.color }"
        />
      </div>
      <div class="col-12 md:col-6 lg:col-3">
        <label class="block text-900 mb-2">{{ t("production.components.icona") }}</label>
        <IconPicker
          v-model="machineStatus.icon"
          :placeholder="t('production.components.seleccionaUnaIcona')"
        />
      </div>
    </div>

    <!-- Fila 2: Checkboxes -->
    <div class="grid mt-3">
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">{{ t("production.components.aturada") }}</label>
        <Checkbox v-model="machineStatus.stopped" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">{{ t("production.components.operaris") }}</label>
        <Checkbox v-model="machineStatus.operatorsAllowed" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">{{ t("production.components.tancada") }}</label>
        <Checkbox v-model="machineStatus.closed" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">{{ t("production.components.preferida") }}</label>
        <Checkbox v-model="machineStatus.preferred" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">{{ t("production.components.permetOf") }}</label>
        <Checkbox v-model="machineStatus.workOrderAllowed" :binary="true" />
      </div>
      <div class="col-6 md:col-4 lg:col-2">
        <label class="block text-900 mb-2">{{ t("production.components.desactivat") }}</label>
        <Checkbox v-model="machineStatus.disabled" :binary="true" />
      </div>
    </div>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
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
    .required(t("production.validation.elNomEsObligatori"))
    .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
  description: Yup.string()
    .required(t("production.validation.laDescripcioEsObligatoria"))
    .max(250, t("production.validation.laDescripcioPotSuperarEls250Caracters")),
  color: Yup.string().required(t("production.validation.elColorEsObligatori")),
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
      summary: t("production.components.formulariInvalid"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>

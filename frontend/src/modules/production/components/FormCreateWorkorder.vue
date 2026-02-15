<template>
  <form v-if="createWorkOrderDto">
    <div>
      <label class="block text-900 mb-2">Ruta</label>
      <Select
        v-model="createWorkOrderDto.workMasterId"
        filter
        :options="
          filteredWorkMasters
            ? filteredWorkMasters
            : workMasterStore.workmasters
        "
        optionValue="id"
        :optionLabel="formatWorkMasterLabel"
        class="w-full"
      />
    </div>
    <div class="mt-2">
      <BaseInput
        class="mb-2 w-full"
        label="Quantitat"
        v-model="createWorkOrderDto.plannedQuantity"
        :type="BaseInputType.NUMERIC"
      ></BaseInput>
    </div>
    <div>
      <label class="block text-900 mb-2">Data Prevista</label>
      <DatePicker
        v-model="createWorkOrderDto.plannedDate"
        dateFormat="dd/mm/yy"
        class="mt-2"
      />
    </div>
    <div class="mt-2">
      <label class="block text-900 mb-2">Comentari Fabriació</label>
      <Textarea class="w-full" v-model="createWorkOrderDto.comment" />
    </div>
    <br />
    <div>
      <Button label="Crear" style="float: right" @click="submitForm"></Button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { CreateWorkOrderDto, WorkMaster } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { useReferenceStore } from "../../shared/store/reference";
import { useWorkMasterStore } from "../store/workmaster";

const props = defineProps<{
  createWorkOrderDto: CreateWorkOrderDto;
  filteredWorkMasters?: Array<WorkMaster>;
}>();

const emit = defineEmits<{
  (e: "submit", createWorkOrderDto: CreateWorkOrderDto): void;
  (e: "cancel"): void;
}>();

const workMasterStore = useWorkMasterStore();
const referenceStore = useReferenceStore();
const toast = useToast();

onMounted(() => {
  if (!workMasterStore.workmasters) {
    workMasterStore.fetchAllActives();
  }
});

const formatWorkMasterLabel = (workMaster: WorkMaster) => {
  const referenceName = referenceStore.getShortNameById(workMaster.referenceId);
  let modeName = workMasterStore.workmasterModes.find(
    (mode) => mode.id === workMaster.mode,
  )?.value;

  return `${referenceName}  (Base = ${workMaster.baseQuantity} )  ${modeName}`;
};

const schema = Yup.object().shape({
  plannedQuantity: Yup.number()
    .min(1, "La quantitat ha de ser superior a 0")
    .required("La quanitat és obligatoria"),
  workMasterId: Yup.string().required("La ruta de fabricació és obligatoria"),
  plannedDate: Yup.string().required("La data prevista és obligatoria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.createWorkOrderDto);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.createWorkOrderDto);
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

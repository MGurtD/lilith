<template>
  <form v-if="createWorkOrderDto">
    <div>
      <label class="block text-900 mb-2">{{ t("production.components.ruta") }}</label>
      <Select
        v-model="createWorkOrderDto.workMasterId"
        :virtualScrollerOptions="{ itemSize: 38 }"
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
        :label="t('production.components.quantitat')"
        v-model="createWorkOrderDto.plannedQuantity"
        :type="BaseInputType.NUMERIC"
      ></BaseInput>
    </div>
    <div>
      <label class="block text-900 mb-2">{{ t("production.components.dataPrevista") }}</label>
      <DatePicker
        v-model="createWorkOrderDto.plannedDate"
        dateFormat="dd/mm/yy"
        class="mt-2"
      />
    </div>
    <div class="mt-2">
      <label class="block text-900 mb-2">{{ t("production.components.comentariFabriacio") }}</label>
      <Textarea class="w-full" v-model="createWorkOrderDto.comment" />
    </div>
    <br />
    <div>
      <Button :label="t('production.components.crear')" style="float: right" @click="submitForm"></Button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import { ref } from "vue";
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

const formatWorkMasterLabel = (workMaster: WorkMaster) => {
  const referenceName = referenceStore.getShortNameById(workMaster.referenceId);
  let modeName = workMasterStore.workmasterModes.find(
    (mode) => mode.id === workMaster.mode,
  )?.value;

  return `${referenceName}  (Base = ${workMaster.baseQuantity} )  ${modeName}`;
};

const schema = Yup.object().shape({
  plannedQuantity: Yup.number()
    .min(1, t("production.validation.laQuantitatHaDeSerSuperiorA0"))
    .required(t("production.validation.laQuanitatEsObligatoria")),
  workMasterId: Yup.string().required(t("production.validation.laRutaDeFabricacioEsObligatoria")),
  plannedDate: Yup.string().required(t("production.validation.laDataPrevistaEsObligatoria")),
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
      summary: t("production.components.formulariInvalid"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>

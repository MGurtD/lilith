<template>
  <SplitButton
    label="Guardar"
    @click="handleSubmit"
    :model="items"
    :size="'small'"
    class="grid_add_row_button"
  />
  <form v-if="workorder" class="pt-3">
    <section class="four-columns">
      <div>
        <BaseInput label="Codi" v-model="workorder.code" disabled />
      </div>
      <div>
        <DropdownReference
          label="Referència"
          v-model="workorder.referenceId"
          :fullName="true"
          disabled
        ></DropdownReference>
      </div>
      <div>
        <label class="block text-900 mb-2">Data prevista</label>
        <DatePicker
          v-model="workorder.plannedDate"
          dateFormat="dd/mm/yy"
          showTime
          hourFormat="24"
          class="mt-2"
          :class="{
            'p-invalid': validation.errors.plannedDate,
          }"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Quantitat prevista"
          v-model="workorder.plannedQuantity"
          :class="{
            'p-invalid': validation.errors.plannedQuantity,
          }"
        />
      </div>
    </section>
    <section class="four-columns">
      <div>
        <label class="block text-900 mb-2">Estat</label>
        <DropdownLifecycleStatusTransitions
          ref="statusTransitionsDropdown"
          v-model="workorder.statusId"
          :statusId="workorder.statusId"
          :class="{
            'p-invalid': validation.errors.statusId,
          }"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Prioritat"
          v-model="workorder.order"
          :class="{
            'p-invalid': validation.errors.plannedorderQuantity,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Període execució</label>
        <DatePicker
          v-model="dateRange"
          selectionMode="range"
          dateFormat="dd/mm/yy"
          showTime
          hourFormat="24"
          showIcon
          class="mt-2"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Quantitat total"
          v-model="workorder.totalQuantity"
          disabled
        />
      </div>
    </section>
    <div>
      <label class="block text-900 mb-2">Comentari fabricació</label>
      <Textarea class="w-full" v-model="workorder.comment"></Textarea>
    </div>
  </form>
</template>

<script setup lang="ts">
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { ref, computed } from "vue";
import { WorkOrder } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { PrimeIcons } from "@primevue/core/api";

const props = defineProps<{
  workorder: WorkOrder;
}>();

const emit = defineEmits<{
  (e: "submit", workorder: WorkOrder): void;
  (e: "cancel"): void;
  (e: "download"): void;
  (e: "downloadPdf"): void;
}>();

const toast = useToast();
const statusTransitionsDropdown = ref<InstanceType<
  typeof DropdownLifecycleStatusTransitions
> | null>(null);

const items = [
  {
    label: "Descarregar",
    icon: PrimeIcons.FILE_WORD,
    command: () => emit("download"),
  },
  {
    label: "Imprimir PDF",
    icon: PrimeIcons.FILE_PDF,
    command: () => emit("downloadPdf"),
  },
];

const dateRange = computed({
  get(): Date[] {
    const range: Date[] = [];
    if (props.workorder.startTime) {
      range.push(
        props.workorder.startTime instanceof Date
          ? props.workorder.startTime
          : new Date(props.workorder.startTime),
      );
    }
    if (props.workorder.endTime) {
      range.push(
        props.workorder.endTime instanceof Date
          ? props.workorder.endTime
          : new Date(props.workorder.endTime),
      );
    }
    return range;
  },
  set(value: Date[] | null) {
    if (value && value.length >= 1) {
      props.workorder.startTime = value[0];
      props.workorder.endTime = value.length >= 2 ? value[1] : null;
    } else {
      props.workorder.startTime = null;
      props.workorder.endTime = null;
    }
  },
});

const schema = Yup.object().shape({
  plannedQuantity: Yup.number()
    .min(1, "La quantitat ha de ser superior a 0")
    .required("La quanitat és obligatoria"),
  referenceId: Yup.string().required("La referència és obligatoria"),
  order: Yup.number().required("L'ordre és obligatori"),
  plannedDate: Yup.string().required("La data prevista és obligatoria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.workorder);
};

const handleSubmit = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.workorder);
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

const reloadLifecycleTransitions = async () => {
  await statusTransitionsDropdown.value?.reloadTransitions();
};

defineExpose({
  submitForm: handleSubmit,
  reloadLifecycleTransitions,
});
</script>

<template>
  <form v-if="workmaster">
    <div class="grid_add_row_button">
      <Button label="Calcular Cost" size="small" @click="calculateCost" />
      &nbsp;
      <Button label="Guardar" size="small" @click="submitForm" />
      <br />
    </div>
    <section class="five-columns">
      <div>
        <DropdownReference
          label="Referència"
          v-model="workmaster.referenceId"
          :fullName="true"
        ></DropdownReference>
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Quantitat Base"
          :decimals="2"
          v-model="workmaster.baseQuantity"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Mode</label>
        <Select
          v-model="workmaster.mode"
          :options="workmasterStore.workmasterModes"
          optionLabel="value"
          optionValue="id"
          placeholder="Seleccione el modo"
          class="w-full"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox v-model="workmaster.disabled" class="w-full" :binary="true" />
      </div>
    </section>
    <section class="six-columns">
      <div class="mt-1">
        <label class="block text-900 mb-2">Cost Operari:</label>
        <span class="summary-field">{{
          formatCurrency(workmaster.operatorCost)
        }}</span>
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">Cost Màquina:</label>
        <span class="summary-field">{{
          formatCurrency(workmaster.machineCost)
        }}</span>
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">Cost Material:</label>
        <span class="summary-field">{{
          formatCurrency(workmaster.materialCost)
        }}</span>
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">Cost Extern:</label>
        <span class="summary-field">{{
          formatCurrency(workmaster.externalCost)
        }}</span>
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">Cost Total:</label>
        <span class="summary-field">{{ formatCurrency(totalCost) }}</span>
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">Pes Total:</label>
        <span class="summary-field">{{ workmaster.totalWeight }} KG</span>
      </div>
    </section>
  </form>
</template>

<script setup lang="ts">
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { ref, computed } from "vue";
import { WorkMaster } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { formatCurrency } from "../../../utils/functions";
import { useToast } from "primevue/usetoast";
import { useWorkMasterStore } from "../store/workmaster";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";

const props = defineProps<{
  workmaster: WorkMaster;
}>();

const emit = defineEmits<{
  (e: "submit", workmaster: WorkMaster): void;
  (e: "calculateCost", workmaster: WorkMaster): void;
  (e: "cancel"): void;
}>();

const workmasterStore = useWorkMasterStore();
const toast = useToast();

const totalCost = computed(() => {
  return (
    props.workmaster.operatorCost +
    props.workmaster.machineCost +
    props.workmaster.materialCost +
    props.workmaster.externalCost
  );
});

const schema = Yup.object().shape({
  baseQuantity: Yup.number()
    .min(1, "La quantitat base ha de ser superior a 0")
    .required("La quanitat base és obligatoria"),
  referenceId: Yup.string().required("La referència és obligatoria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.workmaster);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.workmaster);
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

const calculateCost = () => {
  validate();
  if (validation.value.result) {
    emit("calculateCost", props.workmaster);
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

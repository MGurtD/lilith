<template>
  <form v-if="exercise">
    <div class="four-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="exercise.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="exercise.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Data inici</label>
        <DatePicker
          v-model="exercise.startDate"
          dateFormat="dd/mm/yy"
          :class="{
            'p-invalid': validation.errors.startDate,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Data fi</label>
        <DatePicker
          v-model="exercise.endDate"
          dateFormat="dd/mm/yy"
          :class="{
            'p-invalid': validation.errors.endDate,
          }"
        />
      </div>
    </div>

    <section class="four-columns">
      <div>
        <BaseInput label="Pressupostos" v-model="exercise.budgetCounter" />
      </div>
      <div>
        <BaseInput
          label="Comandes de venta"
          v-model="exercise.salesOrderCounter"
        />
      </div>
      <div>
        <BaseInput
          label="Albarans de venta"
          v-model="exercise.deliveryNoteCounter"
        />
      </div>
      <div>
        <BaseInput
          label="Factures de venta"
          v-model="exercise.salesInvoiceCounter"
        />
      </div>
    </section>

    <div class="four-columns mt-2">
      <div>
        <BaseInput
          label="Comandes de compra"
          v-model="exercise.purchaseOrderCounter"
        />
      </div>
      <div>
        <BaseInput
          label="Albarans de recepció"
          v-model="exercise.receiptCounter"
        />
      </div>
      <div>
        <BaseInput
          label="Factures de compra"
          v-model="exercise.purchaseInvoiceCounter"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox v-model="exercise.disabled" class="w-full" :binary="true" />
      </div>
    </div>

    <section class="four-columns mt-2">
      <div>
        <BaseInput
          label="Marge material per defecte (%)"
          v-model.number="exercise.materialProfit"
          :type="BaseInputType.NUMERIC"
          :decimals="2"
        />
      </div>
      <div>
        <BaseInput
          label="Marge extern per defecte (%)"
          v-model.number="exercise.externalProfit"
          :type="BaseInputType.NUMERIC"
          :decimals="2"
        />
      </div>
    </section>

    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { Exercise } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { convertDateTimeToJSON } from "../../../utils/functions";
import { BaseInputType } from "@/types/component";

const props = defineProps<{
  exercise: Exercise;
}>();

const emit = defineEmits<{
  (e: "submit", exercise: Exercise): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 carácters"),
  description: Yup.string()
    .required("La descripció és obligatori")
    .max(250, "La descripció pot superar els 250 carácters"),
  startDate: Yup.date().required("La data d'inici es obligatoria"),
  endDate: Yup.date()
    .required("La data final es obligatoria")
    .min(
      Yup.ref("startDate"),
      "La data final de l'exercici ha de ser posterior a l'inici",
    ),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.exercise);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    props.exercise.startDate = convertDateTimeToJSON(props.exercise.startDate);
    props.exercise.endDate = convertDateTimeToJSON(props.exercise.endDate);

    emit("submit", props.exercise);
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

<template>
  <form v-if="shiftdetail">
    <section class="three-columns">
      <DatePicker
        class="mb-2"
        label="Inici Torn:"
        v-model="shiftdetail.startTime"
        timeOnly
        hourFormat="24"
      />
      <DatePicker
        class="mb-2"
        label="Fi Torn:"
        v-model="shiftdetail.endTime"
        timeOnly
        hourFormat="24"
      />
      <div class="mb-4">
        <label class="block text-900 mb-2">Temps productiu:</label>
        <Checkbox v-model="shiftdetail.isProductiveTime" :binary="true" />
      </div>
    </section>
    <Button label="Confirmar" @click="submitForm" style="float: right" />
  </form>
</template>
<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { ShiftDetail } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { extractTime } from "../../../utils/functions";

const props = defineProps<{
  shiftdetail: ShiftDetail;
}>();

const emit = defineEmits<{
  (e: "submit", shiftdetail: ShiftDetail): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.shiftdetail);
};

const submitForm = async () => {
  props.shiftdetail.startTime = extractTime(props.shiftdetail.startTime);
  props.shiftdetail.endTime = extractTime(props.shiftdetail.endTime);

  validate();
  if (validation.value.result) {
    emit("submit", props.shiftdetail);
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

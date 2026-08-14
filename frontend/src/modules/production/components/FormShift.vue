<template>
  <form v-if="shift">
    <section class="two-columns">
      <BaseInput
        class="mb-2"
        :label="t('production.components.nom')"
        v-model="shift.name"
        :class="{
          'p-invalid': validation.errors.baseAmount,
        }"
      ></BaseInput>
      <div class="mb-4">
        <label class="block text-900 mb-2">{{ t("production.components.deshabilitat") }}</label>
        <Checkbox v-model="shift.disabled" :binary="true" />
      </div>
    </section>
    <Button :label="t('production.components.confirmar')" @click="submitForm" style="float: right" />
  </form>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { Shift } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const props = defineProps<{
  shift: Shift;
}>();

const emit = defineEmits<{
  (e: "submit", shift: Shift): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("production.validation.elNomEsObligatori"))
    .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.shift);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.shift);
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

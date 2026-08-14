<template>
  <form v-if="tax">
    <div class="two-columns">
      <BaseInput
        class="mb-2"
        :label="$t('shared.taxes.form.name')"
        id="name"
        v-model="tax.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        :type="BaseInputType.NUMERIC"
        :label="$t('shared.taxes.form.percentage')"
        id="percentatge"
        v-model="tax.percentatge"
        :class="{
          'p-invalid': validation.errors.percentatge,
        }"
      ></BaseInput>
    </div>

    <div class="two-columns mt-2">
      <div>
        <label class="block text-900 mb-2">{{ $t('shared.taxes.form.reverseCharge') }}</label>
        <Checkbox v-model="tax.isReverseCharge" class="w-full" :binary="true" />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ $t('shared.taxes.form.disabled') }}</label>
        <Checkbox v-model="tax.disabled" class="w-full" :binary="true" />
      </div>
    </div>

    <div class="mt-2">
      <Button :label="$t('shared.taxes.form.save')" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import BaseInput from "../../../components/BaseInput.vue";
import { Tax } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { BaseInputType } from "../../../types/component";

const { t } = useI18n();

const props = defineProps<{
  tax: Tax;
}>();

const emit = defineEmits<{
  (e: "submit", tax: Tax): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom comercial no pot superar els 250 carácters"),
  percentatge: Yup.number().required("La freqüència és obligatoria"),
  isReverseCharge: Yup.bool().required("El camp inversió del subjecte passiu és obligatori"),
  disabled: Yup.bool().required("El camp deshabilitat és obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.tax);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.tax);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("shared.common.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>

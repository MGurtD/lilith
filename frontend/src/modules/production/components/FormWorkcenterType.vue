<template>
  <form v-if="workcenterType">
    <section class="four-columns">
      <BaseInput
        class="mb-2"
        :label="t('production.components.nom')"
        id="name"
        v-model="workcenterType.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        :label="t('production.components.descripcio')"
        id="description"
        v-model="workcenterType.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <BaseInput
        :type="BaseInputType.NUMERIC"
        :minFractionDigits="2"
        class="mb-2"
        :label="t('production.components.margeDeBenefici')"
        id="profitPercentage"
        v-model="workcenterType.profitPercentage"
        suffix="%"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.desactivat") }}</label>
        <Checkbox
          v-model="workcenterType.disabled"
          class="w-full"
          :binary="true"
        />
      </div>
    </section>

    <div class="mt-2">
      <Button :label="t('production.components.guardar')" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { Area, WorkcenterType } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";
import { BaseInputType } from "../../../types/component";

const props = defineProps<{
  workcentertype: WorkcenterType;
}>();

onMounted(async () => {
  await plantModelStore.fetchWorkcenterTypes();
});

const emit = defineEmits<{
  (e: "submit", workcentertype: WorkcenterType): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const { workcenterType } = storeToRefs(plantModelStore);

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("production.validation.elNomEsObligatori"))
    .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
  description: Yup.string()
    .required(t("production.validation.laDescripcioEsObligatori"))
    .max(250, t("production.validation.laDescripcioPotSuperarEls250Caracters")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.workcentertype);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.workcentertype);
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

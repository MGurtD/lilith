<template>
  <form v-if="workcenter">
    <div class="grid_add_row_button">
      <Button :label="t('production.components.guardar')" style="float: right" @click="submitForm" />
    </div>
    <section class="four-columns mb-2">
      <BaseInput
        :label="t('production.components.nom')"
        id="name"
        v-model="workcenter.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('production.components.descripcio')"
        id="description"
        v-model="workcenter.description"
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
        v-model="workcenter.profitPercentage"
        suffix="%"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.desactivat") }}</label>
        <Checkbox v-model="workcenter.disabled" class="w-full" :binary="true" />
      </div>
    </section>
    <section class="three-columns">
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.tipus") }}</label>
        <Select
          v-model="workcenter.workcenterTypeId"
          :options="plantModelStore.workcenterTypes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.workcenterTypeId,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.area") }}</label>
        <Select
          v-model="workcenter.areaId"
          :options="plantModelStore.areas"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.areaId,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.torn") }}</label>
        <Select
          v-model="workcenter.shiftId"
          :options="shiftStore.shifts"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.shiftId,
          }"
        />
      </div>
    </section>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { Workcenter } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";
import { useShiftStore } from "../store/shift";

const props = defineProps<{
  workcenter: Workcenter;
}>();

const emit = defineEmits<{
  (e: "submit", workcenter: Workcenter): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const shiftStore = useShiftStore();
const { workcenter } = storeToRefs(plantModelStore);

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("production.validation.elNomEsObligatori"))
    .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
  description: Yup.string()
    .required(t("production.validation.laDescripcioEsObligatori"))
    .max(250, t("production.validation.laDescripcioPotSuperarEls250Caracters")),
  workcenterTypeId: Yup.string().required(t("production.validation.elTipusEsObligatori")),
  areaId: Yup.string().required(t("production.validation.lAreaEsObligatoria")),
  shiftId: Yup.string().required(t("production.validation.elTornEsObligatori")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.workcenter);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.workcenter);
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

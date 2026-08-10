<template>
  <form v-if="operatorType">
    <section class="four-columns">
      <BaseInput
        class="mb-2"
        :label="t('production.components.nom')"
        id="name"
        v-model="operatorType.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        :label="t('production.components.descripcio')"
        id="description"
        v-model="operatorType.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        :label="t('production.components.costHora')"
        id="cost"
        :type="BaseInputType.CURRENCY"
        v-model="operatorType.cost"
        :class="{
          'p-invalid': validation.errors.cost,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.desactivat") }}</label>
        <Checkbox
          v-model="operatorType.disabled"
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
import { OperatorType } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";
import { BaseInputType } from "@/types/component";

const props = defineProps<{
  operatortype: OperatorType;
}>();

onMounted(async () => {
  await plantModelStore.fetchOperatorTypes();
});

const emit = defineEmits<{
  (e: "submit", operatorType: OperatorType): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const { operatorType } = storeToRefs(plantModelStore);

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("production.validation.elNomEsObligatori"))
    .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
  description: Yup.string()
    .required(t("production.validation.laDescripcioEsObligatori"))
    .max(250, t("production.validation.laDescripcioPotSuperarEls250Caracters")),
  cost: Yup.number().required(t("production.validation.elCostEsObligatori")).min(0),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.operatortype);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.operatortype);
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

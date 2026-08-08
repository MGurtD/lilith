<template>
  <form v-if="operator">
    <section class="three-columns">
      <BaseInput
        class="mb-2"
        :label="t('production.components.nom')"
        id="name"
        v-model="operator.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        :label="t('production.components.cognom')"
        id="surname"
        v-model="operator.surname"
        :class="{
          'p-invalid': validation.errors.surname,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        :label="t('production.components.codi')"
        id="code"
        v-model="operator.code"
        :class="{
          'p-invalid': validation.errors.surname,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns">
      <BaseInput
        class="mb-2"
        label="NIF"
        id="vatNumber"
        v-model="operator.vatNumber"
        :class="{
          'p-invalid': validation.errors.vatNumber,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.tipusDOperari") }}</label>
        <Select
          v-model="operator.operatorTypeId"
          :options="plantModelStore.operatorTypes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.operatorTypeId,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.desactivat") }}</label>
        <Checkbox v-model="operator.disabled" class="w-full" :binary="true" />
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
import { Operator } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";

const props = defineProps<{
  operator: Operator;
}>();

onMounted(async () => {
  await plantModelStore.fetchOperators();
  await plantModelStore.fetchOperatorTypes();
});

const emit = defineEmits<{
  (e: "submit", operator: Operator): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const { workcenterType } = storeToRefs(plantModelStore);

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("production.validation.elNomEsObligatori"))
    .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
  surname: Yup.string()
    .required(t("production.validation.elCognomEsObligatori"))
    .max(250, t("production.validation.elCognomNoPotSuperarEls250Caracters")),
  code: Yup.string()
    .required(t("production.validation.elCodiEsObligatori"))
    .max(10, t("production.validation.elCodiNoPotSuperarEls10Caracters")),
  vatNumber: Yup.string()
    .required(t("production.validation.elNifEsObligatori"))
    .max(20, t("production.validation.elNifNoPotSuperarEls20Caracters")),
  operatorTypeId: Yup.string().required(t("production.validation.elTipusDOperariEsObligatori")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.operator);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.operator);
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

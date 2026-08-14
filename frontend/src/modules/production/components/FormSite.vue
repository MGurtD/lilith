<template>
  <form v-if="site">
    <section class="three-columns mb-2">
      <BaseInput
        :label="t('production.components.nom')"
        id="name"
        v-model="site.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('production.components.descripcio')"
        id="description"
        v-model="site.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <BaseInput
        label="CIF"
        id="vatNumber"
        v-model="site.vatNumber"
        :class="{
          'p-invalid': validation.errors.vatNumber,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns mb-2">
      <BaseInput
        :label="t('production.components.telefon')"
        id="phone"
        v-model="site.phoneNumber"
        :class="{
          'p-invalid': validation.errors.phone,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('production.components.emailGeneral')"
        id="email"
        v-model="site.email"
        :class="{
          'p-invalid': validation.errors.email,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('production.components.emailCompres')"
        id="emailPurchase"
        v-model="site.emailPurchase"
        :class="{
          'p-invalid': validation.errors.emailPurchase,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns mb-2">
      <BaseInput
        :label="t('production.components.emailVentes')"
        id="emailSales"
        v-model="site.emailSales"
        :class="{
          'p-invalid': validation.errors.emailSales,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.empresa") }}</label>
        <Select
          v-model="site.enterpriseId"
          :options="siteStore.enterprises"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.enterpriseId,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.desactivat") }}</label>
        <Checkbox v-model="site.disabled" class="w-full" :binary="true" />
      </div>
    </section>

    <LocationFields
      :model-value="site"
      :validation-errors="validation.errors"
    />

    <div class="flex justify-content-end">
      <Button :label="t('production.components.guardar')" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import LocationFields from "@/components/LocationFields.vue";
import { Site } from "../types";
import { usePlantModelStore } from "../store/plantmodel";

import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const props = defineProps<{
  site: Site;
}>();

const emit = defineEmits<{
  (e: "submit", site: Site): void;
  (e: "cancel"): void;
}>();

onMounted(async () => {
  await siteStore.fetchEnterprises();
});

const toast = useToast();
const siteStore = usePlantModelStore();

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("production.validation.elNomEsObligatori"))
    .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
  description: Yup.string()
    .required(t("production.validation.laDescripcioEsObligatoria"))
    .max(250, t("production.validation.laDescripcioNoPotSuperarEls250Caracters")),
  email: Yup.string()
    .email(t("production.validation.elCorreuElectronicNoEsValid"))
    .required(t("production.validation.elCorreuElectronicEsObligatori")),
  emailSales: Yup.string()
    .email(t("production.validation.elCorreuElectronicDeVentesNoEsValid"))
    .required(t("production.validation.elCorreuElectronicDeVentesEsObligatori")),
  emailPurchase: Yup.string()
    .email(t("production.validation.elCorreuElectronicDeCompresNoEsValid"))
    .required(t("production.validation.elCorreuElectronicDeCompresEsObligatori")),
  enterpriseId: Yup.string().required(t("production.validation.lEmpresaEsObligatoria")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.site);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.site);
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


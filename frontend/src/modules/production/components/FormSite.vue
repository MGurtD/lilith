<template>
  <form v-if="site">
    <section class="three-columns mb-2">
      <BaseInput
        label="Nom"
        id="name"
        v-model="site.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        label="Descripció"
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
      <div>
        <label class="block text-900 mb-2">País</label>
        <Select
          v-model="site.country"
          :options="['Espanya']"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.country,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Província</label>
        <Select
          v-model="site.region"
          :options="spanishGeo.regions"
          optionValue="nm"
          optionLabel="nm"
          @change="onRegionChanged"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.region,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Municipi</label>
        <Select
          v-model="site.city"
          :options="spanishGeo.getTownsByRegionName(site.region)"
          optionValue="nm"
          optionLabel="nm"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.city,
          }"
        />
      </div>
    </section>
    <section class="three-columns mb-2">
      <BaseInput
        label="Codi Postal"
        id="postalCode"
        v-model="site.postalCode"
        :class="{
          'p-invalid': validation.errors.postalCode,
        }"
      ></BaseInput>
      <BaseInput
        label="Direcció"
        id="address"
        v-model="site.address"
        :class="{
          'p-invalid': validation.errors.address,
        }"
      ></BaseInput>
      <BaseInput
        label="Telèfon"
        id="phone"
        v-model="site.phoneNumber"
        :class="{
          'p-invalid': validation.errors.phone,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns mb-2">
      <BaseInput
        label="Email general"
        id="email"
        v-model="site.email"
        :class="{
          'p-invalid': validation.errors.email,
        }"
      ></BaseInput>
      <BaseInput
        label="Email compres"
        id="email"
        v-model="site.emailPurchase"
        :class="{
          'p-invalid': validation.errors.emailPurchase,
        }"
      ></BaseInput>
      <BaseInput
        label="Email ventes"
        id="email"
        v-model="site.emailSales"
        :class="{
          'p-invalid': validation.errors.emailSales,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns mb-2">
      <div>
        <label class="block text-900 mb-2">Empresa</label>
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
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox v-model="site.disabled" class="w-full" :binary="true" />
      </div>
    </section>

    <div>
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { useSpanishGeography } from "../../../store/geography";
import { Site } from "../types";
import { storeToRefs } from "pinia";
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
const spanishGeo = useSpanishGeography();
const siteStore = usePlantModelStore();
const { site } = storeToRefs(siteStore);

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 carácters"),
  description: Yup.string()
    .required("La descripció és obligatori")
    .max(250, "La descripció pot superar els 250 carácters"),
  email: Yup.string()
    .email("El correu electrònic no és vàlid")
    .required("El correu electrònic és obligatori"),
  emailSales: Yup.string()
    .email("El correu electrònic de ventes no és vàlid")
    .required("El correu electrònic de ventes és obligatori"),
  emailPurchase: Yup.string()
    .email("El correu electrònic de compres no és vàlid")
    .required("El correu electrònic de compres és obligatori"),
  enterpriseId: Yup.string().required("L'empresa es obligatoria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.site);
};

const onRegionChanged = () => {
  (site.value as Site).address = "";
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
      summary: "Formulari inválid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>

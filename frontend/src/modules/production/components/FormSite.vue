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
      <DropdownCountry
        v-model="site.country"
        label="País"
        :class="{
          'p-invalid': validation.errors.country,
        }"
      />
      <div class="col-span-2">
        <label class="block text-900 mb-2">{{ t("location.searchLabel") }}</label>
        <AutocompleteLocation
          v-model="locationSelection"
          :label="''"
          :placeholder="t('location.placeholder')"
          :country-code="autocompleteCountryCode"
          :disabled="!site.country"
          @select="onLocationSelected"
          @clear="onLocationCleared"
        />
      </div>
    </section>
    <section class="four-columns mb-2">
      <BaseInput
        label="Direcció"
        id="address"
        v-model="site.address"
        :class="{
          'p-invalid': validation.errors.address,
        }"
      ></BaseInput>
      <BaseInput
        label="Ciutat"
        id="city"
        v-model="site.city"
        :class="{
          'p-invalid': validation.errors.city,
        }"
      ></BaseInput>
      <BaseInput
        label="Província"
        id="region"
        v-model="site.region"
        :class="{
          'p-invalid': validation.errors.region,
        }"
      ></BaseInput>
      <BaseInput
        label="Codi Postal"
        id="postalCode"
        v-model="site.postalCode"
        :class="{
          'p-invalid': validation.errors.postalCode,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns mb-2">
      <BaseInput
        label="Telèfon"
        id="phone"
        v-model="site.phoneNumber"
        :class="{
          'p-invalid': validation.errors.phone,
        }"
      ></BaseInput>
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
        id="emailPurchase"
        v-model="site.emailPurchase"
        :class="{
          'p-invalid': validation.errors.emailPurchase,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns mb-2">
      <BaseInput
        label="Email ventes"
        id="emailSales"
        v-model="site.emailSales"
        :class="{
          'p-invalid': validation.errors.emailSales,
        }"
      ></BaseInput>
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
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import BaseInput from "../../../components/BaseInput.vue";
import AutocompleteLocation from "../../../components/AutocompleteLocation.vue";
import DropdownCountry from "../../shared/components/DropdownCountry.vue";
import { Site } from "../types";
import { usePlantModelStore } from "../store/plantmodel";
import type { AddressAutocompleteResult } from "@/types";

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
const { t } = useI18n();

const locationSelection = ref<AddressAutocompleteResult | null>(null);

const autocompleteCountryCode = computed(() => {
  return props.site?.country?.toLowerCase() ?? "es";
});

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 caràcters"),
  description: Yup.string()
    .required("La descripció és obligatòria")
    .max(250, "La descripció no pot superar els 250 caràcters"),
  email: Yup.string()
    .email("El correu electrònic no és vàlid")
    .required("El correu electrònic és obligatori"),
  emailSales: Yup.string()
    .email("El correu electrònic de ventes no és vàlid")
    .required("El correu electrònic de ventes és obligatori"),
  emailPurchase: Yup.string()
    .email("El correu electrònic de compres no és vàlid")
    .required("El correu electrònic de compres és obligatori"),
  enterpriseId: Yup.string().required("L'empresa és obligatòria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.site);
};

const onLocationSelected = (result: AddressAutocompleteResult) => {
  const s = props.site;
  const addressParts = [result.street, result.housenumber].filter(Boolean);
  s.address = addressParts.join(", ") || result.addressLine1;
  s.city = result.city;
  s.region = result.state;
  s.postalCode = result.postcode;
  s.latitude = result.lat;
  s.longitude = result.lon;
};

const onLocationCleared = () => {
  const s = props.site;
  s.address = "";
  s.city = "";
  s.region = "";
  s.postalCode = "";
  s.latitude = 0;
  s.longitude = 0;
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
      summary: "Formulari invàlid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>

<style scoped>
.col-span-2 {
  grid-column: span 2;
}
</style>

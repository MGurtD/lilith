<template>
  <form v-if="address">
    <section class="three-columns mb-2">
      <BaseInput
        id="name"
        label="Nom"
        v-model="address.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Principal</label>
        <Checkbox v-model="address.main" class="w-full" :binary="true" />
      </div>
      <div>
        <label class="block text-900 mb-2">Desactivada</label>
        <Checkbox v-model="address.disabled" class="w-full" :binary="true" />
      </div>
    </section>

    <section class="three-columns mb-2">
      <DropdownCountry
        v-model="address.country"
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
          :disabled="!address.country"
          @select="onLocationSelected"
          @clear="onLocationCleared"
        />
      </div>
    </section>

    <section class="four-columns mb-2">
      <BaseInput
        label="Direcció"
        id="address"
        v-model="address.address"
        :class="{
          'p-invalid': validation.errors.address,
        }"
      ></BaseInput>
      <BaseInput
        label="Ciutat"
        id="city"
        v-model="address.city"
        :class="{
          'p-invalid': validation.errors.city,
        }"
      ></BaseInput>
      <BaseInput
        label="Província"
        id="region"
        v-model="address.region"
        :class="{
          'p-invalid': validation.errors.region,
        }"
      ></BaseInput>
      <BaseInput
        label="Codi Postal"
        id="postalCode"
        v-model="address.postalCode"
        :class="{
          'p-invalid': validation.errors.postalCode,
        }"
      ></BaseInput>
    </section>

    <section class="three-columns mb-2">
      <BaseInput
        disabled
        label="Distància des de la seu (km)"
        id="distanceFromSite"
        :modelValue="address.distanceFromSite ?? null"
      ></BaseInput>
    </section>

    <div>
      <label class="block text-900 mb-2">Observacions</label>
      <Textarea v-model="address.observations" class="w-full" />
    </div>

    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import BaseInput from "../../../components/BaseInput.vue";
import AutocompleteLocation from "../../../components/AutocompleteLocation.vue";
import DropdownCountry from "../../shared/components/DropdownCountry.vue";
import type { AddressAutocompleteResult } from "@/types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { CustomerAddress } from "../types";

const props = defineProps<{
  address: CustomerAddress;
}>();

const emit = defineEmits<{
  (e: "submit", address: CustomerAddress): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const { t } = useI18n();

const locationSelection = ref<AddressAutocompleteResult | null>(null);

const autocompleteCountryCode = computed(() => {
  return props.address?.country?.toLowerCase() ?? "es";
});

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 caràcters"),
  country: Yup.string().required("El país és obligatori"),
  region: Yup.string().required("La província és obligatòria"),
  city: Yup.string().required("El municipi és obligatori"),
  postalCode: Yup.string().required("El codi postal és obligatori"),
  address: Yup.string().required("La direcció és obligatòria"),
  main: Yup.boolean().required(),
  disabled: Yup.boolean().required(),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.address);
};

const onLocationSelected = (result: AddressAutocompleteResult) => {
  const a = props.address;
  const addressParts = [result.street, result.housenumber].filter(Boolean);
  a.address = addressParts.join(", ") || result.addressLine1;
  a.city = result.city;
  a.region = result.state;
  a.postalCode = result.postcode;
  a.latitude = result.lat;
  a.longitude = result.lon;
};

const onLocationCleared = () => {
  const a = props.address;
  a.address = "";
  a.city = "";
  a.region = "";
  a.postalCode = "";
  a.latitude = 0;
  a.longitude = 0;
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.address);
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

<template>
  <section class="three-columns mb-2">
    <DropdownCountry
      v-model="model.country"
      label="País"
      :class="{
        'p-invalid': validationErrors?.country,
      }"
    />
    <div class="col-span-2">
      <label class="block text-900 mb-2">{{ t("location.searchLabel") }}</label>
      <AutocompleteLocation
        v-model="locationSelection"
        :label="''"
        :placeholder="t('location.placeholder')"
        :country-code="autocompleteCountryCode"
        :disabled="!model.country"
        @select="onLocationSelected"
        @clear="onLocationCleared"
      />
    </div>
  </section>

  <section class="four-columns mb-2">
    <BaseInput
      label="Direcció"
      id="location-address"
      v-model="model.address"
      :class="{
        'p-invalid': validationErrors?.address,
      }"
    ></BaseInput>
    <BaseInput
      label="Ciutat"
      id="location-city"
      v-model="model.city"
      :class="{
        'p-invalid': validationErrors?.city,
      }"
    ></BaseInput>
    <BaseInput
      label="Província"
      id="location-region"
      v-model="model.region"
      :class="{
        'p-invalid': validationErrors?.region,
      }"
    ></BaseInput>
    <BaseInput
      label="Codi Postal"
      id="location-postalCode"
      v-model="model.postalCode"
      :class="{
        'p-invalid': validationErrors?.postalCode,
      }"
    ></BaseInput>
  </section>

  <Panel
    :header="t('location.coordinatesSection')"
    :toggleable="true"
    :collapsed="true"
    class="mt-2 mb-2"
  >
    <section class="three-columns">
      <div>
        <label class="block text-900 mb-2">{{ t("location.latitude") }}</label>
        <InputNumber
          v-model="model.latitude"
          :minFractionDigits="2"
          :maxFractionDigits="8"
          class="w-full"
          mode="decimal"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("location.longitude") }}</label>
        <InputNumber
          v-model="model.longitude"
          :minFractionDigits="2"
          :maxFractionDigits="8"
          class="w-full"
          mode="decimal"
        />
      </div>
      <BaseInput
        v-if="showDistance"
        disabled
        :label="t('location.distanceFromSite')"
        id="location-distanceFromSite"
        :modelValue="model.distanceFromSite ?? null"
      ></BaseInput>
    </section>
  </Panel>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import BaseInput from "@/components/BaseInput.vue";
import AutocompleteLocation from "@/components/AutocompleteLocation.vue";
import DropdownCountry from "@/modules/shared/components/DropdownCountry.vue";
import type { AddressAutocompleteResult, LocationData } from "@/types";

const props = withDefaults(
  defineProps<{
    modelValue: LocationData;
    showDistance?: boolean;
    validationErrors?: Record<string, unknown>;
  }>(),
  {
    showDistance: false,
    validationErrors: undefined,
  },
);

const { t } = useI18n();

const model = computed(() => props.modelValue);

const locationSelection = ref<AddressAutocompleteResult | null>(null);

const autocompleteCountryCode = computed(() => {
  return model.value?.country?.toLowerCase() ?? "es";
});

function onLocationSelected(result: AddressAutocompleteResult) {
  const m = model.value;
  const addressParts = [result.street, result.housenumber].filter(Boolean);
  m.address = addressParts.join(", ") || result.addressLine1;
  m.city = result.city;
  m.region = result.state;
  m.postalCode = result.postcode;
  m.latitude = result.lat;
  m.longitude = result.lon;
}

function onLocationCleared() {
  const m = model.value;
  m.address = "";
  m.city = "";
  m.region = "";
  m.postalCode = "";
  m.latitude = 0;
  m.longitude = 0;
}
</script>

<style scoped>
.col-span-2 {
  grid-column: span 2;
}
</style>

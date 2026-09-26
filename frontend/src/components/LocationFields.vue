<script setup lang="ts">
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { AddressAutocompleteResult, LocationData } from "@/types";
import AutocompleteLocation from "@/components/AutocompleteLocation.vue";
import BaseInput from "@/components/BaseInput.vue";
import DropdownCountry from "@/modules/shared/components/DropdownCountry.vue";

const props = withDefaults(
  defineProps<{
    modelValue: LocationData;
    showDistance?: boolean;
    validationErrors?: Record<string, unknown>;
    showValidationMessages?: boolean;
    disabled?: boolean;
  }>(),
  {
    showDistance: false,
    validationErrors: undefined,
    showValidationMessages: false,
    disabled: false,
  },
);

const emit = defineEmits<{
  (event: "update:modelValue", value: LocationData): void;
}>();

const { t } = useI18n();
const locationSelection = ref<AddressAutocompleteResult | null>(null);

const autocompleteCountryCode = computed(
  () => props.modelValue.country?.toLowerCase() ?? "es",
);
const hasCoordinates = computed(
  () =>
    props.modelValue.latitude !== 0 || props.modelValue.longitude !== 0,
);
const mapUrl = computed(
  () =>
    `https://www.google.com/maps?q=${props.modelValue.latitude},${props.modelValue.longitude}`,
);

const updateField = <K extends keyof LocationData>(
  field: K,
  value: LocationData[K],
): void => {
  emit("update:modelValue", { ...props.modelValue, [field]: value });
};

const errorMessage = (field: keyof LocationData): string => {
  const error = props.validationErrors?.[field];
  if (typeof error === "string") return error;
  if (Array.isArray(error)) return error.length ? String(error[0]) : "";
  if (error && typeof error === "object" && "message" in error) {
    return String((error as { message?: unknown }).message ?? "");
  }
  return "";
};

const onLocationSelected = (result: AddressAutocompleteResult): void => {
  const addressParts = [result.street, result.housenumber].filter(Boolean);
  emit("update:modelValue", {
    ...props.modelValue,
    address: addressParts.join(", ") || result.addressLine1,
    city: result.city,
    region: result.state,
    postalCode: result.postcode,
    latitude: result.lat,
    longitude: result.lon,
  });
};

const onLocationCleared = (): void => {
  emit("update:modelValue", {
    ...props.modelValue,
    address: "",
    city: "",
    region: "",
    postalCode: "",
    latitude: 0,
    longitude: 0,
  });
};

const openMap = (): void => {
  window.open(mapUrl.value, "_blank", "noopener,noreferrer");
};
</script>

<template>
  <section class="three-columns mb-2">
    <div>
      <DropdownCountry
        :model-value="modelValue.country"
        :label="t('location.country')"
        :disabled="disabled"
        :class="{ 'p-invalid': validationErrors?.country }"
        @update:model-value="updateField('country', $event)"
      />
      <small
        v-if="showValidationMessages && errorMessage('country')"
        class="p-error"
        role="alert"
      >
        {{ errorMessage("country") }}
      </small>
    </div>
    <div class="col-span-2">
      <label class="block text-900 mb-2">{{ t("location.searchLabel") }}</label>
      <AutocompleteLocation
        v-model="locationSelection"
        label=""
        :placeholder="t('location.placeholder')"
        :country-code="autocompleteCountryCode"
        :disabled="disabled || !modelValue.country"
        @select="onLocationSelected"
        @clear="onLocationCleared"
      />
    </div>
  </section>

  <section class="four-columns mb-2">
    <div>
      <BaseInput
        :label="t('location.address')"
        id="location-address"
        :model-value="modelValue.address"
        :disabled="disabled"
        :class="{ 'p-invalid': validationErrors?.address }"
        @update:model-value="updateField('address', $event)"
      />
      <small
        v-if="showValidationMessages && errorMessage('address')"
        class="p-error"
        role="alert"
      >
        {{ errorMessage("address") }}
      </small>
    </div>
    <div>
      <BaseInput
        :label="t('location.city')"
        id="location-city"
        :model-value="modelValue.city"
        :disabled="disabled"
        :class="{ 'p-invalid': validationErrors?.city }"
        @update:model-value="updateField('city', $event)"
      />
      <small
        v-if="showValidationMessages && errorMessage('city')"
        class="p-error"
        role="alert"
      >
        {{ errorMessage("city") }}
      </small>
    </div>
    <div>
      <BaseInput
        :label="t('location.region')"
        id="location-region"
        :model-value="modelValue.region"
        :disabled="disabled"
        :class="{ 'p-invalid': validationErrors?.region }"
        @update:model-value="updateField('region', $event)"
      />
      <small
        v-if="showValidationMessages && errorMessage('region')"
        class="p-error"
        role="alert"
      >
        {{ errorMessage("region") }}
      </small>
    </div>
    <div>
      <BaseInput
        :label="t('location.postalCode')"
        id="location-postalCode"
        :model-value="modelValue.postalCode"
        :disabled="disabled"
        :class="{ 'p-invalid': validationErrors?.postalCode }"
        @update:model-value="updateField('postalCode', $event)"
      />
      <small
        v-if="showValidationMessages && errorMessage('postalCode')"
        class="p-error"
        role="alert"
      >
        {{ errorMessage("postalCode") }}
      </small>
    </div>
  </section>

  <Panel
    :header="
      showDistance
        ? t('location.coordinatesSection')
        : t('location.coordinatesSectionNoDistance')
    "
    toggleable
    collapsed
    class="mt-2 mb-2"
  >
    <section class="location-coordinates-grid">
      <div>
        <label class="block text-900 mb-2">{{ t("location.latitude") }}</label>
        <InputNumber
          :model-value="modelValue.latitude"
          :minFractionDigits="2"
          :maxFractionDigits="8"
          :disabled="disabled"
          class="w-full"
          mode="decimal"
          @update:model-value="updateField('latitude', $event ?? 0)"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("location.longitude") }}</label>
        <InputNumber
          :model-value="modelValue.longitude"
          :minFractionDigits="2"
          :maxFractionDigits="8"
          :disabled="disabled"
          class="w-full"
          mode="decimal"
          @update:model-value="updateField('longitude', $event ?? 0)"
        />
      </div>
      <BaseInput
        v-if="showDistance"
        disabled
        :label="t('location.distanceFromSite')"
        id="location-distanceFromSite"
        :model-value="modelValue.distanceFromSite ?? null"
      />
      <div class="map-link-cell mb-2">
        <Button
          v-if="hasCoordinates"
          :label="t('location.viewOnMap')"
          icon="pi pi-map-marker"
          severity="secondary"
          text
          size="small"
          :disabled="disabled"
          @click="openMap"
        />
      </div>
    </section>
  </Panel>
</template>

<style scoped>
.col-span-2 {
  grid-column: span 2;
}

.location-coordinates-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
}

.map-link-cell {
  display: flex;
  align-self: end;
}

@media (max-width: 960px) {
  .location-coordinates-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 576px) {
  .location-coordinates-grid {
    grid-template-columns: 1fr;
  }
}
</style>

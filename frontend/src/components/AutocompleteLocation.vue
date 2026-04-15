<template>
  <div class="autocomplete-location">
    <FloatLabel v-if="resolvedLabel">
      <AutoComplete
        :inputId="inputId"
        v-model="selectedSuggestion"
        :suggestions="results"
        optionLabel="formatted"
        :placeholder="resolvedPlaceholder"
        :disabled="disabled"
        :minLength="minChars"
        :delay="delay"
        showClear
        dropdown
        dropdownMode="current"
        fluid
        @complete="handleComplete"
        @item-select="onItemSelect"
        @clear="clearSelection"
      >
        <template #option="slotProps">
          <div class="autocomplete-location__option">
            <div class="autocomplete-location__title">
              {{ slotProps.option.formatted }}
            </div>
            <div class="autocomplete-location__meta">
              {{ formatMeta(slotProps.option) }}
            </div>
          </div>
        </template>
      </AutoComplete>
      <label :for="inputId">{{ resolvedLabel }}</label>
    </FloatLabel>

    <AutoComplete
      v-else
      :inputId="inputId"
      v-model="selectedSuggestion"
      :suggestions="results"
      optionLabel="formatted"
      :placeholder="resolvedPlaceholder"
      :disabled="disabled"
      :minLength="minChars"
      :delay="delay"
      showClear
      dropdown
      dropdownMode="current"
      fluid
      @complete="handleComplete"
      @item-select="onItemSelect"
      @clear="clearSelection"
    >
      <template #option="slotProps">
        <div class="autocomplete-location__option">
          <div class="autocomplete-location__title">
            {{ slotProps.option.formatted }}
          </div>
          <div class="autocomplete-location__meta">
            {{ formatMeta(slotProps.option) }}
          </div>
        </div>
      </template>
    </AutoComplete>

    <small v-if="helperText" class="autocomplete-location__helper">
      {{ helperText }}
    </small>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from "vue";
import { useI18n } from "vue-i18n";
import AutoComplete from "primevue/autocomplete";
import FloatLabel from "primevue/floatlabel";
import geoapifyService from "@/api/geoapify.service";
import type { AddressAutocompleteResult } from "@/types";

interface Props {
  modelValue?: AddressAutocompleteResult | null;
  label?: string;
  placeholder?: string;
  helperText?: string;
  disabled?: boolean;
  /** Minimum characters before searching. Maps to PrimeVue minLength. */
  minChars?: number;
  /** Debounce delay in ms. Maps to PrimeVue delay. */
  delay?: number;
  limit?: number;
  countryCode?: string;
  type?: string;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: null,
  label: undefined,
  placeholder: undefined,
  helperText: "",
  disabled: false,
  minChars: 3,
  delay: 350,
  limit: undefined,
  countryCode: "es",
  type: "",
});

const emit = defineEmits<{
  (e: "update:modelValue", value: AddressAutocompleteResult | null): void;
  (e: "select", value: AddressAutocompleteResult): void;
  (e: "clear"): void;
}>();

const { t } = useI18n();

const resolvedLabel = computed(() => props.label ?? t("location.label"));
const resolvedPlaceholder = computed(
  () => props.placeholder ?? t("location.placeholder"),
);

const inputId = `location-input-${Math.random().toString(36).slice(2, 9)}`;
const selectedSuggestion = ref<AddressAutocompleteResult | string | null>(
  props.modelValue ?? null,
);
const results = ref<AddressAutocompleteResult[]>([]);

watch(
  () => props.modelValue,
  (value) => {
    selectedSuggestion.value = value ?? null;
  },
);

async function handleComplete(event: { query: string }) {
  const query =
    typeof event.query === "string" ? event.query.trim() : "";

  // PrimeVue's minLength already gates this, but guard for dropdown button clicks
  if (query.length < props.minChars) {
    results.value = [];
    return;
  }

  const response = await geoapifyService.autocomplete(
    query,
    props.countryCode,
    props.limit,
    props.type || undefined,
  );
  results.value = response;
}

function onItemSelect(event: { value: AddressAutocompleteResult }) {
  const item = event.value;
  selectedSuggestion.value = item;
  emit("update:modelValue", item);
  emit("select", item);
}

function clearSelection() {
  selectedSuggestion.value = null;
  results.value = [];
  emit("update:modelValue", null);
  emit("clear");
}

function formatMeta(item: AddressAutocompleteResult): string {
  const typeKey = `location.resultTypes.${item.resultType || "unknown"}`;
  const translatedType = t(typeKey, t("location.resultTypes.unknown"));
  return [translatedType, item.city, item.state, item.country]
    .filter(Boolean)
    .join(" · ");
}
</script>

<style scoped>
.autocomplete-location {
  width: 100%;
}

.autocomplete-location__option {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.autocomplete-location__title {
  font-size: 0.95rem;
  font-weight: 600;
}

.autocomplete-location__meta {
  font-size: 0.825rem;
  color: var(--p-text-muted-color);
}

.autocomplete-location__helper {
  display: block;
  margin-top: 0.5rem;
  color: var(--p-text-muted-color);
}
</style>

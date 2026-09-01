<template>
  <div class="mb-2">
    <label v-if="label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      showClear
      filter
      :options="countries"
      :placeholder="placeholder || 'Selecciona un país'"
      optionValue="code"
      optionLabel="name"
      :filterFields="['name', 'code']"
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
      <template #value="slotProps">
        <div v-if="slotProps.value" class="flex align-items-center gap-2">
          <img
            :src="countryStore.getFlagUrl(slotProps.value)"
            :alt="slotProps.value"
            style="width: 20px; height: 15px"
            loading="lazy"
          />
          <span>{{ countryStore.getCountryByCode(slotProps.value)?.name }}</span>
        </div>
        <span v-else>
          {{ slotProps.placeholder }}
        </span>
      </template>
      <template #option="slotProps">
        <div class="flex align-items-center gap-2">
          <img
            :src="countryStore.getFlagUrl(slotProps.option.code)"
            :alt="slotProps.option.name"
            style="width: 20px; height: 15px"
            loading="lazy"
          />
          <span>{{ slotProps.option.name }}</span>
        </div>
      </template>
    </Select>
  </div>
</template>
<script setup lang="ts">
import { storeToRefs } from "pinia";
import { useCountryStore } from "../store/country";

const countryStore = useCountryStore();
const { countries } = storeToRefs(countryStore);

defineProps<{
  label: string;
  modelValue: string | null | undefined;
  placeholder?: string;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();
</script>

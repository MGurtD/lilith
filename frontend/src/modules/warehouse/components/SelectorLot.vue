<template>
  <div>
    <label v-if="label.length > 0" class="block text-900 mb-2">{{ label }}</label>
    <AutoComplete
      v-model="selected"
      :suggestions="suggestions"
      optionLabel="code"
      :placeholder="referenceId ? 'Sense lot' : 'Selecciona primer una referència'"
      :disabled="!referenceId"
      showClear
      dropdown
      dropdownMode="current"
      fluid
      @complete="handleComplete"
      @dropdown-click="handleComplete"
      @item-select="onItemSelect"
      @clear="onClear"
    >
      <template #option="slotProps">
        <span v-if="slotProps.option.isCreateNew">
          Crear lot "{{ slotProps.option.code }}"
        </span>
        <span v-else>
          {{ slotProps.option.code || "(lot buit)" }}
        </span>
      </template>
    </AutoComplete>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import AutoComplete from "primevue/autocomplete";
import { useToast } from "primevue/usetoast";
import Services from "../services";
import { Lot } from "../types";
import { getNewUuid } from "../../../utils/functions";

// Selector de lot reutilitzable: llista els lots oberts d'una referència i permet
// crear-ne un de nou al vol si el codi introduït no existeix (alta ràpida a la recepció de compra).
type LotOption = Lot & { isCreateNew?: boolean };

const props = withDefaults(
  defineProps<{
    referenceId: string | null | undefined;
    modelValue: string | null | undefined;
    label?: string;
  }>(),
  {
    label: "Lot",
  },
);

const emit = defineEmits<{
  (e: "update:modelValue", lotId: string | null): void;
  (e: "update:lotCode", lotCode: string): void;
}>();

const toast = useToast();
const lotOptions = ref<LotOption[]>([]);
const suggestions = ref<LotOption[]>([]);
const selected = ref<LotOption | string | null>(null);

const loadOptions = async () => {
  lotOptions.value = props.referenceId
    ? await Services.Lot.getOpenByReference(props.referenceId)
    : [];
};

// Sincronitza el valor seleccionat quan canvia el lotId extern (p.ex. en carregar una línia existent)
const syncSelectedFromModelValue = () => {
  selected.value =
    lotOptions.value.find((lot) => lot.id === props.modelValue) ?? null;
};

watch(
  () => props.referenceId,
  async () => {
    await loadOptions();
    syncSelectedFromModelValue();
  },
  { immediate: true },
);

watch(
  () => props.modelValue,
  () => syncSelectedFromModelValue(),
);

const handleComplete = (event: { query: string }) => {
  const query = event.query.trim().toLowerCase();
  const filtered = query
    ? lotOptions.value.filter((lot) => lot.code.toLowerCase().includes(query))
    : lotOptions.value;

  const hasExactMatch = lotOptions.value.some(
    (lot) => lot.code.toLowerCase() === query,
  );

  suggestions.value =
    query && !hasExactMatch
      ? [...filtered, { id: "__create__", code: event.query.trim(), isCreateNew: true } as LotOption]
      : filtered;
};

const createLot = async (code: string) => {
  if (!props.referenceId) return;

  const newLot: Lot = {
    id: getNewUuid(),
    referenceId: props.referenceId,
    code,
    remainingQuantity: 0,
  };

  const created = await Services.Lot.create(newLot);
  if (!created) {
    toast.add({
      severity: "error",
      summary: "No s'ha pogut crear el lot",
      detail: `No s'ha pogut crear el lot "${code}"`,
      life: 5000,
    });
    return;
  }

  lotOptions.value.push(newLot);
  selected.value = newLot;
  emit("update:modelValue", newLot.id);
  emit("update:lotCode", newLot.code);
};

const onItemSelect = async (event: { value: LotOption }) => {
  if (event.value.isCreateNew) {
    await createLot(event.value.code);
    return;
  }

  emit("update:modelValue", event.value.id);
  emit("update:lotCode", event.value.code);
};

const onClear = () => {
  emit("update:modelValue", null);
  emit("update:lotCode", "");
};
</script>

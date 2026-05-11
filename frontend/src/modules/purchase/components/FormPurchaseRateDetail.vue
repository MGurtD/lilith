<template>
  <form v-if="model">
    <section class="mb-2">
      <label class="block text-900 mb-2">Referència</label>
      <Select
        v-model="model.referenceId"
        :options="referenceStore.references"
        optionLabel="code"
        optionValue="id"
        placeholder="Selecciona una referència"
        filter
        class="w-full"
        :virtualScrollerOptions="{ itemSize: 38 }"
      >
        <template #option="slotProps">
          <div class="flex flex-column">
            <span class="font-bold">{{ slotProps.option.code }}</span>
            <small class="text-600">{{ slotProps.option.description }}</small>
          </div>
        </template>
      </Select>
    </section>

    <section class="mb-2">
      <label class="block text-900 mb-2">Tipus de càlcul</label>
      <Select
        v-model="model.calculationType"
        :options="calculationTypes"
        optionLabel="label"
        optionValue="value"
        class="w-full"
      />
    </section>

    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">Des de</label>
        <InputNumber
          v-model="model.from"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Fins a</label>
        <InputNumber
          v-model="model.to"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
    </section>

    <section class="mb-2">
      <label class="block text-900 mb-2">Preu (€)</label>
      <InputNumber
        v-model="model.price"
        class="w-full"
        :minFractionDigits="2"
        :maxFractionDigits="4"
      />
    </section>

    <div class="mt-2 text-right">
      <Button label="Guardar" icon="pi pi-save" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { PurchaseRateDetail, CalculationType } from "../types";
import { useReferenceStore } from "../../shared/store/reference";

const props = defineProps<{
  detail: PurchaseRateDetail;
}>();

const emit = defineEmits<{
  (e: "submit", detail: PurchaseRateDetail): void;
}>();

const referenceStore = useReferenceStore();
const model = ref<PurchaseRateDetail>({ ...props.detail });

const calculationTypes = [
  { label: "Unitats", value: CalculationType.Units },
  { label: "Volum", value: CalculationType.Volume },
  { label: "Pes", value: CalculationType.Weight },
];

onMounted(async () => {
  if (!referenceStore.references) {
    await referenceStore.fetchReferencesByModule("purchase");
  }
});

const submitForm = () => {
  emit("submit", model.value);
};
</script>

<template>
  <form v-if="model">
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">Pes mínim (kg)</label>
        <InputNumber
          v-model="model.minWeight"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Pes màxim (kg)</label>
        <InputNumber
          v-model="model.maxWeight"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
    </section>
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">Volum mínim (m³)</label>
        <InputNumber
          v-model="model.minVolume"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Volum màxim (m³)</label>
        <InputNumber
          v-model="model.maxVolume"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
    </section>
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">Distància mínima (km)</label>
        <InputNumber
          v-model="model.minDistance"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Distància màxima (km)</label>
        <InputNumber
          v-model="model.maxDistance"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
    </section>
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">Preu (€)</label>
        <InputNumber
          v-model="model.price"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
    </section>
    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { TransportRateDetail } from "../types";

const props = defineProps<{
  detail: TransportRateDetail;
}>();

const emit = defineEmits<{
  (e: "submit", detail: TransportRateDetail): void;
}>();

const model = ref<TransportRateDetail>({ ...props.detail });

const submitForm = () => {
  emit("submit", model.value);
};
</script>

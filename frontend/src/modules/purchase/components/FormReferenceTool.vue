<template>
  <section class="three-columns">
    <div class="mt-1">
      <label class="block text-900 mb-2">Impost</label>
      <Select
        v-model="reference.taxId"
        :options="taxesStore.taxes"
        optionValue="id"
        optionLabel="name"
        class="w-full"
      />
    </div>
    <div class="mt-1">
      <label class="block text-900 mb-2">Àrea de producció</label>
      <Select
        v-model="reference.areaId"
        :options="plantModelStore.areas"
        optionValue="id"
        optionLabel="name"
        class="w-full"
      />
    </div>
  </section>
</template>
<script setup lang="ts">
import { Reference } from "../../shared/types";
import { useTaxesStore } from "../../shared/store/tax";
import { onMounted } from "vue";
import { usePlantModelStore } from "../../production/store/plantmodel";

const props = defineProps<{
  reference: Reference;
}>();

const taxesStore = useTaxesStore();
const plantModelStore = usePlantModelStore();

onMounted(async () => {
  if (!taxesStore.taxes) await taxesStore.fetchAll();
  await plantModelStore.fetchAreas();
});
</script>

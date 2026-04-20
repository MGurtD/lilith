<template>
  <form v-if="model">
    <section class="mb-2">
      <BaseInput
        label="Nom"
        id="prName"
        v-model="model.name"
        class="w-full"
      />
    </section>
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">Data inici</label>
        <DatePicker
          v-model="model.validFrom"
          class="w-full"
          dateFormat="dd/mm/yy"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Data fi</label>
        <DatePicker
          v-model="model.validTo"
          class="w-full"
          dateFormat="dd/mm/yy"
        />
      </div>
    </section>
    <div class="mt-2 text-right">
      <Button label="Guardar" icon="pi pi-save" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { PurchaseRate } from "../types";

const props = defineProps<{
  purchaseRate: PurchaseRate;
}>();

const emit = defineEmits<{
  (e: "submit", rate: PurchaseRate): void;
}>();

const model = ref<PurchaseRate>({ ...props.purchaseRate });

const submitForm = () => {
  emit("submit", model.value);
};
</script>

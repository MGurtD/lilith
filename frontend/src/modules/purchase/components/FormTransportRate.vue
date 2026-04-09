<template>
  <form v-if="model">
    <section class="two-columns mb-2">
      <BaseInput
        label="Nom"
        id="trName"
        v-model="model.name"
      />
      <BaseInput
        label="Descripció"
        id="trDescription"
        v-model="model.description"
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
    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { TransportRate } from "../types";

const props = defineProps<{
  transportRate: TransportRate;
}>();

const emit = defineEmits<{
  (e: "submit", rate: TransportRate): void;
}>();

const model = ref<TransportRate>({ ...props.transportRate });


const submitForm = () => {
  emit("submit", model.value);
};
</script>

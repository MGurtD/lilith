<template>
  <div class="mb-2">
    <label v-if="label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      showClear
      :options="lifecycle?.statuses"
      placeholder="Selecciona..."
      optionValue="id"
      optionLabel="name"
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
      <template #option="slotProps">
        <div v-if="slotProps.option" class="flex align-items-center">
          {{ slotProps.option.name }}
        </div>
      </template>
    </Select>
  </div>
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import SharedServices from "../services";
import { Lifecycle } from "../types";

const props = defineProps<{
  label: string;
  modelValue: string | undefined;
  name: string;
}>();

const lifecycle = ref(undefined as Lifecycle | undefined);
onMounted(async () => {
  lifecycle.value = await SharedServices.Lifecycle.getByName(props.name);

  if (props.modelValue === "") {
    emit("update:modelValue", lifecycle.value?.initialStatusId!);
  }
});

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();
</script>

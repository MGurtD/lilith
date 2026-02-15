<template>
  <div
    :style="{
      backgroundColor: normalizedColor,
      width: '30px',
      height: '20px',
      borderRadius: '4px',
    }"
  ></div>
</template>

<script setup lang="ts">
import { computed } from "vue";

const props = defineProps<{
  value?: string;
}>();

const normalizedColor = computed(() => {
  if (!props.value) return "#000000";
  return props.value.startsWith("#") ? props.value : `#${props.value}`;
});
</script>

<template>
  <i
    class="pi"
    :class="{
      'true-icon': showColor && value,
      'false-icon': showColor && !value,
      'pi-circle-fill': value,
      'pi-circle': !value,
    }"
  ></i>
</template>

<script setup lang="ts">
defineProps<{
  value: boolean;
  showColor?: boolean;
}>();
</script>

<style scoped>
.true-icon {
  color: var(--p-green-700);
}
.false-icon {
  color: var(--p-red-700);
}
</style>

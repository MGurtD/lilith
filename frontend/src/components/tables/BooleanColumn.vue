<template>
  <i
    v-if="value === true"
    class="pi"
    :class="[showColor ? 'pi-circle-fill true-icon' : 'pi-circle-fill']"
  />
  <i
    v-else-if="value === false"
    class="pi"
    :class="[showColor ? 'pi-circle false-icon' : 'pi-circle']"
  />
  <span v-else class="text-color-secondary">—</span>
</template>

<script setup lang="ts">
defineProps<{
  value: boolean | null | undefined;
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

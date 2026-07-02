<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from "vue";

const props = withDefaults(
  defineProps<{ value: string; truncate?: boolean }>(),
  { truncate: true },
);

const wrapperRef = ref<HTMLElement | null>(null);
const isOverflowing = ref(false);
let observer: ResizeObserver | null = null;

// Empty string hides the tooltip; PrimeVue's v-tooltip skips falsy values.
const tooltipValue = computed(() => (isOverflowing.value ? props.value : ""));

function checkOverflow() {
  const el = wrapperRef.value;
  if (!el) return;
  isOverflowing.value = el.scrollWidth > el.clientWidth;
}

onMounted(() => {
  if (!props.truncate) return;
  // Wait one frame so the parent cell has its final width before measuring.
  requestAnimationFrame(() => {
    checkOverflow();
    if (wrapperRef.value && typeof ResizeObserver !== "undefined") {
      observer = new ResizeObserver(checkOverflow);
      observer.observe(wrapperRef.value);
    }
  });
});

onUnmounted(() => {
  observer?.disconnect();
  observer = null;
});

// Re-measure when the text content changes (sorting, filtering, etc.)
watch(
  () => props.value,
  () => {
    if (!props.truncate) return;
    requestAnimationFrame(checkOverflow);
  },
);
</script>

<template>
  <span v-if="truncate" ref="wrapperRef" class="truncated-cell">
    <span v-tooltip.top="tooltipValue">{{ value }}</span>
  </span>
  <span v-else>{{ value }}</span>
</template>

<style scoped>
.truncated-cell {
  display: block;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
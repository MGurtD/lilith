<template>
  <div :class="['info-panel', `info-panel--${severity}`]">
    <i :class="iconClass"></i>
    <p>{{ text }}</p>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { PrimeIcons } from "@primevue/core/api";

type Severity = "info" | "success" | "warn" | "error";

interface Props {
  severity?: Severity;
  text: string;
}

const props = withDefaults(defineProps<Props>(), {
  severity: "info",
});

const iconClass = computed(() => {
  switch (props.severity) {
    case "success":
      return PrimeIcons.CHECK_CIRCLE;
    case "warn":
      return PrimeIcons.EXCLAMATION_TRIANGLE;
    case "error":
      return PrimeIcons.TIMES_CIRCLE;
    case "info":
    default:
      return PrimeIcons.INFO_CIRCLE;
  }
});
</script>

<style scoped>
.info-panel {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.25rem;
  border: 2px solid;
  border-radius: var(--border-radius);
  margin-bottom: 1rem;
}

.info-panel i {
  font-size: 1.5rem;
  flex-shrink: 0;
}

.info-panel p {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 500;
  line-height: 1.5;
}

/* Info severity (default) */
.info-panel--info {
  background: var(--p-blue-50);
  border-color: var(--p-blue-300);
  color: var(--p-blue-900);
}

.info-panel--info i {
  color: var(--p-blue-600);
}

/* Success severity */
.info-panel--success {
  background: var(--p-green-50);
  border-color: var(--p-green-300);
  color: var(--p-green-900);
}

.info-panel--success i {
  color: var(--p-green-600);
}

/* Warn severity */
.info-panel--warn {
  background: var(--p-orange-50);
  border-color: var(--p-orange-300);
  color: var(--p-orange-900);
}

.info-panel--warn i {
  color: var(--p-orange-600);
}

/* Error severity */
.info-panel--error {
  background: var(--p-red-50);
  border-color: var(--p-red-300);
  color: var(--p-red-900);
}

.info-panel--error i {
  color: var(--p-red-600);
}
</style>

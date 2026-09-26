<template>
  <!-- A check for yes and a quiet dash for no: circles read as radio buttons. -->
  <i
    v-if="value === true"
    class="pi pi-check boolean-icon"
    :class="{ 'true-icon': showColor }"
    role="img"
    :aria-label="t('common.yes')"
  />
  <span
    v-else-if="value === false"
    class="boolean-no"
    :class="{ 'false-icon': showColor }"
    role="img"
    :aria-label="t('common.no')"
    >–</span
  >
  <span v-else class="text-color-secondary">—</span>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

defineProps<{
  value: boolean | null | undefined;
  showColor?: boolean;
}>();

const { t } = useI18n();
</script>

<style scoped>
.boolean-icon {
  font-size: 0.9286rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.boolean-no {
  color: var(--p-surface-400);
}

.true-icon {
  color: var(--p-green-700);
}

.false-icon {
  color: var(--p-red-700);
}
</style>

<template>
  <div class="dimension-chips">
    <span
      v-for="dimension in formattedDimensions"
      :key="dimension"
      class="dimension-chip"
    >
      {{ dimension }}
    </span>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";

interface Props {
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
}

const props = defineProps<Props>();
const { t } = useI18n();

const formattedDimensions = computed(() => {
  const dimensions = [
    { label: t("measurements.width"), value: props.width },
    { label: t("measurements.length"), value: props.length },
    { label: t("measurements.height"), value: props.height },
    { label: t("measurements.diameter"), value: props.diameter },
    { label: t("measurements.thickness"), value: props.thickness },
  ]
    .filter((dimension) => dimension.value > 0)
    .map((dimension) => `${dimension.label} ${dimension.value}`);

  return dimensions.length > 0 ? dimensions : [t("measurements.none")];
});
</script>

<style scoped>
.dimension-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}

.dimension-chip {
  border: 1px solid var(--surface-border);
  border-radius: 999px;
  padding: 0.2rem 0.55rem;
  font-size: 0.78rem;
  color: var(--text-color-secondary);
  background: var(--surface-50);
}
</style>
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
import { formatDimensions } from "@/utils/functions";

interface Props {
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
}

const props = defineProps<Props>();
const { t } = useI18n();

const formattedDimensions = computed(() =>
  formatDimensions(t, {
    width: props.width,
    length: props.length,
    height: props.height,
    diameter: props.diameter,
    thickness: props.thickness,
  }),
);
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
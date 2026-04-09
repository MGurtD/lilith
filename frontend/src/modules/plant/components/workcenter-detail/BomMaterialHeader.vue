<template>
  <div class="bom-header">
    <div class="bom-header-info">
      <i class="pi pi-box"></i>
      <span class="font-semibold">{{ referenceCode }}</span>
      <span class="text-500">{{ referenceDescription }}</span>
      <span class="bom-header-qty">Quantitat: {{ quantity }}</span>
      <div v-if="formattedMeasures.length > 0" class="bom-header-measures">
        <span
          v-for="measure in formattedMeasures"
          :key="measure"
          class="bom-header-measure-chip"
        >
          {{ measure }}
        </span>
      </div>
    </div>
    <div v-if="formatDescription" class="bom-header-format">
      {{ formatDescription }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { formatDimensions } from "@/utils/functions";

interface Props {
  referenceCode: string;
  referenceDescription: string;
  quantity: number;
  width?: number;
  length?: number;
  height?: number;
  diameter?: number;
  thickness?: number;
  formatDescription?: string;
}

const props = withDefaults(defineProps<Props>(), {
  width: 0,
  length: 0,
  height: 0,
  diameter: 0,
  thickness: 0,
  formatDescription: "",
});

const { t } = useI18n();

const formattedMeasures = computed(() => {
  const dims = formatDimensions(t, {
    width: props.width,
    length: props.length,
    height: props.height,
    diameter: props.diameter,
    thickness: props.thickness,
  });
  // Don't show the "no dimensions" fallback in the header — just hide the section
  return dims[0] === t("measurements.none") ? [] : dims;
});
</script>

<style scoped>
.bom-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  padding: 0.6rem 0.75rem;
  border-radius: 6px;
  background: var(--p-green-50);
  color: var(--p-green-700);
  border: 1px solid var(--p-green-200);
}

.bom-header-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.bom-header-qty {
  font-size: 0.85rem;
  background: var(--p-green-100);
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.bom-header-measures {
  display: flex;
  flex-wrap: wrap;
  gap: 0.3rem;
}

.bom-header-measure-chip {
  font-size: 0.8rem;
  background: var(--p-green-100);
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.bom-header-format {
  font-size: 0.82rem;
  font-weight: 600;
  flex-shrink: 0;
}
</style>

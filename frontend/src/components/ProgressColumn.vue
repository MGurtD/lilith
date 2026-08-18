<template>
  <Column
    :field="field"
    :header="header"
    :sortable="sortable"
    :style="style"
  >
    <template #body="{ data }">
      <div class="flex align-items-center gap-2">
        <ProgressBar
          :value="barValue(data)"
          :showValue="false"
          :class="{ overrun: isOverrun(data) && appliesToBar }"
          style="height: 0.75rem; flex: 1"
        />
        <span
          v-if="showValue"
          v-tooltip.top="tooltipValue(data)"
          class="text-sm white-space-nowrap"
          :class="{ 'text-red-500 font-medium': isOverrun(data) && appliesToLabel }"
        >
          {{ rawValue(data) }}%
        </span>
      </div>
    </template>
  </Column>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { resolveFieldValue } from "@/components/tables/field-value";

type RowData = Record<string, unknown>;
type ProgressValue = number | ((data: never) => number);
type TooltipValue = string | ((data: never) => string);
type OverrunSeverity = "text" | "bar" | "both";

interface Props {
  field: string;
  header: string;
  sortable?: boolean;
  style?: string | Record<string, string>;
  value?: ProgressValue;
  showValue?: boolean;
  cap?: boolean;
  overrunSeverity?: OverrunSeverity;
  tooltip?: TooltipValue;
}

const props = withDefaults(defineProps<Props>(), {
  sortable: true,
  showValue: true,
  cap: true,
  overrunSeverity: "bar",
  style: undefined,
  value: undefined,
  tooltip: undefined,
});

const appliesToBar = computed(
  () => props.overrunSeverity === "bar" || props.overrunSeverity === "both",
);
const appliesToLabel = computed(
  () => props.overrunSeverity === "text" || props.overrunSeverity === "both",
);

const rawValue = (data: RowData): number => {
  const value =
    typeof props.value === "function"
      ? props.value(data as never)
      : (props.value ?? resolveFieldValue(data, props.field));
  return typeof value === "number" && Number.isFinite(value) ? value : 0;
};

const barValue = (data: RowData): number =>
  props.cap ? Math.min(rawValue(data), 100) : rawValue(data);

const isOverrun = (data: RowData): boolean => rawValue(data) > 100;

const tooltipValue = (data: RowData): string | undefined => {
  if (typeof props.tooltip === "function") return props.tooltip(data as never);
  return props.tooltip;
};
</script>

<style scoped>
:deep(.overrun .p-progressbar-value) {
  background: var(--red-500);
}
</style>

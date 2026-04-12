<template>
  <Tag :value="label" :severity="severity" />
</template>
<script setup lang="ts">
import { computed } from "vue";

const props = defineProps<{
  movementType: string;
}>();

const MOVEMENT_TYPE_MAP: Record<
  string,
  { label: string; severity: string }
> = {
  INPUT: { label: "Entrada", severity: "success" },
  OUTPUT: { label: "Sortida", severity: "danger" },
  SUPPLY: { label: "Subministrament", severity: "info" },
  CONSUMPTION: { label: "Consum", severity: "warn" },
  PRODUCTION: { label: "Producció", severity: "contrast" },
};

const label = computed(
  () => MOVEMENT_TYPE_MAP[props.movementType]?.label ?? props.movementType,
);

const severity = computed(
  () =>
    (MOVEMENT_TYPE_MAP[props.movementType]?.severity ?? "secondary") as
      | "success"
      | "danger"
      | "info"
      | "warn"
      | "contrast"
      | "secondary",
);
</script>

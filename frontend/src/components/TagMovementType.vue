<template>
  <Tag :value="label" :severity="severity" />
</template>
<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  movementType: string;
}>();
const { t } = useI18n();

const MOVEMENT_TYPE_MAP: Record<
  string,
  { label: string; severity: string }
> = {
  INPUT: { label: "movementTypes.input", severity: "success" },
  OUTPUT: { label: "movementTypes.output", severity: "danger" },
  SUPPLY: { label: "movementTypes.supply", severity: "info" },
  CONSUMPTION: { label: "movementTypes.consumption", severity: "warn" },
  PRODUCTION: { label: "movementTypes.production", severity: "contrast" },
};

const label = computed(() => {
  const movementType = MOVEMENT_TYPE_MAP[props.movementType];
  return movementType ? t(movementType.label) : props.movementType;
});

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

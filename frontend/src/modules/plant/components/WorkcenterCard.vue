<template>
  <div class="workcenter-card" @click="handleClick">
    <div class="workcenter-card__status" :style="borderTopStyle"></div>

    <div class="workcenter-card__header">
      <div class="workcenter-card__title">
        <i :class="statusIcon" class="workcenter-card__icon"></i>
        <h3>{{ workcenter.config.description }}</h3>
      </div>
      <span class="workcenter-card__badge" :style="badgeStyle">
        {{ currentMachineStatus?.name || "Sense dades" }}
      </span>
    </div>

    <div class="workcenter-card__content">
      <div class="workcenter-card__stat">
        <div class="stat__label">
          <i class="pi pi-hashtag"></i>
          <span>Ordre</span>
        </div>
        <div class="stat__value">
          {{ currentWorkOrder?.workOrderCode || "-" }}
        </div>
      </div>

      <div class="workcenter-card__stat">
        <div class="stat__label">
          <i class="pi pi-cog"></i>
          <span>Fase</span>
        </div>
        <div class="stat__value stat__value--phase">
          {{ formattedPhase }}
        </div>
      </div>

      <div class="workcenter-card__stat">
        <div class="stat__label">
          <i class="pi pi-tag"></i>
          <span>Referència</span>
        </div>
        <div class="stat__value stat__value--reference">
          {{ formattedReference }}
        </div>
      </div>

      <div class="workcenter-card__stat">
        <div class="stat__label">
          <i :class="PrimeIcons.CLOCK"></i>
          <span>Temps</span>
        </div>
        <div class="stat__value stat__value--time">
          {{ formattedTime }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";
import { computed } from "vue";
import { WorkcenterViewState, WorkcenterRealtime } from "../types";
import {
  getBorderTopStyle,
  normalizeColor,
  isColorLight,
} from "../../../utils/functions";
import { usePlantDataStore } from "../store";

interface Props {
  workcenter: WorkcenterViewState;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  (e: "click", workcenterId: string): void;
}>();

const dataStore = usePlantDataStore();

// Obtener el machine status desde el store
const currentMachineStatus = computed(() => {
  const statusId = props.workcenter.realtime?.statusId;
  if (!statusId) return undefined;
  return dataStore.getMachineStatusById(statusId);
});

// Estilo del borde superior basado en el color del status desde el store
const borderTopStyle = computed(() => {
  const statusColor = currentMachineStatus.value?.color;
  return getBorderTopStyle(statusColor, "f59e0b"); // Yellow-500 as default
});

// Icono del header basado en el status desde el store
const statusIcon = computed((): string => {
  const machineStatus = currentMachineStatus.value;
  if (!machineStatus || !machineStatus.icon) {
    return PrimeIcons.CIRCLE;
  }
  return machineStatus.icon;
});

// Obtener el trabajo activo actual (primer elemento del array workorders)
const currentWorkOrder = computed(() => {
  const workorders = props.workcenter.realtime?.workorders;
  if (!workorders || workorders.length === 0) return undefined;
  return workorders[0];
});

// Formatear fase con código y descripción
const formattedPhase = computed((): string => {
  const wo = currentWorkOrder.value;
  if (!wo) return "-";
  if (wo.workOrderPhaseDescription) {
    return `${wo.workOrderPhaseCode} - ${wo.workOrderPhaseDescription}`;
  }
  return wo.workOrderPhaseCode || "-";
});

// Formatear referencia con código y descripción
const formattedReference = computed((): string => {
  const wo = currentWorkOrder.value;
  if (!wo) return "-";
  if (wo.referenceDescription && wo.referenceDescription !== wo.referenceCode) {
    return `${wo.referenceCode} - ${wo.referenceDescription}`;
  }
  return wo.referenceCode || "-";
});

// Formatear tiempo desde statusStartTime
const formattedTime = computed((): string => {
  const startTime = props.workcenter.realtime?.statusStartTime;
  if (!startTime) return "--:--:--";

  const start = new Date(startTime);
  const now = new Date();
  const diff = Math.floor((now.getTime() - start.getTime()) / 1000); // segundos

  if (diff < 0) return "--:--:--";

  const hours = Math.floor(diff / 3600);
  const minutes = Math.floor((diff % 3600) / 60);
  const seconds = diff % 60;

  return `${hours.toString().padStart(2, "0")}:${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
});

// Estilo dinámico del badge basado en el color del status
const badgeStyle = computed(() => {
  const statusColor = currentMachineStatus.value?.color;
  if (!statusColor) {
    // Default: yellow (idle)
    return {
      backgroundColor: "var(--p-yellow-100)",
      color: "var(--p-yellow-800)",
    };
  }

  const normalizedColor = normalizeColor(statusColor);
  const isLight = isColorLight(normalizedColor);

  return {
    backgroundColor: normalizedColor,
    color: isLight ? "#000000" : "#ffffff",
  };
});

const handleClick = () => {
  emit("click", props.workcenter.config.id);
};
</script>

<style scoped>
/* Workcenter Card */
.workcenter-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.3s ease;
  border: 2px solid var(--p-surface-border);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  position: relative;
}

.workcenter-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  border-color: var(--p-primary-400);
}

.workcenter-card__status {
  height: 6px;
  width: 100%;
  /* Background is set dynamically via :style binding */
}

/* Card Header */
.workcenter-card__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--p-surface-border);
  gap: 0.2rem;
}

.workcenter-card__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.workcenter-card__icon {
  font-size: 1.25rem;
  color: var(--p-primary-600);
}

.workcenter-card__title h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
  color: var(--text-color);
}

.workcenter-card__badge {
  padding: 0.25rem 0.6rem;
  border-radius: 20px;
  font-size: 0.65rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  white-space: nowrap;
  /* Background and color set dynamically via :style binding */
}

/* Card Content */
.workcenter-card__content {
  padding: 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.workcenter-card__stat {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem 0.75rem;
  background: var(--p-surface-50);
  border-radius: 6px;
  transition: background 0.2s ease;
  overflow: hidden;
  gap: 0.75rem; /* Separación asegurada */
}

.workcenter-card__stat:hover {
  background: var(--p-surface-100);
}

.stat__label {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  color: var(--text-color-secondary);
  font-size: 0.8rem;
  font-weight: 500;
  flex-shrink: 0;
}

.stat__label i {
  font-size: 0.9rem;
  color: var(--p-primary-500);
}

.stat__value {
  font-weight: 700;
  font-size: 0.9rem;
  color: var(--text-color);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  text-align: right;
  flex: 1;
  min-width: 0; /* Necesario para truncate flex item */
}

.stat__value--time {
  font-family: "Courier New", monospace;
  font-size: 1rem;
  color: var(--p-primary-600);
}

.stat__value--phase {
  /* Hereda truncate de .stat__value */
}

.stat__value--reference {
  /* Hereda truncate de .stat__value */
}

.stat__value--phase {
  /* Hereda truncate de .stat__value */
}

.stat__value--reference {
  /* Hereda truncate de .stat__value */
}
</style>

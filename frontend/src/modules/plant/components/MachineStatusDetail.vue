<template>
  <div class="machine-status-detail">
    <!-- Status Card -->
    <div class="status-card" :style="cardStyle">
      <div class="status-card__content">
        <div class="status-card__icon">
          <i :class="statusIcon"></i>
        </div>
        <div class="status-card__info">
          <h3 class="status-card__name">{{ statusName }}</h3>
          <div class="status-card__duration" v-if="duration">
            <i :class="PrimeIcons.CLOCK"></i>
            <span>{{ duration }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Reason Card (if exists) -->
    <div v-if="reason" class="reason-card" :style="reasonStyle">
      <div class="reason-card__icon">
        <i :class="reason.icon || PrimeIcons.TAG"></i>
      </div>
      <div class="reason-card__content">
        <span class="reason-card__label">Motiu:</span>
        <span class="reason-card__name">{{ reason.name }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { MachineStatus, MachineStatusReason } from "../../production/types";
import {
  normalizeColor,
  getStatusCardStyle,
  calculateDuration,
  isColorLight,
} from "../../../utils/functions";

interface Props {
  status?: MachineStatus;
  reason?: MachineStatusReason;
  startTime?: string;
}

const props = defineProps<Props>();

const duration = ref("--");
let timer: ReturnType<typeof setInterval> | undefined;

const statusName = computed(() => props.status?.name || "Desconegut");
const statusIcon = computed(
  () => props.status?.icon || PrimeIcons.QUESTION_CIRCLE
);

const cardStyle = computed(() => {
  return getStatusCardStyle(props.status?.color);
});

const reasonStyle = computed(() => {
  const bgColor = normalizeColor(props.reason?.color);
  const isLight = isColorLight(bgColor);

  return {
    backgroundColor: bgColor,
    color: isLight ? "#000000" : "#ffffff",
    borderColor: isLight ? "rgba(0,0,0,0.1)" : "rgba(255,255,255,0.2)",
  };
});

const updateDuration = () => {
  if (props.startTime) {
    duration.value = calculateDuration(props.startTime);
  } else {
    duration.value = "--";
  }
};

// Watch for startTime changes to update duration immediately
watch(
  () => props.startTime,
  () => {
    updateDuration();
  }
);

onMounted(() => {
  updateDuration();
  timer = setInterval(updateDuration, 60000); // Update every minute
});

onUnmounted(() => {
  if (timer) clearInterval(timer);
});
</script>

<style scoped>
.machine-status-detail {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.status-card {
  background: var(--status-gradient);
  border-radius: 8px;
  padding: 1rem;
  color: var(--status-text-color, white);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
}

.status-card__content {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.status-card__icon {
  font-size: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 50%;
}

.status-card__info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.status-card__name {
  margin: 0;
  font-size: 1.2rem;
  font-weight: 700;
  line-height: 1.2;
}

.status-card__duration {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
  opacity: 0.9;
}

.reason-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  border: 1px solid transparent;
  font-size: 0.95rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.reason-card__icon {
  font-size: 1.2rem;
}

.reason-card__content {
  display: flex;
  flex-direction: column;
}

.reason-card__label {
  font-size: 0.75rem;
  opacity: 0.8;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.reason-card__name {
  font-weight: 600;
}
</style>

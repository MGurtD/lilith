<template>
  <div class="queue">
    <div class="queue__head" aria-hidden="true">
      <span>{{ t("plant.placa.order") }}</span>
      <span>{{ t("plant.placa.reference") }}</span>
      <span class="queue__num">{{ t("plant.detail.pieces") }}</span>
      <span>{{ t("plant.detail.plannedDate") }}</span>
      <span class="queue__num">{{ t("plant.detail.priority") }}</span>
      <span></span>
    </div>

    <p v-if="loading && !workOrders.length" class="queue__empty">
      <ProgressSpinner style="width: 2rem; height: 2rem" />
    </p>
    <p v-else-if="!workOrders.length" class="queue__empty">
      {{ t("plant.detail.noQueue") }}
    </p>

    <ul v-else class="queue__list">
      <li
        v-for="workOrder in workOrders"
        :key="workOrder.workOrderId"
        class="queue__row"
        :class="{ 'queue__row--loaded': isLoaded(workOrder) }"
      >
        <span class="queue__code">{{ workOrder.workOrderCode }}</span>
        <span class="queue__reference">
          <span>{{ workOrder.salesReferenceDisplay }}</span>
          <span class="queue__muted">{{
            [workOrder.customerName, workOrder.workOrderStatus]
              .filter(Boolean)
              .join(" · ")
          }}</span>
        </span>
        <span class="queue__num queue__qty">{{ workOrder.plannedQuantity }}</span>
        <span class="queue__date" :class="{ 'queue__date--late': isLate(workOrder) }">{{
          workOrder.plannedDate ? formatDate(workOrder.plannedDate) : "—"
        }}</span>
        <span class="queue__num queue__priority">{{ workOrder.priority }}</span>
        <Button
          :label="isLoaded(workOrder) ? t('plant.detail.loaded') : t('plant.detail.load')"
          :disabled="isLoaded(workOrder)"
          class="queue__load"
          @click="emit('workorder-selected', workOrder)"
        />
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from "vue";
import { storeToRefs } from "pinia";
import { useI18n } from "vue-i18n";
import ProgressSpinner from "primevue/progressspinner";
import { WorkOrderWithPhases } from "../../../production/types";
import { usePlantWorkcenterStore } from "../../store";
import { formatDate } from "../../../../utils/functions";

// "Fases disponibles": the work orders planned for this machine type, as
// readable rows with a load button (replaces the dense selection table).
interface Props {
  workcenterTypeId: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "workorder-selected", workOrder: WorkOrderWithPhases): void;
}>();

const { t } = useI18n();
const workcenterStore = usePlantWorkcenterStore();
const { availableWorkOrders, availableWorkOrdersLoading, workcenterRt } =
  storeToRefs(workcenterStore);

const workOrders = computed(() => availableWorkOrders.value ?? []);
const loading = computed(() => availableWorkOrdersLoading.value);

const loadedCodes = computed(
  () => new Set((workcenterRt.value?.workorders ?? []).map((wo) => wo.workOrderCode)),
);

const isLoaded = (workOrder: WorkOrderWithPhases) =>
  loadedCodes.value.has(workOrder.workOrderCode);

const startOfToday = new Date();
startOfToday.setHours(0, 0, 0, 0);
const isLate = (workOrder: WorkOrderWithPhases) =>
  !!workOrder.plannedDate && new Date(workOrder.plannedDate) < startOfToday;

onMounted(async () => {
  await workcenterStore.fetchAvailableWorkOrders(props.workcenterTypeId);
});
</script>

<style scoped>
.queue {
  padding: 0.25rem 1.125rem 0;
}

.queue__head,
.queue__row {
  display: grid;
  grid-template-columns: 5.5rem minmax(0, 1fr) 4.5rem 6.5rem 4.5rem 8rem;
  align-items: center;
  gap: 0.75rem;
}

.queue__head {
  min-height: 2.25rem;
  border-bottom: 1px solid var(--p-steel-200);
  font-family: var(--font-condensed);
  font-size: 0.8125rem;
  font-weight: 500;
  color: var(--p-steel-600);
}

.queue__list {
  margin: 0;
  padding: 0;
  list-style: none;
}

.queue__row {
  min-height: 4rem;
  border-bottom: 1px solid var(--p-steel-100);
  font-size: 0.9375rem;
  color: var(--p-steel-900);
}

.queue__row--loaded {
  background: var(--p-steel-50);
}

.queue__code {
  font-family: var(--font-condensed);
  font-size: 1.1875rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

.queue__reference {
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.queue__muted {
  font-size: 0.8125rem;
  color: var(--p-steel-600);
}

.queue__num {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.queue__date {
  font-variant-numeric: tabular-nums;
}

.queue__date--late {
  color: var(--p-red-800);
  font-weight: 500;
}

.queue__priority {
  color: var(--p-steel-700);
}

.queue__load {
  min-height: 48px;
}

.queue__empty {
  display: flex;
  justify-content: center;
  margin: 0;
  padding: 3rem 1rem;
  text-align: center;
  color: var(--p-steel-700);
}

@media (max-width: 767.98px) {
  .queue {
    padding: 0 0.75rem;
  }

  .queue__head {
    display: none;
  }

  .queue__row {
    grid-template-columns: auto minmax(0, 1fr) auto;
    grid-template-areas:
      "code date load"
      "ref ref load";
    row-gap: 0.25rem;
    padding: 0.625rem 0;
  }

  .queue__code { grid-area: code; }
  .queue__date { grid-area: date; }
  .queue__reference { grid-area: ref; }
  .queue__load { grid-area: load; }
  .queue__qty,
  .queue__priority {
    display: none;
  }
}
</style>

<template>
  <div class="phase-selector">
    <DataTable
      :value="workOrders"
      :loading="loading"
      scrollable
      scrollHeight="flex"
      stripedRows
      v-model:selectionKeys="selectedKey"
      selectionMode="single"
      :rowClass="getRowClass"
      @row-select="onRowSelect"
    >
      <!-- Code -->
      <Column field="workOrderCode" header="Codi">
        <template #body="slotProps">
          <span class="font-bold">{{ slotProps.data.workOrderCode }}</span>
        </template>
      </Column>

      <!-- Customer Name -->
      <Column field="customerName" header="Client">
        <template #body="slotProps">
          <span>{{ slotProps.data.customerName }}</span>
        </template>
      </Column>

      <!-- Reference -->
      <Column field="salesReferenceDisplay" header="Referència">
        <template #body="slotProps">
          <span
            class="reference-text"
            :title="slotProps.data.salesReferenceDisplay"
          >
            {{ slotProps.data.salesReferenceDisplay }}
          </span>
        </template>
      </Column>

      <!-- Quantity -->
      <Column
        field="plannedQuantity"
        header="Quantitat"
        :sortable="true"
        style="min-width: 100px; text-align: right"
      />

      <!-- Planned Date -->
      <Column header="Planificada">
        <template #body="slotProps">
          <span v-if="slotProps.data.plannedDate">
            {{ formatDate(slotProps.data.plannedDate) }}
          </span>
        </template>
      </Column>

      <!-- Start Time -->
      <Column header="Iniciada">
        <template #body="slotProps">
          <span v-if="slotProps.data.startTime">
            {{ formatDate(slotProps.data.startTime) }}
          </span>
        </template>
      </Column>

      <!-- Work Order Status -->
      <Column header="Estat">
        <template #body="slotProps">
          <Tag
            :value="slotProps.data.workOrderStatus"
            severity="info"
            rounded
          />
        </template>
      </Column>

      <!-- Priority -->
      <Column
        field="priority"
        header="Prioritat"
        style="min-width: 100px; text-align: center"
      />

      <template #empty>
        <div class="no-data">
          <i :class="PrimeIcons.INBOX" style="font-size: 2rem"></i>
          <p>
            No s'han trobat ordres de fabricació planificades per aquest tipus
            de màquina
          </p>
        </div>
      </template>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { WorkOrderWithPhases } from "../../../production/types";
import { usePlantWorkcenterStore } from "../../store";
import { formatDate } from "../../../../utils/functions";

interface Props {
  workcenterTypeId: string;
  excludePhaseId?: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "workorder-selected", workOrder: WorkOrderWithPhases): void;
}>();

const workcenterStore = usePlantWorkcenterStore();
const { availableWorkOrders, availableWorkOrdersLoading, workcenterRt } =
  storeToRefs(workcenterStore);

const selectedKey = ref();

const workOrders = computed(() => availableWorkOrders.value);
const loading = computed(() => availableWorkOrdersLoading.value);

// Set of loaded work order codes for quick lookup
const loadedWorkOrderCodes = computed(() => {
  if (!workcenterRt.value?.workorders) return new Set<string>();
  return new Set(workcenterRt.value.workorders.map((wo) => wo.workOrderCode));
});

const getRowClass = (data: WorkOrderWithPhases) => {
  if (loadedWorkOrderCodes.value.has(data.workOrderCode)) {
    return "loaded-workorder-row";
  }
  return "";
};

const onRowSelect = (event: any) => {
  emit("workorder-selected", event.data);
};

onMounted(async () => {
  await workcenterStore.fetchAvailableWorkOrders(props.workcenterTypeId);
});
</script>

<style scoped>
.phase-selector {
  width: 100%;
}

.phase-selector :deep(.loaded-workorder-row) {
  background: var(--loaded-row-bg) !important;
  border-left: 4px solid var(--loaded-row-border) !important;
}

.no-data {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 3rem 1rem;
  color: var(--text-color-secondary);
}

.no-data i {
  color: var(--text-color-secondary);
  opacity: 0.5;
}

.no-data p {
  margin: 0;
  font-size: 1rem;
}

/* Reference text truncation for smaller screens */
.reference-text {
  display: block;
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Hide priority column on tablets and smaller devices */
@media screen and (max-width: 1024px) {
  .reference-text {
    max-width: 150px;
  }
}

@media screen and (max-width: 768px) {
  .reference-text {
    max-width: 100px;
  }
}
</style>

<template>
  <Dialog
    v-model:visible="dialogVisible"
    :modal="true"
    :draggable="false"
    :style="{ width: '92vw', maxWidth: '980px' }"
    header="Estoc disponible"
  >
    <div class="stock-dialog">
      <div class="stock-dialog-header">
        <div>
          <span class="stock-dialog-title">Material seleccionat</span>
          <div class="stock-dialog-ref">{{ bomCode }}</div>
        </div>
        <span class="stock-dialog-caption">
          Mou tota la quantitat disponible a la ubicació d'aprovisionament.
        </span>
      </div>

      <div v-if="stockItems.length === 0" class="stock-empty">
        <i class="pi pi-exclamation-circle"></i>
        <span>Sense estoc disponible</span>
      </div>

      <DataTable
        v-else
        :value="stockItems"
        size="small"
        scrollable
        class="stock-table"
      >
        <Column field="warehouseName" header="Magatzem" style="min-width: 140px" />
        <Column header="Ubicació" style="min-width: 180px">
          <template #body="slotProps">
            <div class="stock-location-cell">
              <span class="font-semibold">{{ slotProps.data.locationName }}</span>
              <span v-if="slotProps.data.locationDescription" class="stock-location-detail">
                {{ slotProps.data.locationDescription }}
              </span>
            </div>
          </template>
        </Column>
        <Column header="Mesures" style="min-width: 260px">
          <template #body="slotProps">
            <div class="stock-measures">
              <span
                v-for="measure in getStockMeasures(slotProps.data)"
                :key="`${slotProps.data.stockId}-${measure}`"
                class="stock-measure-chip"
              >
                {{ measure }}
              </span>
            </div>
          </template>
        </Column>
        <Column
          field="quantity"
          header="Quantitat"
          style="width: 110px; text-align: right"
        >
          <template #body="slotProps">
            <span class="font-semibold">{{ slotProps.data.quantity }}</span>
          </template>
        </Column>
        <Column header="Moure" style="width: 90px; text-align: center">
          <template #body="slotProps">
            <Button
              icon="pi pi-arrow-right"
              text
              rounded
              severity="secondary"
              :loading="movingStockId === slotProps.data.stockId"
              :disabled="movingStockId !== null"
              @click="emit('move-stock', slotProps.data)"
              v-tooltip.top="'Moure a ubicació d\'aprovisionament'"
            />
          </template>
        </Column>
      </DataTable>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed } from "vue";
import Dialog from "primevue/dialog";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Button from "primevue/button";
import type { StockResponse } from "../../../warehouse/types";

interface Props {
  visible: boolean;
  bomCode: string;
  stockItems: StockResponse[];
  movingStockId: string | null;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "move-stock", stockItem: StockResponse): void;
}>();

const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit("update:visible", value),
});

function getStockMeasures(stockItem: StockResponse): string[] {
  const measures = [
    { label: "Ample", value: stockItem.width },
    { label: "Llarg", value: stockItem.length },
    { label: "Alt", value: stockItem.height },
    { label: "Diam.", value: stockItem.diameter },
    { label: "Gruix", value: stockItem.thickness },
  ]
    .filter((measure) => measure.value > 0)
    .map((measure) => `${measure.label} ${measure.value}`);

  return measures.length > 0 ? measures : ["Sense mesures"];
}
</script>

<style scoped>
.stock-dialog {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.stock-dialog-header {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--surface-border);
}

.stock-dialog-title {
  display: block;
  font-weight: 600;
  font-size: 0.95rem;
}

.stock-dialog-ref {
  margin-top: 0.25rem;
  font-size: 0.85rem;
  color: var(--text-color-secondary);
}

.stock-dialog-caption {
  max-width: 280px;
  text-align: right;
  font-size: 0.85rem;
  color: var(--text-color-secondary);
}

.stock-empty {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 0;
  color: var(--text-color-secondary);
  font-size: 0.9rem;
}

.stock-table {
  width: 100%;
}

.stock-location-cell {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.stock-location-detail {
  font-size: 0.8rem;
  color: var(--text-color-secondary);
}

.stock-measures {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}

.stock-measure-chip {
  border: 1px solid var(--surface-border);
  border-radius: 999px;
  padding: 0.2rem 0.55rem;
  font-size: 0.78rem;
  color: var(--text-color-secondary);
  background: var(--surface-50);
}

@media (max-width: 768px) {
  .stock-dialog-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .stock-dialog-caption {
    max-width: none;
    text-align: left;
  }
}
</style>
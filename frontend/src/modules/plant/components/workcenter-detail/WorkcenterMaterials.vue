<template>
  <div class="workcenter-materials">
    <div v-if="!activePhaseStore.hasBillOfMaterials" class="empty-state">
      <i :class="PrimeIcons.BOX" class="empty-icon"></i>
      <p class="empty-text">Sense materials</p>
      <span class="empty-subtext">
        Aquesta fase no té materials associats.
      </span>
    </div>

    <DataTable
      v-else
      :value="activePhaseStore.billOfMaterials"
      scrollable
      scrollHeight="flex"
      stripedRows
      class="materials-table"
    >
      <Column field="referenceCode" header="Referència" style="width: 200px">
        <template #body="slotProps">
          <span class="font-semibold">{{ slotProps.data.referenceCode }}</span>
        </template>
      </Column>
      <Column field="referenceDescription" header="Descripció" />
      <Column
        field="quantity"
        header="Quantitat"
        style="width: 120px; text-align: right"
      >
        <template #body="slotProps">
          <span class="font-semibold">{{ slotProps.data.quantity }}</span>
        </template>
      </Column>
      <Column header="Estoc" style="width: 80px; text-align: center">
        <template #body="slotProps">
          <Button
            icon="pi pi-warehouse"
            text
            rounded
            severity="secondary"
            :loading="loadingBomId === slotProps.data.id"
            @click="showStock($event, slotProps.data)"
          />
        </template>
      </Column>
    </DataTable>

    <!-- Popover estoc disponible -->
    <Popover ref="stockPopover">
      <div class="stock-popover">
        <div class="stock-popover-header">
          <span class="stock-popover-title">Estoc disponible</span>
          <span class="stock-popover-ref">{{ selectedBomCode }}</span>
        </div>

        <div v-if="stockItems.length === 0" class="stock-empty">
          <i class="pi pi-exclamation-circle"></i>
          <span>Sense estoc disponible</span>
        </div>

        <DataTable
          v-else
          :value="stockItems"
          size="small"
          class="stock-table"
        >
          <Column field="warehouseName" header="Magatzem" />
          <Column field="locationName" header="Ubicació" />
          <Column
            field="quantity"
            header="Quantitat"
            style="width: 90px; text-align: right"
          >
            <template #body="slotProps">
              <span class="font-semibold">{{ slotProps.data.quantity }}</span>
            </template>
          </Column>
          <Column header="Moure" style="width: 80px; text-align: center">
            <template #body="slotProps">
              <Button
                icon="pi pi-arrow-right"
                text
                rounded
                severity="secondary"
                :loading="movingStockId === slotProps.data.stockId"
                :disabled="movingStockId !== null"
                @click="moveStock(slotProps.data)"
                v-tooltip.top="'Moure a ubicació d\'aprovisionament'"
              />
            </template>
          </Column>
        </DataTable>
      </div>
    </Popover>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRoute } from "vue-router";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Button from "primevue/button";
import Popover from "primevue/popover";
import { usePlantActivePhaseStore } from "../../store";
import type { BillOfMaterialsItem } from "../../../production/types";
import type { StockResponse } from "../../../warehouse/types";
import WarehouseServices from "../../../warehouse/services";

const route = useRoute();
const toast = useToast();
const activePhaseStore = usePlantActivePhaseStore();

const workcenterId = route.params.id as string;
const stockPopover = ref<InstanceType<typeof Popover> | null>(null);
const stockItems = ref<StockResponse[]>([]);
const loadingBomId = ref<string | null>(null);
const selectedBomCode = ref("");
const movingStockId = ref<string | null>(null);

async function showStock(event: Event, bom: BillOfMaterialsItem) {
  selectedBomCode.value = bom.referenceCode;
  loadingBomId.value = bom.id;

  const target = event.currentTarget as HTMLElement;

  try {
    stockItems.value = await WarehouseServices.Stock.getByBillOfMaterialsId(bom.id);
  } finally {
    loadingBomId.value = null;
  }

  stockPopover.value?.show({ currentTarget: target } as Event);
}

async function moveStock(stockItem: StockResponse) {
  movingStockId.value = stockItem.stockId;
  
  try {
    const result = await WarehouseServices.Stock.moveToWorkcenterSupply({
      stockId: stockItem.stockId,
      workcenterId: workcenterId,
      quantity: stockItem.quantity,
    });

    if (result) {
      toast.add({
        severity: "success",
        summary: "Stock mogut correctament",
        detail: `${stockItem.quantity} unitats de ${stockItem.referenceCode} mogudes a la ubicació d'aprovisionament`,
        life: 4000,
      });
      
      // Refresh stock data
      const bomId = activePhaseStore.billOfMaterials.find(
        (bom) => bom.referenceCode === selectedBomCode.value
      )?.id;
      if (bomId) {
        stockItems.value = await WarehouseServices.Stock.getByBillOfMaterialsId(bomId);
      }
    } else {
      toast.add({
        severity: "error",
        summary: "Error al moure l'stock",
        detail: "No s'ha pogut moure l'stock a la ubicació d'aprovisionament",
        life: 4000,
      });
    }
  } finally {
    movingStockId.value = null;
  }
}
</script>

<style scoped>
.workcenter-materials {
  height: 100%;
  display: flex;
  flex-direction: column;
  padding: 1rem;
  overflow-y: auto;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  min-height: 200px;
  text-align: center;
  color: var(--text-color-secondary);
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.empty-text {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0 0 0.5rem 0;
  color: var(--text-color);
}

.empty-subtext {
  font-size: 0.9rem;
  max-width: 300px;
}

.materials-table {
  height: 100%;
}

/* Popover */
.stock-popover {
  min-width: 380px;
  max-width: 480px;
}

.stock-popover-header {
  display: flex;
  align-items: baseline;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--surface-border);
}

.stock-popover-title {
  font-weight: 600;
  font-size: 0.95rem;
}

.stock-popover-ref {
  font-size: 0.85rem;
  color: var(--text-color-secondary);
}

.stock-loading,
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
</style>

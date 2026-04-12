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
          <div class="reference-cell">
            <span class="font-semibold">{{ slotProps.data.referenceCode }}</span>
            <Tag
              v-if="activePhaseStore.hasMaterialsConsumed"
              value="Consumit"
              severity="success"
              icon="pi pi-check-circle"
            />
            <Tag
              v-else-if="isMaterialProvisioned(slotProps.data.id)"
              value="Aprovisionat"
              severity="warn"
              icon="pi pi-check-circle"
            />
          </div>
        </template>
      </Column>
      <Column field="referenceDescription" header="Descripció" />
      <Column header="Mesures" style="min-width: 260px">
        <template #body="slotProps">
          <DimensionChips
            :width="slotProps.data.width"
            :length="slotProps.data.length"
            :height="slotProps.data.height"
            :diameter="slotProps.data.diameter"
            :thickness="slotProps.data.thickness"
          />
        </template>
      </Column>
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
            @click="showStock(slotProps.data)"
          />
        </template>
      </Column>
    </DataTable>

    <div
      v-if="activePhaseStore.materialsProvisioningLoading"
      class="provisioning-status"
    >
      <i class="pi pi-spin pi-spinner"></i>
      <span>Comprovant aprovisionament dels materials...</span>
    </div>

    <div
      v-else-if="activePhaseStore.materialsProvisioningError"
      class="provisioning-status provisioning-status-error"
    >
      <i class="pi pi-exclamation-triangle"></i>
      <span>{{ activePhaseStore.materialsProvisioningError }}</span>
    </div>

    <AvailableStockDialog
      v-if="selectedBomItem"
      v-model:visible="stockDialogVisible"
      :bom-item="selectedBomItem"
      :stock-items="stockItems"
      :moving-stock-id="movingStockId"
      :workcenter-location-ids="workcenterStore.associatedLocationIds"
      @move-stock="moveStock"
      @return-stock="returnStock"
    />
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
import Tag from "primevue/tag";
import { usePlantActivePhaseStore, usePlantWorkcenterStore } from "../../store";
import type { BillOfMaterialsItem } from "../../../production/types";
import type { StockResponse } from "../../../warehouse/types";
import WarehouseServices from "../../../warehouse/services";
import ProductionServices from "../../../production/services";
import AvailableStockDialog from "./AvailableStockDialog.vue";
import DimensionChips from "./DimensionChips.vue";

const route = useRoute();
const toast = useToast();
const activePhaseStore = usePlantActivePhaseStore();
const workcenterStore = usePlantWorkcenterStore();

const workcenterId = route.params.id as string;
const stockDialogVisible = ref(false);
const stockItems = ref<StockResponse[]>([]);
const loadingBomId = ref<string | null>(null);
const selectedBomItem = ref<BillOfMaterialsItem | null>(null);
const movingStockId = ref<string | null>(null);

function isMaterialProvisioned(bomId: string): boolean {
  return activePhaseStore.bomProvisioningById[bomId] === true;
}

async function showStock(bom: BillOfMaterialsItem) {
  selectedBomItem.value = bom;
  loadingBomId.value = bom.id;

  try {
    stockItems.value = await WarehouseServices.Stock.getByBillOfMaterialsId(bom.id);
    stockDialogVisible.value = true;
  } finally {
    loadingBomId.value = null;
  }
}

async function moveStock(payload: { stockItem: StockResponse; quantity: number }) {
  const { stockItem, quantity } = payload;
  const workOrderPhaseId = activePhaseStore.activePhase?.phaseId;
  if (!workOrderPhaseId) {
    toast.add({
      severity: "error",
      summary: "Error al moure l'stock",
      detail: "No s'ha pogut determinar la fase activa",
      life: 4000,
    });
    return;
  }

  movingStockId.value = stockItem.stockId;
  
  try {
    const result = await ProductionServices.WorkOrderStock.moveToWorkcenterSupply({
      stockId: stockItem.stockId,
      workcenterId: workcenterId,
      workOrderPhaseId: workOrderPhaseId,
      quantity: quantity,
    });

    if (result) {
      toast.add({
        severity: "success",
        summary: "Stock mogut correctament",
        detail: `${quantity} unitats de ${stockItem.referenceCode} mogudes a la ubicació d'aprovisionament`,
        life: 4000,
      });

      if (selectedBomItem.value) {
        await activePhaseStore.refreshMaterialProvisioning(selectedBomItem.value.id);
        stockItems.value = await WarehouseServices.Stock.getByBillOfMaterialsId(selectedBomItem.value.id);
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

async function returnStock(payload: { stockItem: StockResponse; quantity: number }) {
  const { stockItem, quantity } = payload;
  const workOrderPhaseId = activePhaseStore.activePhase?.phaseId;
  if (!workOrderPhaseId) {
    toast.add({
      severity: "error",
      summary: "Error al retornar l'stock",
      detail: "No s'ha pogut determinar la fase activa",
      life: 4000,
    });
    return;
  }

  movingStockId.value = stockItem.stockId;

  try {
    const result = await ProductionServices.WorkOrderStock.returnFromWorkcenterSupply({
      stockId: stockItem.stockId,
      workcenterId: workcenterId,
      workOrderPhaseId: workOrderPhaseId,
      quantity: quantity,
    });

    if (result) {
      toast.add({
        severity: "success",
        summary: "Stock retornat correctament",
        detail: `${quantity} unitats de ${stockItem.referenceCode} retornades a la ubicació per defecte`,
        life: 4000,
      });

      if (selectedBomItem.value) {
        await activePhaseStore.refreshMaterialProvisioning(selectedBomItem.value.id);
        stockItems.value = await WarehouseServices.Stock.getByBillOfMaterialsId(selectedBomItem.value.id);
      }
    } else {
      toast.add({
        severity: "error",
        summary: "Error al retornar l'stock",
        detail: "No s'ha pogut retornar l'stock a la ubicació per defecte",
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

.reference-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
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

.provisioning-status {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 0.75rem;
  color: var(--text-color-secondary);
  font-size: 0.9rem;
}

.provisioning-status-error {
  color: var(--p-red-500);
}
</style>

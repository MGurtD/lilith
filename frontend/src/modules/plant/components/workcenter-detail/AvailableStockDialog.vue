<template>
  <Dialog
    v-model:visible="dialogVisible"
    :modal="true"
    :draggable="false"
    :style="{ width: '92vw', maxWidth: '1080px' }"
    header="Estoc disponible"
  >
    <div class="stock-dialog">
      <div class="stock-dialog-header">
        <span class="stock-dialog-caption">
          Selecciona la quantitat i l'acció a realitzar sobre l'estoc.
        </span>
      </div>

      <BomMaterialHeader
        :reference-code="bomItem.referenceCode"
        :reference-description="bomItem.referenceDescription"
        :quantity="bomItem.quantity"
        :width="bomItem.width"
        :length="bomItem.length"
        :height="bomItem.height"
        :diameter="bomItem.diameter"
        :thickness="bomItem.thickness"
        :format-description="formatDescription"
      />

      <div v-if="stockItems.length === 0" class="stock-empty">
        <i class="pi pi-exclamation-circle"></i>
        <span>Sense estoc disponible</span>
      </div>

      <template v-else>
        <!-- Supply stock (at workcenter location) -->
        <div v-if="supplyStockItems.length > 0" class="stock-group">
          <div class="stock-group-header stock-group-header--supply">
            <i class="pi pi-box"></i>
            <span>Estoc aprovisionat</span>
          </div>
          <DataTable
            :value="supplyStockItems"
            size="small"
            scrollable
            class="stock-table"
          >
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
              header="Disponible"
              style="width: 110px; text-align: right"
            >
              <template #body="slotProps">
                <span class="font-semibold">{{ slotProps.data.quantity }}</span>
              </template>
            </Column>
            <Column header="Quantitat" style="width: 140px">
              <template #body="slotProps">
                <InputNumber
                  v-model="moveQuantities[slotProps.data.stockId]"
                  :min="1"
                  :max="slotProps.data.quantity"
                  :disabled="movingStockId !== null"
                  showButtons
                  buttonLayout="horizontal"
                  incrementButtonIcon="pi pi-plus"
                  decrementButtonIcon="pi pi-minus"
                  inputClass="stock-qty-input"
                  class="stock-qty-spinner"
                />
              </template>
            </Column>
            <Column header="Retornar" style="width: 90px; text-align: center">
              <template #body="slotProps">
                <Button
                  icon="pi pi-arrow-left"
                  text
                  rounded
                  severity="warn"
                  :loading="movingStockId === slotProps.data.stockId"
                  :disabled="movingStockId !== null || !isValidQuantity(slotProps.data.stockId, slotProps.data.quantity)"
                  @click="handleReturnStock(slotProps.data)"
                  v-tooltip.top="'Retornar a ubicació per defecte'"
                />
              </template>
            </Column>
          </DataTable>
        </div>

        <!-- Other stock (available to move to supply) -->
        <div v-if="otherStockItems.length > 0" class="stock-group">
          <div class="stock-group-header stock-group-header--available">
            <i class="pi pi-warehouse"></i>
            <span>Estoc disponible</span>
          </div>
          <DataTable
            :value="otherStockItems"
            size="small"
            scrollable
            class="stock-table"
          >
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
              header="Disponible"
              style="width: 110px; text-align: right"
            >
              <template #body="slotProps">
                <span class="font-semibold">{{ slotProps.data.quantity }}</span>
              </template>
            </Column>
            <Column header="Quantitat" style="width: 140px">
              <template #body="slotProps">
                <InputNumber
                  v-model="moveQuantities[slotProps.data.stockId]"
                  :min="0"
                  :max="slotProps.data.quantity"
                  :disabled="movingStockId !== null"
                  showButtons
                  buttonLayout="horizontal"
                  incrementButtonIcon="pi pi-plus"
                  decrementButtonIcon="pi pi-minus"
                  inputClass="stock-qty-input"
                  class="stock-qty-spinner"
                />
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
                  :disabled="movingStockId !== null || !isValidQuantity(slotProps.data.stockId, slotProps.data.quantity)"
                  @click="handleMoveStock(slotProps.data)"
                  v-tooltip.top="'Moure a ubicació d\'aprovisionament'"
                />
              </template>
            </Column>
          </DataTable>
        </div>
      </template>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, reactive, watch } from "vue";
import { useI18n } from "vue-i18n";
import Dialog from "primevue/dialog";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Button from "primevue/button";
import InputNumber from "primevue/inputnumber";
import type { BillOfMaterialsItem } from "../../../production/types";
import type { StockResponse } from "../../../warehouse/types";
import { formatDimensions } from "@/utils/functions";
import BomMaterialHeader from "./BomMaterialHeader.vue";

interface Props {
  visible: boolean;
  bomItem: BillOfMaterialsItem;
  stockItems: StockResponse[];
  movingStockId: string | null;
  workcenterLocationIds: string[];
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "move-stock", payload: { stockItem: StockResponse; quantity: number }): void;
  (e: "return-stock", payload: { stockItem: StockResponse; quantity: number }): void;
}>();

const { t } = useI18n();

const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit("update:visible", value),
});

/** Derive format description from the first stock item (all share the same reference format). */
const formatDescription = computed(() => {
  return props.stockItems[0]?.referenceFormatDescription ?? "";
});

const supplyStockItems = computed(() =>
  props.stockItems.filter((item) =>
    props.workcenterLocationIds.includes(item.locationId),
  ),
);

const otherStockItems = computed(() =>
  props.stockItems.filter(
    (item) => !props.workcenterLocationIds.includes(item.locationId),
  ),
);

const moveQuantities = reactive<Record<string, number>>({});

// Initialize/reset quantities when stock items change
watch(
  () => props.stockItems,
  (items) => {
    // Clear previous quantities
    Object.keys(moveQuantities).forEach((key) => delete moveQuantities[key]);
    items.forEach((item) => {
      // Supply stock: pre-fill with BOM quantity capped by available
      // Available stock: start at 0 so the user explicitly chooses
      const isSupply = props.workcenterLocationIds.includes(item.locationId);
      moveQuantities[item.stockId] = isSupply
        ? Math.min(props.bomItem.quantity, item.quantity)
        : 0;
    });
  },
  { immediate: true },
);

function isValidQuantity(stockId: string, maxQuantity: number): boolean {
  const qty = moveQuantities[stockId];
  return qty != null && qty > 0 && qty <= maxQuantity;
}

function handleMoveStock(stockItem: StockResponse) {
  const quantity = moveQuantities[stockItem.stockId];
  if (!isValidQuantity(stockItem.stockId, stockItem.quantity)) return;
  emit("move-stock", { stockItem, quantity });
  // Reset only this row so user must explicitly choose again
  moveQuantities[stockItem.stockId] = 0;
}

function handleReturnStock(stockItem: StockResponse) {
  const quantity = moveQuantities[stockItem.stockId];
  if (!isValidQuantity(stockItem.stockId, stockItem.quantity)) return;
  emit("return-stock", { stockItem, quantity });
}

function getStockMeasures(stockItem: StockResponse): string[] {
  return formatDimensions(t, {
    width: stockItem.width,
    length: stockItem.length,
    height: stockItem.height,
    diameter: stockItem.diameter,
    thickness: stockItem.thickness,
  });
}
</script>

<style scoped>
.stock-dialog {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.stock-dialog-header {
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--surface-border);
}

.stock-dialog-caption {
  font-size: 0.9rem;
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

.stock-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.stock-group-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  font-weight: 600;
  font-size: 0.88rem;
}

.stock-group-header--supply {
  background: var(--p-orange-50);
  color: var(--p-orange-700);
  border: 1px solid var(--p-orange-200);
}

.stock-group-header--available {
  background: var(--p-blue-50);
  color: var(--p-blue-700);
  border: 1px solid var(--p-blue-200);
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

.stock-qty-spinner {
  width: 100%;
}

:deep(.stock-qty-input) {
  width: 3rem !important;
  text-align: center;
  font-weight: 600;
}
</style>

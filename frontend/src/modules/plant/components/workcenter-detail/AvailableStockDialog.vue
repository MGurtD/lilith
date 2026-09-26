<template>
  <Dialog
    v-model:visible="dialogVisible"
    :modal="true"
    :draggable="false"
    :style="{ width: '92vw', maxWidth: '1080px' }"
    :header="$t('plant.stock.availableTitle')"
  >
    <div class="stock-dialog">
      <div class="stock-dialog-header">
        <span class="stock-dialog-caption">{{ $t("plant.stock.caption") }}</span>
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
        <span>{{ $t("plant.stock.none") }}</span>
      </div>

      <template v-else>
        <section
          v-for="group in groups"
          :key="group.key"
          class="stock-group"
          :aria-labelledby="`stock-group-${group.key}`"
        >
          <h3 :id="`stock-group-${group.key}`" class="stock-group-header">
            <i :class="group.icon" aria-hidden="true"></i>
            {{ group.title }}
          </h3>
          <Table
            :items="group.items"
            :columns="columns"
            :card-layout="cardLayout"
            phone-layout="cards"
            :show-filters="false"
            :show-create="false"
            data-key="stockId"
            class="stock-table"
          >
            <template #body-locationName="{ data }">
              <span class="stock-location-cell">
                <span class="stock-location-name">{{ data.locationName }}</span>
                <span v-if="data.locationDescription" class="stock-location-detail">{{
                  data.locationDescription
                }}</span>
              </span>
            </template>
            <template #body-dimensions="{ data }">
              <DimensionChips
                :width="data.width"
                :length="data.length"
                :height="data.height"
                :diameter="data.diameter"
                :thickness="data.thickness"
              />
            </template>
            <template #body-lotCode="{ data }">{{ data.lotCode || "—" }}</template>
            <template #body-moveQty="{ data }">
              <InputNumber
                v-model="moveQuantities[data.stockId]"
                :min="group.min"
                :max="data.quantity"
                :disabled="movingStockId !== null"
                showButtons
                buttonLayout="horizontal"
                incrementButtonIcon="pi pi-plus"
                decrementButtonIcon="pi pi-minus"
                :inputProps="{ 'aria-label': t('plant.stock.quantity') }"
                inputClass="stock-qty-input"
                class="stock-qty-spinner"
              />
            </template>
            <template #body-action="{ data }">
              <Button
                :icon="group.actionIcon"
                :aria-label="group.actionLabel"
                v-tooltip.top="group.actionLabel"
                outlined
                severity="secondary"
                class="stock-action"
                :loading="movingStockId === data.stockId"
                :disabled="movingStockId !== null || !isValidQuantity(data.stockId, data.quantity)"
                @click="group.run(data)"
              />
            </template>
            <template #card-actions="{ data }">
              <Button
                :icon="group.actionIcon"
                :aria-label="group.actionLabel"
                outlined
                severity="secondary"
                class="stock-action"
                :loading="movingStockId === data.stockId"
                :disabled="movingStockId !== null || !isValidQuantity(data.stockId, data.quantity)"
                @click="group.run(data)"
              />
            </template>
          </Table>
        </section>
      </template>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, reactive, watch } from "vue";
import { useI18n } from "vue-i18n";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputNumber from "primevue/inputnumber";
import type { BillOfMaterialsItem } from "../../../production/types";
import type { StockResponse } from "../../../warehouse/types";
import Table from "@/components/tables/Table.vue";
import { ColumnType, type CardLayout, type Column } from "@/components/tables/types";
import DimensionChips from "@/components/DimensionChips.vue";
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

// Both groups share the table; each row moves or returns stock with its
// own quantity. On phones the rows become cards with the action button.
const columns = computed<Column[]>(() => [
  { field: "locationName", header: t("plant.stock.location") },
  { field: "dimensions", header: t("plant.stock.dimensions") },
  { field: "lotCode", header: t("plant.stock.lot") },
  {
    field: "quantity",
    header: t("plant.stock.available"),
    columnType: ColumnType.Number,
    style: "width: 7rem",
  },
  { field: "moveQty", header: t("plant.stock.quantity"), style: "width: 11rem" },
  { field: "action", header: t("plant.stock.action"), style: "width: 5rem" },
]);

const cardLayout: CardLayout = {
  title: "locationName",
  trailing: "quantity",
  subtitle: "dimensions",
  meta: ["lotCode", "moveQty"],
};

const groups = computed(() =>
  [
    {
      key: "supply",
      title: t("plant.stock.supplyTitle"),
      icon: "pi pi-box",
      items: supplyStockItems.value,
      min: 1,
      actionIcon: "pi pi-arrow-left",
      actionLabel: t("plant.tooltips.returnToDefaultLocation"),
      run: handleReturnStock,
    },
    {
      key: "available",
      title: t("plant.stock.availableTitle"),
      icon: "pi pi-warehouse",
      items: otherStockItems.value,
      min: 0,
      actionIcon: "pi pi-arrow-right",
      actionLabel: t("plant.tooltips.moveToSupplyLocation"),
      run: handleMoveStock,
    },
  ].filter((group) => group.items.length > 0),
);
</script>

<style scoped>
.stock-dialog {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.stock-dialog-header {
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--p-steel-200);
}

.stock-dialog-caption {
  font-size: 0.9375rem;
  color: var(--p-steel-700);
}

.stock-empty {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1.5rem;
  color: var(--p-steel-700);
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
  margin: 0;
  font-family: var(--font-condensed);
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--p-steel-900);
}

.stock-location-cell {
  display: flex;
  flex-direction: column;
}

.stock-location-name {
  font-weight: 600;
}

.stock-location-detail {
  font-size: 0.8125rem;
  color: var(--p-steel-600);
}

.stock-qty-spinner :deep(.p-inputtext),
.stock-qty-spinner :deep(.p-button) {
  min-height: 48px;
}

.stock-qty-spinner :deep(.stock-qty-input) {
  width: 4rem;
  text-align: center;
  font-variant-numeric: tabular-nums;
}

.stock-action {
  width: 48px;
  height: 48px;
}
</style>

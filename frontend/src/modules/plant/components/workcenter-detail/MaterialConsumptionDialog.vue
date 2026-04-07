<template>
  <Dialog
    v-model:visible="dialogVisible"
    :modal="true"
    :draggable="false"
    :closable="!submitting"
    :style="{ width: '95vw', maxWidth: '1200px' }"
    header="Consum de materials"
  >
    <div class="consumption-dialog">
      <div class="consumption-dialog-header">
        <span class="consumption-dialog-caption">
          Informa el material restant després de la fabricació. El sistema
          calcularà el consum automàticament i retornarà el sobrant al magatzem.
        </span>
      </div>

      <div v-if="loading" class="consumption-loading">
        <i class="pi pi-spin pi-spinner" style="font-size: 1.5rem"></i>
        <span>Carregant estoc aprovisionat...</span>
      </div>

      <template v-else>
        <div
          v-for="group in bomGroups"
          :key="group.bomId"
          class="consumption-bom-group"
        >
          <!-- BOM item header -->
          <div class="consumption-bom-header">
            <div class="consumption-bom-info">
              <i class="pi pi-box"></i>
              <span class="font-semibold">{{ group.bomCode }}</span>
              <span class="text-500">{{ group.bomDescription }}</span>
              <span class="consumption-bom-qty"
                >Quantitat: {{ group.bomQuantity }}</span
              >
              <div
                v-if="group.bomMeasures.length > 0"
                class="consumption-bom-measures"
              >
                <span
                  v-for="measure in group.bomMeasures"
                  :key="measure"
                  class="consumption-bom-measure-chip"
                >
                  {{ measure }}
                </span>
              </div>
            </div>
            <div class="consumption-bom-format">
              {{ group.formatDescription }}
            </div>
          </div>

          <!-- Provisioned stock items -->
          <DataTable
            :value="group.lines"
            size="small"
            scrollable
            class="consumption-table"
          >
            <Column header="Ubicació" style="min-width: 120px">
              <template #body="slotProps">
                <span>{{ slotProps.data.locationName }}</span>
              </template>
            </Column>
            <Column header="Mesures estoc" style="min-width: 180px">
              <template #body="slotProps">
                <div
                  v-if="slotProps.data.originalMeasures.length > 0"
                  class="stock-measures"
                >
                  <span
                    v-for="measure in slotProps.data.originalMeasures"
                    :key="measure"
                    class="stock-measure-chip"
                  >
                    {{ measure }}
                  </span>
                </div>
                <span v-else class="text-500">Sense mesures</span>
              </template>
            </Column>

            <Column
              header="Disponible"
              style="width: 90px; text-align: right"
            >
              <template #body="slotProps">
                <span class="font-semibold">{{
                  slotProps.data.availableQuantity
                }}</span>
              </template>
            </Column>

            <!-- Remaining quantity input -->
            <Column header="Qttat. restant" style="width: 140px">
              <template #body="slotProps">
                <InputNumber
                  v-model="slotProps.data.remainingQuantity"
                  :min="0"
                  :max="slotProps.data.availableQuantity"
                  :disabled="submitting"
                  showButtons
                  buttonLayout="horizontal"
                  incrementButtonIcon="pi pi-plus"
                  decrementButtonIcon="pi pi-minus"
                  inputClass="stock-qty-input"
                  class="stock-qty-spinner"
                />
              </template>
            </Column>

            <!-- Dynamic remaining dimension inputs based on format -->
            <Column
              v-if="showField(group.formatCode, 'length')"
              header="Llarg. restant"
              style="width: 130px"
            >
              <template #body="slotProps">
                <InputNumber
                  v-model="slotProps.data.remainingLength"
                  :min="0"
                  :max="slotProps.data.originalLength"
                  :minFractionDigits="0"
                  :maxFractionDigits="2"
                  :disabled="submitting"
                  inputClass="dimension-input"
                  class="w-full"
                />
              </template>
            </Column>
            <Column
              v-if="showField(group.formatCode, 'width')"
              header="Ample restant"
              style="width: 130px"
            >
              <template #body="slotProps">
                <InputNumber
                  v-model="slotProps.data.remainingWidth"
                  :min="0"
                  :max="slotProps.data.originalWidth"
                  :minFractionDigits="0"
                  :maxFractionDigits="2"
                  :disabled="submitting"
                  inputClass="dimension-input"
                  class="w-full"
                />
              </template>
            </Column>
            <Column
              v-if="showField(group.formatCode, 'height')"
              header="Alçada restant"
              style="width: 130px"
            >
              <template #body="slotProps">
                <InputNumber
                  v-model="slotProps.data.remainingHeight"
                  :min="0"
                  :max="slotProps.data.originalHeight"
                  :minFractionDigits="0"
                  :maxFractionDigits="2"
                  :disabled="submitting"
                  inputClass="dimension-input"
                  class="w-full"
                />
              </template>
            </Column>

            <!-- Computed consumption summary -->
            <Column header="Consum calculat" style="min-width: 180px">
              <template #body="slotProps">
                <div class="computed-consumption">
                  <span
                    v-if="getConsumedQuantity(slotProps.data) > 0"
                    class="consumed-badge"
                  >
                    {{ getConsumedQuantity(slotProps.data) }} ut.
                    <template
                      v-if="getConsumedDimensions(slotProps.data, group.formatCode).length > 0"
                    >
                      &mdash;
                      {{ getConsumedDimensions(slotProps.data, group.formatCode).join(" × ") }}
                    </template>
                  </span>
                  <span v-else class="no-consumption-badge">Sense consum</span>
                </div>
              </template>
            </Column>
          </DataTable>
        </div>
      </template>

      <!-- Action Buttons -->
      <div class="consumption-actions">
        <Button
          icon="pi pi-times"
          label="Cancel·lar"
          severity="secondary"
          :disabled="submitting"
          @click="onCancel"
        />
        <Button
          icon="pi pi-check"
          label="Confirmar consum"
          severity="success"
          :disabled="submitting || !hasAnyConsumption"
          :loading="submitting"
          @click="onConfirm"
        />
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import Dialog from "primevue/dialog";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Button from "primevue/button";
import InputNumber from "primevue/inputnumber";
import { useToast } from "primevue/usetoast";
import type { BillOfMaterialsItem } from "../../../production/types";
import type {
  StockResponse,
  ConsumeStockItem,
} from "../../../warehouse/types";
import WarehouseServices from "../../../warehouse/services";
import { usePlantWorkcenterStore } from "../../store";

interface Props {
  visible: boolean;
  billOfMaterials: BillOfMaterialsItem[];
  workcenterId: string;
  workOrderPhaseId: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "confirm", items: ConsumeStockItem[]): void;
}>();

const toast = useToast();
const workcenterStore = usePlantWorkcenterStore();

const loading = ref(false);
const submitting = ref(false);

const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit("update:visible", value),
});

// ----- Line model -----
interface ConsumptionLine {
  lineId: string;
  stockId: string;
  locationName: string;
  originalMeasures: string[];
  availableQuantity: number;
  // Original dimensions (read-only reference)
  originalWidth: number;
  originalLength: number;
  originalHeight: number;
  originalDiameter: number;
  originalThickness: number;
  // Remaining dimensions (user input)
  remainingQuantity: number;
  remainingWidth: number;
  remainingLength: number;
  remainingHeight: number;
}

interface BomGroup {
  bomId: string;
  bomCode: string;
  bomDescription: string;
  bomQuantity: number;
  bomMeasures: string[];
  formatCode: string;
  formatDescription: string;
  provisionedItems: StockResponse[];
  lines: ConsumptionLine[];
}

const bomGroups = ref<BomGroup[]>([]);

// ----- Format-driven field visibility -----
// Only editable cutting axes are shown; diameter/thickness are invariant
function showField(
  formatCode: string,
  field: "width" | "length" | "height",
): boolean {
  switch (formatCode) {
    case "UNITATS":
      return false;
    case "RODO":
    case "TUB":
      return field === "length";
    case "PLACA":
      return field === "width" || field === "height" || field === "length";
    default:
      return true;
  }
}

// ----- Measures display -----
function getBomMeasures(bom: BillOfMaterialsItem): string[] {
  return [
    { label: "Ample", value: bom.width },
    { label: "Llarg", value: bom.length },
    { label: "Alt", value: bom.height },
    { label: "Diam.", value: bom.diameter },
    { label: "Gruix", value: bom.thickness },
  ]
    .filter((m) => m.value > 0)
    .map((m) => `${m.label} ${m.value}`);
}

function getStockMeasures(stock: StockResponse): string[] {
  return [
    { label: "Ample", value: stock.width },
    { label: "Llarg", value: stock.length },
    { label: "Alt", value: stock.height },
    { label: "Diam.", value: stock.diameter },
    { label: "Gruix", value: stock.thickness },
  ]
    .filter((m) => m.value > 0)
    .map((m) => `${m.label} ${m.value}`);
}

// ----- Computed consumption helpers -----
function getConsumedQuantity(line: ConsumptionLine): number {
  return line.availableQuantity - line.remainingQuantity;
}

function getConsumedDimensions(
  line: ConsumptionLine,
  formatCode: string,
): string[] {
  const dims: string[] = [];

  if (formatCode === "RODO" || formatCode === "TUB") {
    const consumedLength = line.originalLength - line.remainingLength;
    if (consumedLength > 0) dims.push(`${consumedLength} llarg`);
  } else if (formatCode === "PLACA") {
    const consumedWidth = line.originalWidth - line.remainingWidth;
    const consumedLength = line.originalLength - line.remainingLength;
    const consumedHeight = line.originalHeight - line.remainingHeight;
    if (consumedWidth > 0) dims.push(`${consumedWidth} ample`);
    if (consumedLength > 0) dims.push(`${consumedLength} llarg`);
    if (consumedHeight > 0) dims.push(`${consumedHeight} alt`);
  }

  return dims;
}

// ----- Load provisioned stock when dialog opens -----
let lineIdCounter = 0;
function nextLineId(): string {
  return `line-${++lineIdCounter}`;
}

watch(
  () => props.visible,
  async (newValue) => {
    if (newValue) {
      await loadProvisionedStock();
    }
  },
);

async function loadProvisionedStock() {
  loading.value = true;
  lineIdCounter = 0;
  bomGroups.value = [];

  try {
    const associatedLocationIds = workcenterStore.associatedLocationIds;
    const groups: BomGroup[] = [];

    for (const bom of props.billOfMaterials) {
      const allStock = await WarehouseServices.Stock.getByBillOfMaterialsId(
        bom.id,
      );

      // Filter to only provisioned stock (at workcenter supply locations)
      const provisioned = allStock.filter((s) =>
        associatedLocationIds.includes(s.locationId),
      );

      if (provisioned.length === 0) continue;

      const formatCode = provisioned[0]?.referenceFormatCode ?? "";
      const formatDescription =
        provisioned[0]?.referenceFormatDescription ?? "";

      const lines: ConsumptionLine[] = provisioned.map((stock) => ({
        lineId: nextLineId(),
        stockId: stock.stockId,
        locationName: stock.locationName,
        originalMeasures: getStockMeasures(stock),
        availableQuantity: stock.quantity,
        // Store original dimensions
        originalWidth: stock.width,
        originalLength: stock.length,
        originalHeight: stock.height,
        originalDiameter: stock.diameter,
        originalThickness: stock.thickness,
        // Default remaining = 0 (assume full consumption)
        remainingQuantity: 0,
        remainingWidth: 0,
        remainingLength: 0,
        remainingHeight: 0,
      }));

      groups.push({
        bomId: bom.id,
        bomCode: bom.referenceCode,
        bomDescription: bom.referenceDescription,
        bomQuantity: bom.quantity,
        bomMeasures: getBomMeasures(bom),
        formatCode,
        formatDescription,
        provisionedItems: provisioned,
        lines,
      });
    }

    bomGroups.value = groups;
  } catch (error) {
    console.error("Error loading provisioned stock:", error);
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "No s'ha pogut carregar l'estoc aprovisionat",
      life: 5000,
    });
  } finally {
    loading.value = false;
  }
}

// ----- Validation -----
const hasAnyConsumption = computed(() => {
  return bomGroups.value.some((group) =>
    group.lines.some((line) => getConsumedQuantity(line) > 0),
  );
});

function validate(): boolean {
  for (const group of bomGroups.value) {
    for (const line of group.lines) {
      if (line.remainingQuantity > line.availableQuantity) {
        toast.add({
          severity: "warn",
          summary: "Quantitat invàlida",
          detail: `La quantitat restant de ${group.bomCode} supera la disponible (${line.availableQuantity})`,
          life: 5000,
        });
        return false;
      }

      if (line.remainingLength > line.originalLength) {
        toast.add({
          severity: "warn",
          summary: "Mesura invàlida",
          detail: `La llargada restant de ${group.bomCode} supera l'original (${line.originalLength})`,
          life: 5000,
        });
        return false;
      }

      if (line.remainingWidth > line.originalWidth) {
        toast.add({
          severity: "warn",
          summary: "Mesura invàlida",
          detail: `L'ample restant de ${group.bomCode} supera l'original (${line.originalWidth})`,
          life: 5000,
        });
        return false;
      }

      if (line.remainingHeight > line.originalHeight) {
        toast.add({
          severity: "warn",
          summary: "Mesura invàlida",
          detail: `L'alçada restant de ${group.bomCode} supera l'original (${line.originalHeight})`,
          life: 5000,
        });
        return false;
      }
    }
  }

  return true;
}

// ----- Actions -----
function onCancel() {
  emit("update:visible", false);
}

function onConfirm() {
  if (!validate()) return;

  const items: ConsumeStockItem[] = [];

  for (const group of bomGroups.value) {
    for (const line of group.lines) {
      const consumedQty = getConsumedQuantity(line);
      if (consumedQty <= 0) continue;

      // Calculate consumed dimensions = original - remaining
      const consumedLength = line.originalLength - line.remainingLength;
      const consumedWidth = line.originalWidth - line.remainingWidth;
      const consumedHeight = line.originalHeight - line.remainingHeight;

      items.push({
        stockId: line.stockId,
        quantity: consumedQty,
        width: consumedWidth > 0 ? consumedWidth : line.originalWidth,
        length: consumedLength > 0 ? consumedLength : line.originalLength,
        height: consumedHeight > 0 ? consumedHeight : line.originalHeight,
        diameter: line.originalDiameter,
        thickness: line.originalThickness,
      });
    }
  }

  emit("confirm", items);
}

// Expose submitting for parent control
defineExpose({ submitting });
</script>

<style scoped>
.consumption-dialog {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.consumption-dialog-header {
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--surface-border);
}

.consumption-dialog-caption {
  font-size: 0.9rem;
  color: var(--text-color-secondary);
}

.consumption-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 2rem;
  color: var(--text-color-secondary);
}

.consumption-bom-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.consumption-bom-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  padding: 0.6rem 0.75rem;
  border-radius: 6px;
  background: var(--p-green-50);
  color: var(--p-green-700);
  border: 1px solid var(--p-green-200);
}

.consumption-bom-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.consumption-bom-qty {
  font-size: 0.85rem;
  background: var(--p-green-100);
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.consumption-bom-measures {
  display: flex;
  flex-wrap: wrap;
  gap: 0.3rem;
}

.consumption-bom-measure-chip {
  font-size: 0.8rem;
  background: var(--p-green-100);
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.consumption-bom-format {
  font-size: 0.82rem;
  font-weight: 600;
}

.consumption-table {
  width: 100%;
}

.consumption-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  padding-top: 0.75rem;
  border-top: 1px solid var(--surface-border);
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

:deep(.dimension-input) {
  width: 100% !important;
  text-align: right;
}

.computed-consumption {
  display: flex;
  align-items: center;
}

.consumed-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.82rem;
  font-weight: 600;
  background: var(--p-orange-50);
  color: var(--p-orange-700);
  border: 1px solid var(--p-orange-200);
}

.no-consumption-badge {
  font-size: 0.82rem;
  color: var(--text-color-secondary);
  font-style: italic;
}
</style>

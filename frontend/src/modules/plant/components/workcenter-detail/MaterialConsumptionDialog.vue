<template>
  <Dialog
    v-model:visible="dialogVisible"
    :modal="true"
    :draggable="false"
    :closable="!submitting"
    :style="{ width: '95vw', maxWidth: '1200px' }"
    :header='$t("plant.consum-de-materials")'
  >
    <div class="consumption-dialog">
      <div class="consumption-dialog-header">
        <span class="consumption-dialog-caption">{{ $t("plant.per-defecte-tot-el-material-aprovisionat-sera-consumit-si-queda-material-sobrant-prem-afegir-peca-per-declarar-lo") }}</span>
      </div>

      <div v-if="loading" class="consumption-loading">
        <i class="pi pi-spin pi-spinner" style="font-size: 1.5rem"></i>
        <span>{{ $t("plant.carregant-estoc-aprovisionat") }}</span>
      </div>

      <template v-else>
        <div
          v-for="group in bomGroups"
          :key="group.bomId"
          class="consumption-bom-group"
        >
          <!-- BOM item header -->
          <BomMaterialHeader
            :reference-code="group.bomCode"
            :reference-description="group.bomDescription"
            :quantity="group.bomQuantity"
            :width="group.bomWidth"
            :length="group.bomLength"
            :height="group.bomHeight"
            :diameter="group.bomDiameter"
            :thickness="group.bomThickness"
            :format-description="group.formatDescription"
          />

          <!-- Provisioned stock items -->
          <div
            v-for="line in group.lines"
            :key="line.lineId"
            class="stock-card"
          >
            <!-- Stock card header -->
            <div class="stock-card-header">
              <div class="stock-card-info">
                <span class="stock-card-label">{{ $t("plant.estoc-aprovisionat-2") }}</span>
                <span class="stock-card-qty"
                  >{{ line.availableQuantity }} ut.</span
                >
                <div
                  v-if="line.originalMeasures.length > 0"
                  class="stock-measures"
                >
                  <span
                    v-for="measure in line.originalMeasures"
                    :key="measure"
                    class="stock-measure-chip"
                  >
                    {{ measure }}
                  </span>
                </div>
              </div>
              <div class="stock-card-summary">
                <span
                  v-if="line.pieces.length === 0"
                  class="consumed-badge consumed-badge--full"
                >
                  Consum total: {{ line.availableQuantity }} ut.
                </span>
                <span
                  v-else-if="isFragmentation(line)"
                  class="consumed-badge consumed-badge--fragment"
                >
                  Fragmentació: {{ line.pieces.length }} peces
                </span>
                <span
                  v-else-if="getConsumedQuantity(line) > 0"
                  class="consumed-badge"
                >
                  Consum: {{ getConsumedQuantity(line) }} ut. · Retorn:
                  {{ getTotalRemaining(line) }} ut.
                </span>
                <span
                  v-else-if="hasDimensionalChange(line)"
                  class="consumed-badge"
                >{{ $t("plant.retall-dimensional") }}</span>
                <span v-else class="consumed-badge consumed-badge--full">
                  Consum total: {{ line.availableQuantity }} ut.
                </span>
              </div>
            </div>

            <!-- Remaining pieces section -->
            <div class="remaining-pieces">
              <!-- UNITATS: simple remaining quantity -->
              <template v-if="group.formatCode === 'UNITATS'">
                <div
                  v-if="line.pieces.length > 0"
                  class="piece-row piece-row-simple"
                >
                  <div class="piece-field">
                    <label class="piece-field-label">{{ $t("plant.quantitat-a-retornar") }}</label>
                    <InputNumber
                      v-model="line.pieces[0].quantity"
                      :min="0"
                      :max="line.availableQuantity"
                      :disabled="submitting"
                      showButtons
                      buttonLayout="horizontal"
                      incrementButtonIcon="pi pi-plus"
                      decrementButtonIcon="pi pi-minus"
                      inputClass="stock-qty-input"
                      class="stock-qty-spinner"
                    />
                  </div>
                  <div class="piece-actions">
                    <Button
                      icon="pi pi-trash"
                      severity="danger"
                      text
                      rounded
                      size="small"
                      :disabled="submitting"
                      @click="removeAllPieces(line)"
                      v-tooltip.top="'Consumir tot'"
                    />
                  </div>
                </div>
                <div v-else class="remaining-pieces-empty">
                  <Button
                    icon="pi pi-plus"
                    :label='$t("plant.declarar-sobrant")'
                    severity="secondary"
                    size="small"
                    outlined
                    :disabled="submitting"
                    @click="addUnitatsPiece(line)"
                  />
                </div>
              </template>

              <!-- Dimensional formats: rows per remaining piece -->
              <template v-else>
                <div
                  v-for="(piece, pieceIdx) in line.pieces"
                  :key="pieceIdx"
                  class="piece-row"
                >
                  <div class="piece-field piece-field-qty">
                    <label class="piece-field-label">{{ $t("plant.quantitat") }}</label>
                    <InputNumber
                      v-model="piece.quantity"
                      :min="1"
                      :max="line.availableQuantity"
                      :disabled="submitting"
                      showButtons
                      buttonLayout="horizontal"
                      incrementButtonIcon="pi pi-plus"
                      decrementButtonIcon="pi pi-minus"
                      inputClass="stock-qty-input"
                      class="stock-qty-spinner"
                    />
                  </div>

                  <div
                    v-if="showField(group.formatCode, 'width')"
                    class="piece-field"
                  >
                    <label class="piece-field-label">{{ $t("plant.ample-mm") }}</label>
                    <InputNumber
                      v-model="piece.width"
                      :min="0"
                      :max="line.originalWidth"
                      :minFractionDigits="0"
                      :maxFractionDigits="2"
                      :disabled="submitting"
                      inputClass="dimension-input"
                      class="w-full"
                    />
                  </div>

                  <div
                    v-if="showField(group.formatCode, 'length')"
                    class="piece-field"
                  >
                    <label class="piece-field-label">{{ $t("plant.llargada-mm") }}</label>
                    <InputNumber
                      v-model="piece.length"
                      :min="0"
                      :max="line.originalLength"
                      :minFractionDigits="0"
                      :maxFractionDigits="2"
                      :disabled="submitting"
                      inputClass="dimension-input"
                      class="w-full"
                    />
                  </div>

                  <div
                    v-if="showField(group.formatCode, 'height')"
                    class="piece-field"
                  >
                    <label class="piece-field-label">{{ $t("plant.alcada-mm") }}</label>
                    <InputNumber
                      v-model="piece.height"
                      :min="0"
                      :max="line.originalHeight"
                      :minFractionDigits="0"
                      :maxFractionDigits="2"
                      :disabled="submitting"
                      inputClass="dimension-input"
                      class="w-full"
                    />
                  </div>

                  <div
                    v-if="showField(group.formatCode, 'diameter')"
                    class="piece-field"
                  >
                    <label class="piece-field-label">{{ $t("plant.diametre-mm") }}</label>
                    <InputNumber
                      v-model="piece.diameter"
                      :min="0"
                      :max="line.originalDiameter"
                      :minFractionDigits="0"
                      :maxFractionDigits="2"
                      :disabled="submitting"
                      inputClass="dimension-input"
                      class="w-full"
                    />
                  </div>

                  <div
                    v-if="showField(group.formatCode, 'thickness')"
                    class="piece-field"
                  >
                    <label class="piece-field-label">{{ $t("plant.gruix-mm") }}</label>
                    <InputNumber
                      v-model="piece.thickness"
                      :min="0"
                      :max="line.originalThickness"
                      :minFractionDigits="0"
                      :maxFractionDigits="2"
                      :disabled="submitting"
                      inputClass="dimension-input"
                      class="w-full"
                    />
                  </div>

                  <div class="piece-actions">
                    <Button
                      icon="pi pi-trash"
                      severity="danger"
                      text
                      rounded
                      size="small"
                      :disabled="submitting"
                      @click="removePiece(line, pieceIdx)"
                    />
                  </div>
                </div>

                <!-- Add piece button -->
                <div class="remaining-pieces-add">
                  <Button
                    icon="pi pi-plus"
                    :label='$t("plant.afegir-peca")'
                    severity="secondary"
                    size="small"
                    outlined
                    :disabled="submitting"
                    @click="addPiece(line, group.formatCode)"
                  />
                </div>
              </template>

              <!-- Validation warning (UNITATS only: cannot return more than provisioned) -->
              <div
                v-if="
                  group.formatCode === 'UNITATS' &&
                  getTotalRemaining(line) > line.availableQuantity
                "
                class="piece-warning"
              >
                <i class="pi pi-exclamation-triangle"></i>
                La quantitat a retornar supera l'estoc aprovisionat ({{
                  line.availableQuantity
                }}
                ut.)
              </div>
            </div>
          </div>
        </div>
      </template>

      <!-- Action Buttons -->
      <div class="consumption-actions">
        <Button
          icon="pi pi-times"
          :label='$t("plant.cancel-lar")'
          severity="secondary"
          :disabled="submitting"
          @click="onCancel"
        />
        <Button
          icon="pi pi-check"
          :label='$t("plant.confirmar-consum")'
          severity="success"
          :disabled="submitting || !isValid"
          :loading="submitting"
          @click="onConfirm"
        />
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputNumber from "primevue/inputnumber";
import { useToast } from "primevue/usetoast";
import type { BillOfMaterialsItem } from "../../../production/types";
import type {
  StockResponse,
  ConsumeStockEntry,
} from "../../../warehouse/types";
import WarehouseServices from "../../../warehouse/services";
import { usePlantWorkcenterStore } from "../../store";
import { formatDimensions } from "@/utils/functions";
import BomMaterialHeader from "./BomMaterialHeader.vue";

interface Props {
  visible: boolean;
  billOfMaterials: BillOfMaterialsItem[];
  workcenterId: string;
  workOrderPhaseId: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "confirm", entries: ConsumeStockEntry[]): void;
}>();

const toast = useToast();
const { t } = useI18n();
const workcenterStore = usePlantWorkcenterStore();

const loading = ref(false);
const submitting = ref(false);

const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit("update:visible", value),
});

// ----- Data model -----
interface PieceInput {
  quantity: number;
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
}

interface ConsumptionLine {
  lineId: string;
  stockId: string;
  originalMeasures: string[];
  availableQuantity: number;
  originalWidth: number;
  originalLength: number;
  originalHeight: number;
  originalDiameter: number;
  originalThickness: number;
  /** Remaining pieces to return. Empty = full consumption (default). */
  pieces: PieceInput[];
}

interface BomGroup {
  bomId: string;
  bomCode: string;
  bomDescription: string;
  bomQuantity: number;
  bomWidth: number;
  bomLength: number;
  bomHeight: number;
  bomDiameter: number;
  bomThickness: number;
  formatCode: string;
  formatDescription: string;
  provisionedItems: StockResponse[];
  lines: ConsumptionLine[];
}

const bomGroups = ref<BomGroup[]>([]);

// ----- Format-driven field visibility -----
function showField(
  formatCode: string,
  field: "width" | "length" | "height" | "diameter" | "thickness",
): boolean {
  switch (formatCode) {
    case "UNITATS":
      return false;
    case "RODO":
      return field === "diameter" || field === "length";
    case "TUB":
      return field === "diameter" || field === "length" || field === "thickness";
    case "PLACA":
      return field === "width" || field === "height" || field === "length";
    default:
      return true;
  }
}

// ----- Measures display -----
function getStockMeasures(stock: StockResponse): string[] {
  return formatDimensions(t, {
    width: stock.width,
    length: stock.length,
    height: stock.height,
    diameter: stock.diameter,
    thickness: stock.thickness,
  });
}

// ----- Piece helpers -----
function addPiece(line: ConsumptionLine, _formatCode: string) {
  // Fragmentation allowed: a physical piece can break into multiple fragments,
  // each with dimensions <= original. No quantity-sum guard needed.
  line.pieces.push({
    quantity: 1,
    width: line.originalWidth,
    length: line.originalLength,
    height: line.originalHeight,
    diameter: line.originalDiameter,
    thickness: line.originalThickness,
  });
}

function addUnitatsPiece(line: ConsumptionLine) {
  line.pieces.push({
    quantity: 1,
    width: 0,
    length: 0,
    height: 0,
    diameter: 0,
    thickness: 0,
  });
}

function removePiece(line: ConsumptionLine, pieceIdx: number) {
  line.pieces.splice(pieceIdx, 1);
}

function removeAllPieces(line: ConsumptionLine) {
  line.pieces.splice(0, line.pieces.length);
}

// ----- Quantity helpers -----
function getTotalRemaining(line: ConsumptionLine): number {
  return line.pieces.reduce((sum, p) => sum + p.quantity, 0);
}

function getConsumedQuantity(line: ConsumptionLine): number {
  return line.availableQuantity - getTotalRemaining(line);
}

/** True when any remaining piece has dimensions different from the original stock */
function hasDimensionalChange(line: ConsumptionLine): boolean {
  return line.pieces.some(
    (p) =>
      p.quantity > 0 &&
      (p.width !== line.originalWidth ||
        p.length !== line.originalLength ||
        p.height !== line.originalHeight ||
        p.diameter !== line.originalDiameter ||
        p.thickness !== line.originalThickness),
  );
}

/** True when returning more pieces than originally provisioned (piece split/break) */
function isFragmentation(line: ConsumptionLine): boolean {
  return getTotalRemaining(line) > line.availableQuantity;
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
        originalMeasures: getStockMeasures(stock),
        availableQuantity: stock.quantity,
        originalWidth: stock.width,
        originalLength: stock.length,
        originalHeight: stock.height,
        originalDiameter: stock.diameter,
        originalThickness: stock.thickness,
        // Empty pieces = full consumption by default
        pieces: [],
      }));

      groups.push({
        bomId: bom.id,
        bomCode: bom.referenceCode,
        bomDescription: bom.referenceDescription,
        bomQuantity: bom.quantity,
        bomWidth: bom.width,
        bomLength: bom.length,
        bomHeight: bom.height,
        bomDiameter: bom.diameter,
        bomThickness: bom.thickness,
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
      summary: t("plant.error"),
      detail: t("plant.messages.provisionedStockLoadError"),
      life: 5000,
    });
  } finally {
    loading.value = false;
  }
}

// ----- Validation -----
const isValid = computed(() => {
  // Must have at least one group loaded
  if (bomGroups.value.length === 0) return false;

  for (const group of bomGroups.value) {
    for (const line of group.lines) {
      // UNITATS: cannot return more than provisioned (no fragmentation concept)
      if (group.formatCode === "UNITATS") {
        if (getTotalRemaining(line) > line.availableQuantity) return false;
      }

      // Validate each piece individually
      for (const piece of line.pieces) {
        if (piece.quantity <= 0) return false;
        // Dimensions cannot exceed originals
        if (piece.width > line.originalWidth) return false;
        if (piece.length > line.originalLength) return false;
        if (piece.height > line.originalHeight) return false;
        if (piece.diameter > line.originalDiameter) return false;
        if (piece.thickness > line.originalThickness) return false;
      }
    }
  }

  return true;
});

// ----- Actions -----
function onCancel() {
  emit("update:visible", false);
}

function onConfirm() {
  if (!isValid.value) return;

  const entries: ConsumeStockEntry[] = [];

  for (const group of bomGroups.value) {
    for (const line of group.lines) {
      const remainingPieces = line.pieces
        .filter((p) => p.quantity > 0)
        .map((p) => ({
          quantity: p.quantity,
          width: p.width,
          length: p.length,
          height: p.height,
          diameter: p.diameter,
          thickness: p.thickness,
        }));

      entries.push({
        stockId: line.stockId,
        remainingPieces,
      });
    }
  }

  emit("confirm", entries);
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

/* Stock card */
.stock-card {
  border: 1px solid var(--surface-border);
  border-radius: 8px;
  overflow: hidden;
  margin-left: 0.5rem;
}

.stock-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  padding: 0.6rem 0.75rem;
  background: var(--surface-50);
  border-bottom: 1px solid var(--surface-border);
}

.stock-card-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.stock-card-label {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-color);
}

.stock-card-qty {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--p-primary-color);
}

.stock-card-summary {
  flex-shrink: 0;
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
  background: var(--surface-0);
}

/* Remaining pieces section */
.remaining-pieces {
  padding: 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.remaining-pieces-empty {
  display: flex;
  align-items: center;
}

.remaining-pieces-add {
  display: flex;
  align-items: center;
}

/* Piece row */
.piece-row {
  display: flex;
  align-items: flex-end;
  gap: 0.5rem;
  padding: 0.5rem;
  border: 1px solid var(--surface-border);
  border-radius: 6px;
  background: var(--surface-0);
}

.piece-row-simple {
  border: none;
  padding: 0.25rem 0;
}

.piece-field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 0;
}

.piece-field-qty {
  flex: 0 0 auto;
}

.piece-field-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-color-secondary);
  white-space: nowrap;
}

.piece-field:not(.piece-field-qty) {
  flex: 1;
}

.piece-actions {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  padding-bottom: 0.1rem;
}

.piece-warning {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.4rem 0.6rem;
  font-size: 0.82rem;
  color: var(--p-red-700);
  background: var(--p-red-50);
  border: 1px solid var(--p-red-200);
  border-radius: 6px;
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

.consumed-badge--full {
  background: var(--p-green-50);
  color: var(--p-green-700);
  border: 1px solid var(--p-green-200);
}

.consumed-badge--fragment {
  background: var(--p-blue-50);
  color: var(--p-blue-700);
  border: 1px solid var(--p-blue-200);
}

.consumption-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  padding-top: 0.75rem;
  border-top: 1px solid var(--surface-border);
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
</style>

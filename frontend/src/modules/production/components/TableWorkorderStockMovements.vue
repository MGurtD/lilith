<template>
  <DataTable
    class="p-datatable-sm"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortField="movementDate"
    :sortOrder="1"
    :value="stockMovements"
    :paginator="(stockMovements?.length ?? 0) > 20"
    :rows="20"
    stripedRows
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Moviments de stock</span>
      </div>
    </template>
    <Column header="Data" field="movementDate" sortable style="width: 10%">
      <template #body="slotProps">
        {{ formatDateTime(slotProps.data.movementDate) }}
      </template>
    </Column>
    <Column header="Referència" style="width: 14%">
      <template #body="slotProps">
        {{
          slotProps.data.reference
            ? `${slotProps.data.reference.code} ${slotProps.data.reference.description}`
            : referenceStore.getFullNameById(slotProps.data.referenceId)
        }}
      </template>
    </Column>
    <Column header="Ubicació" style="width: 10%">
      <template #body="slotProps">
        {{ slotProps.data.location?.name }}
      </template>
    </Column>
    <Column header="Dimensions" style="width: 24%">
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
    <Column header="Tipus" field="movementType" style="width: 10%">
      <template #body="slotProps">
        <TagMovementType :movementType="slotProps.data.movementType" />
      </template>
    </Column>
    <Column field="quantity" header="Quantitat" style="width: 8%"></Column>
    <Column field="description" header="Descripció" style="width: 24%"></Column>
  </DataTable>
</template>

<script setup lang="ts">
import TagMovementType from "../../../components/TagMovementType.vue";
import DimensionChips from "../../plant/components/workcenter-detail/DimensionChips.vue";
import { useReferenceStore } from "../../shared/store/reference";
import { formatDateTime } from "../../../utils/functions";
import { StockMovement } from "../../warehouse/types";

defineProps<{
  stockMovements: Array<StockMovement>;
}>();

const referenceStore = useReferenceStore();
</script>

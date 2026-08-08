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
        <span class="text-900 font-bold">{{ t("production.components.movimentsDeStock") }}</span>
      </div>
    </template>
    <Column :header="t('production.components.data')" field="movementDate" sortable style="width: 10%">
      <template #body="slotProps">
        {{ formatDateTime(slotProps.data.movementDate) }}
      </template>
    </Column>
    <Column :header="t('production.components.referencia')" style="width: 14%">
      <template #body="slotProps">
        {{
          slotProps.data.reference
            ? `${slotProps.data.reference.code} ${slotProps.data.reference.description}`
            : referenceStore.getFullNameById(slotProps.data.referenceId)
        }}
      </template>
    </Column>
    <Column :header="t('production.components.ubicacio')" style="width: 10%">
      <template #body="slotProps">
        {{ slotProps.data.location?.name }}
      </template>
    </Column>
    <Column :header="t('production.components.dimensions')" style="width: 24%">
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
    <Column :header="t('production.components.tipus')" field="movementType" style="width: 10%">
      <template #body="slotProps">
        <TagMovementType :movementType="slotProps.data.movementType" />
      </template>
    </Column>
    <Column field="quantity" :header="t('production.components.quantitat')" style="width: 8%"></Column>
    <Column field="description" :header="t('production.components.descripcio')" style="width: 24%"></Column>
  </DataTable>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
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

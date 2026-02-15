<template>
  <DataTable
    :value="productionPartStore.productionParts"
    class="p-datatable-sm"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    stripedRows
    :rowHover="true"
  >
    <template #header>
      <slot name="header"></slot>
    </template>
    <template #empty> No s'han trobat tiquets. </template>
    <template #loading> Carregant tiquets. Si us plau espera. </template>
    <Column field="operatorId" header="Operari" style="width: 20%">
      <template #body="slotProps">
        {{ plantModelStore.getOperatorNameById(slotProps.data.operatorId) }}
      </template>
    </Column>
    <Column field="workcenterId" header="Màquina" style="width: 20%">
      <template #body="slotProps">
        {{ plantModelStore.getWorkcenterNameById(slotProps.data.workcenterId) }}
      </template>
    </Column>
    <Column field="workOrderPhaseId" header="Fase/Estat" style="width: 25%">
      <template #body="slotProps">
        {{ getWorkOrderPhaseName(slotProps.data) }}
      </template>
    </Column>
    <Column field="date" header="Data" style="width: 10%">
      <template #body="slotProps">
        {{ formatDateTime(slotProps.data.date) }}
      </template>
    </Column>
    <Column field="quantity" header="Quantitat" style="width: 7.5%"></Column>
    <Column field="workcenterTime" header="Temps màquina" style="width: 10%">
      <template #body="slotProps">
        {{ slotProps.data.workcenterTime }} min.
      </template>
    </Column>
    <Column field="operatorTime" header="Temps operari" style="width: 10%">
      <template #body="slotProps">
        {{ slotProps.data.operatorTime }} min.
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>

<script setup lang="ts">
import { usePlantModelStore } from "../store/plantmodel";
import { useProductionPartStore } from "../store/productionpart";
import { formatDateTime } from "../../../utils/functions";
import { ProductionPart } from "../types";
import { PrimeIcons } from "@primevue/core/api";

const productionPartStore = useProductionPartStore();
const plantModelStore = usePlantModelStore();

const getWorkOrderPhaseName = (productionPart: ProductionPart) => {
  if (!productionPart) return "";

  if (productionPart.workOrderPhase && productionPart.workOrderPhaseDetail) {
    const statusDesc = plantModelStore.getMachineStatusNameById(
      productionPart.workOrderPhaseDetail.machineStatusId,
    );

    return `(${productionPart.workOrderPhase.code}) ${productionPart.workOrderPhase.description} - ${statusDesc}`;
  }
};

const emits = defineEmits(["delete"]);

const onDeleteRow = (event: Event, productionPart: ProductionPart) => {
  event.stopPropagation();
  emits("delete", productionPart);
};
</script>

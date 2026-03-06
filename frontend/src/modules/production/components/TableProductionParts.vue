<template>
  <DataTable
    :value="productionPartStore.productionParts"
    class="p-datatable-sm"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    stripedRows
    :rowHover="true"
    sortField="date"
    :sortOrder="-1"
    v-model:filters="filters"
    filterDisplay="row"
    :globalFilterFields="['operatorId', 'workcenterId', 'date']"
  >
    <template #header>
      <slot name="header"></slot>
    </template>
    <template #empty> No s'han trobat tiquets. </template>
    <template #loading> Carregant tiquets. Si us plau espera. </template>

    <Column field="operatorId" header="Operari" style="width: 20%" :showFilterMenu="false">
      <template #body="slotProps">
        {{ slotProps.data.operatorId
          ? plantModelStore.getOperatorNameById(slotProps.data.operatorId)
          : "—" }}
      </template>
      <template #filter="{ filterModel, filterCallback }">
        <Select
          v-model="filterModel.value"
          :options="operatorOptions"
          optionLabel="label"
          optionValue="value"
          placeholder="Tots"
          showClear
          @change="filterCallback()"
          class="w-full"
          size="small"
        />
      </template>
    </Column>

    <Column field="workcenterId" header="Màquina" style="width: 20%" :showFilterMenu="false">
      <template #body="slotProps">
        {{ plantModelStore.getWorkcenterNameById(slotProps.data.workcenterId) }}
      </template>
      <template #filter="{ filterModel, filterCallback }">
        <Select
          v-model="filterModel.value"
          :options="workcenterOptions"
          optionLabel="label"
          optionValue="value"
          placeholder="Totes"
          showClear
          @change="filterCallback()"
          class="w-full"
          size="small"
        />
      </template>
    </Column>

    <Column field="workOrderPhaseId" header="Fase/Estat" style="width: 25%" :showFilterMenu="false">
      <template #body="slotProps">
        {{ getWorkOrderPhaseName(slotProps.data) }}
      </template>
      <template #filter="{ filterModel, filterCallback }">
        <InputText
          v-model="filterModel.value"
          @input="filterCallback()"
          placeholder="Cercar fase..."
          class="w-full"
          size="small"
        />
      </template>
    </Column>

    <Column field="date" header="Data" style="width: 10%" sortable :showFilterMenu="false">
      <template #body="slotProps">
        {{ formatDateTime(slotProps.data.date) }}
      </template>
      <template #filter="{ filterModel, filterCallback }">
        <InputText
          v-model="filterModel.value"
          @input="filterCallback()"
          placeholder="dd/mm/aa..."
          class="w-full"
          size="small"
        />
      </template>
    </Column>

    <Column field="quantity" header="Quantitat" style="width: 7.5%" sortable :showFilterMenu="false">
      <template #filter> <span /> </template>
    </Column>

    <Column field="workcenterTime" header="Temps màquina" style="width: 10%" sortable :showFilterMenu="false">
      <template #body="slotProps">
        {{ slotProps.data.workcenterTime }} min.
      </template>
      <template #filter> <span /> </template>
    </Column>

    <Column field="operatorTime" header="Temps operari" style="width: 10%" sortable :showFilterMenu="false">
      <template #body="slotProps">
        {{ slotProps.data.operatorTime }} min.
      </template>
      <template #filter> <span /> </template>
    </Column>

    <Column style="width: 5%" :showFilterMenu="false">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
      </template>
      <template #filter> <span /> </template>
    </Column>
  </DataTable>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { FilterMatchMode } from "@primevue/core/api";
import { usePlantModelStore } from "../store/plantmodel";
import { useProductionPartStore } from "../store/productionpart";
import { formatDateTime } from "../../../utils/functions";
import { ProductionPart } from "../types";
import { PrimeIcons } from "@primevue/core/api";

const productionPartStore = useProductionPartStore();
const plantModelStore = usePlantModelStore();

const filters = ref({
  operatorId: { value: null, matchMode: FilterMatchMode.EQUALS },
  workcenterId: { value: null, matchMode: FilterMatchMode.EQUALS },
  workOrderPhaseId: { value: null, matchMode: FilterMatchMode.CONTAINS },
  date: { value: null, matchMode: FilterMatchMode.CONTAINS },
});

const operatorOptions = computed(() => {
  if (!plantModelStore.operators) return [];
  return plantModelStore.operators.map((o) => ({
    label: `${o.name} ${o.surname}`,
    value: o.id,
  }));
});

const workcenterOptions = computed(() => {
  if (!plantModelStore.workcenters) return [];
  return plantModelStore.workcenters.map((w) => ({
    label: w.description,
    value: w.id,
  }));
});

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

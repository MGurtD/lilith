<template>
  <DataTable
    @row-click="onEditRow"
    class="p-datatable-sm clickable-rows"
    :value="details"
    tableStyle="min-width: 100%"
    sort-field="order"
    :sort-order="1"
    stripedRows
    :rowHover="true"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Pasos de fabricació</span>
        <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAdd" />
      </div>
    </template>

    <Column sortable field="order" header="Ordre" style="width: 10%"></Column>

    <Column header="Estat de màquina" style="width: 25%">
      <template #body="slotProps">
        {{ getMachineStatus(slotProps.data.machineStatusId) }}
      </template>
    </Column>

    <Column header="Temps de cicle" style="width: 25%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isCycleTime"></BooleanColumn>
      </template>
    </Column>
    <Column
      field="estimatedTime"
      header="Temps màquina (min)"
      style="width: 25%"
    ></Column>
    <Column
      field="estimatedOperatorTime"
      header="Temps operari (min)"
      style="width: 25%"
    ></Column>
    <Column style="width: 25%">
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
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { WorkMasterPhase, WorkMasterPhaseDetail } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { useConfirm } from "primevue/useconfirm";
import { usePlantModelStore } from "../store/plantmodel";
import BooleanColumn from "../../../components/tables/BooleanColumn.vue";
import { useWorkMasterStore } from "../store/workmaster";

const props = defineProps<{
  workmasterPhase: WorkMasterPhase;
  details: Array<WorkMasterPhaseDetail>;
}>();

const emit = defineEmits<{
  (e: "add", phase: WorkMasterPhaseDetail): void;
  (e: "edit", phase: WorkMasterPhaseDetail): void;
  (e: "delete", phase: WorkMasterPhaseDetail): void;
}>();

const confirm = useConfirm();
const workmasterStore = useWorkMasterStore();
const plantModelStore = usePlantModelStore();

const getMachineStatus = (id: string) => {
  if (!plantModelStore.machineStatuses) return "";
  const entity = plantModelStore.machineStatuses?.find((e) => id === e.id);
  if (!entity) return "";
  return entity.name;
};

const onAdd = () => {
  const defaultInstance = {
    id: getNewUuid(),
    workMasterPhaseId: props.workmasterPhase.id,
    estimatedTime: 0,
    estimatedOperatorTime: 0,
    isCycleTime: false,
    machineStatusId: "",
    comment: "",
    order: getNextOrderNumber(),
  } as WorkMasterPhaseDetail;
  emit("add", defaultInstance);
};

const getNextOrderNumber = () => {
  let defaultOrder = 10;
  if (props.details) {
    return (props.details.length + 1) * 10;
  }

  return defaultOrder;
};

const onEditRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, detail: WorkMasterPhaseDetail) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el pas de la fase?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", detail);
    },
  });
};
</script>

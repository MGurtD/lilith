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
        <span class="text-900 font-bold">Detalls de la plantilla</span>
        <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAdd" />
      </div>
    </template>

    <Column sortable field="order" header="Ordre" style="width: 15%"></Column>

    <Column header="Estat de màquina" style="width: 40%">
      <template #body="slotProps">
        {{ getMachineStatus(slotProps.data.machineStatusId) }}
      </template>
    </Column>

    <Column field="comment" header="Comentari" style="width: 35%"></Column>

    <Column style="width: 10%">
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
import { PhaseTemplate, PhaseTemplateDetail } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { useConfirm } from "primevue/useconfirm";
import { usePlantModelStore } from "../store/plantmodel";

const props = defineProps<{
  phaseTemplate: PhaseTemplate;
  details: Array<PhaseTemplateDetail>;
}>();

const emit = defineEmits<{
  (e: "add", detail: PhaseTemplateDetail): void;
  (e: "edit", detail: PhaseTemplateDetail): void;
  (e: "delete", detail: PhaseTemplateDetail): void;
}>();

const confirm = useConfirm();
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
    phaseTemplateId: props.phaseTemplate.id,
    machineStatusId: "",
    comment: "",
    order: getNextOrderNumber(),
  } as PhaseTemplateDetail;
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
      "grid_delete_column_button",
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, detail: PhaseTemplateDetail) => {
  confirm.require({
    target: event.currentTarget,
    message: `Està segur que vol eliminar el detall de la plantilla?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", detail);
    },
  });
};
</script>

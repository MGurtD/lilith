<template>
  <DataTable
    @row-click="onEditRow"
    :value="props.transitions"
    tableStyle="min-width: 100%"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Transicions</span>
        <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAdd" />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 25%"></Column>
    <Column header="Transició" style="width: 50%">
      <template #body="slotProps">
        {{ getTransitionDescription(slotProps.data) }}
      </template>
    </Column>
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
import { Status, StatusTransition } from "../types";
import { getNewUuid } from "../../../utils/functions";

const props = defineProps<{
  statuses: Array<Status> | undefined;
  transitions: Array<StatusTransition> | undefined;
}>();

const emit = defineEmits<{
  (e: "add", transition: StatusTransition): void;
  (e: "edit", transition: StatusTransition): void;
  (e: "delete", transition: StatusTransition): void;
}>();

const getTransitionDescription = (transition: StatusTransition) => {
  const status = props.statuses?.find((s) => s.id === transition.statusId);
  const statusTo = props.statuses?.find((s) => s.id === transition.statusToId);

  if (status && statusTo) return `${status.name} > ${statusTo.name}`;
  else return "";
};

const onAdd = () => {
  const defaultImport = {
    id: getNewUuid(),
    statusId: "",
    statusToId: "",
    name: "",
  } as StatusTransition;
  emit("add", defaultImport);
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

const onDeleteRow = (event: any, transition: StatusTransition) => {
  emit("delete", transition);
};
</script>

<template>
  <DataTable
    @row-click="onEditRow"
    :value="props.statuses"
    tableStyle="min-width: 100%"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Estats</span>
        <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAdd" />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 75%"></Column>
    <Column style="width: 25%">
      <template #body="slotProps">
        <i
          v-if="!hasTransitions(slotProps.data)"
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
  lifecycleId: string;
  statuses: Array<Status> | undefined;
  transitions: Array<StatusTransition> | undefined;
}>();

const emit = defineEmits<{
  (e: "add", status: Status): void;
  (e: "edit", status: Status): void;
  (e: "delete", status: Status): void;
}>();

const hasTransitions = (status: Status) => {
  let has = false;
  if (props.transitions) {
    has =
      props.transitions.filter(
        (t) => t.statusId === status.id || t.statusToId === status.id
      ).length > 0;
  }

  return has;
};

const onAdd = () => {
  const defaultImport = {
    id: getNewUuid(),
    lifecycleId: props.lifecycleId,
    name: "",
    disabled: false,
    transitions: [],
    color: "",
  } as Status;
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

const onDeleteRow = (event: any, status: Status) => {
  emit("delete", status);
};
</script>

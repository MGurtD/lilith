<template>
  <DataTable
    :value="lifecyclesStore.lifecycles"
    tableStyle="min-width: 100%"
    @row-click="edit"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Cicles de vida</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 25%"></Column>
    <Column field="description" header="Descripció" style="width: 25%"></Column>
    <Column header="Estat Inicial" style="width: 25%">
      <template #body="slotProps">
        {{ getInitialStatusName(slotProps.data) }}
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
      </template>
      ></Column
    >
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { PrimeIcons } from "@primevue/core/api";
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useStore } from "../../../store";
import { useLifecyclesStore } from "../store/lifecycle";
import { Lifecycle } from "../types";
import { useToast } from "primevue/usetoast";

const router = useRouter();
const store = useStore();
const lifecyclesStore = useLifecyclesStore();
const resource = "lifecycle";

onMounted(async () => {
  await lifecyclesStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.REFRESH,
    title: "Gestió de cicles de vida",
  });
});

// Generic function to get status name by ID
const getStatusNameById = (
  lifecycle: Lifecycle,
  statusId: string | undefined
) => {
  if (!statusId || statusId.length === 0) {
    return "";
  }

  const status = lifecycle.statuses.find((s) => s.id === statusId);
  return status?.name || "";
};

// Helper functions for better readability
const getInitialStatusName = (lifecycle: Lifecycle) =>
  getStatusNameById(lifecycle, lifecycle.initialStatusId);

const createButtonClick = () => {
  router.push({ path: `/${resource}/${uuidv4()}` });
};

const edit = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    router.push({ path: `/${resource}/${row.data.id}` });
  }
};

const toast = useToast();
const onDeleteRow = async (event: any, lifecycle: Lifecycle) => {
  await lifecyclesStore.fetchOne(lifecycle.id);

  if (
    !lifecyclesStore.lifecycle ||
    lifecyclesStore.lifecycle.statuses.length > 0 ||
    lifecyclesStore.transitions.length > 0
  ) {
    toast.add({
      summary: "Eliminar cicle de vida",
      detail: `El cicle de vida ${lifecyclesStore.lifecycle?.name} té dependencies`,
      severity: "warn",
      life: 5000,
    });
    return;
  }

  await lifecyclesStore.delete(lifecycle.id);
};
</script>

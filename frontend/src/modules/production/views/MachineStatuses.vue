<template>
  <DataTable
    :value="plantmodelStore.machineStatuses"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    @row-click="editRow"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Estats de màquina</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 25%"></Column>
    <Column field="description" header="Descripció" style="width: 40%"></Column>
    <Column header="Color" style="width: 5%">
      <template #body="slotProps">
        <ColorColumn :value="slotProps.data.color" />
      </template>
    </Column>
    <Column header="Icona" style="width: 5%">
      <template #body="slotProps">
        <IconColumn :value="slotProps.data.icon" />
      </template>
    </Column>
    <Column header="Aturada" style="width: 2%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.stopped" />
      </template>
    </Column>
    <Column header="Operaris" style="width: 2%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.operatorsAllowed" />
      </template>
    </Column>
    <Column header="Tancada" style="width: 2%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.closed" />
      </template>
    </Column>
    <Column header="Preferit" style="width: 2%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.preferred" />
      </template>
    </Column>
    <Column header="Permet OF" style="width: 2%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.workOrderAllowed" />
      </template>
    </Column>
    <Column header="Desactivat" style="width: 2%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteButton($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { usePlantModelStore } from "../store/plantmodel";
import { onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { MachineStatus } from "../types";
import ColorColumn from "../../../components/tables/ColorColumn.vue";
import IconColumn from "../../../components/tables/IconColumn.vue";

const router = useRouter();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const toast = useToast();
const confirm = useConfirm();

onMounted(async () => {
  await plantmodelStore.fetchMachineStatuses();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: "Gestió d'estats de màquina",
  });
});

const createButtonClick = () => {
  router.push({ path: `/machinestatus/${uuidv4()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/machinestatus/${row.data.id}` });
  }
};
const deleteButton = (event: any, machineStatus: MachineStatus) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar l'estat de màquina ${machineStatus.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteMachineStatus(
        machineStatus.id,
      );

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await plantmodelStore.fetchMachineStatuses();
      }
    },
  });
};
</script>

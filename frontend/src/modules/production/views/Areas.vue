<template>
  <DataTable
    :value="plantmodelStore.areas"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    @row-click="editRow"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Area</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 25%"></Column>
    <Column field="description" header="Descripció" style="width: 50%"></Column>
    <Column header="Visible planta" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isVisibleInPlant" />
      </template>
    </Column>
    <Column header="Desactivada" style="width: 10%">
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
import { usePlantModelStore } from "../store/plantmodel";
import { onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { Area } from "../types";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const plantmodelStore = usePlantModelStore();

onMounted(async () => {
  await plantmodelStore.fetchAreas();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: "Gestió d'arees",
  });
});

const createButtonClick = () => {
  router.push({ path: `/area/${uuidv4()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/area/${row.data.id}` });
  }
};
const deleteButton = (event: any, area: Area) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar l'area ${area.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteArea(area.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await plantmodelStore.fetchAreas();
      }
    },
  });
};
</script>

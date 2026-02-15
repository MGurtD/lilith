<template>
  <DataTable
    :value="plantmodelStore.workcenterTypes"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    @row-click="editRow"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Tipus de màquina</span>
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
    <Column field="profitPercentage" header="% Benefici" style="width: 10%">
      <template #body="slotProps">
        {{ slotProps.data.profitPercentage }} %
      </template>
    </Column>
    <Column header="Desactivat" style="width: 10%">
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
import { WorkcenterType } from "../types";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const plantmodelStore = usePlantModelStore();

onMounted(async () => {
  await plantmodelStore.fetchWorkcenterTypes();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: "Gestió de tipus de màquina",
  });
});

const createButtonClick = () => {
  router.push({ path: `/workcentertype/${uuidv4()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/workcentertype/${row.data.id}` });
  }
};
const deleteButton = (event: any, entity: WorkcenterType) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar l'empresa ${entity.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteWorkcenterType(entity.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await plantmodelStore.fetchWorkcenterTypes();
      }
    },
  });
};
</script>

<template>
  <DataTable
    :value="plantmodelStore.sites"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    @row-click="editSite"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Local</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 20%"></Column>
    <Column field="description" header="Descripció" style="width: 30%"></Column>
    <Column field="city" header="Població" style="width: 20%"></Column>
    <Column field="address" header="Adreça" style="width: 30%"></Column>
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
import { Site } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const plantmodelStore = usePlantModelStore();

onMounted(async () => {
  await plantmodelStore.fetchSites();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: "Gestió de locals",
  });
});

const createButtonClick = () => {
  router.push({ path: `/site/${uuidv4()}` });
};

const editSite = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/site/${row.data.id}` });
  }
};

const deleteButton = (event: any, entity: Site) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el site ${entity.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteSite(entity.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await plantmodelStore.fetchSites();
      }
    },
  });
};
</script>

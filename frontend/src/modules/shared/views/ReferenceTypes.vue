<template>
  <DataTable
    :value="referenceTypeStore.referenceTypes"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sort-mode="single"
    sort-field="name"
    :sort-order="1"
    @row-click="editRow"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Tipus de matèries primes</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" sortable header="Nom" style="width: 25%"></Column>
    <Column
      field="description"
      sortable
      header="Descripció"
      style="width: 40%"
    ></Column>
    <Column
      field="density"
      header="Densitat (cm/m3)"
      style="width: 15%"
    ></Column>
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
import { onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { DataTableRowClickEvent } from "primevue/datatable";
import { ReferenceType } from "../types";
import { useReferenceTypeStore } from "../store/referenceType";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const referenceTypeStore = useReferenceTypeStore();

onMounted(async () => {
  await referenceTypeStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.BOX,
    title: "Gestió de tipus de materials",
  });
});

const createButtonClick = () => {
  router.push({ path: `/referencetype/${uuidv4()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/referencetype/${row.data.id}` });
  }
};

const deleteButton = (event: any, rawmaterialtype: ReferenceType) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el tipus de materials ${rawmaterialtype.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await referenceTypeStore.deleteReferenceType(
        rawmaterialtype.id,
      );

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await referenceTypeStore.fetchAll();
      }
    },
  });
};
</script>

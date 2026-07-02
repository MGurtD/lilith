<template>
  <DataTable
    :value="taxStore.taxes"
    tableStyle="min-width: 100%"
    @row-click="edit"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Impostos</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 30%"></Column>
    <Column
      field="percentatge"
      header="% Percentatge"
      style="width: 25%"
    ></Column>
    <Column header="Inversió subjecte passiu" style="width: 25%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isReverseCharge" :showColor="false" />
      </template>
    </Column>
    <Column header="Desactivada" style="width: 15%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" :showColor="false" />
      </template>
    </Column>
    <Column style="width: 10%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteTax($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { PrimeIcons } from "@primevue/core/api";
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useTaxesStore } from "../store/tax";
import { Tax } from "../types";

const router = useRouter();
const store = useStore();
const taxStore = useTaxesStore();
const confirm = useConfirm();
const toast = useToast();

onMounted(async () => {
  await taxStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: "Gestió d'impostos",
  });
});

const createButtonClick = () => {
  router.push({ path: `/tax/${uuidv4()}` });
};

const edit = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    router.push({ path: `/tax/${row.data.id}` });
  }
};

const deleteTax = (event: any, tax: Tax) => {
  confirm.require({
    target: event.currentTarget,
    message: `Està segur que vol eliminar l'impost?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await taxStore.delete(tax.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
      } else {
        toast.add({
          severity: "error",
          summary: "No s'ha pogut eliminar l'impost",
          life: 4000,
        });
      }
    },
  });
};
</script>

<template>
  <DataTable
    :value="expenseStore.expenseTypes"
    tableStyle="min-width: 100%"
    @row-click="editExpenseType"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Tipus de despesa</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 20%"></Column>
    <Column field="description" header="Descripció" style="width: 50%"></Column>
    <Column header="Desactivada" style="width: 20%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" :showColor="false" />
      </template>
    </Column>
    <Column style="width: 10%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteExpenseType($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useExpenseStore } from "../store/expense";
import { onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { ExpenseType } from "../types";

const router = useRouter();
const store = useStore();
const expenseStore = useExpenseStore();

onMounted(async () => {
  await expenseStore.fetchExpenseTypes();

  store.setMenuItem({
    icon: PrimeIcons.FLAG,
    title: "Gestió de tipus de despesa",
  });
});

const createButtonClick = () => {
  router.push({ path: `/expensetype/${uuidv4()}` });
};

const editExpenseType = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    router.push({ path: `/expensetype/${row.data.id}` });
  }
};

const confirm = useConfirm();
const toast = useToast();
const deleteExpenseType = (event: any, expenseType: ExpenseType) => {
  confirm.require({
    target: event.currentTarget,
    message: `Està segur que vol eliminar el tipus de despesa?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await expenseStore.deleteExpenseType(expenseType.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminada",
          life: 3000,
        });
        await expenseStore.fetchExpenseTypes();
      }
    },
  });
};
</script>

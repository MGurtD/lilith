<template>
  <DataTable
    :value="purchaseStore.purchaseInvoiceSeries"
    tableStyle="min-width: 100%"
    @row-click="editPurchaseInvoiceSerie"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Series</span>
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
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deletePurchaseInvoiceSerie($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { InvoiceSerie } from "../types";
import { useStore } from "../../../store";
import { usePurchaseInvoiceSeries } from "../store/purchaseInvoiceSeries";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const purchaseStore = usePurchaseInvoiceSeries();

onMounted(async () => {
  await purchaseStore.fetchPurchaseInvoiceSeries();
  store.setMenuItem({
    icon: PrimeIcons.SERVER,
    title: "Sèries Factures de Compra",
  });
});
const createButtonClick = () => {
  router.push({ path: `/purchaseinvoiceserie/${uuidv4()}` });
};

const editPurchaseInvoiceSerie = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    router.push({ path: `/purchaseinvoiceserie/${row.data.id}` });
  }
};

const deletePurchaseInvoiceSerie = (
  event: any,
  purchaseInvoiceSerie: InvoiceSerie
) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar la sèrie de factures de compra ${purchaseInvoiceSerie.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await purchaseStore.deletePurchaseInvoiceSerie(
        purchaseInvoiceSerie.id
      );

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await purchaseStore.fetchPurchaseInvoiceSeries();
      }
    },
  });
};
</script>

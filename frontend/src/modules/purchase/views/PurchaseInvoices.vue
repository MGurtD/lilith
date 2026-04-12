<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :paginator="
      purchaseInvoiceStore.purchaseInvoices &&
      purchaseInvoiceStore.purchaseInvoices.length > 20
    "
    :rows="20"
    sortMode="multiple"
    :value="purchaseInvoiceStore.purchaseInvoices"
    @row-click="editPurchaseInvoice"
  >
    <template #header>
      <div class="filter-toolbar">
        <div class="filter-toolbar__field">
          <label class="filter-label">Període</label>
          <DatePicker
            v-model="filter.dates"
            selectionMode="range"
            dateFormat="dd/mm/yy"
            placeholder="Seleccioni un període"
            showIcon
            @date-select="filterInvoices"
          />
        </div>
        <div class="filter-toolbar__field">
          <label class="filter-label">Proveïdor</label>
          <Select
            v-model="filter.supplierId"
            :options="puchaseMasterDataStore.masterData.suppliers"
            optionValue="id"
            optionLabel="comercialName"
            showClear
          />
        </div>
        <div class="filter-toolbar__field">
          <label class="filter-label">Mètode de pagament</label>
          <Select
            v-model="filter.paymentMethodId"
            :options="puchaseMasterDataStore.masterData.paymentMethods"
            optionValue="id"
            optionLabel="name"
            showClear
          />
        </div>
        <div class="filter-toolbar__actions">
          <Button
            :icon="PrimeIcons.FILTER"
            rounded
            raised
            @click="filterInvoices"
          />
          <Button
            :icon="PrimeIcons.FILTER_SLASH"
            rounded
            raised
            @click="cleanFilter"
          />
          <Button
            :icon="PrimeIcons.PLUS"
            rounded
            raised
            @click="createButtonClick"
          />
        </div>
      </div>
    </template>
    <Column
      field="number"
      header="Número"
      :sortable="true"
      style="width: 10%"
    ></Column>
    <Column
      header="Data"
      field="purchaseInvoiceDate"
      sortable
      style="width: 10%"
    >
      <template #body="slotProps">
        {{ formatDate(slotProps.data.purchaseInvoiceDate) }}
      </template>
    </Column>
    <Column header="Proveïdor" style="width: 15%">
      <template #body="slotProps">
        {{ getSupplierNameById(slotProps.data.supplierId) }}
      </template>
    </Column>
    <Column
      header="Núm. Fra. Proveïdor"
      style="width: 15%"
      field="supplierNumber"
    ></Column>
    <Column header="Estat" style="width: 15%">
      <template #body="slotProps">
        {{ getStatusNameById(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column header="Venciment" style="width: 10%">
      <template #body="slotProps">
        {{ getLastDueDate(slotProps.data) }}
      </template>
    </Column>
    <Column header="Import" style="width: 10%">
      <template #body="slotProps"> {{ slotProps.data.netAmount }} € </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="getStatusNameById(slotProps.data.statusId) === 'Nova'"
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deletePurchaseInvoice($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { usePurchaseMasterDataStore } from "../store/purchase";
import { DataTableRowClickEvent } from "primevue/datatable";
import { usePurchaseInvoiceStore } from "../store/purchaseInvoices";
import { onMounted, onUnmounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import {
  formatDateForQueryParameter,
  formatDate,
} from "../../../utils/functions";
import { PurchaseInvoice } from "../types";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useUserFilterStore } from "../../../store/userfilter";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const userFilterStore = useUserFilterStore();
const lifecycleName = "PurchaseInvoice";
const lifecycleStore = useLifecyclesStore();
const puchaseMasterDataStore = usePurchaseMasterDataStore();
const purchaseInvoiceStore = usePurchaseInvoiceStore();

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  supplierId: undefined as string | undefined,
  paymentMethodId: undefined as string | undefined,
});

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: "Factures de compra",
  });

  await lifecycleStore.fetchOneByName(lifecycleName);
  await puchaseMasterDataStore.fetchMasterData();
  getUserFilter();

  if (!filter.value.dates) {
    setCurrentYear();
  }

  await filterInvoices();
});
onUnmounted(() => {
  userFilterStore.addFilter("PurchaseInvoices", "", filter.value);
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("PurchaseInvoices", "");
  if (userFilter) {
    filter.value.supplierId = userFilter.supplierId;
    filter.value.paymentMethodId = userFilter.paymentMethodId;
    if (userFilter.dates) {
      filter.value.dates = [
        new Date(userFilter.dates[0]),
        new Date(userFilter.dates[1]),
      ];
    }
  }
};

const setCurrentYear = () => {
  const year = new Date().getFullYear().toString();
  const currentExercise = puchaseMasterDataStore.masterData.exercises?.find(
    (e) => e.name === year,
  );

  if (currentExercise) {
    filter.value.dates = [
      new Date(currentExercise.startDate),
      new Date(currentExercise.endDate),
    ];
  }
};

const cleanFilter = () => {
  filter.value.dates = undefined;
  filter.value.supplierId = undefined;
  filter.value.paymentMethodId = undefined;
  setCurrentYear();
};

const filterLocalStorageKey = "temges.purchaseinvoice.filter";
const filterInvoices = async () => {
  if (filter.value.dates && filter.value.dates.length === 2 && filter.value.dates[1]) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await purchaseInvoiceStore.GetFiltered(
      startTime,
      endTime,
      undefined,
      undefined,
      filter.value.supplierId,
      filter.value.paymentMethodId,
    );

    localStorage.setItem(filterLocalStorageKey, JSON.stringify(filter.value));
  } else {
    toast.add({
      severity: "info",
      summary: "Filtre invàlid",
      detail: "Seleccioni un període",
      life: 5000,
    });
  }
};

const getSupplierNameById = (id: string) => {
  const supplier = puchaseMasterDataStore.masterData.suppliers?.find(
    (s) => s.id === id,
  );
  if (supplier) return supplier.comercialName;
  else return "";
};

const getStatusNameById = (id: string) => {
  const status = lifecycleStore.lifecycle?.statuses?.find((s) => s.id === id);
  if (status) return status.name;
  else return "";
};

const getLastDueDate = (invoice: PurchaseInvoice): string => {
  if (!invoice.purchaseInvoiceDueDates) {
    return "";
  } else if (invoice.purchaseInvoiceDueDates.length === 0) {
    return formatDate(invoice.purchaseInvoiceDate);
  } else {
    return formatDate(
      invoice.purchaseInvoiceDueDates[
        invoice.purchaseInvoiceDueDates.length - 1
      ].dueDate,
    );
  }
};

const createButtonClick = () => {
  router.push({ path: `/purchaseInvoice/${uuidv4()}` });
};

const editPurchaseInvoice = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/purchaseinvoice/${row.data.id}` });
  }
};

const deletePurchaseInvoice = (event: any, invoice: PurchaseInvoice) => {
  confirm.require({
    target: event.currentTarget,
    message: `Està segur que vol eliminar la factura ${invoice.number}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await purchaseInvoiceStore.Delete(invoice.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminada",
          life: 3000,
        });
        await filterInvoices();
      }
    },
  });
};
</script>

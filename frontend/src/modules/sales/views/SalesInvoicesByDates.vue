<template>
  <Table
    :columns="columns"
    :items="invoiceStore.invoices ?? []"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    preset="crud-list"
    page="SalesInvoicesByDates"
    class="small-datatable"
    tableStyle="min-width: 100%"
    sort-mode="multiple"
    :selection-mode="'multiple'"
    v-model:selection="selectedInvoices"
    @clear="clearFilter"
    @filter="filterInvoices"
  >
    <template #prepend>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">Període</label>
        <DatePicker
          v-model="filter.dates"
          :numberOfMonths="2"
          selectionMode="range"
          dateFormat="dd/mm/yy"
          size="small"
          class="w-full"
        />
      </div>
    </template>

    <template #append>
      <Button
        :icon="PrimeIcons.CHECK"
        :disabled="selectedInvoices.length === 0"
        rounded
        raised
        severity="success"
        size="small"
        @click="updateSelectedInvoiceStatusToManaged"
      />
    </template>

    <template #body-invoiceNumber="{ data }">
      {{ data.invoiceNumber }}
    </template>
    <template #body-_status="{ data }">
      <span :class="{ 'managed-status': isManagedStatus(data.statusId) }">
        {{ getStatusNameById(data.statusId) }}
      </span>
    </template>
    <template #body-_dueDate="{ data }">
      {{ getLastDueDate(data) }}
    </template>
    <template #body-download="{ data }">
      <i
        :class="PrimeIcons.DOWNLOAD"
        class="download_column"
        @click="downloadInvoices(data)"
      />
    </template>
  </Table>
</template>
<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import type { FilterBodyWidth, FilterConfig } from "../../../components/tables/TableFilter.vue";
import { useStore } from "../../../store";
import { useSalesInvoiceStore } from "../store/invoice";
import { SalesInvoice } from "../types";
import {
  formatDate,
  formatDateForQueryParameter,
} from "../../../utils/functions";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useCustomersStore } from "../store/customers";
import { PurchaseInvoiceUpdateStatues as InvoiceUpdateStatues } from "../../purchase/types";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "../../../components/tables/types";

const toast = useToast();
const store = useStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();
const invoiceStore = useSalesInvoiceStore();

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  showManaged: false,
});
const filterBodyWidth: FilterBodyWidth = {
  desktop: "50%",
  tablet: "100%",
};
const filterConfig = computed<Array<FilterConfig>>(() => [
  {
    key: "showManaged",
    label: "Gestionades",
    type: "checkbox",
    size: "sm",
  },
]);

const columns = ref<Column[]>([
  { field: "invoiceNumber", header: "Número", sortable: true, style: "width: 10%" },
  {
    field: "customerId",
    header: "Client",
    columnType: ColumnType.Lookup,
    resolver: customerStore.getCustomerNameById,
    style: "width: 15%",
  },
  { field: "_status", header: "Estat", style: "width: 15%" },
  { field: "invoiceDate", header: "Data", sortable: true, columnType: ColumnType.Date, style: "width: 15%" },
  { field: "_dueDate", header: "Venciment", style: "width: 15%" },
  { field: "baseAmount", header: "Import Base", columnType: ColumnType.Currency, style: "width: 15%" },
  { field: "download", header: "", style: "width: 2%" },
]);

const selectedInvoices = ref([] as Array<SalesInvoice>);
const lifecycleName = "SalesInvoice";

onMounted(async () => {
  invoiceStore.invoices = [];

  if (!customerStore.customers) {
    customerStore.fetchCustomers();
  }
  lifecycleStore.fetchOneByName(lifecycleName);

  store.setMenuItem({
    icon: PrimeIcons.SERVER,
    title: "Comptabilització de factures de venta",
  });
});

onUnmounted(() => {
  invoiceStore.invoices = undefined;
});

watch(
  () => filter.value.showManaged,
  (newValue, oldValue) => {
    if (newValue === oldValue) return;
    if (filter.value.dates?.[1]) {
      filterInvoices();
    }
  },
);

const getStatusNameById = (id: string) => {
  const status = lifecycleStore.lifecycle?.statuses?.find((s) => s.id === id);
  if (status) return status.name;
  else return "";
};

const getLastDueDate = (invoice: SalesInvoice): string => {
  if (!invoice.salesInvoiceDueDates) {
    return "";
  } else if (invoice.salesInvoiceDueDates.length === 0) {
    return formatDate(invoice.invoiceDate);
  } else {
    return formatDate(
      invoice.salesInvoiceDueDates[invoice.salesInvoiceDueDates.length - 1]
        .dueDate,
    );
  }
};

const isManagedStatus = (statusId: string): boolean => {
  const managedStatus = lifecycleStore.lifecycle?.statuses?.find(
    (s) => s.name === "Gestionada",
  );

  return (managedStatus && managedStatus.id === statusId) as boolean;
};

const clearFilter = () => {
  filter.value.dates = undefined;
  filter.value.showManaged = false;
  invoiceStore.invoices = [];
};

const filterInvoices = async () => {
  if (filter.value.dates) {
    let managedStatus = undefined;
    if (!filter.value.showManaged) {
      managedStatus = lifecycleStore.lifecycle?.statuses?.find(
        (s) => s.name === "Gestionada",
      );
    }

    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);
    const statusId = managedStatus ? managedStatus.id : undefined;

    await invoiceStore.GetFiltered(
      startTime,
      endTime,
      undefined,
      undefined,
      statusId,
    );
  } else {
    toast.add({
      severity: "info",
      summary: "Filtre invàlid",
      detail: "Seleccioni un període",
      life: 5000,
    });
  }
};

const updateSelectedInvoiceStatusToManaged = async () => {
  const statusTo = lifecycleStore.lifecycle?.statuses?.find(
    (s) => s.name === "Gestionada",
  );
  if (statusTo) {
    const ids = selectedInvoices.value.map((i) => i.id);
    const request = {
      ids,
      statusToId: statusTo.id,
    } as InvoiceUpdateStatues;

    const updated = await invoiceStore.UpdateInvoicesStatuses(request);
    if (updated) {
      toast.add({
        severity: "success",
        summary: "Comptabilització de factures de venta",
        detail: `Factures comptabilitzades: ${selectedInvoices.value.length}`,
        life: 5000,
      });

      await filterInvoices();
    }
  }
};

const downloadInvoices = async (invoice: SalesInvoice) => {
  const printed = await invoiceStore.PrintInvoice(
    invoice.id,
    invoice.invoiceNumber,
  );

  if (printed) {
    toast.add({
      severity: "success",
      summary: "Comptabilització de factures de venta",
      detail: `Factura ${invoice.invoiceNumber} descarregada`,
      life: 5000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Comptabilització de factures de venta",
      detail: `Error al descarregar la factura ${invoice.invoiceNumber}`,
      life: 5000,
    });
  }
};
</script>
<style scoped>
.download_column:hover {
  color: var(--p-blue-500);
  cursor: pointer;
}

.managed-status {
  color: green;
}
</style>
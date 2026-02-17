<template>
  <div>
    <DataTable
      class="small-datatable"
      tableStyle="min-width: 100%"
      scrollable
      scrollHeight="flex"
      sortMode="multiple"
      :value="invoiceStore.invoices"
      v-model:selection="selectedInvoices"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <div class="datatable-filter">
            <div class="filter-field">
              <label class="block text-900 mb-2">Període</label>
              <DatePicker
                v-model="filter.dates"
                :numberOfMonths="2"
                selectionMode="range"
                dateFormat="dd/mm/yy"
              />
            </div>
            <div class="filter-field">
              <label class="block text-900 mb-2 mr-3">Gestionades</label>
              <div class="flex flex-wrap gap-3">
                <div class="flex align-items-center">
                  <Checkbox
                    v-model="filter.showManaged"
                    :binary="true"
                    @change="filterInvoices"
                  />
                </div>
              </div>
            </div>
          </div>
          <div class="datatable-buttons">
            <Button
              class="datatable-button"
              :icon="PrimeIcons.FILTER"
              rounded
              raised
              @click="filterInvoices"
            />
            <Button
              :icon="PrimeIcons.CHECK"
              :disabled="selectedInvoices.length === 0"
              rounded
              raised
              severity="success"
              @click="updateSelectedInvoiceStatusToManaged"
            />
          </div>
        </div>
      </template>
      <Column selectionMode="multiple" style="width: 2%"></Column>
      <Column
        field="invoiceNumber"
        header="Número"
        sortable
        style="width: 10%"
      ></Column>
      <Column header="Client" style="width: 15%">
        <template #body="slotProps">
          {{ customerStore.getCustomerNameById(slotProps.data.customerId) }}
        </template>
      </Column>
      <Column header="Estat" style="width: 15%">
        <template #body="slotProps">
          <span
            :class="{
              'managed-status': isManagedStatus(slotProps.data.statusId),
            }"
          >
            {{ getStatusNameById(slotProps.data.statusId) }}
          </span>
        </template>
      </Column>
      <Column header="Data" field="invoiceDate" sortable style="width: 15%">
        <template #body="slotProps">
          {{ formatDate(slotProps.data.invoiceDate) }}
        </template>
      </Column>
      <Column header="Venciment" style="width: 15%">
        <template #body="slotProps">
          {{ getLastDueDate(slotProps.data) }}
        </template>
      </Column>
      <Column field="baseAmount" header="Import Base" style="width: 15%">
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.baseAmount) }}
        </template>
      </Column>
      <Column style="width: 2%">
        <template #body="slotProps">
          <i
            :class="PrimeIcons.DOWNLOAD"
            class="download_column"
            @click="downloadInvoices(slotProps.data)"
          />
        </template>
      </Column>
    </DataTable>
  </div>
</template>
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useSalesInvoiceStore } from "../store/invoice";
import { SalesInvoice } from "../types";
import {
  formatCurrency,
  formatDate,
  formatDateForQueryParameter,
} from "../../../utils/functions";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useCustomersStore } from "../store/customers";
import { PurchaseInvoiceUpdateStatues as InvoiceUpdateStatues } from "../../purchase/types";

const toast = useToast();
const store = useStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();
const invoiceStore = useSalesInvoiceStore();

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  showManaged: false,
});
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
.datatable-button {
  margin-right: 1rem;
}

.download_column:hover {
  color: var(--p-blue-500);
  cursor: pointer;
}

.managed-status {
  color: green;
}
</style>

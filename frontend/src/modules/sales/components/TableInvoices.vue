<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortMode="multiple"
    :value="invoices"
    paginator
    :rows="20"
    @row-click="editButtonClick"
  >
    <template #header>
      <slot name="filter"></slot>
    </template>
    <Column
      field="invoiceNumber"
      header="Número"
      :sortable="true"
      style="width: 10%"
    ></Column>
    <Column
      header="Data"
      field="purchaseInvoiceDate"
      sortable
      style="width: 15%"
    >
      <template #body="slotProps">
        {{ formatDate(slotProps.data.invoiceDate) }}
      </template>
    </Column>
    <Column header="Client" style="width: 15%">
      <template #body="slotProps">
        {{ getCustomerNameById(slotProps.data.customerId) }}
      </template>
    </Column>
    <Column header="Estat" style="width: 15%">
      <template #body="slotProps">
        {{ getStatusNameById(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column header="Venciment" style="width: 15%">
      <template #body="slotProps">
        {{ getLastDueDate(slotProps.data) }}
      </template>
    </Column>
    <Column header="Import" style="width: 15%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.netAmount) }}
      </template>
    </Column>
    <!-- <Column header="Verifactu" style="width: 10%">
      <template #body="slotProps">
        <span
          :class="getVerifactuStatusClass(slotProps.data.integrationStatusId)"
        >
          {{
            invoiceStore.getVerifactuStatusById(
              slotProps.data.integrationStatusId
            )
          }}
        </span>
      </template>
    </Column> -->
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="
            slotProps.data.statusId ===
            lifecycleStore.lifecycle?.initialStatusId
          "
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteButtonClick($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { onMounted } from "vue";
import { Customer, SalesInvoice } from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useSalesInvoiceStore } from "../store/invoice";
import { formatCurrency, formatDate } from "../../../utils/functions";
import { DataTableRowClickEvent } from "primevue/datatable";

const lifecycleStore = useLifecyclesStore();
const invoiceStore = useSalesInvoiceStore();

const props = defineProps<{
  invoices: Array<SalesInvoice> | undefined;
  customers: Array<Customer> | undefined;
}>();

onMounted(async () => {
  await lifecycleStore.fetchOneByName("SalesInvoice");
});

const emit = defineEmits<{
  (e: "edit", invoice: SalesInvoice): void;
  (e: "delete", invoice: SalesInvoice): void;
}>();

const getCustomerNameById = (id: string) => {
  const supplier = props.customers?.find((s) => s.id === id);
  if (supplier) return supplier.comercialName;
  else return "";
};

const getStatusNameById = (id: string) => {
  const status = lifecycleStore.lifecycle?.statuses.find((s) => s.id === id);
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

const getVerifactuStatusClass = (integrationStatusId: string) => {
  const status = invoiceStore.getVerifactuStatusById(integrationStatusId);

  if (status === "OK") {
    return "status-ok";
  } else if (status === "Error") {
    return "status-error";
  } else if (status === "Pendent") {
    return "status-pending";
  }

  return "";
};

const editButtonClick = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    emit("edit", row.data);
  }
};

const deleteButtonClick = (event: any, invoice: SalesInvoice) => {
  emit("delete", invoice);
};
</script>

<style scoped>
.status-ok {
  color: #28a745; /* Green */
  font-weight: bold;
}

.status-error {
  color: #dc3545; /* Red */
  font-weight: bold;
}

.status-pending {
  color: #6c757d; /* Gray */
  font-weight: bold;
}
</style>

<template>
  <Table
    :columns="columns"
    :items="invoiceStore.invoices ?? []"
    :filter-config="[]"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    preset="crud-list"
    page="SalesInvoices"
    sortMode="multiple"
    showDeleteColumn
    :canDelete="(item) => item.statusId === lifecycleStore.lifecycle?.initialStatusId"
    @filter="filterInvoices"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteSalesInvoice"
    @row-click="editRow"
  >
    <template #prepend>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">Període</label>
        <DatePicker
          v-model="filter.dates"
          selectionMode="range"
          dateFormat="dd/mm/yy"
          placeholder="Selecciona període"
          showIcon
          class="w-full"
          size="small"
        />
      </div>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">Client</label>
        <DropdownCustomers label="" v-model="filter.customerId" />
      </div>
    </template>

    <template #body-invoiceDate="{ data }">
      {{ formatDate(data.invoiceDate) }}
    </template>
    <template #body-customerId="{ data }">
      {{ getCustomerNameById(data.customerId) }}
    </template>
    <template #body-statusId="{ data }">
      {{ getStatusNameById(data.statusId) }}
    </template>
    <template #body-dueDate="{ data }">
      {{ getLastDueDate(data) }}
    </template>
    <template #body-netAmount="{ data }">
      {{ formatCurrency(data.netAmount) }}
    </template>
  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreateOrderOrInvoice
      :create-request="createRequest"
      @submit="createInvoice"
    />
  </Dialog>
</template>
<script setup lang="ts">
import DropdownCustomers from "../components/DropdownCustomers.vue";
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import Table from "../../../components/tables/Table.vue";
import type { Column } from "../../../components/tables/Table.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useSalesInvoiceStore } from "../store/invoice";
import { useCustomersStore } from "../store/customers";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { onMounted, onUnmounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  formatDateForQueryParameter,
  formatDate,
  formatCurrency,
  getNewUuid,
} from "../../../utils/functions";
import { CreateSalesHeaderRequest, SalesInvoice } from "../types";
import { DialogOptions } from "../../../types/component";
import { useUserFilterStore } from "../../../store/userfilter";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const userFilterStore = useUserFilterStore();
const customersStore = useCustomersStore();
const invoiceStore = useSalesInvoiceStore();
const lifecycleStore = useLifecyclesStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

const columns = ref<Column[]>([
  { field: "invoiceNumber", header: "Número", sortable: true, style: "width: 10%" },
  { field: "invoiceDate", header: "Data", sortable: true, style: "width: 15%" },
  { field: "customerId", header: "Client", style: "width: 25%" },
  { field: "statusId", header: "Estat", style: "width: 15%" },
  { field: "dueDate", header: "Venciment", style: "width: 15%" },
  { field: "netAmount", header: "Import", style: "width: 20%" },
]);

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  customerId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: "Crear factura",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const setCurrentYear = () => {
  const now = new Date();
  filter.value.dates = [
    new Date(now.getFullYear(), 0, 1),
    new Date(now.getFullYear(), 11, 31),
  ];
};

onMounted(async () => {
  customersStore.fetchCustomers();
  lifecycleStore.fetchOneByName("SalesInvoice");

  setCurrentYear();
  getUserFilter();
  await filterInvoices();

  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: "Factures de venta",
  });
});

onUnmounted(() => {
  userFilterStore.addFilter("SalesInvoices", "", filter.value);
  invoiceStore.invoices = undefined;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("SalesInvoices", "");
  if (userFilter) {
    filter.value.customerId = userFilter.customerId;
    if (userFilter.dates) {
      filter.value.dates = [
        new Date(userFilter.dates[0]),
        new Date(userFilter.dates[1]),
      ];
    }
  }
};

const cleanFilter = () => {
  filter.value.customerId = undefined;
  setCurrentYear();
};

const filterInvoices = async () => {
  let startTime = "";
  let endTime = "";

  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    startTime = formatDateForQueryParameter(filter.value.dates[0]);
    endTime = formatDateForQueryParameter(filter.value.dates[1]);
  }

  await invoiceStore.GetFiltered(
    startTime,
    endTime,
    undefined,
    filter.value.customerId,
    undefined,
  );
};

const getCustomerNameById = (id: string) => {
  const customer = customersStore.customers?.find((c) => c.id === id);
  return customer ? customer.comercialName : "";
};

const getStatusNameById = (id: string) => {
  const status = lifecycleStore.lifecycle?.statuses.find((s) => s.id === id);
  return status ? status.name : "";
};

const getLastDueDate = (invoice: SalesInvoice): string => {
  if (!invoice.salesInvoiceDueDates) return "";
  if (invoice.salesInvoiceDueDates.length === 0) return formatDate(invoice.invoiceDate);
  return formatDate(
    invoice.salesInvoiceDueDates[invoice.salesInvoiceDueDates.length - 1].dueDate,
  );
};

const createRequest = ref({} as CreateSalesHeaderRequest);
const generateNewRequest = (): CreateSalesHeaderRequest => {
  return {
    id: getNewUuid(),
    customerId: "",
    exerciseId: "",
    date: new Date(),
  };
};

const createButtonClick = () => {
  createRequest.value = generateNewRequest();
  dialogOptions.visible = true;
};

const createInvoice = async () => {
  const response = await invoiceStore.Create(createRequest.value);
  if (response && !response?.result) {
    const errorMessage =
      response.errors.length > 0
        ? response.errors[0]
        : "Error desconegut, contacte amb l'administrador.";

    toast.add({
      severity: "warn",
      summary: "Error al crear la factura",
      detail: errorMessage,
      life: 10000,
    });
    return;
  }

  if (response)
    router.push({ path: `/sales-invoice/${createRequest.value.id}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/sales-invoice/${row.data.id}` });
};

const deleteSalesInvoice = (invoice: SalesInvoice) => {
  confirm.require({
    message: `Està segur que vol eliminar la factura?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await invoiceStore.Delete(invoice.id);
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

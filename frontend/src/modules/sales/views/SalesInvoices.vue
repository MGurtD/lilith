<template>
  <Table
    :card-layout="cardLayout"
    :columns="columns"
    :items="invoiceStore.invoices ?? []"
    :filter-config="filterConfig"
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
    <template #filter-customerId="{ value, update }">
      <DropdownCustomers size="small" label="" :model-value="value" @update:model-value="update" />
    </template>

    <template #body-dueDate="{ data }">
      {{ getLastDueDate(data) }}
    </template>
  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="t('sales.invoices.createTitle')"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreateOrderOrInvoice
      :create-request="createRequest"
      :loading="creating"
      @submit="createInvoice"
      @cancel="dialogOptions.visible = false"
    />
  </Dialog>
</template>
<script setup lang="ts">
import DropdownCustomers from "../components/DropdownCustomers.vue";
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "../../../components/tables/types";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "../../../components/tables/TableFilter.vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useSalesInvoiceStore } from "../store/invoice";
import { useCustomersStore } from "../store/customers";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { computed, onMounted, onUnmounted, reactive, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  formatDateForQueryParameter,
  formatDate,
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
const { locale, t } = useI18n();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

const columns = computed<Column[]>(() => [
  { field: "invoiceNumber", header: t("common.number"), sortable: true, style: "width: 10%" },
  { field: "invoiceDate", header: t("common.date"), sortable: true, columnType: ColumnType.Date, style: "width: 15%" },
  {
    field: "customerId",
    header: t("common.customer"),
    columnType: ColumnType.Lookup,
    resolver: customersStore.getCustomerNameById,
    style: "width: 25%",
  },
  {
    field: "statusId",
    header: t("common.status"),
    columnType: ColumnType.Status,
    resolver: lifecycleStore.getStatusNameById,
    severity: lifecycleStore.getStatusColorById,
    style: "width: 15%",
  },
  { field: "dueDate", header: t("sales.list.columns.dueDate"), style: "width: 15%", sortable: true },
  { field: "netAmount", header: t("common.amount"), columnType: ColumnType.Currency, style: "width: 20%" },
]);

const cardLayout: CardLayout = {
  title: "invoiceNumber",
  subtitle: "customerId",
  badge: "statusId",
  trailing: "netAmount",
  meta: ["invoiceDate", "dueDate"],
};

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "dates",
    label: t("common.period"),
    type: "date-range",
    placeholder: t("sales.list.periodPlaceholder"),
  },
  {
    key: "customerId",
    label: t("common.customer"),
    type: "slot",
    valueLabel: (value) => customersStore.getCustomerNameById(String(value)),
  },
]);

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  customerId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
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

  setMenuItem();
});

const setMenuItem = () => {
  store.setMenuItem({ icon: PrimeIcons.MONEY_BILL, title: t("sales.invoices.title") });
};

watch(locale, setMenuItem);

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

const getLastDueDate = (invoice: SalesInvoice): string => {
  if (!invoice.salesInvoiceDueDates) return "";
  if (invoice.salesInvoiceDueDates.length === 0) return formatDate(invoice.invoiceDate);
  return formatDate(
    invoice.salesInvoiceDueDates[invoice.salesInvoiceDueDates.length - 1].dueDate,
  );
};

const createRequest = ref({} as CreateSalesHeaderRequest);
const creating = ref(false);
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

const createInvoice = async (request: CreateSalesHeaderRequest) => {
  if (creating.value) return;

  creating.value = true;
  try {
    const response = await invoiceStore.Create(request);
    if (response && !response?.result) {
      const errorMessage =
        response.errors.length > 0
          ? response.errors[0]
          : t("sales.list.messages.unknownError");

      toast.add({
        severity: "warn",
        summary: t("sales.invoices.messages.createError"),
        detail: errorMessage,
        life: 10000,
      });
      return;
    }

    if (response)
      router.push({ path: `/sales-invoice/${request.id}` });
  } finally {
    creating.value = false;
  }
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/sales-invoice/${row.data.id}` });
};

const deleteSalesInvoice = (invoice: SalesInvoice) => {
  confirm.require({
    message: t("sales.invoices.messages.confirmDelete"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await invoiceStore.Delete(invoice.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("sales.list.messages.deleted"),
          life: 3000,
        });
        await filterInvoices();
      }
    },
  });
};
</script>

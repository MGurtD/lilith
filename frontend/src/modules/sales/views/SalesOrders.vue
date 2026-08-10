<template>
  <Table
    :columns="columns"
    :items="salesOrderStore.salesOrders ?? []"
    :filter-config="[]"
    :filter-labels="filterMetadata.filterLabels"
    :filter-value-resolvers="filterMetadata.filterValueResolvers"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    page="SalesOrders"
    preset="crud-list"
    class="small-datatable"
    tableStyle="min-width: 100%"
    sort-field="salesOrderNumber"
    :sort-order="1"
    :attachment-config="{
      entity: 'SalesOrder',
      title: t('sales.orders.attachmentsTitle'),
      titleField: 'number',
    }"
    showDeleteColumn
    :canDelete="(item) => item.statusId === lifecycleStore.lifecycle?.initialStatusId"
    @filter="filterSalesOrder"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteSalesInvoice"
    @row-click="editRow"
  >
    <template #prepend>
      <div
        class="table-filter-prepend-field table-filter-prepend-field--md"
      >
        <label class="filter-label table-filter-prepend-label"
           >{{ t("common.period") }}</label
        >
        <DatePicker
          v-model="filter.dates"
          selectionMode="range"
          dateFormat="dd/mm/yy"
          :placeholder="t('sales.list.periodPlaceholder')"
          showIcon
          class="w-full"
          size="small"
        />
      </div>
      <div
        class="table-filter-prepend-field table-filter-prepend-field--md"
      >
        <label class="filter-label table-filter-prepend-label"
           >{{ t("common.customer") }}</label
        >
        <DropdownCustomers label="" v-model="filter.customerId" />
      </div>
      <div
        class="table-filter-prepend-field table-filter-prepend-field--md"
      >
        <label class="filter-label table-filter-prepend-label">{{ t("common.status") }}</label>
        <DropdownLifecycle
          label=""
          name="SalesOrder"
          v-model="filter.statusId"
        />
      </div>
    </template>

  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="t('sales.orders.createTitle')"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreateOrderOrInvoice
      :create-request="createRequest"
      @submit="createOrder"
    />
  </Dialog>
</template>
<script setup lang="ts">
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import DropdownLifecycle from "../../shared/components/DropdownLifecycle.vue";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "../../../components/tables/types";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { computed, onMounted, onUnmounted, reactive, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useSalesOrderStore } from "../store/order";
import { useCustomersStore } from "../store/customers";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  formatDateForQueryParameter,
  getNewUuid,
} from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import { CreateSalesHeaderRequest } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useUserFilterStore } from "../../../store/userfilter";
import { createSalesTableViewFilterMetadata } from "@/modules/sales/utils/sales-table-view-filter-metadata";

const router = useRouter();
const toast = useToast();
const confirm = useConfirm();

const store = useStore();
const userFilterStore = useUserFilterStore();
const salesOrderStore = useSalesOrderStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();
const { locale, t } = useI18n();

const columns = computed<Column[]>(() => [
  { field: "number", header: t("common.number"), sortable: true, style: "width: 10%" },
  { field: "date", header: t("common.date"), sortable: true, columnType: ColumnType.Date, style: "width: 10%" },
  { field: "expectedDate", header: t("sales.list.columns.deliveryDate"), sortable: true, columnType: ColumnType.Date, style: "width: 10%" },
  { field: "customerComercialName", header: t("common.customer"), style: "width: 30%" },
  { field: "customerNumber", header: t("sales.orders.columns.customerOrder"), style: "width: 15%" },
  {
    field: "statusId",
    header: t("common.status"),
    columnType: ColumnType.Lookup,
    resolver: lifecycleStore.getStatusNameById,
    style: "width: 20%",
  },
]);

const filterMetadata = computed(() =>
  createSalesTableViewFilterMetadata(columns.value, {
    customerResolver: customerStore.getCustomerNameById,
  }),
);

const filterBodyWidth: FilterBodyWidth = { desktop: "66%", tablet: "100%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  customerId: undefined as string | undefined,
  statusId: undefined as string | undefined,
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
  lifecycleStore.fetchOneByName("SalesOrder");
  customerStore.fetchCustomers();

  setCurrentYear();
  getUserFilter();
  await filterSalesOrder();

  setMenuItem();
});

const setMenuItem = () => {
  store.setMenuItem({ icon: PrimeIcons.APPLE, title: t("sales.orders.title") });
};

watch(locale, setMenuItem);
onUnmounted(() => {
  userFilterStore.addFilter("SalesOrders", "", filter.value);
  salesOrderStore.salesOrders = undefined;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("SalesOrders", "");
  if (userFilter) {
    filter.value.statusId = userFilter.statusId;
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
  filter.value.statusId = undefined;
  setCurrentYear();
  filterSalesOrder();
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

const filterSalesOrder = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await salesOrderStore.GetFiltered(
      startTime,
      endTime,
      filter.value.customerId,
      filter.value.statusId,
    );
  } else {
    toast.add({
      severity: "info",
      summary: t("sales.list.messages.invalidFilter"),
      detail: t("sales.list.messages.selectPeriod"),
      life: 5000,
    });
  }
};

const createOrder = async () => {
  const response = await salesOrderStore.Create(createRequest.value);
  if (!response?.result) {
    toast.add({
      severity: "warn",
      summary: t("sales.orders.messages.createError"),
      detail:
        response?.errors?.[0] ??
        t("sales.list.messages.unknownError"),
      life: 10000,
    });
    return;
  }
  dialogOptions.visible = false;
  router.push({ path: `/salesorder/${createRequest.value.id}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/salesorder/${row.data.id}` });
};

const deleteSalesInvoice = (order: any) => {
  confirm.require({
    message: t("sales.orders.messages.confirmDelete"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await salesOrderStore.Delete(order.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("sales.list.messages.deleted"),
          life: 3000,
        });

        await filterSalesOrder();
      }
    },
  });
};
</script>

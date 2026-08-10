<template>
  <Table
    :columns="columns"
    :items="deliveryNoteStore.deliveryNotes ?? []"
    :filter-config="[]"
    :filter-labels="filterMetadata.filterLabels"
    :filter-value-resolvers="filterMetadata.filterValueResolvers"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    preset="crud-list"
    page="DeliveryNotes"
    class="small-datatable"
    tableStyle="min-width: 100%"
    sort-field="number"
    :sort-order="1"
    showDeleteColumn
    :canDelete="(item) => item.statusId === lifecycleStore.lifecycle?.initialStatusId"
    @filter="filterData"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteDeliveryNote"
    @row-click="editRow"
  >
    <template #prepend>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">{{ t("common.period") }}</label>
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
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">{{ t("common.customer") }}</label>
        <DropdownCustomers label="" v-model="filter.customerId" />
      </div>
    </template>

  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="t('sales.deliveryNotes.createTitle')"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreateOrderOrInvoice
      :create-request="createRequest"
      @submit="createDeliveryNote"
    />
  </Dialog>
</template>
<script setup lang="ts">
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "../../../components/tables/types";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { computed, onMounted, onUnmounted, reactive, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useCustomersStore } from "../store/customers";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  formatDateForQueryParameter,
  getNewUuid,
} from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import { CreateSalesHeaderRequest, SalesOrderHeader } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useDeliveryNoteStore } from "../store/deliveryNote";
import { createSalesTableViewFilterMetadata } from "@/modules/sales/utils/sales-table-view-filter-metadata";

const router = useRouter();
const toast = useToast();
const confirm = useConfirm();
const store = useStore();
const deliveryNoteStore = useDeliveryNoteStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();
const { locale, t } = useI18n();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

const columns = computed<Column[]>(() => [
  { field: "number", header: t("common.number"), sortable: true, style: "width: 15%" },
  { field: "createdOn", header: t("sales.list.columns.createdOn"), sortable: true, columnType: ColumnType.Date, style: "width: 15%" },
  { field: "deliveryDate", header: t("sales.list.columns.deliveryDate"), columnType: ColumnType.Date, sortable: true, style: "width: 15%" },
  {
    field: "customerId",
    header: t("common.customer"),
    columnType: ColumnType.Lookup,
    resolver: customerStore.getCustomerNameById,
    style: "width: 30%",
  },
  {
    field: "statusId",
    header: t("common.status"),
    columnType: ColumnType.Lookup,
    resolver: lifecycleStore.getStatusNameById,
    style: "width: 30%",
  },
]);

const filterMetadata = computed(() =>
  createSalesTableViewFilterMetadata(columns.value),
);

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
  lifecycleStore.fetchOneByName("DeliveryNote");
  customerStore.fetchCustomers();

  setCurrentYear();
  await filterData();

  setMenuItem();
});

const setMenuItem = () => {
  store.setMenuItem({ icon: PrimeIcons.APPLE, title: t("sales.deliveryNotes.title") });
};

watch(locale, setMenuItem);

onUnmounted(() => {
  deliveryNoteStore.deliveryNotes = undefined;
});

const cleanFilter = () => {
  filter.value.customerId = undefined;
  setCurrentYear();
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

const filterData = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await deliveryNoteStore.GetFiltered(
      startTime,
      endTime,
      filter.value.customerId,
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

const createDeliveryNote = async () => {
  const response = await deliveryNoteStore.Create(createRequest.value);
  if (!response?.result) {
    toast.add({
      severity: "warn",
      summary: t("sales.deliveryNotes.messages.createError"),
      detail:
        response?.errors?.[0] ??
        t("sales.list.messages.unknownError"),
      life: 10000,
    });
    return;
  }
  dialogOptions.visible = false;
  router.push({ path: `/deliverynote/${createRequest.value.id}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/deliverynote/${row.data.id}` });
};

const deleteDeliveryNote = async (order: SalesOrderHeader) => {
  confirm.require({
    message: t("sales.deliveryNotes.messages.confirmDelete"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await deliveryNoteStore.Delete(order.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("sales.list.messages.deleted"),
          life: 3000,
        });

        await filterData();
      }
    },
  });
};
</script>

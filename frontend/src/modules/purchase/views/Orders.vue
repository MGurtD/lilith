<template>
  <Table
    preset="crud-list"
    :columns="columns"
    :items="ordersStore.orders ?? []"
    :filter-config="[]"
    :filter-labels="filterMetadata.filterLabels"
    :filter-value-resolvers="filterMetadata.filterValueResolvers"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    page="PurchaseOrders"
    class="small-datatable"
    tableStyle="min-width: 100%"
    sortMode="multiple"
    delete-column-width="5%"
    show-delete-column
    :can-delete="canDelete"
    @filter="filterData"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="remove"
    @row-click="edit"
  >
    <template #prepend>
      <div
        class="table-filter-prepend-field table-filter-prepend-field--md"
      >
        <label class="filter-label table-filter-prepend-label"
          >{{ t("purchase.orders.filters.period") }}</label
        >
        <DatePicker
          v-model="filter.dates"
          selectionMode="range"
          dateFormat="dd/mm/yy"
          :placeholder="t('purchase.orders.placeholders.selectPeriod')"
          showIcon
          class="w-full"
          size="small"
        />
      </div>
      <div
        class="table-filter-prepend-field table-filter-prepend-field--md"
      >
        <label class="filter-label table-filter-prepend-label"
          >{{ t("purchase.order.fields.supplier") }}</label
        >
        <DropdownSupplier label="" v-model="filter.supplierId" />
      </div>
    </template>
  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreatePurchaseDocument
      :create-request="createRequest"
      @submit="create"
    />
  </Dialog>
</template>
<script setup lang="ts">
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
  type Column,
} from "../../../components/tables/types";
import { createTableViewFilterMetadata } from "../../../components/tables/table-view-filter-metadata";
import FormCreatePurchaseDocument from "../components/FormCreatePurchaseDocument.vue";
import DropdownSupplier from "../components/DropdownSupplier.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useOrderStore } from "../store/order";
import { useSuppliersStore } from "../store/suppliers";
import { DataTableRowClickEvent } from "primevue/datatable";
import { computed, onMounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DialogOptions } from "../../../types/component";
import {
  formatDateForQueryParameter,
  getNewUuid,
} from "../../../utils/functions";
import { CreatePurchaseDocumentRequest, PurchaseOrder } from "../types";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useI18n } from "vue-i18n";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const suppliersStore = useSuppliersStore();
const lifecycleStore = useLifecyclesStore();
const ordersStore = useOrderStore();
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "number",
    header: t("purchase.order.fields.number"),
    sortable: true,
    style: "width: 20%",
  },
  {
    field: "date",
    header: t("purchase.order.fields.date"),
    sortable: true,
    columnType: ColumnType.Date,
    style: "width: 20%",
  },
  {
    field: "supplierId",
    header: t("purchase.order.fields.supplier"),
    columnType: ColumnType.Lookup,
    resolver: getSupplierNameById,
    style: "width: 30%",
  },
  {
    field: "statusId",
    header: t("purchase.order.fields.status"),
    columnType: ColumnType.Lookup,
    resolver: getStatusNameById,
    style: "width: 25%",
  },
]);

const filterMetadata = computed(() =>
  createTableViewFilterMetadata(columns.value, {
    labels: {
      dates: t("purchase.orders.filters.period"),
      supplierId: t("purchase.order.fields.supplier"),
    },
  }),
);

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  supplierId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: t("purchase.orders.dialogs.create"),
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
  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: t("purchase.orders.title"),
  });

  suppliersStore.fetchSuppliers();
  await lifecycleStore.fetchOneByName("PurchaseOrder");
  setCurrentYear();

  await filterData();
});

const cleanFilter = () => {
  filter.value.supplierId = undefined;
  setCurrentYear();
};

const filterData = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await ordersStore.fetchFiltered(
      startTime,
      endTime,
      filter.value.supplierId,
    );
  } else {
    toast.add({
      severity: "info",
      summary: t("purchase.messages.invalidFilter"),
      detail: t("purchase.orders.messages.selectPeriod"),
      life: 5000,
    });
  }
};
const getSupplierNameById = (id: string) => {
  const supplier = suppliersStore.suppliers?.find((s) => s.id === id);
  if (supplier) return supplier.comercialName;
  else return "";
};
const getStatusNameById = (id: string) => {
  if (lifecycleStore.lifecycle) {
    const status = lifecycleStore.lifecycle.statuses.find((s) => s.id === id);
    if (status) return status.name;
  }
  return "";
};
const createButtonClick = () => {
  createRequest.value = generateNewRequest();
  dialogOptions.visible = true;
};
const createRequest = ref({} as CreatePurchaseDocumentRequest);
const generateNewRequest = (): CreatePurchaseDocumentRequest => {
  return {
    id: getNewUuid(),
    supplierId: "",
    exerciseId: "",
    date: new Date(),
  };
};
const create = async () => {
  const created = await ordersStore.create(createRequest.value);
  dialogOptions.visible = false;
  if (created)
    router.push({ path: `/purchase-orders/${createRequest.value.id}` });
};

const edit = (row: DataTableRowClickEvent) => {
  router.push({ path: `/purchase-orders/${row.data.id}` });
};

const canDelete = (order: PurchaseOrder) =>
  lifecycleStore.lifecycle?.initialStatusId === order.statusId;

const remove = (order: PurchaseOrder) => {
  confirm.require({
    message: t("purchase.orders.messages.confirmDelete", { number: order.number }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await ordersStore.delete(order.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("purchase.messages.deleted"),
          life: 3000,
        });
        await filterData();
      }
    },
  });
};
</script>

<template>
  <Table
    preset="crud-list"
    :columns="columns"
    :items="ordersStore.orders ?? []"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    page="PurchaseOrders"
    :card-layout="cardLayout"
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
    <template #filter-supplierId="{ value, update }">
      <DropdownSupplier size="small" label="" :model-value="value" @update:model-value="update" />
    </template>
  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable && !creating"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreatePurchaseDocument
      :create-request="createRequest"
      :loading="creating"
      @submit="create"
    />
  </Dialog>
</template>
<script setup lang="ts">
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "../../../components/tables/types";
import FormCreatePurchaseDocument from "../components/FormCreatePurchaseDocument.vue";
import DropdownSupplier from "../components/DropdownSupplier.vue";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "../../../components/tables/TableFilter.vue";
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
    columnType: ColumnType.Status,
    resolver: getStatusNameById,
    severity: lifecycleStore.getStatusColorById,
    style: "width: 25%",
  },
]);

// Phone card: the default for this screen; a saved view may override it.
const cardLayout: CardLayout = {
  title: "number",
  subtitle: "supplierId",
  badge: "statusId",
  trailing: "date",
};

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "dates",
    label: t("purchase.orders.filters.period"),
    type: "date-range",
    placeholder: t("purchase.orders.placeholders.selectPeriod"),
  },
  {
    key: "supplierId",
    label: t("purchase.order.fields.supplier"),
    type: "slot",
    valueLabel: (value) => getSupplierNameById(String(value)),
  },
]);

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
const creating = ref(false);

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
const create = async (request: CreatePurchaseDocumentRequest) => {
  if (creating.value) return;

  creating.value = true;
  try {
    const created = await ordersStore.create(request);
    if (!created) return;

    dialogOptions.visible = false;
    router.push({ path: `/purchase-orders/${request.id}` });
  } finally {
    creating.value = false;
  }
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

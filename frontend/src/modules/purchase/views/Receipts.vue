<template>
  <Table
    preset="crud-list"
    :columns="columns"
    :items="receiptsStore.receipts ?? []"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    page="Receipts"
    class="small-datatable"
    tableStyle="min-width: 100%"
    paginator
    :rows="20"
    delete-column-width="5%"
    show-delete-column
    :can-delete="canDelete"
    @filter="filterReceipts"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteReceipt"
    @row-click="editReceipt"
  >
    <template #filter-supplierId="{ value, update }">
      <DropdownSupplier size="small" label="" :model-value="value" @update:model-value="update" />
    </template>
  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable && !creatingReceipt"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreatePurchaseDocument
      :create-request="createRequest"
      :loading="creatingReceipt"
      @submit="createReceipt"
    />
  </Dialog>
</template>
<script setup lang="ts">
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
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
import { useReceiptsStore } from "../store/receipt";
import { useSuppliersStore } from "../store/suppliers";
import { DataTableRowClickEvent } from "primevue/datatable";
import { computed, onMounted, reactive, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { DialogOptions } from "../../../types/component";
import {
  formatDateForQueryParameter,
  getNewUuid,
} from "../../../utils/functions";
import { CreatePurchaseDocumentRequest, Receipt } from "../types";
import { useLifecyclesStore } from "../../shared/store/lifecycle";

const toast = useToast();
const { locale, t } = useI18n();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const lifecycleStore = useLifecyclesStore();
const receiptsStore = useReceiptsStore();
const suppliersStore = useSuppliersStore();

const columns = computed<Column[]>(() => [
  {
    field: "number",
    header: t("purchase.receipts.columns.number"),
    sortable: true,
    style: "width: 15%",
  },
  {
    field: "date",
    header: t("purchase.receipts.columns.date"),
    sortable: true,
    columnType: ColumnType.Date,
    style: "width: 15%",
  },
  {
    field: "supplierId",
    header: t("purchase.receipts.columns.supplier"),
    columnType: ColumnType.Lookup,
    resolver: getSupplierNameById,
    style: "width: 25%",
  },
  {
    field: "supplierNumber",
    header: t("purchase.receipts.columns.supplierNumber"),
    style: "width: 20%",
  },
  {
    field: "statusId",
    header: t("purchase.receipts.columns.status"),
    columnType: ColumnType.Status,
    resolver: getStatusNameById,
    severity: lifecycleStore.getStatusColorById,
    style: "width: 20%",
  },
]);

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "dates",
    label: t("purchase.receipts.filters.period"),
    type: "date-range",
    placeholder: t("purchase.receipts.placeholders.selectPeriod"),
  },
  {
    key: "supplierId",
    label: t("purchase.receipts.filters.supplier"),
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
  title: t("purchase.receipts.dialogs.create"),
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);
const creatingReceipt = ref(false);

const setCurrentYear = () => {
  const now = new Date();
  filter.value.dates = [
    new Date(now.getFullYear(), 0, 1),
    new Date(now.getFullYear(), 11, 31),
  ];
};

const setMenuItem = () => {
  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: t("purchase.receipts.title"),
  });
};

onMounted(async () => {
  setMenuItem();

  suppliersStore.fetchSuppliers();
  lifecycleStore.fetchOneByName("Receipts");
  setCurrentYear();

  await filterReceipts();
});

watch(locale, setMenuItem);

const cleanFilter = () => {
  filter.value.supplierId = undefined;
  setCurrentYear();
};

const filterReceipts = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await receiptsStore.fetchFiltered(
      startTime,
      endTime,
      filter.value.supplierId,
    );
  } else {
    toast.add({
      severity: "info",
      summary: t("purchase.receipts.messages.invalidFilter"),
      detail: t("purchase.receipts.messages.selectPeriod"),
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
const createReceipt = async (request: CreatePurchaseDocumentRequest) => {
  if (creatingReceipt.value) return;

  creatingReceipt.value = true;
  try {
    const created = await receiptsStore.createReceipt(request);
    if (!created) return;

    dialogOptions.visible = false;
    router.push({ path: `/receipts/${request.id}` });
  } finally {
    creatingReceipt.value = false;
  }
};

const editReceipt = (row: DataTableRowClickEvent) => {
  router.push({ path: `/receipts/${row.data.id}` });
};

const canDelete = (receipt: Receipt) =>
  lifecycleStore.lifecycle?.initialStatusId === receipt.statusId;

const deleteReceipt = (receipt: Receipt) => {
  confirm.require({
    message: t("purchase.receipts.messages.confirmDelete", { number: receipt.number }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await receiptsStore.deleteReceipt(receipt.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("purchase.receipts.messages.deleted"),
          life: 3000,
        });
        await filterReceipts();
      }
    },
  });
};
</script>

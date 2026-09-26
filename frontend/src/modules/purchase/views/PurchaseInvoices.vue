<template>
  <Table
    preset="crud-list"
    :columns="columns"
    :items="purchaseInvoiceStore.purchaseInvoices ?? []"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    page="PurchaseInvoices"
    :card-layout="cardLayout"
    class="small-datatable"
    tableStyle="min-width: 100%"
    sortMode="multiple"
    delete-column-width="3%"
    show-delete-column
    :can-delete="canDelete"
    @filter="filterInvoices"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deletePurchaseInvoice"
    @row-click="editPurchaseInvoice"
  >
    <template #action-prepend>
      <Button
        v-tooltip.bottom="t('purchase.purchaseInvoices.importPdf')"
        :aria-label="t('purchase.purchaseInvoices.importPdf')"
        icon="pi pi-file-pdf"
        size="small"
        severity="secondary"
        outlined
        @click="router.push({ name: 'PurchaseInvoiceImport' })"
      />
    </template>
    <template #filter-supplierId="{ value, update }">
      <DropdownSupplier size="small" label="" :model-value="value" @update:model-value="update" />
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "../../../components/tables/types";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { usePurchaseMasterDataStore } from "../store/purchase";
import { DataTableRowClickEvent } from "primevue/datatable";
import { usePurchaseInvoiceStore } from "../store/purchaseInvoices";
import { useSuppliersStore } from "../store/suppliers";
import { onMounted, ref, computed } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import {
  formatDateForQueryParameter,
  formatCurrency,
  getNewUuid,
} from "../../../utils/functions";
import { PurchaseInvoice } from "../types";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useUserFilterStore } from "../../../store/userfilter";
import DropdownSupplier from "../components/DropdownSupplier.vue";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "../../../components/tables/TableFilter.vue";
import { useI18n } from "vue-i18n";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const userFilterStore = useUserFilterStore();
const lifecycleName = "PurchaseInvoice";
const lifecycleStore = useLifecyclesStore();
const puchaseMasterDataStore = usePurchaseMasterDataStore();
const purchaseInvoiceStore = usePurchaseInvoiceStore();
const suppliersStore = useSuppliersStore();
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "number",
    header: t("purchase.purchaseInvoices.columns.number"),
    sortable: true,
    style: "width: 10%",
  },
  {
    field: "purchaseInvoiceDate",
    header: t("purchase.purchaseInvoices.columns.date"),
    sortable: true,
    columnType: ColumnType.Date,
    style: "width: 10%",
  },
  {
    field: "supplierId",
    header: t("purchase.purchaseInvoices.columns.supplier"),
    columnType: ColumnType.Lookup,
    resolver: getSupplierNameById,
    style: "width: 15%",
  },
  {
    field: "supplierNumber",
    header: t("purchase.purchaseInvoices.columns.supplierInvoiceNumber"),
    style: "width: 15%",
  },
  {
    field: "statusId",
    header: t("purchase.purchaseInvoices.columns.status"),
    columnType: ColumnType.Status,
    resolver: getStatusNameById,
    severity: lifecycleStore.getStatusColorById,
    style: "width: 15%",
  },
  {
    field: "dueDate",
    header: t("purchase.purchaseInvoices.columns.dueDate"),
    columnType: ColumnType.Date,
    resolver: resolveLastDueDate,
    style: "width: 15%",
  },
  {
    field: "netAmount",
    header: t("purchase.purchaseInvoices.columns.amount"),
    columnType: ColumnType.Currency,
    total: "sum",
    totalFormat: formatCurrency,
    style: "width: 10%; text-align: right",
  },
]);

// Phone card: the default for this screen; a saved view may override it.
const cardLayout: CardLayout = {
  title: "number",
  subtitle: "supplierId",
  badge: "statusId",
  trailing: "netAmount",
  meta: ["purchaseInvoiceDate", "dueDate", "supplierNumber"],
};

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "dates",
    label: t("purchase.purchaseInvoices.filters.period"),
    type: "date-range",
    placeholder: t("purchase.purchaseInvoices.placeholders.selectPeriod"),
  },
  {
    key: "supplierId",
    label: t("purchase.purchaseInvoices.filters.supplier"),
    type: "slot",
    valueLabel: (value) => getSupplierNameById(String(value)),
  },
  {
    key: "paymentMethodId",
    label: t("purchase.purchaseInvoices.filters.paymentMethod"),
    type: "select",
    filter: false,
    options: puchaseMasterDataStore.masterData.paymentMethods ?? [],
    optionLabel: "name",
    optionValue: "id",
    valueLabel: getPaymentMethodNameById,
  },
  {
    key: "accountNumber",
    label: t("purchase.purchaseInvoices.filters.accountNumber"),
    type: "select",
    filter: false,
    options: (suppliersStore.accountNumbers ?? []).map((accountNumber) => ({
      label: accountNumber,
      value: accountNumber,
    })),
    size: "sm",
  },
  {
    key: "dueDates",
    label: t("purchase.purchaseInvoices.filters.dueDate"),
    type: "date-range",
    placeholder: t("purchase.purchaseInvoices.placeholders.selectPeriod"),
  },
]);

const filterBodyWidth: FilterBodyWidth = { desktop: "100%", tablet: "100%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  dueDates: undefined as Array<Date> | undefined,
  supplierId: undefined as string | undefined,
  paymentMethodId: undefined as string | undefined,
  accountNumber: undefined as string | undefined,
});

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: t("purchase.purchaseInvoices.title"),
  });

  await lifecycleStore.fetchOneByName(lifecycleName);
  await puchaseMasterDataStore.fetchMasterData();
  await suppliersStore.fetchAccountNumbers();
  getUserFilter();

  if (!filter.value.dates) {
    setCurrentYear();
  }

  await filterInvoices();
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("PurchaseInvoices", "");
  if (userFilter) {
    filter.value.supplierId = userFilter.supplierId;
    filter.value.paymentMethodId = userFilter.paymentMethodId;
    filter.value.accountNumber = userFilter.accountNumber;
    if (userFilter.dates) {
      filter.value.dates = [
        new Date(userFilter.dates[0]),
        new Date(userFilter.dates[1]),
      ];
    }
    if (userFilter.dueDates) {
      filter.value.dueDates = [
        new Date(userFilter.dueDates[0]),
        new Date(userFilter.dueDates[1]),
      ];
    }
  }
};

const setCurrentYear = () => {
  const now = new Date();
  filter.value.dates = [
    new Date(now.getFullYear(), 0, 1),
    new Date(now.getFullYear(), 11, 31),
  ];
};

const cleanFilter = () => {
  filter.value.dates = undefined;
  filter.value.dueDates = undefined;
  filter.value.supplierId = undefined;
  filter.value.paymentMethodId = undefined;
  filter.value.accountNumber = undefined;
  setCurrentYear();
};

const filterInvoices = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);
    const dueDateStartTime =
      filter.value.dueDates &&
      filter.value.dueDates.length === 2 &&
      filter.value.dueDates[1]
        ? formatDateForQueryParameter(filter.value.dueDates[0])
        : undefined;
    const dueDateEndTime =
      filter.value.dueDates &&
      filter.value.dueDates.length === 2 &&
      filter.value.dueDates[1]
        ? formatDateForQueryParameter(filter.value.dueDates[1])
        : undefined;

    await purchaseInvoiceStore.GetFiltered(
      startTime,
      endTime,
      undefined,
      undefined,
      filter.value.supplierId,
      filter.value.paymentMethodId,
      dueDateStartTime,
      dueDateEndTime,
      filter.value.accountNumber,
    );

    userFilterStore.addFilter("PurchaseInvoices", "", filter.value);
  } else {
    toast.add({
      severity: "info",
      summary: t("purchase.messages.invalidFilter"),
      detail: t("purchase.purchaseInvoices.messages.selectPeriod"),
      life: 5000,
    });
  }
};

const getSupplierNameById = (id: string) => {
  const supplier = puchaseMasterDataStore.masterData.suppliers?.find(
    (s) => s.id === id,
  );
  if (supplier) return supplier.comercialName;
  else return "";
};

const getStatusNameById = (id: string) => {
  const status = lifecycleStore.lifecycle?.statuses?.find((s) => s.id === id);
  if (status) return status.name;
  else return "";
};

function getPaymentMethodNameById(value: unknown): string {
  if (typeof value !== "string") return "";
  return (
    puchaseMasterDataStore.masterData.paymentMethods?.find(
      (method) => method.id === value,
    )?.name ?? ""
  );
}

const resolveLastDueDate = (_value: unknown, data: unknown): string | Date => {
  if (!data || typeof data !== "object") return "";
  const invoice = data as PurchaseInvoice;
  if (!invoice.purchaseInvoiceDueDates) {
    return "";
  } else if (invoice.purchaseInvoiceDueDates.length === 0) {
    return invoice.purchaseInvoiceDate;
  } else {
    const sortedDueDates = [...invoice.purchaseInvoiceDueDates].sort(
      (left, right) =>
        new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime(),
    );
    const lastDueDate = sortedDueDates[sortedDueDates.length - 1];

    return lastDueDate.dueDate;
  }
};

const createButtonClick = () => {
  router.push({ path: `/purchaseInvoice/${getNewUuid()}` });
};

const editPurchaseInvoice = (row: DataTableRowClickEvent) => {
  router.push({ path: `/purchaseinvoice/${row.data.id}` });
};

const canDelete = (invoice: PurchaseInvoice) =>
  lifecycleStore.lifecycle?.initialStatusId === invoice.statusId;

const deletePurchaseInvoice = (invoice: PurchaseInvoice) => {
  confirm.require({
    message: t("purchase.purchaseInvoices.messages.confirmDelete", {
      number: invoice.number,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await purchaseInvoiceStore.Delete(invoice.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("purchase.messages.deleted"),
          life: 3000,
        });
        await filterInvoices();
      }
    },
  });
};
</script>

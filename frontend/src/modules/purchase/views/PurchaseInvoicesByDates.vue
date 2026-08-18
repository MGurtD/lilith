<template>
  <Table
    :columns="columns"
    :items="purchaseInvoiceStore.purchaseInvoices ?? []"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    :show-create="false"
    show-selection-column
    selection-column-width="2%"
    preset="crud-list"
    page="PurchaseInvoicesByDates"
    class="small-datatable"
    tableStyle="min-width: 100%"
    sortMode="multiple"
    :selection-mode="'multiple'"
    v-model:selection="selectedInvoices"
    @filter="filterInvoices"
    @clear="clearFilter"
  >
    <template #append>
      <Button
        :icon="PrimeIcons.CHECK"
        :disabled="selectedInvoices.length === 0"
        rounded
        raised
        severity="success"
        size="small"
        @click="updateSelectedInvoiceStatusToManaged"
      />
    </template>
    <template #body-_status="{ data }">
      <span :class="{ 'managed-status': isManagedStatus(data.statusId) }">
        {{ getStatusNameById(data.statusId) }}
      </span>
    </template>
    <template #body-_dueDate="{ data }">
      {{ getLastDueDate(data) }}
    </template>
    <template #body-download="{ data }">
      <i
        :class="PrimeIcons.DOWNLOAD"
        class="download_column"
        @click="downloadInvoices(data)"
      />
    </template>
  </Table>
</template>
<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import {
  type FilterBodyWidth,
  type FilterConfig,
} from "../../../components/tables/TableFilter.vue";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "../../../components/tables/types";
import { useStore } from "../../../store";
import { usePurchaseMasterDataStore } from "../store/purchase";
import { usePurchaseInvoiceStore } from "../store/purchaseInvoices";
import { PurchaseInvoice, PurchaseInvoiceUpdateStatues } from "../types";
import SharedServices from "../../../services";
import {
  createBlobAndDownloadFile,
  formatDate,
  formatDateForQueryParameter,
} from "../../../utils/functions";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useI18n } from "vue-i18n";

const toast = useToast();
const store = useStore();
const purchaseStore = usePurchaseMasterDataStore();
const lifecycleStore = useLifecyclesStore();
const purchaseInvoiceStore = usePurchaseInvoiceStore();
const { t } = useI18n();

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  showManaged: false,
  supplierId: undefined as string | undefined,
});
const filterBodyWidth: FilterBodyWidth = {
  desktop: "66%",
  tablet: "100%",
};
const filterConfig = computed<Array<FilterConfig>>(() => [
  {
    key: "dates",
    label: t("purchase.purchaseInvoicesByDates.filters.period"),
    type: "date-range",
    size: "md",
  },
  {
    key: "supplierId",
    label: t("purchase.purchaseInvoicesByDates.filters.supplier"),
    type: "select",
    options: (purchaseStore.masterData.suppliers ?? []).map((supplier) => ({
      label: supplier.comercialName,
      value: supplier.id,
    })),
    placeholder: t(
      "purchase.purchaseInvoicesByDates.placeholders.selectSupplier",
    ),
    size: "lg",
  },
  {
    key: "showManaged",
    label: t("purchase.purchaseInvoicesByDates.filters.showManaged"),
    type: "checkbox",
    size: "sm",
  },
]);

const columns = computed<Column[]>(() => [
  {
    field: "number",
    header: t("purchase.purchaseInvoicesByDates.columns.number"),
    sortable: true,
    style: "width: 10%",
  },
  {
    field: "supplierId",
    header: t("purchase.purchaseInvoicesByDates.columns.supplier"),
    columnType: ColumnType.Lookup,
    resolver: getSupplierNameById,
    style: "width: 15%",
  },
  {
    field: "supplierNumber",
    header: t("purchase.purchaseInvoicesByDates.columns.supplierInvoiceNumber"),
    style: "width: 12%",
  },
  {
    field: "_status",
    header: t("purchase.purchaseInvoicesByDates.columns.status"),
    style: "width: 15%",
  },
  {
    field: "purchaseInvoiceDate",
    header: t("purchase.purchaseInvoicesByDates.columns.date"),
    columnType: ColumnType.Date,
    sortable: true,
    style: "width: 15%",
  },
  {
    field: "_dueDate",
    header: t("purchase.purchaseInvoicesByDates.columns.dueDate"),
    style: "width: 15%",
  },
  {
    field: "baseAmount",
    header: t("purchase.purchaseInvoicesByDates.columns.baseAmount"),
    columnType: ColumnType.Currency,
    style: "width: 15%",
  },
  { field: "download", header: "", style: "width: 2%" },
]);
const selectedInvoices = ref([] as Array<PurchaseInvoice>);
const lifecycleName = "PurchaseInvoice";

onMounted(async () => {
  purchaseInvoiceStore.purchaseInvoices = [];

  purchaseStore.fetchMasterData();
  lifecycleStore.fetchOneByName(lifecycleName);

  store.setMenuItem({
    icon: PrimeIcons.SERVER,
    title: t("purchase.purchaseInvoicesByDates.title"),
  });
});

watch(
  () => filter.value.showManaged,
  (newValue, oldValue) => {
    if (newValue === oldValue) return;
    if (filter.value.dates?.[1]) {
      filterInvoices();
    }
  },
);

const getSupplierNameById = (id: string) => {
  const supplier = purchaseStore.masterData.suppliers?.find((s) => s.id === id);
  if (supplier) return supplier.comercialName;
  else return "";
};

const getStatusNameById = (id: string) => {
  const status = lifecycleStore.lifecycle?.statuses?.find((s) => s.id === id);
  if (status) return status.name;
  else return "";
};

const getLastDueDate = (invoice: PurchaseInvoice): string => {
  if (!invoice.purchaseInvoiceDueDates) {
    return "";
  } else if (invoice.purchaseInvoiceDueDates.length === 0) {
    return formatDate(invoice.purchaseInvoiceDate);
  } else {
    return formatDate(
      invoice.purchaseInvoiceDueDates[
        invoice.purchaseInvoiceDueDates.length - 1
      ].dueDate,
    );
  }
};

const isManagedStatus = (statusId: string): boolean => {
  const managedStatus = lifecycleStore.lifecycle?.statuses?.find(
    (s) => s.name === "Gestionada",
  );

  return (managedStatus && managedStatus.id === statusId) as boolean;
};

const clearFilter = () => {
  filter.value.dates = undefined;
  filter.value.showManaged = false;
  filter.value.supplierId = undefined;
  purchaseInvoiceStore.purchaseInvoices = [];
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

    await purchaseInvoiceStore.GetFiltered(
      startTime,
      endTime,
      undefined,
      statusId,
      filter.value.supplierId,
    );
  } else {
    toast.add({
      severity: "info",
      summary: t("purchase.messages.invalidFilter"),
      detail: t("purchase.purchaseInvoicesByDates.messages.selectPeriod"),
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
    } as PurchaseInvoiceUpdateStatues;

    const updated = await purchaseInvoiceStore.UpdateInvoicesStatus(request);
    if (updated) {
      toast.add({
        severity: "success",
        summary: t(
          "purchase.purchaseInvoicesByDates.messages.accountingCompleted",
        ),
        detail: t(
          "purchase.purchaseInvoicesByDates.messages.accountedInvoices",
          {
            count: selectedInvoices.value.length,
          },
        ),
        life: 5000,
      });

      await filterInvoices();
    }
  }
};

const downloadInvoices = async (invoice: PurchaseInvoice) => {
  const files = await SharedServices.File.GetEntityFiles(
    "PurchaseInvoice",
    invoice.id,
  );
  if (files) {
    files.forEach(async (f) => {
      const { blob, contentType } = await SharedServices.File.Download(f);
      createBlobAndDownloadFile(f.originalName, blob, contentType);
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

<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :paginator="
      purchaseInvoiceStore.purchaseInvoices &&
      purchaseInvoiceStore.purchaseInvoices.length > 20
    "
    :rows="20"
    sortMode="multiple"
    :value="purchaseInvoiceStore.purchaseInvoices"
    @row-click="editPurchaseInvoice"
  >
    <template #header>
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :body-width="filterBodyWidth"
        embedded
        @filter="filterInvoices"
        @clear="cleanFilter"
        @create="createButtonClick"
      >
        <template #prepend>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Període</label
            >
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
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Proveïdor</label
            >
            <DropdownSupplier label="" v-model="filter.supplierId" />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Mètode de pagament</label
            >
            <Select
              v-model="filter.paymentMethodId"
              :options="puchaseMasterDataStore.masterData.paymentMethods"
              optionValue="id"
              optionLabel="name"
              showClear
              class="w-full"
              size="small"
            />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--sm"
          >
            <label class="filter-label table-filter-prepend-label"
              >Número de compte</label
            >
            <Select
              v-model="filter.accountNumber"
              :options="suppliersStore.accountNumbers"
              showClear
              class="w-full"
              size="small"
            />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Venciment</label
            >
            <DatePicker
              v-model="filter.dueDates"
              selectionMode="range"
              dateFormat="dd/mm/yy"
              placeholder="Selecciona període"
              showIcon
              class="w-full"
              size="small"
            />
          </div>
        </template>
      </TableFilter>
    </template>
    <Column
      field="number"
      header="Número"
      :sortable="true"
      style="width: 10%"
    ></Column>
    <Column
      header="Data"
      field="purchaseInvoiceDate"
      sortable
      style="width: 10%"
    >
      <template #body="slotProps">
        {{ formatDate(slotProps.data.purchaseInvoiceDate) }}
      </template>
    </Column>
    <Column header="Proveïdor" style="width: 15%">
      <template #body="slotProps">
        {{ getSupplierNameById(slotProps.data.supplierId) }}
      </template>
    </Column>
    <Column
      header="Núm. Fra. Proveïdor"
      style="width: 15%"
      field="supplierNumber"
    ></Column>
    <Column header="Estat" style="width: 15%">
      <template #body="slotProps">
        {{ getStatusNameById(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column header="Venciment" style="width: 10%">
      <template #body="slotProps">
        {{ getLastDueDate(slotProps.data) }}
      </template>
    </Column>
    <Column header="Import" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.netAmount) }}
      </template>
      <template #footer>
        <div class="total-footer">
          <span class="total-label">Total</span>
          <span class="total-value">{{ formatCurrency(totalNetAmount) }}</span>
        </div>
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="getStatusNameById(slotProps.data.statusId) === 'Nova'"
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deletePurchaseInvoice($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
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
import { formatDateForQueryParameter, formatDate, formatCurrency, getNewUuid } from "../../../utils/functions";
import { PurchaseInvoice } from "../types";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useUserFilterStore } from "../../../store/userfilter";
import DropdownSupplier from "../components/DropdownSupplier.vue";
import TableFilter from "../../../components/tables/TableFilter.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";

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
    title: "Factures de compra",
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
      summary: "Filtre invàlid",
      detail: "Seleccioni un període",
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

const getLastDueDate = (invoice: PurchaseInvoice): string => {
  if (!invoice.purchaseInvoiceDueDates) {
    return "";
  } else if (invoice.purchaseInvoiceDueDates.length === 0) {
    return formatDate(invoice.purchaseInvoiceDate);
  } else {
    const sortedDueDates = [...invoice.purchaseInvoiceDueDates].sort(
      (left, right) =>
        new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime(),
    );
    const lastDueDate = sortedDueDates[sortedDueDates.length - 1];

    return formatDate(lastDueDate.dueDate);
  }
};

const totalNetAmount = computed(() =>
  (purchaseInvoiceStore.purchaseInvoices ?? []).reduce(
    (sum, inv) => sum + (inv.netAmount ?? 0),
    0,
  ),
);

const createButtonClick = () => {
  router.push({ path: `/purchaseInvoice/${getNewUuid()}` });
};

const editPurchaseInvoice = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/purchaseinvoice/${row.data.id}` });
  }
};

const deletePurchaseInvoice = (event: any, invoice: PurchaseInvoice) => {
  confirm.require({
    target: event.currentTarget,
    message: `Està segur que vol eliminar la factura ${invoice.number}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await purchaseInvoiceStore.Delete(invoice.id);
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

<style scoped>
.total-footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 0.5rem;
}

.total-label {
  font-weight: 600;
  color: var(--p-text-muted-color);
  font-size: 0.85rem;
}

.total-value {
  font-weight: 700;
}
</style>

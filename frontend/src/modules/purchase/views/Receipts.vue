<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    paginator
    :rows="20"
    :value="receiptsStore.receipts"
    @row-click="editReceipt"
  >
    <template #header>
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :body-width="filterBodyWidth"
        embedded
        @filter="filterReceipts"
        @clear="cleanFilter"
        @create="createButtonClick"
      >
        <template #prepend>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >{{ t("purchase.receipts.filters.period") }}</label
            >
            <DatePicker
              v-model="filter.dates"
              selectionMode="range"
              dateFormat="dd/mm/yy"
              :placeholder="t('purchase.receipts.placeholders.selectPeriod')"
              showIcon
              class="w-full"
              size="small"
            />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >{{ t("purchase.receipts.filters.supplier") }}</label
            >
            <DropdownSupplier label="" v-model="filter.supplierId" />
          </div>
        </template>
      </TableFilter>
    </template>
    <Column
      field="number"
      :header="t('purchase.receipts.columns.number')"
      :sortable="true"
      style="width: 10%"
    ></Column>
    <Column :header="t('purchase.receipts.columns.date')" field="date" sortable style="width: 10%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
    <Column :header="t('purchase.receipts.columns.supplier')" style="width: 15%">
      <template #body="slotProps">
        {{ getSupplierNameById(slotProps.data.supplierId) }}
      </template>
    </Column>
    <Column
      :header="t('purchase.receipts.columns.supplierNumber')"
      style="width: 15%"
      field="supplierNumber"
    ></Column>
    <Column :header="t('purchase.receipts.columns.status')" style="width: 15%">
      <template #body="slotProps">
        {{ getStatusNameById(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="
            lifecycleStore.lifecycle?.initialStatusId ===
            slotProps.data.statusId
          "
          :class="PrimeIcons.TIMES"
          :aria-label="t('purchase.receipts.actions.delete')"
          class="grid_delete_column_button"
          @click="deleteReceipt($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreatePurchaseDocument
      :create-request="createRequest"
      @submit="createReceipt"
    />
  </Dialog>
</template>
<script setup lang="ts">
import FormCreatePurchaseDocument from "../components/FormCreatePurchaseDocument.vue";
import DropdownSupplier from "../components/DropdownSupplier.vue";
import TableFilter from "../../../components/tables/TableFilter.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useReceiptsStore } from "../store/receipt";
import { useSuppliersStore } from "../store/suppliers";
import { DataTableRowClickEvent } from "primevue/datatable";
import { onMounted, reactive, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { DialogOptions } from "../../../types/component";
import {
  formatDateForQueryParameter,
  formatDate,
  getNewUuid,
} from "../../../utils/functions";
import { CreatePurchaseDocumentRequest, PurchaseInvoice } from "../types";
import { useLifecyclesStore } from "../../shared/store/lifecycle";

const toast = useToast();
const { locale, t } = useI18n();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const lifecycleStore = useLifecyclesStore();
const receiptsStore = useReceiptsStore();
const suppliersStore = useSuppliersStore();

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
const createReceipt = async () => {
  const created = await receiptsStore.createReceipt(createRequest.value);
  dialogOptions.visible = false;
  if (created) router.push({ path: `/receipts/${createRequest.value.id}` });
};

const editReceipt = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/receipts/${row.data.id}` });
  }
};

const deleteReceipt = (event: any, invoice: PurchaseInvoice) => {
  confirm.require({
    target: event.currentTarget,
    message: t("purchase.receipts.messages.confirmDelete", { number: invoice.number }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await receiptsStore.deleteReceipt(invoice.id);
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

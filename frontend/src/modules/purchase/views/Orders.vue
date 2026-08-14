<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortMode="multiple"
    :paginator="ordersStore.orders && ordersStore.orders.length > 20"
    :rows="20"
    :value="ordersStore.orders"
    @row-click="edit"
  >
    <template #header>
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :body-width="filterBodyWidth"
        embedded
        @filter="filterData"
        @clear="cleanFilter"
        @create="createButtonClick"
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
      </TableFilter>
    </template>
    <Column
      field="number"
      :header="t('purchase.order.fields.number')"
      :sortable="true"
      style="width: 10%"
    ></Column>
    <Column :header="t('purchase.order.fields.date')" field="date" sortable style="width: 10%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
    <Column :header="t('purchase.order.fields.supplier')" style="width: 15%">
      <template #body="slotProps">
        {{ getSupplierNameById(slotProps.data.supplierId) }}
      </template>
    </Column>
    <Column :header="t('purchase.order.fields.status')" style="width: 15%">
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
          class="grid_delete_column_button"
          @click="remove($event, slotProps.data)"
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
      @submit="create"
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
import { useOrderStore } from "../store/order";
import { useSuppliersStore } from "../store/suppliers";
import { DataTableRowClickEvent } from "primevue/datatable";
import { onMounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DialogOptions } from "../../../types/component";
import {
  formatDateForQueryParameter,
  formatDate,
  getNewUuid,
} from "../../../utils/functions";
import { CreatePurchaseDocumentRequest, PurchaseInvoice } from "../types";
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
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/purchase-orders/${row.data.id}` });
  }
};

const remove = (event: any, invoice: PurchaseInvoice) => {
  confirm.require({
    target: event.currentTarget,
    message: t("purchase.orders.messages.confirmDelete", { number: invoice.number }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await ordersStore.delete(invoice.id);
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

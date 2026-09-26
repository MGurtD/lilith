<template>
  <Table
    preset="crud-list"
    :columns="columns"
    :items="budgetStore.budgets ?? []"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    page="Budgets"
    class="small-datatable"
    tableStyle="min-width: 100%"
    sort-field="salesOrderNumber"
    :sort-order="1"
    showDeleteColumn
    :canDelete="(item) => item.statusId === lifecycleStore.lifecycle?.initialStatusId"
    @row-click="editRow"
    @filter="filterBudget"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteBudget"
  >
    <template #filter-customerId="{ value, update }">
      <DropdownCustomers size="small" label="" :model-value="value" @update:model-value="update" />
    </template>

  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="t('sales.budgets.createTitle')"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreateOrderOrInvoice
      :create-request="createRequest"
      :loading="creating"
      @submit="createOrder"
      @cancel="dialogOptions.visible = false"
    />
  </Dialog>


</template>
<script setup lang="ts">
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "@/components/tables/TableFilter.vue";
import { computed, onMounted, onUnmounted, reactive, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { useStore } from "@/store";
import { useCustomersStore } from "../store/customers";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  formatDateForQueryParameter,
  getNewUuid,
} from "@/utils/functions";
import { DialogOptions } from "@/types/component";
import { Budget, CreateSalesHeaderRequest } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useBudgetStore } from "../store/budget";

const router = useRouter();
const toast = useToast();
const confirm = useConfirm();
const store = useStore();
const budgetStore = useBudgetStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();
const { locale, t } = useI18n();

const filterBodyWidth: FilterBodyWidth = { desktop: "66%", tablet: "100%" };

const columns = computed<Column[]>(() => [
  { field: "number", header: t("common.number") },
  { field: "date", header: t("common.date"), sortable: true, columnType: ColumnType.Date },
  {
    field: "customerId",
    header: t("common.customer"),
    columnType: ColumnType.Lookup,
    resolver: customerStore.getCustomerNameById,
  },
  {
    field: "statusId",
    header: t("common.status"),
    columnType: ColumnType.Status,
    resolver: lifecycleStore.getStatusNameById,
    severity: lifecycleStore.getStatusColorById,
  },
  { field: "acceptanceDate", header: t("sales.budgets.columns.acceptanceDate"), columnType: ColumnType.Date },
  { field: "deliveryDays", header: t("sales.budgets.columns.deliveryDays") },
]);

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "dates",
    label: t("common.period"),
    type: "date-range",
    placeholder: t("sales.list.periodPlaceholder"),
  },
  {
    key: "customerId",
    label: t("common.customer"),
    type: "slot",
    valueLabel: (value) => customerStore.getCustomerNameById(String(value)),
  },
  {
    key: "statusIds",
    label: t("common.status"),
    type: "multiselect",
    options: lifecycleStore.lifecycle?.statuses || [],
    optionLabel: "name",
    optionValue: "id",
    placeholder: t("sales.list.statusesPlaceholder"),
    display: "chip",
    filter: false,
    showToggleAll: false,
  },
]);

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  customerId: undefined as string | undefined,
  statusIds: [] as Array<string>,
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
  await lifecycleStore.fetchOneByName("Budget");
  await customerStore.fetchCustomers();

  setCurrentYear();
  await filterBudget();

  setMenuItem();
});
onUnmounted(() => {
  budgetStore.budgets = undefined;
});

const setMenuItem = () => {
  store.setMenuItem({ icon: PrimeIcons.APPLE, title: t("sales.budgets.title") });
};

watch(locale, setMenuItem);

const cleanFilter = () => {
  filter.value.customerId = undefined;
  filter.value.statusIds = [];
  setCurrentYear();
  filterBudget();
};

const createRequest = ref({} as CreateSalesHeaderRequest);
const creating = ref(false);
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

const filterBudget = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await budgetStore.GetFiltered(
      startTime,
      endTime,
      filter.value.customerId,
      filter.value.statusIds,
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

const createOrder = async (request: CreateSalesHeaderRequest) => {
  if (creating.value) return;

  creating.value = true;
  try {
    const response = await budgetStore.Create(request);
    if (!response) {
      toast.add({
        severity: "warn",
        summary: t("sales.budgets.messages.createError"),
        detail: t("sales.list.messages.unknownError"),
        life: 10000,
      });
      return;
    }
    dialogOptions.visible = false;
    router.push({ path: `/budget/${request.id}` });
  } finally {
    creating.value = false;
  }
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/budget/${row.data.id}` });
};

const deleteBudget = async (budget: Budget) => {
  await budgetStore.GetAssociatedSalesOrders(budget.id);

  if (budgetStore.order) {
    toast.add({
      severity: "warn",
      summary: t("sales.budgets.messages.cannotDelete"),
      detail: t("sales.budgets.messages.associatedOrder", { number: budgetStore.order.number }),
      life: 5000,
    });
    return;
  }

  confirm.require({
    message: t("sales.budgets.messages.confirmDelete"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await budgetStore.Delete(budget.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("sales.list.messages.deleted"),
          life: 3000,
        });

        await filterBudget();
      }
    },
  });
};
</script>

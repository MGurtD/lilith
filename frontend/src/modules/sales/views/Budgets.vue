<template>
  <Table
    preset="crud-list"
    :columns="columns"
    :items="budgetStore.budgets ?? []"
    :filter-config="[]"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    page="Budgets"
    class="small-datatable"
    tableStyle="min-width: 100%"
    sort-field="salesOrderNumber"
    :sort-order="1"
    :attachment-config="{
      entity: 'Budget',
      title: 'Adjunts del pressupost',
      titleField: 'number',
    }"
    showDeleteColumn
    :canDelete="(item) => item.statusId === lifecycleStore.lifecycle?.initialStatusId"
    @row-click="editRow"
    @filter="filterBudget"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteBudget"
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
          >Client</label
        >
        <DropdownCustomers label="" v-model="filter.customerId" />
      </div>
      <div
        class="table-filter-prepend-field table-filter-prepend-field--md"
      >
        <label class="filter-label table-filter-prepend-label">Estat</label>
        <MultiSelect
          v-model="statusIds"
          :options="lifecycleStore.lifecycle?.statuses || []"
          optionLabel="name"
          optionValue="id"
          placeholder="Selecciona estats"
          display="chip"
          :showToggleAll="false"
          class="w-full"
        />
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
    <FormCreateOrderOrInvoice
      :create-request="createRequest"
      @submit="createOrder"
    />
  </Dialog>


</template>
<script setup lang="ts">
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type { FilterBodyWidth } from "@/components/tables/TableFilter.vue";
import { onMounted, onUnmounted, reactive, ref } from "vue";
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

const filterBodyWidth: FilterBodyWidth = { desktop: "66%", tablet: "100%" };

const columns = ref<Column[]>([
  { field: "number", header: "Número" },
  { field: "date", header: "Data", sortable: true, columnType: ColumnType.Date },
  {
    field: "customerId",
    header: "Client",
    columnType: ColumnType.Lookup,
    resolver: customerStore.getCustomerNameById,
  },
  {
    field: "statusId",
    header: "Estat",
    columnType: ColumnType.Lookup,
    resolver: lifecycleStore.getStatusNameById,
  },
  { field: "acceptanceDate", header: "Data d'acceptació", columnType: ColumnType.Date },
  { field: "deliveryDays", header: "Dies d'entrega" },
]);

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  customerId: undefined as string | undefined,
});
const statusIds = ref<Array<string>>([]);

const dialogOptions = reactive({
  visible: false,
  title: "Crear pressupost",
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

  store.setMenuItem({
    icon: PrimeIcons.APPLE,
    title: "Pressupostos",
  });
});
onUnmounted(() => {
  budgetStore.budgets = undefined;
});

const cleanFilter = () => {
  filter.value.customerId = undefined;
  statusIds.value = [];
  setCurrentYear();
  filterBudget();
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
      statusIds.value,
    );
  } else {
    toast.add({
      severity: "info",
      summary: "Filtre invàlid",
      detail: "Seleccioni un període",
      life: 5000,
    });
  }
};

const createOrder = async () => {
  const response = await budgetStore.Create(createRequest.value);
  if (!response) {
    toast.add({
      severity: "warn",
      summary: "Error al crear el pressupost",
      detail: "Error desconegut, contacte amb l'administrador.",
      life: 10000,
    });
    return;
  }
  dialogOptions.visible = false;
  router.push({ path: `/budget/${createRequest.value.id}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/budget/${row.data.id}` });
};

const deleteBudget = async (budget: Budget) => {
  await budgetStore.GetAssociatedSalesOrders(budget.id);

  if (budgetStore.order) {
    toast.add({
      severity: "warn",
      summary: "No es pot eliminar",
      detail: `El pressupost té la comanda ${budgetStore.order.number} associada`,
      life: 5000,
    });
    return;
  }

  confirm.require({
    message: `Està segur que vol eliminar el pressupost?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await budgetStore.Delete(budget.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminada",
          life: 3000,
        });

        await filterBudget();
      }
    },
  });
};
</script>

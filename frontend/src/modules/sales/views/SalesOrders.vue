<template>
  <DataTable
    :value="salesOrderStore.salesOrders"
    class="small-datatable"
    tableStyle="min-width: 100%"
    sort-field="salesOrderNumber"
    sort-mode="single"
    :sort-order="1"
    scrollable
    scrollHeight="flex"
    paginator
    :rows="20"
    @row-click="editRow"
  >
    <template #header>
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :body-width="filterBodyWidth"
        embedded
        @filter="filterSalesOrder"
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
              >Client</label
            >
            <DropdownCustomers label="" v-model="filter.customerId" />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label">Estat</label>
            <DropdownLifecycle
              label=""
              name="SalesOrder"
              v-model="filter.statusId"
            />
          </div>
        </template>
      </TableFilter>
    </template>
    <Column field="number" header="Número" sortable style="width: 10%"></Column>
    <Column field="date" header="Data" style="width: 10%" sortable>
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
    <Column
      field="expectedDate"
      header="Data Entrega"
      style="width: 10%"
      sortable
    >
      <template #body="slotProps">
        {{
          slotProps.data.expectedDate
            ? formatDate(slotProps.data.expectedDate)
            : ""
        }}
      </template>
    </Column>
    <Column
      field="customerComercialName"
      header="Client"
      style="width: 30%"
    ></Column>
    <Column
      field="customerNumber"
      header="Comanda client"
      style="width: 15%"
    ></Column>
    <Column header="Estat" style="width: 20%">
      <template #body="slotProps">
        {{ getStatusNameById(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="
            slotProps.data.statusId ===
            lifecycleStore.lifecycle?.initialStatusId
          "
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteSalesInvoice($event, slotProps.data)"
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
    <FormCreateOrderOrInvoice
      :create-request="createRequest"
      @submit="createOrder"
    />
  </Dialog>
</template>
<script setup lang="ts">
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import DropdownLifecycle from "../../shared/components/DropdownLifecycle.vue";
import TableFilter from "../../../components/tables/TableFilter.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { onMounted, onUnmounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useSalesOrderStore } from "../store/order";
import { useCustomersStore } from "../store/customers";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  formatDateForQueryParameter,
  formatDate,
  getNewUuid,
} from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import { CreateSalesHeaderRequest, SalesOrderHeader } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useUserFilterStore } from "../../../store/userfilter";

const router = useRouter();
const toast = useToast();
const confirm = useConfirm();

const store = useStore();
const userFilterStore = useUserFilterStore();
const salesOrderStore = useSalesOrderStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "66%", tablet: "100%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  customerId: undefined as string | undefined,
  statusId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: "Crear comanda",
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
  lifecycleStore.fetchOneByName("SalesOrder");
  customerStore.fetchCustomers();

  setCurrentYear();
  getUserFilter();
  await filterSalesOrder();

  store.setMenuItem({
    icon: PrimeIcons.APPLE,
    title: "Comandes",
  });
});
onUnmounted(() => {
  userFilterStore.addFilter("SalesOrders", "", filter.value);
  salesOrderStore.salesOrders = undefined;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("SalesOrders", "");
  if (userFilter) {
    filter.value.statusId = userFilter.statusId;
    filter.value.customerId = userFilter.customerId;
    if (userFilter.dates) {
      filter.value.dates = [
        new Date(userFilter.dates[0]),
        new Date(userFilter.dates[1]),
      ];
    }
  }
};

const cleanFilter = () => {
  filter.value.customerId = undefined;
  filter.value.statusId = undefined;
  setCurrentYear();
  filterSalesOrder();
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

const filterSalesOrder = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await salesOrderStore.GetFiltered(
      startTime,
      endTime,
      filter.value.customerId,
      filter.value.statusId,
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

const getStatusNameById = (id: string) => {
  const status = lifecycleStore.lifecycle?.statuses?.find((s) => s.id === id);
  if (status) return status.name;
  else return "";
};

const createOrder = async () => {
  const response = await salesOrderStore.Create(createRequest.value);
  if (!response?.result) {
    toast.add({
      severity: "warn",
      summary: "Error al crear la comanda",
      detail:
        response?.errors?.[0] ??
        "Error desconegut, contacte amb l'administrador.",
      life: 10000,
    });
    return;
  }
  dialogOptions.visible = false;
  router.push({ path: `/salesorder/${createRequest.value.id}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/salesorder/${row.data.id}` });
  }
};

const deleteSalesInvoice = (event: any, order: SalesOrderHeader) => {
  confirm.require({
    message: `Està segur que vol eliminar la comanda?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await salesOrderStore.Delete(order.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminada",
          life: 3000,
        });

        await filterSalesOrder();
      }
    },
  });
};
</script>

<template>
  <Table
    :columns="columns"
    :items="deliveryNoteStore.deliveryNotes ?? []"
    :filter-config="[]"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    preset="crud-list"
    page="DeliveryNotes"
    class="small-datatable"
    tableStyle="min-width: 100%"
    showDeleteColumn
    :canDelete="(item) => !item.salesInvoiceId && item.statusId === lifecycleStore.lifecycle?.initialStatusId"
    @filter="filterData"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteDeliveryNote"
    @row-click="editRow"
  >
    <template #prepend>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">Període</label>
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
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">Client</label>
        <DropdownCustomers label="" v-model="filter.customerId" />
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
      @submit="createDeliveryNote"
    />
  </Dialog>
</template>
<script setup lang="ts">
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "../../../components/tables/types";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { onMounted, onUnmounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useCustomersStore } from "../store/customers";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  formatDateForQueryParameter,
  getNewUuid,
} from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import { CreateSalesHeaderRequest, SalesOrderHeader } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useDeliveryNoteStore } from "../store/deliveryNote";

const router = useRouter();
const toast = useToast();
const confirm = useConfirm();
const store = useStore();
const deliveryNoteStore = useDeliveryNoteStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

const columns = ref<Column[]>([
  { field: "number", header: "Número", sortable: true, style: "width: 15%" },
  { field: "createdOn", header: "Data Creació", sortable: true, columnType: ColumnType.Date, style: "width: 15%" },
  { field: "deliveryDate", header: "Data Entrega", columnType: ColumnType.Date, style: "width: 15%" },
  {
    field: "customerId",
    header: "Client",
    columnType: ColumnType.Lookup,
    resolver: customerStore.getCustomerNameById,
    style: "width: 30%",
  },
  {
    field: "statusId",
    header: "Estat",
    columnType: ColumnType.Lookup,
    resolver: lifecycleStore.getStatusNameById,
    style: "width: 30%",
  },
]);

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  customerId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: "Crear albarà",
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
  lifecycleStore.fetchOneByName("DeliveryNote");
  customerStore.fetchCustomers();

  setCurrentYear();
  await filterData();

  store.setMenuItem({
    icon: PrimeIcons.APPLE,
    title: "Albarans d'entrega",
  });
});

onUnmounted(() => {
  deliveryNoteStore.deliveryNotes = undefined;
});

const cleanFilter = () => {
  filter.value.customerId = undefined;
  setCurrentYear();
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

const filterData = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await deliveryNoteStore.GetFiltered(
      startTime,
      endTime,
      filter.value.customerId,
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

const createDeliveryNote = async () => {
  const response = await deliveryNoteStore.Create(createRequest.value);
  if (!response?.result) {
    toast.add({
      severity: "warn",
      summary: "Error al crear l'albarà",
      detail:
        response?.errors?.[0] ??
        "Error desconegut, contacte amb l'administrador.",
      life: 10000,
    });
    return;
  }
  dialogOptions.visible = false;
  router.push({ path: `/deliverynote/${createRequest.value.id}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/deliverynote/${row.data.id}` });
};

const deleteDeliveryNote = async (order: SalesOrderHeader) => {
  confirm.require({
    message: `Està segur que vol eliminar l'albarà?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await deliveryNoteStore.Delete(order.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminada",
          life: 3000,
        });

        await filterData();
      }
    },
  });
};
</script>
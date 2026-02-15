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
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter-3">
          <div class="filter-field">
            <ExerciseDatePicker
              :exercises="sharedStore.exercises"
              @range-selected="filterSalesOrder"
            />
          </div>
          <div class="filter-field">
            <label class="block text-900">Client</label>
            <DropdownCustomers label="" v-model="filter.customerId" />
          </div>
          <div class="filter-field">
            <label class="block text-900">Estat</label>
            <DropdownLifecycle
              label=""
              name="SalesOrder"
              v-model="filter.statusId"
            />
          </div>
        </div>
        <div class="datatable-buttons">
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER"
            rounded
            raised
            @click="filterSalesOrder"
          />
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER_SLASH"
            rounded
            raised
            @click="cleanFilter"
          />
          <Button
            :icon="PrimeIcons.PLUS"
            rounded
            raised
            @click="createButtonClick"
          />
        </div>
      </div>
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
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import DropdownLifecycle from "../../shared/components/DropdownLifecycle.vue";
import { onMounted, onUnmounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useSalesOrderStore } from "../store/order";
import { useReferenceStore } from "../../shared/store/reference";
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
import { useSharedDataStore } from "../../shared/store/masterData";
import { useConfirm } from "primevue/useconfirm";
import { useUserFilterStore } from "../../../store/userfilter";

const router = useRouter();
const toast = useToast();
const confirm = useConfirm();

const store = useStore();
const userFilterStore = useUserFilterStore();
const sharedStore = useSharedDataStore();
const salesOrderStore = useSalesOrderStore();
const referenceStore = useReferenceStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();

const filter = ref({
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

onMounted(async () => {
  lifecycleStore.fetchOneByName("SalesOrder");
  referenceStore.fetchReferences();
  customerStore.fetchCustomers();
  await sharedStore.fetchMasterData();

  setCurrentYear();
  getUserFilter();
  await filterSalesOrder();

  store.setMenuItem({
    icon: PrimeIcons.APPLE,
    title: "Comandes",
  });
});
onUnmounted(() => {
  const savedFilter = {
    statusId: filter.value.statusId,
    customerId: filter.value.customerId,
    exercisePicker: store.exercisePicker,
  };

  userFilterStore.addFilter("SalesOrders", "", savedFilter);

  salesOrderStore.salesOrders = undefined;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("SalesOrders", "");
  if (userFilter) {
    filter.value.statusId = userFilter.statusId;
    filter.value.customerId = userFilter.customerId;
    if (userFilter.exercisePicker) {
      store.exercisePicker.exercise = userFilter.exercisePicker.exercise;
      store.exercisePicker.dates = [
        new Date(userFilter.exercisePicker.dates[0]),
        new Date(userFilter.exercisePicker.dates[1]),
      ];
    }
  }
};

const setCurrentYear = () => {
  const year = new Date().getFullYear().toString();
  const currentExercise = sharedStore.exercises?.find((e) => e.name === year);

  if (currentExercise) {
    store.exercisePicker.exercise = currentExercise;
    store.exercisePicker.dates = [
      new Date(store.exercisePicker.exercise.startDate),
      new Date(store.exercisePicker.exercise.endDate),
    ];
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
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0],
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);

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
  dialogOptions.visible = false;
  const created = await salesOrderStore.Create(createRequest.value);
  if (created) {
    router.push({ path: `/salesorder/${createRequest.value.id}` });
  }
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

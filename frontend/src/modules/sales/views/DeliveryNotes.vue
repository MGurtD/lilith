<template>
  <DataTable
    :value="deliveryNoteStore.deliveryNotes"
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortMode="single"
    paginator
    :rows="20"
    @row-click="editRow"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter">
          <div class="filter-field">
            <ExerciseDatePicker
              :exercises="sharedDataStore.exercises"
              @range-selected="filterData"
            />
          </div>
          <div class="filter-field">
            <label>Client</label>
            <DropdownCustomers label="" v-model="filter.customerId" />
          </div>
        </div>
        <div class="datatable-buttons">
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER"
            rounded
            raised
            @click="filterData"
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
    <Column field="number" header="Número" sortable style="width: 15%"></Column>
    <Column field="createdOn" header="Data Creació" sortable style="width: 15%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.createdOn) }}
      </template>
    </Column>
    <Column header="Data Entrega" style="width: 15%">
      <template #body="slotProps">
        {{
          slotProps.data.deliveryDate
            ? formatDate(slotProps.data.deliveryDate)
            : ""
        }}
      </template>
    </Column>
    <Column header="Client" style="width: 30%">
      <template #body="slotProps">
        {{ getCustomerById(slotProps.data.customerId) }}
      </template>
    </Column>
    <Column header="Estat" style="width: 30%">
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
      @submit="createDeliveryNote"
    />
  </Dialog>
</template>
<script setup lang="ts">
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import { onMounted, onUnmounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
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
import { useConfirm } from "primevue/useconfirm";
import { useDeliveryNoteStore } from "../store/deliveryNote";
import { useSharedDataStore } from "../../shared/store/masterData";
import { useUserFilterStore } from "../../../store/userfilter";

const router = useRouter();
const toast = useToast();
const confirm = useConfirm();
const store = useStore();
const userFilterStore = useUserFilterStore();
const sharedDataStore = useSharedDataStore();
const deliveryNoteStore = useDeliveryNoteStore();
const referenceStore = useReferenceStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();

const filter = ref({
  customerId: undefined as string | undefined,
  referenceId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: "Crear albarà",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  lifecycleStore.fetchOneByName("DeliveryNote");
  referenceStore.fetchReferences();
  customerStore.fetchCustomers();
  await sharedDataStore.fetchMasterData();

  setCurrentYear();
  getUserFilter();
  await filterData();

  store.setMenuItem({
    icon: PrimeIcons.APPLE,
    title: "Albarans d'entrega",
  });
});

onUnmounted(() => {
  const savedFilter = {
    referenceId: filter.value.referenceId,
    customerId: filter.value.customerId,
    exercisePicker: store.exercisePicker,
  };

  userFilterStore.addFilter("DeliveryNotes", "", savedFilter);

  deliveryNoteStore.deliveryNotes = undefined;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("DeliveryNotes", "");
  if (userFilter) {
    filter.value.referenceId = userFilter.referenceId;
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
  const currentExercise = sharedDataStore.exercises?.find(
    (e) => e.name === year,
  );

  if (currentExercise) {
    store.exercisePicker.exercise = currentExercise;
    store.exercisePicker.dates = [
      new Date(store.exercisePicker.exercise.startDate),
      new Date(store.exercisePicker.exercise.endDate),
    ];
  }
};

const cleanFilter = () => {
  store.cleanExercisePicker();
  filter.value.customerId = undefined;
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
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0],
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);

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

const getStatusNameById = (id: string) => {
  const status = lifecycleStore.lifecycle?.statuses?.find((s) => s.id === id);
  if (status) return status.name;
  else return "";
};

const getCustomerById = (id: string) => {
  const status = customerStore.customers?.find((s) => s.id === id);
  if (status) return status.comercialName;
  else return "";
};

const createDeliveryNote = async () => {
  dialogOptions.visible = false;
  const created = await deliveryNoteStore.Create(createRequest.value);
  if (created) {
    router.push({ path: `/deliverynote/${createRequest.value.id}` });
  }
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/deliverynote/${row.data.id}` });
  }
};

const deleteSalesInvoice = (event: any, order: SalesOrderHeader) => {
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

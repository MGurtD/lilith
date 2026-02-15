<template>
  <TableInvoices
    :invoices="invoiceStore.invoices"
    :customers="customersStore.customers"
    @edit="editSalesInvoice"
    @delete="deleteSalesInvoice"
  >
    <template #filter>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-1"
      >
        <div class="datatable-filter">
          <div class="filter-field">
            <ExerciseDatePicker
              :exercises="sharedStore.exercises"
              @range-selected="filterInvoices"
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
            @click="filterInvoices"
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
  </TableInvoices>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreateOrderOrInvoice
      :create-request="createRequest"
      @submit="createInvoice"
    />
  </Dialog>
</template>
<script setup lang="ts">
import TableInvoices from "../components/TableInvoices.vue";
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useSalesInvoiceStore } from "../store/invoice";
import { useCustomersStore } from "../store/customers";
import { useSharedDataStore } from "../../shared/store/masterData";
import { onMounted, onUnmounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import {
  formatDateForQueryParameter,
  getNewUuid,
} from "../../../utils/functions";
import { CreateSalesHeaderRequest, SalesInvoice } from "../types";
import { DialogOptions } from "../../../types/component";
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import { useUserFilterStore } from "../../../store/userfilter";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const userFilterStore = useUserFilterStore();
const sharedStore = useSharedDataStore();
const customersStore = useCustomersStore();
const invoiceStore = useSalesInvoiceStore();

const filter = ref({
  customerId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: "Crear factura",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  customersStore.fetchCustomers();
  await sharedStore.fetchMasterData();
  setCurrentYear();
  getUserFilter();
  await filterInvoices();

  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: "Factures de venta",
  });
});

onUnmounted(() => {
  const savedFilter = {
    customerId: filter.value.customerId,
    exercisePicker: store.exercisePicker,
  };
  userFilterStore.addFilter("SalesInvoices", "", savedFilter);

  invoiceStore.invoices = undefined;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("SalesInvoices", "");
  if (userFilter) {
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

const cleanFilter = () => {
  store.cleanExercisePicker();
  filter.value.customerId = undefined;
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

const filterInvoices = async () => {
  let startTime = "";
  let endTime = "";

  if (store.exercisePicker.dates) {
    startTime = formatDateForQueryParameter(store.exercisePicker.dates[0]);
    endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);
  }

  await invoiceStore.GetFiltered(
    startTime,
    endTime,
    undefined,
    filter.value.customerId,
    undefined,
  );
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

const createInvoice = async () => {
  const response = await invoiceStore.Create(createRequest.value);
  if (response && !response?.result) {
    const errorMessage =
      response.errors.length > 0
        ? response.errors[0]
        : "Error desconegut, contacte amb l'administrador.";

    toast.add({
      severity: "warn",
      summary: "Error al crear la factura",
      detail: errorMessage,
      life: 10000,
    });
    return;
  }

  if (response)
    router.push({ path: `/sales-invoice/${createRequest.value.id}` });
};

const editSalesInvoice = (invoice: SalesInvoice) => {
  router.push({ path: `/sales-invoice/${invoice.id}` });
};

const deleteSalesInvoice = (invoice: SalesInvoice) => {
  confirm.require({
    message: `Està segur que vol eliminar la factura?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await invoiceStore.Delete(invoice.id);
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

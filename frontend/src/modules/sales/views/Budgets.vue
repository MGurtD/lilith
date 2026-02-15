<template>
  <DataTable
    :value="budgetStore.budgets"
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
              :exercises="exerciseStore.exercises"
              @range-selected="filterBudget"
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
              name="Budget"
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
            @click="filterBudget"
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
    <Column field="number" header="Número" style="width: 10%"></Column>
    <Column field="date" header="Data" style="width: 15%" sortable>
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
    <Column header="Client" style="width: 20%">
      <template #body="slotProps">
        {{ getCustomerById(slotProps.data.customerId) }}
      </template>
    </Column>
    <Column header="Estat" style="width: 20%">
      <template #body="slotProps">
        {{ getStatusNameById(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column
      field="acceptanceDate"
      header="Data d'acceptació"
      style="width: 15%"
    >
      <template #body="slotProps">
        {{
          slotProps.data.acceptanceDate
            ? formatDate(slotProps.data.acceptanceDate)
            : ""
        }}
      </template>
    </Column>
    <Column
      field="deliveryDays"
      header="Dies d'entrega"
      style="width: 10%"
    ></Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="
            slotProps.data.statusId ===
            lifecycleStore.lifecycle?.initialStatusId
          "
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteBudget($event, slotProps.data)"
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
import { Budget, CreateSalesHeaderRequest } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useBudgetStore } from "../store/budget";
import { useUserFilterStore } from "../../../store/userfilter";
import { useExerciseStore } from "../../shared/store/exercise";
import { Exercise } from "../../shared/types";

const router = useRouter();
const toast = useToast();
const confirm = useConfirm();
const store = useStore();
const userFilterStore = useUserFilterStore();
const exerciseStore = useExerciseStore();
const budgetStore = useBudgetStore();
const referenceStore = useReferenceStore();
const customerStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();

const filter = ref({
  customerId: undefined as string | undefined,
  statusId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: "Crear pressupost",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const currentExercise = ref(undefined as Exercise | undefined);

onMounted(async () => {
  lifecycleStore.fetchOneByName("Budget");
  referenceStore.fetchReferences();
  customerStore.fetchCustomers();
  await exerciseStore.fetchActive();

  setCurrentYear();
  getUserFilter();
  await filterBudget();

  store.setMenuItem({
    icon: PrimeIcons.APPLE,
    title: "Pressupostos",
  });
});
onUnmounted(() => {
  const savedFilter = {
    statusId: filter.value.statusId,
    customerId: filter.value.customerId,
    exercisePicker: store.exercisePicker,
  };

  userFilterStore.addFilter("Budgets", "", savedFilter);

  budgetStore.budgets = undefined;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("Budgets", "");
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
  currentExercise.value = exerciseStore.exercises?.find((e) => e.name === year);

  if (currentExercise.value) {
    store.exercisePicker.exercise = currentExercise.value;
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

  filterBudget();
};

const createRequest = ref({} as CreateSalesHeaderRequest);
const generateNewRequest = (): CreateSalesHeaderRequest => {
  return {
    id: getNewUuid(),
    customerId: "",
    exerciseId: currentExercise.value?.id || "",
    date: new Date(),
  };
};

const createButtonClick = () => {
  createRequest.value = generateNewRequest();
  dialogOptions.visible = true;
};

const filterBudget = async () => {
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0],
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);

    await budgetStore.GetFiltered(
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
const getCustomerById = (id: string) => {
  const customer = customerStore.customers?.find((c) => c.id === id);
  if (customer) return customer.comercialName;
  else return "";
};

const createOrder = async () => {
  dialogOptions.visible = false;
  const created = await budgetStore.Create(createRequest.value);
  if (created) {
    router.push({ path: `/budget/${createRequest.value.id}` });
  }
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/budget/${row.data.id}` });
  }
};

const deleteBudget = async (event: any, budget: Budget) => {
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

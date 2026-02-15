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
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter">
          <div class="filter-field">
            <ExerciseDatePicker
              :exercises="exerciseStore.exercises"
              @range-selected="filterData"
            />
          </div>
          <div class="filter-field">
            <label class="block text-900 mb-2">Proveïdor</label>
            <Select
              v-model="filter.supplierId"
              :options="suppliersStore.suppliers"
              optionValue="id"
              optionLabel="comercialName"
              class="w-full"
            />
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
    <Column
      field="number"
      header="Número"
      :sortable="true"
      style="width: 10%"
    ></Column>
    <Column header="Data" field="date" sortable style="width: 10%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
    <Column header="Proveïdor" style="width: 15%">
      <template #body="slotProps">
        {{ getSupplierNameById(slotProps.data.supplierId) }}
      </template>
    </Column>
    <Column header="Estat" style="width: 15%">
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
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useOrderStore } from "../store/order";
import { useSuppliersStore } from "../store/suppliers";
import { useExerciseStore } from "../../shared/store/exercise";
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
import { Exercise } from "../../shared/types";
import { useLifecyclesStore } from "../../shared/store/lifecycle";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const suppliersStore = useSuppliersStore();
const exerciseStore = useExerciseStore();
const lifecycleStore = useLifecyclesStore();
const ordersStore = useOrderStore();

const filter = ref({
  supplierId: undefined as string | undefined,
  exercise: undefined as Exercise | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: "Crear comanda",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: "Comandes de compra",
  });

  suppliersStore.fetchSuppliers();
  await exerciseStore.fetchActive();
  await lifecycleStore.fetchOneByName("PurchaseOrder");
  setCurrentYear();

  await filterData();
});

const setCurrentYear = () => {
  const year = new Date().getFullYear().toString();
  const currentExercise = exerciseStore.exercises?.find((e) => e.name === year);

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
  filter.value.supplierId = undefined;
};

const filterData = async () => {
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0],
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);

    await ordersStore.fetchFiltered(
      startTime,
      endTime,
      filter.value.supplierId,
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
    message: `Està segur que vol eliminar la comanda ${invoice.number}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await ordersStore.delete(invoice.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await filterData();
      }
    },
  });
};
</script>

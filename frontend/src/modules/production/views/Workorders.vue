<template>
  <TableWorkorders
    :workorders="filteredWorkorders"
    @edit="editRow"
    @delete="deleteButton"
  >
    <template #header>
      <TableFilter
        :config="filterConfig"
        :model-value="filter"
        @filter="filterData"
        @clear="cleanFilter"
        @create="createButtonClick"
      >
        <template #prepend>
          <ExerciseDatePicker :exercises="exerciseStore.exercises" />
        </template>
      </TableFilter>
    </template>
  </TableWorkorders>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <FormCreateWorkorder
      :createWorkOrderDto="createWorkOrderDto"
      @submit="createWorkOrder"
    ></FormCreateWorkorder>
  </Dialog>
</template>
<script setup lang="ts">
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";
import FormCreateWorkorder from "../components/FormCreateWorkorder.vue";
import TableWorkorders from "../components/TableWorkorders.vue";
import TableFilter from "../../../components/tables/TableFilter.vue";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useReferenceStore } from "../../shared/store/reference";
import { CreateWorkOrderDto, WorkOrder } from "../types";
import { formatDateForQueryParameter } from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import { useExerciseStore } from "../../shared/store/exercise";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useWorkOrderStore } from "../store/workorder";
import { useWorkMasterStore } from "../store/workmaster";
import { useUserFilterStore } from "../../../store/userfilter";
import { useCustomersStore } from "../../sales/store/customers";
import { FilterConfig } from "../../../components/tables/TableFilter.vue";

const router = useRouter();
const store = useStore();
const userFilterStore = useUserFilterStore();
const toast = useToast();
const confirm = useConfirm();
const workMasterStore = useWorkMasterStore();
const workOrderStore = useWorkOrderStore();
const referenceStore = useReferenceStore();
const exerciseStore = useExerciseStore();
const lifecycleStore = useLifecyclesStore();
const customersStore = useCustomersStore();

const filter = ref({
  referenceId: undefined,
  statusId: undefined as string | undefined, // Type correction for compatibility
  customerId: undefined,
  plannedQuantity: undefined,
  code: undefined,
});

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "customerId",
    label: "Client",
    type: "select",
    options: customersStore.customers || [],
    optionLabel: "comercialName",
    optionValue: "id",
    placeholder: "Selecciona un client",
    row: 0,
  },
  {
    key: "statusId",
    label: "Estat",
    type: "select",
    options: lifecycleStore.lifecycle?.statuses || [],
    optionLabel: "name",
    optionValue: "id",
    placeholder: "Selecciona un estat",
    row: 0,
  },
  {
    key: "code",
    label: "Codi",
    type: "text",
    placeholder: "Codi",
    row: 1,
  },
  {
    key: "plannedQuantity",
    label: "Quantitat planificada",
    type: "number",
    placeholder: "Quantitat planificada",
    row: 1,
  },
]);

const cleanFilter = () => {
  filter.value.referenceId = undefined;
  filter.value.statusId = undefined;
  filter.value.customerId = undefined;
  filter.value.code = undefined;
  filter.value.plannedQuantity = undefined;

  userFilterStore.removeFilter("Workorders", "");
};
const filteredWorkorders = computed(() => {
  let result = workOrderStore.workorders;
  if (filter.value.plannedQuantity) {
    result = result.filter(
      (w) => w.plannedQuantity === filter.value.plannedQuantity,
    );
  }
  return result;
});
const filterData = async () => {
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0],
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);

    await workOrderStore.fetchFiltered(
      startTime,
      endTime,
      filter.value.statusId,
      filter.value.referenceId,
      filter.value.customerId,
      filter.value.code,
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

const dialogOptions = reactive({
  visible: false,
  title: "Crear ordre",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const createWorkOrderDto = ref({
  workMasterId: "",
  plannedDate: "",
  plannedQuantity: 0,
  comment: "",
} as CreateWorkOrderDto);

onMounted(async () => {
  await referenceStore.fetchReferencesByModule("sales");
  await exerciseStore.fetchActive();
  // We need to fetch customers for the filter
  if (!customersStore.customers) await customersStore.fetchCustomers();

  // We need to wait for lifecycle to populate filter options
  await lifecycleStore.fetchOneByName("WorkOrder");
  workMasterStore.fetchAllActives();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: "Ordres de fabricació",
  });

  getUserFilter();
  if (!store.exercisePicker.exercise) store.setCurrentYear();
  filterData();
});
onUnmounted(() => {
  const savedFilter = {
    referenceId: filter.value.referenceId,
    statusId: filter.value.statusId,
    customerId: filter.value.customerId,
    exercisePicker: store.exercisePicker,
  };

  userFilterStore.addFilter("Workorders", "", savedFilter);
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("Workorders", "");
  if (userFilter) {
    filter.value.referenceId = userFilter.referenceId;
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

const createButtonClick = () => {
  dialogOptions.visible = true;
};

const editRow = (workorder: WorkOrder) => {
  router.push({ path: `/workorder/${workorder.id}` });
};

const createWorkOrder = async () => {
  if (!createWorkOrderDto.value) return;

  const created = await workOrderStore.create(createWorkOrderDto.value);
  if (created && workOrderStore.workorder)
    router.push({ path: `/workorder/${workOrderStore.workorder.id}` });
};

const deleteButton = (workorder: WorkOrder) => {
  confirm.require({
    message: `Está segur que vol eliminar la ordre ${workorder.code}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await workOrderStore.delete(workorder.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminada",
          life: 3000,
        });
        filterData();
      }
    },
  });
};
</script>

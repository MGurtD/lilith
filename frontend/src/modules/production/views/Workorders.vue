<template>
  <TableWorkorders
    :workorders="workOrderStore.workorders"
    @edit="editRow"
    @delete="deleteButton"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter-3">
          <div class="filter-field">
            <ExerciseDatePicker :exercises="exerciseStore.exercises" />
          </div>
          <div class="filter-field">
            <label class="block text-900">Client</label>
            <DropdownCustomers label="" v-model="filter.customerId" />
          </div>
          <div class="filter-field">
            <label class="block text-900">Estat</label>
            <DropdownLifecycle
              label=""
              name="WorkOrder"
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
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";
import DropdownLifecycle from "../../shared/components/DropdownLifecycle.vue";
import FormCreateWorkorder from "../components/FormCreateWorkorder.vue";
import TableWorkorders from "../components/TableWorkorders.vue";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { onMounted, onUnmounted, reactive, ref } from "vue";
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

const filter = ref({
  referenceId: undefined,
  statusId: undefined,
  customerId: undefined,
});
const cleanFilter = () => {
  filter.value.referenceId = undefined;
  filter.value.statusId = undefined;
  filter.value.customerId = undefined;

  userFilterStore.removeFilter("Workorders", "");
};
const filterData = async () => {
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0]
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);

    await workOrderStore.fetchFiltered(
      startTime,
      endTime,
      filter.value.statusId,
      filter.value.referenceId,
      filter.value.customerId
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
  lifecycleStore.fetchOneByName("WorkOrder");
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

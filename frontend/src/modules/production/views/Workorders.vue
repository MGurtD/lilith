<template>
  <TableWorkorders
    :workorders="filteredWorkorders"
    @edit="editRow"
    @delete="deleteButton"
  >
    <template #header>
      <TableFilter
        :config="filterConfig"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :body-width="filterBodyWidth"
        embedded
        @filter="filterData"
        @clear="cleanFilter"
        @create="createButtonClick"
      >
        <template #prepend>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >{{ pt("Període") }}</label
            >
            <DatePicker
              v-model="filter.dates"
              selectionMode="range"
              dateFormat="dd/mm/yy"
              :placeholder="pt('Seleccioni un període')"
              showIcon
              class="w-full"
              size="small"
            />
          </div>
        </template>
      </TableFilter>
    </template>
  </TableWorkorders>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '600px' }"
  >
    <FormCreateWorkorder
      :createWorkOrderDto="createWorkOrderDto"
      @submit="createWorkOrder"
    ></FormCreateWorkorder>
  </Dialog>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
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
import {
  FilterConfig,
  FilterBodyWidth,
} from "../../../components/tables/TableFilter.vue";

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

const filterBodyWidth: FilterBodyWidth = { desktop: "75%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  referenceId: undefined,
  statusId: undefined as string | undefined,
  customerId: undefined,
  code: undefined,
});

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "customerId",
    label: pt("Client"),
    type: "select",
    options: customersStore.customers || [],
    optionLabel: "comercialName",
    optionValue: "id",
    placeholder: pt("Selecciona un client"),
    size: "md",
    row: 0,
  },
  {
    key: "code",
    label: pt("Codi"),
    type: "text",
    placeholder: pt("Codi"),
    size: "md",
    row: 0,
  },
  {
    key: "statusId",
    label: pt("Estat"),
    type: "select",
    options: lifecycleStore.lifecycle?.statuses || [],
    optionLabel: "name",
    optionValue: "id",
    placeholder: pt("Selecciona un estat"),
    size: "md",
    row: 0,
  },
]);

const setCurrentYear = () => {
  const year = new Date().getFullYear().toString();
  const currentExercise = exerciseStore.exercises?.find((e) => e.name === year);

  if (currentExercise) {
    filter.value.dates = [
      new Date(currentExercise.startDate),
      new Date(currentExercise.endDate),
    ];
  }
};

const cleanFilter = () => {
  filter.value.referenceId = undefined;
  filter.value.statusId = undefined;
  filter.value.customerId = undefined;
  filter.value.code = undefined;
  filter.value.dates = undefined;

  setCurrentYear();
  userFilterStore.removeFilter("Workorders", "");
};
const filteredWorkorders = computed(() => {
  return workOrderStore.workorders ?? [];
});
const filterData = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

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
      summary: pt("Filtre invàlid"),
      detail: pt("Seleccioni un període"),
      life: 5000,
    });
  }
};

const dialogOptions = reactive({
  visible: false,
  title: pt("Crear ordre"),
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
    title: pt("Ordres de fabricació"),
  });

  getUserFilter();
  if (!filter.value.dates) setCurrentYear();
  filterData();
});
onUnmounted(() => {
  userFilterStore.addFilter("Workorders", "", filter.value);
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("Workorders", "");
  if (userFilter) {
    filter.value.referenceId = userFilter.referenceId;
    filter.value.statusId = userFilter.statusId;
    filter.value.customerId = userFilter.customerId;
    filter.value.code = userFilter.code;
    if (userFilter.dates) {
      filter.value.dates = [
        new Date(userFilter.dates[0]),
        new Date(userFilter.dates[1]),
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
    message: pt("Confirmar l'eliminació de l'ordre de fabricació"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await workOrderStore.delete(workorder.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: pt("Eliminada"),
          life: 3000,
        });
        filterData();
      }
    },
  });
};
</script>

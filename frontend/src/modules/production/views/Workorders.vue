<template>
  <Table
    :items="workOrderStore.workorders ?? []"
    :columns="columns"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-labels="filterMetadata.filterLabels"
    :filter-value-resolvers="filterMetadata.filterValueResolvers"
    :filter-body-width="filterBodyWidth"
    page="Workorders"
    preset="crud-list"
    tableStyle="min-width: 100%"
    sort-mode="multiple"
    show-delete-column
    @filter="filterData"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editRow"
  >
    <template #prepend>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">{{
          pt("Període")
        }}</label>
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
  </Table>

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
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { createTableViewFilterMetadata } from "@/components/tables/table-view-filter-metadata";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import FormCreateWorkorder from "../components/FormCreateWorkorder.vue";
import { onBeforeRouteLeave, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { computed, onMounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
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

const columns = computed<Column[]>(() => [
  {
    field: "code",
    header: t("production.components.codi"),
    style: "width: 15%",
  },
  {
    field: "referenceId",
    header: t("production.components.referencia"),
    columnType: ColumnType.Lookup,
    resolver: referenceStore.getFullNameById,
    style: "width: 40%",
  },
  {
    field: "reference.customerId",
    header: t("production.components.client"),
    columnType: ColumnType.Lookup,
    resolver: customersStore.getCustomerNameById,
    style: "width: 15%",
  },
  {
    field: "statusId",
    header: t("production.components.estat"),
    columnType: ColumnType.Lookup,
    resolver: lifecycleStore.getStatusNameById,
    style: "width: 10%",
  },
  {
    field: "plannedDate",
    header: t("production.components.dataPrevista"),
    sortable: true,
    columnType: ColumnType.Date,
    style: "width: 12%",
  },
  {
    field: "order",
    header: t("production.components.prioritat"),
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
  {
    field: "plannedQuantity",
    header: t("production.components.quantitat"),
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
]);

const filterMetadata = computed(() =>
  createTableViewFilterMetadata(columns.value, {
    labels: {
      dates: pt("Període"),
      customerId: pt("Client"),
      referenceId: pt("Referència"),
    },
    valueResolvers: {
      customerId: (value) =>
        typeof value === "string"
          ? (customersStore.getCustomerNameById(value) ?? "")
          : "",
      referenceId: (value) =>
        typeof value === "string"
          ? (referenceStore.getFullNameById(value) ?? "")
          : "",
    },
  }),
);

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
onBeforeRouteLeave(async () => {
  await userFilterStore.addFilter("Workorders", "", filter.value);
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

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/workorder/${row.data.id}` });
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

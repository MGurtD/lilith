<template>
  <Table
    :items="calculatedProductionParts ?? []"
    :columns="columns"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-labels="filterMetadata.filterLabels"
    :filter-value-resolvers="filterMetadata.filterValueResolvers"
    :filter-body-width="filterBodyWidth"
    page="ProductionParts"
    class="p-datatable-sm small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :sort-order="1"
    sort-field="date"
    paginator
    :rows="20"
    show-delete-column
    @filter="filterData"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteProductionPart"
  >
    <template #empty> {{ pt("No s'han trobat tiquets.") }} </template>
    <template #loading>
      {{ pt("Carregant tiquets. Si us plau espera.") }}
    </template>
  </Table>
  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <FormProductionPart
      :productionPart="productionPartRequest"
      :avoid-work-order-refresh="false"
      @submit="createProductionPart"
    />
  </Dialog>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { createTableViewFilterMetadata } from "@/components/tables/table-view-filter-metadata";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "@/components/tables/TableFilter.vue";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import { onBeforeRouteLeave, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useConfirm } from "primevue/useconfirm";
import { computed, onMounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { ProductionPart } from "../types";
import {
  formatDateForQueryParameter,
  formatCurrency,
  getNewUuid,
} from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import { useExerciseStore } from "../../shared/store/exercise";
import { useProductionPartStore } from "../store/productionpart";
import { usePlantModelStore } from "../store/plantmodel";
import { useWorkOrderStore } from "../store/workorder";
import FormProductionPart from "../components/FormProductionPart.vue";
import _ from "lodash";
import { useUserFilterStore } from "../../../store/userfilter";

const router = useRouter();
const store = useStore();
const userFilterStore = useUserFilterStore();
const toast = useToast();
const productionPartStore = useProductionPartStore();
const exerciseStore = useExerciseStore();
const plantModelStore = usePlantModelStore();
const workOrderStore = useWorkOrderStore();
const confirm = useConfirm();

const filterBodyWidth: FilterBodyWidth = { desktop: "80%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  operatorId: "" as string,
  workcenterId: "" as string,
  workorderId: "" as string,
});

const workcenterOptions = computed(() =>
  [...(plantModelStore.workcenters ?? [])].sort((a, b) =>
    a.description.localeCompare(b.description),
  ),
);

const operatorOptions = computed(() =>
  [...(plantModelStore.operators ?? [])]
    .sort((a, b) => a.surname.localeCompare(b.surname))
    .map((operator) => ({
      value: operator.id,
      label: `${operator.surname}, ${operator.name}`,
    })),
);

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "dates",
    label: pt("Període"),
    type: "date-range",
    placeholder: pt("Seleccioni un període"),
    size: "lg",
  },
  {
    key: "workcenterId",
    label: pt("Màquina"),
    type: "select",
    options: workcenterOptions.value,
    optionValue: "id",
    optionLabel: "description",
    size: "md",
  },
  {
    key: "operatorId",
    label: pt("Operari"),
    type: "select",
    options: operatorOptions.value,
    optionValue: "value",
    optionLabel: "label",
    size: "md",
  },
  {
    key: "workorderId",
    label: "OF",
    type: "select",
    options: workOrderStore.workorders ?? [],
    optionValue: "id",
    optionLabel: "code",
    size: "md",
  },
]);

const filterData = async () => {
  if (filter.value.dates?.[0] && filter.value.dates[1]) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await productionPartStore.fetchFiltered(
      startTime,
      endTime,
      filter.value.workcenterId,
      filter.value.operatorId,
      filter.value.workorderId,
    );
    await workOrderStore.fetchFiltered(startTime, endTime);
  } else {
    toast.add({
      severity: "info",
      summary: pt("Filtre invàlid"),
      detail: pt("Seleccioni un període"),
      life: 5000,
    });
  }
};

const cleanFilter = () => {
  filter.value.dates = undefined;
  filter.value.workcenterId = "";
  filter.value.operatorId = "";
  filter.value.workorderId = "";
};

const dialogOptions = reactive({
  visible: false,
  title: pt("Crear tíquet de producció"),
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CLOUD,
    title: pt("Tíquets de producció"),
  });

  plantModelStore.fetchWorkcenters();
  plantModelStore.fetchOperators();
  plantModelStore.fetchOperatorTypes();
  plantModelStore.fetchMachineStatuses();
  await plantModelStore.fetchWorkcenterCosts();

  await exerciseStore.fetchActive();
  getUserFilter();
  if (!filter.value.dates) setCurrentYear();
  await filterData();

  workOrderStore.detailedWorkOrders = undefined;
});
onBeforeRouteLeave(async () => {
  await userFilterStore.addFilter("ProductionParts", "", filter.value);
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("ProductionParts", "");
  if (userFilter) {
    filter.value.operatorId = userFilter.operatorId;
    filter.value.workcenterId = userFilter.workcenterId;
    filter.value.workorderId = userFilter.workorderId;
    if (userFilter.dates) {
      filter.value.dates = userFilter.dates.map(
        (date: Date | string) => new Date(date),
      );
    }
    if (userFilter.exercisePicker) {
      if (!filter.value.dates && userFilter.exercisePicker.dates) {
        filter.value.dates = [
          new Date(userFilter.exercisePicker.dates[0]),
          new Date(userFilter.exercisePicker.dates[1]),
        ];
      }
    }
  }
};

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

const calculatedProductionParts = computed(() => {
  if (productionPartStore.productionParts) {
    return productionPartStore.productionParts.map((productionPart) => {
      return {
        ...productionPart,
        personalCost: getPersonalCost(productionPart),
        workcenterCost: getWorkCenterCost(productionPart),
      };
    });
  }
});

const totalPersonalCost = computed(() => {
  return (calculatedProductionParts.value ?? []).reduce(
    (acc, productionPart) => acc + (productionPart.personalCost ?? 0),
    0,
  );
});

const columns = computed<Column[]>(() => [
  {
    field: "workcenterId",
    header: pt("Màquina"),
    columnType: ColumnType.Lookup,
    resolver: plantModelStore.getWorkcenterNameById,
    style: "width: 15%",
  },
  {
    field: "operatorId",
    header: pt("Operari"),
    columnType: ColumnType.Lookup,
    resolver: plantModelStore.getOperatorNameById,
    style: "width: 15%",
  },
  {
    field: "workOrderId",
    header: "OF",
    resolver: (_value, data) =>
      getWorkOrderDetailedName(data as ProductionPart) ?? "",
    style: "width: 20%",
  },
  {
    field: "date",
    header: pt("Data"),
    sortable: true,
    columnType: ColumnType.Date,
    style: "width: 10%",
  },
  {
    field: "quantity",
    header: pt("Quantitat"),
    columnType: ColumnType.Number,
    total: "sum",
    style: "width: 5%",
  },
  {
    field: "workcenterTime",
    header: pt("Temps Maq."),
    columnType: ColumnType.Number,
    total: "sum",
    style: "width: 10%",
  },
  {
    field: "operatorTime",
    header: pt("Temps Oper."),
    columnType: ColumnType.Number,
    total: "sum",
    style: "width: 10%",
  },
  {
    field: "personalCost",
    header: pt("Cost Operari"),
    columnType: ColumnType.Currency,
    total: "sum",
    totalFormat: formatCurrency,
    style: "width: 10%",
  },
  {
    field: "workcenterCost",
    header: pt("Cost Màquina"),
    columnType: ColumnType.Currency,
    total: "sum",
    totalFormat: (value) =>
      `${formatCurrency(value)} = ${formatCurrency(value + totalPersonalCost.value)}`,
    style: "width: 10%",
  },
]);

const filterMetadata = computed(() =>
  createTableViewFilterMetadata(columns.value, {
    labels: { dates: pt("Període") },
    valueResolvers: {
      workorderId: (value) =>
        typeof value === "string"
          ? (workOrderStore.workorders?.find((item) => item.id === value)
              ?.code ?? "")
          : "",
    },
  }),
);

const productionPartRequest = ref({} as ProductionPart);
const generateNewRequest = (): ProductionPart => {
  return {
    id: getNewUuid(),
    operatorId: "",
    workcenterId: "",
    workOrderId: "",
    workOrderPhaseId: "",
    workOrderPhaseDetailId: "",
    operatorHourCost: 0,
    machineHourCost: 0,
    operatorTime: 0,
    workcenterTime: 0,
    quantity: 0,
    date: new Date(),
  };
};

const getWorkOrderDetailedName = (productionPart: ProductionPart) => {
  if (
    productionPart.workOrder &&
    productionPart.workOrderPhase &&
    productionPart.workOrderPhaseDetail
  ) {
    const statusDesc = plantModelStore.getMachineStatusNameById(
      productionPart.workOrderPhaseDetail.machineStatusId,
    );

    return `${productionPart.workOrder.code} - ${productionPart.workOrderPhase.code} (${productionPart.workOrderPhase.description}) - ${statusDesc}`;
  }
};

const getWorkCenterCost = (
  productionPart: ProductionPart,
): number | undefined => {
  const cost =
    (productionPart.machineHourCost * productionPart.workcenterTime) / 60;
  return _.round(cost, 2);
};

const getPersonalCost = (
  productionPart: ProductionPart,
): number | undefined => {
  const cost =
    (productionPart.operatorHourCost * productionPart.operatorTime) / 60;
  return _.round(cost, 2);
};

const createButtonClick = () => {
  productionPartRequest.value = generateNewRequest();
  dialogOptions.visible = true;
};

const createProductionPart = async () => {
  dialogOptions.visible = false;
  const created = await productionPartStore.create(productionPartRequest.value);
  if (created) {
    router.push({ path: `/productionpart` });
    filterData();
  }
};

const deleteProductionPart = (productionPart: ProductionPart) => {
  confirm.require({
    message: pt("Confirmar l'eliminació del tiquet de producció"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await productionPartStore.delete(productionPart.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: pt("Eliminat"),
          life: 3000,
        });
        await filterData();
      }
    },
  });
};
</script>

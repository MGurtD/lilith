<template>
  <DataTable
    :value="calculatedProductionParts"
    class="p-datatable-sm small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :sort-order="1"
    sort-field="date"
    paginator
    :rows="20"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter-5">
          <div class="filter-field">
            <ExerciseDatePicker :exercises="exerciseStore.exercises" />
          </div>
          <div class="filter-field">
            <label>Màquina</label>
            <Select
              v-model="filter.workcenterId"
              :show-clear="true"
              :filter="true"
              :options="
                plantModelStore.workcenters?.sort((a, b) =>
                  a.description.localeCompare(b.description),
                )
              "
              optionValue="id"
              optionLabel="description"
              class="w-full"
            />
          </div>
          <div class="filter-field">
            <label>Operari</label>
            <Select
              v-model="filter.operatorId"
              :show-clear="true"
              :filter="true"
              :options="
                plantModelStore.operators
                  ?.sort((a, b) => a.surname.localeCompare(b.surname))
                  .map((operator) => ({
                    value: operator.id,
                    label: operator.surname + ', ' + operator.name,
                  }))
              "
              optionValue="value"
              optionLabel="label"
              class="w-full"
            />
          </div>
          <div class="filter-field">
            <label>OF</label>
            <Select
              v-model="filter.workorderId"
              :show-clear="true"
              :filter="true"
              :options="workOrderStore.workorders"
              optionValue="id"
              optionLabel="code"
              class="w-full"
            />
          </div>
          <div>
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
      </div>
    </template>
    <template #empty> No s'han trobat tiquets. </template>
    <template #loading> Carregant tiquets. Si us plau espera. </template>
    <Column field="workcenterId" header="Màquina" style="width: 15%">
      <template #body="slotProps">
        {{ plantModelStore.getWorkcenterNameById(slotProps.data.workcenterId) }}
      </template>
    </Column>
    <Column field="operatorId" header="Operari" style="width: 15%">
      <template #body="slotProps">
        {{ plantModelStore.getOperatorNameById(slotProps.data.operatorId) }}
      </template>
    </Column>
    <Column field="workOrderId" header="OF" style="width: 20%">
      <template #body="slotProps">
        {{ getWorkOrderDetailedName(slotProps.data) }}
      </template>
    </Column>
    <Column field="date" header="Data" style="width: 10%" sortable>
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
    <Column field="quantity" header="Quantitat" style="width: 5%"></Column>
    <Column
      field="workcenterTime"
      header="Temps Maq."
      style="width: 10%"
    ></Column>
    <Column
      field="operatorTime"
      header="Temps Oper."
      style="width: 10%"
    ></Column>
    <Column header="Cost Operari" style="width: 10%" field="operatorHourCost">
      <template #body="slotProps">
        {{
          formatCurrency(
            (slotProps.data.operatorHourCost / 60) *
              slotProps.data.operatorTime,
          )
        }}
      </template>
    </Column>
    <Column header="Cost Màquina" style="width: 10%" field="machineHourCost">
      <template #body="slotProps">
        {{
          formatCurrency(
            (slotProps.data.machineHourCost / 60) *
              slotProps.data.workcenterTime,
          )
        }}
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteProductionPart($event, slotProps.data)"
        />
      </template>
    </Column>
    <template #footer>
      <div
        class="flex-right"
        v-if="calculatedProductionParts && calculatedProductionParts.length > 0"
      >
        <span>
          Quantitat:
          {{ totalProductionQuantity }} unitats
          <br />
          Temps màquina: {{ totalWorkcenterTime }} / Temps operari :
          {{ totalOperatorTime }}
          <br />
          Cost màquina: {{ formatCurrency(totalWorkcenterCost!) }} / Cost
          operari: {{ formatCurrency(totalPersonalCost!) }} =
          {{ formatCurrency(totalPersonalCost! + totalWorkcenterCost!) }}
        </span>
      </div>
    </template>
  </DataTable>
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
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useConfirm } from "primevue/useconfirm";
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { ProductionPart } from "../types";
import {
  formatDateForQueryParameter,
  formatDate,
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

const filter = ref({
  operatorId: "" as string,
  workcenterId: "" as string,
  workorderId: "" as string,
});

const filterData = async () => {
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0],
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);

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
      summary: "Filtre invàlid",
      detail: "Seleccioni un període",
      life: 5000,
    });
  }
};

const cleanFilter = () => {
  store.cleanExercisePicker();
  filter.value.workcenterId = "";
  filter.value.operatorId = "";
};

const dialogOptions = reactive({
  visible: false,
  title: "Crear tíquet de producció",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CLOUD,
    title: "Tíquets de producció",
  });

  plantModelStore.fetchWorkcenters();
  plantModelStore.fetchOperators();
  plantModelStore.fetchOperatorTypes();
  plantModelStore.fetchMachineStatuses();
  await plantModelStore.fetchWorkcenterCosts();

  await exerciseStore.fetchActive();
  getUserFilter();
  filterData();

  workOrderStore.detailedWorkOrders = undefined;
});
onUnmounted(() => {
  const savedFilter = {
    ...filter.value,
    exercisePicker: store.exercisePicker,
  };

  userFilterStore.addFilter("ProductionParts", "", savedFilter);
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("ProductionParts", "");
  if (userFilter) {
    filter.value.operatorId = userFilter.operatorId;
    filter.value.workcenterId = userFilter.workcenterId;
    filter.value.workorderId = userFilter.workorderId;
    if (userFilter.exercisePicker) {
      store.exercisePicker.exercise = userFilter.exercisePicker.exercise;
      store.exercisePicker.dates = [
        new Date(userFilter.exercisePicker.dates[0]),
        new Date(userFilter.exercisePicker.dates[1]),
      ];
    }
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

const totalOperatorTime = computed(() => {
  if (productionPartStore.productionParts) {
    return productionPartStore.productionParts.reduce(
      (acc, productionPart) => acc + productionPart.operatorTime,
      0,
    );
  }
});
const totalWorkcenterTime = computed(() => {
  if (productionPartStore.productionParts) {
    return productionPartStore.productionParts.reduce(
      (acc, productionPart) => acc + productionPart.workcenterTime,
      0,
    );
  }
});
const totalProductionQuantity = computed(() => {
  if (productionPartStore.productionParts) {
    return productionPartStore.productionParts.reduce(
      (acc, productionPart) => acc + productionPart.quantity,
      0,
    );
  }
});
const totalPersonalCost = computed(() => {
  if (
    calculatedProductionParts.value &&
    calculatedProductionParts.value.length > 0
  ) {
    return calculatedProductionParts.value.reduce(
      (acc, productionPart) =>
        acc + (productionPart.personalCost ? productionPart.personalCost : 0),
      0,
    );
  }
});
const totalWorkcenterCost = computed(() => {
  if (
    calculatedProductionParts.value &&
    calculatedProductionParts.value.length > 0
  ) {
    return calculatedProductionParts.value.reduce(
      (acc, productionPart) =>
        acc +
        (productionPart.workcenterCost ? productionPart.workcenterCost : 0),
      0,
    );
  }
});

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

const deleteProductionPart = (event: any, productionPart: ProductionPart) => {
  confirm.require({
    target: event.currentTarget,
    message: `Està segur que vol eliminar el tíquet de producció?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await productionPartStore.delete(productionPart.id);
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

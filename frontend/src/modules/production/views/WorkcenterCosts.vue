<template>
  <Table
    :items="filteredData"
    :columns="columns"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    :show-filter-action="false"
    preset="crud-list"
    tableStyle="min-width: 100%"
    sort-field="workcenterName"
    :sort-order="1"
    show-delete-column
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editRow"
  />
</template>

<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "@/components/tables/TableFilter.vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { usePlantModelStore } from "../store/plantmodel";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { WorkcenterCost } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { useUserFilterStore } from "../../../store/userfilter";

const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
const router = useRouter();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const userFilterStore = useUserFilterStore();
const toast = useToast();
const confirm = useConfirm();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "workcenterId",
    label: pt("Màquina"),
    type: "select",
    options: plantmodelStore.workcenters ?? [],
    optionLabel: "name",
    optionValue: "id",
    size: "md",
  },
  {
    key: "zerocost",
    label: pt("Cost 0"),
    type: "checkbox",
    size: "sm",
  },
]);

const columns = computed<Column[]>(() => [
  {
    field: "workcenterName",
    header: pt("Màquina"),
    sortable: true,
    style: "width: 30%",
  },
  {
    field: "machineStatusName",
    header: pt("Estat de màquina"),
    style: "width: 30%",
  },
  {
    field: "cost",
    header: pt("Cost"),
    columnType: ColumnType.Currency,
    style: "width: 30%",
  },
  {
    field: "disabled",
    header: pt("Desactivada"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("WorkcenterCosts", "");
  if (userFilter) {
    filter.value = userFilter;
  }
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: pt("Costs per màquina"),
  });

  await plantmodelStore.fetchWorkcenterCosts();
  await plantmodelStore.fetchWorkcenters();
  await plantmodelStore.fetchMachineStatuses();
  getUserFilter();
});
onUnmounted(() => {
  userFilterStore.addFilter("WorkcenterCosts", "", filter.value);
});

const mappedWorkcenterCosts = computed(() => {
  if (!plantmodelStore.workcentercosts) return [];

  return plantmodelStore.workcentercosts.map((c) => {
    return {
      workcenterName: getWorkcenterById(c.workcenterId),
      machineStatusName: getMachineStatusById(c.machineStatusId),
      ...c,
    };
  });
});

// Filter data
const filter = ref({
  workcenterId: undefined as undefined | string,
  zerocost: false,
});

const filteredData = computed(() => {
  let results = mappedWorkcenterCosts.value;

  if (filter.value.workcenterId) {
    results = results.filter(
      (wc) => wc.workcenterId === filter.value.workcenterId,
    );
  }

  if (filter.value.zerocost) {
    results = results.filter((wc) => wc.cost === 0);
  }

  return results;
});

const cleanFilter = () => {
  filter.value.workcenterId = undefined;
  filter.value.zerocost = false;
};

const getWorkcenterById = (id: string) => {
  const workcenter = plantmodelStore.workcenters?.find((st) => st.id === id);
  if (workcenter) {
    return workcenter.name;
  }
};

const getMachineStatusById = (id: string) => {
  const machineStatus = plantmodelStore.machineStatuses?.find(
    (st) => st.id === id,
  );
  if (machineStatus) {
    return machineStatus.name;
  }
};

const createButtonClick = () => {
  router.push({ path: `/workcentercost/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/workcentercost/${row.data.id}` });
};

const deleteButton = (workcentercost: WorkcenterCost) => {
  confirm.require({
    message: pt("Confirmar l'eliminació del cost"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteWorkcenterCost(
        workcentercost.id,
      );

      if (deleted) {
        toast.add({
          severity: "success",
          summary: pt("Eliminat"),
          life: 3000,
        });
        await plantmodelStore.fetchWorkcenterCosts();
      }
    },
  });
};
</script>

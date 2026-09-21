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
import { getNewUuid } from "../../../utils/functions";
import { onBeforeRouteLeave, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useUserFilterStore } from "../../../store/userfilter";
import { usePlantModelStore } from "../store/plantmodel";
import { computed, onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { Workcenter } from "../types";
import { useShiftStore } from "../store/shift";

const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const plantmodelStore = usePlantModelStore();
const shiftStore = useShiftStore();
const userFilterStore = useUserFilterStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "workcenterTypeId",
    label: pt("Tipus"),
    type: "select",
    options: plantmodelStore.workcenterTypes ?? [],
    optionLabel: "name",
    optionValue: "id",
    placeholder: pt("Tots"),
    size: "md",
  },
  {
    key: "areaId",
    label: pt("Àrea"),
    type: "select",
    options: plantmodelStore.areas ?? [],
    optionLabel: "name",
    optionValue: "id",
    placeholder: pt("Totes"),
    size: "md",
  },
]);

const columns = computed<Column[]>(() => [
  { field: "name", header: pt("Nom"), style: "width: 20%" },
  {
    field: "description",
    header: pt("Descripció"),
    style: "width: 40%",
  },
  {
    field: "workcenterTypeId",
    header: pt("Tipus"),
    columnType: ColumnType.Lookup,
    resolver: (value) =>
      typeof value === "string" ? getWorkcenterTypeNameById(value) : "",
    style: "width: 15%",
  },
  {
    field: "areaId",
    header: pt("Area"),
    columnType: ColumnType.Lookup,
    resolver: (value) =>
      typeof value === "string" ? getAreaNameById(value) : "",
    style: "width: 15%",
  },
  {
    field: "disabled",
    header: pt("Desactivat"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

onMounted(async () => {
  await plantmodelStore.fetchWorkcenters();
  await plantmodelStore.fetchActiveWorkcenterTypes();
  await plantmodelStore.fetchActiveAreas();
  await shiftStore.fetchAllShifts();

  const userFilter = userFilterStore.getFilter("Workcenters", "");
  if (userFilter) filter.value = userFilter;

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: pt("Gestió de màquines"),
  });
});

onBeforeRouteLeave(async () => {
  await userFilterStore.addFilter("Workcenters", "", filter.value);
});

// Filter data
const filter = ref({
  areaId: undefined as undefined | string,
  workcenterTypeId: undefined as undefined | string,
});

const filteredData = computed(() => {
  if (!plantmodelStore.workcenters) return [];

  let filteredWc = plantmodelStore.workcenters;
  if (filter.value.areaId) {
    filteredWc = filteredWc.filter((w) => w.areaId === filter.value.areaId);
  }
  if (filter.value.workcenterTypeId) {
    filteredWc = filteredWc.filter(
      (w) => w.workcenterTypeId === filter.value.workcenterTypeId,
    );
  }

  return filteredWc;
});

const cleanFilter = () => {
  filter.value.areaId = undefined;
  filter.value.workcenterTypeId = undefined;
};

// Format columns
const getAreaNameById = (id: string) => {
  const type = plantmodelStore.areas?.find((s) => s.id === id);
  if (type) return type.name;
  else return "";
};
const getWorkcenterTypeNameById = (id: string) => {
  const type = plantmodelStore.workcenterTypes?.find((s) => s.id === id);
  if (type) return type.name;
  else return "";
};

// Actions
const createButtonClick = () => {
  router.push({ path: `/workcenter/${getNewUuid()}` });
};
const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/workcenter/${row.data.id}` });
};
const deleteButton = (entity: Workcenter) => {
  confirm.require({
    message: pt("Confirmar l'eliminació de la màquina"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteWorkcenter(entity.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: pt("Eliminat"),
          life: 3000,
        });
        await plantmodelStore.fetchWorkcenters();
      }
    },
  });
};
</script>

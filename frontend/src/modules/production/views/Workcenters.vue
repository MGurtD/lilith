<template>
  <DataTable
    :value="filteredData"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortMode="multiple"
    @row-click="editRow"
  >
    <template #header>
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :show-filter-action="false"
        :body-width="filterBodyWidth"
        embedded
        @clear="cleanFilter"
        @create="createButtonClick"
      >
        <template #prepend>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label">{{ pt("Tipus") }}</label>
            <Select
              v-model="filter.workcenterTypeId"
              :options="plantmodelStore.workcenterTypes"
              optionValue="id"
              optionLabel="name"
              :placeholder="pt('Tots')"
              :showClear="true"
              class="w-full"
              size="small"
            />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label">{{ pt("Àrea") }}</label>
            <Select
              v-model="filter.areaId"
              :options="plantmodelStore.areas"
              optionValue="id"
              optionLabel="name"
              :placeholder="pt('Totes')"
              :showClear="true"
              class="w-full"
              size="small"
            />
          </div>
        </template>
      </TableFilter>
    </template>
    <Column field="name" :header="pt('Nom')" style="width: 20%"></Column>
    <Column field="description" :header="pt('Descripció')" style="width: 40%"></Column>
    <Column :header="pt('Tipus')" style="width: 15%">
      <template #body="slotProps">
        {{ getWorkcenterTypeNameById(slotProps.data.workcenterTypeId) }}
      </template>
    </Column>
    <Column :header="pt('Area')" style="width: 15%">
      <template #body="slotProps">
        {{ getAreaNameById(slotProps.data.areaId) }}
      </template>
    </Column>
    <Column :header="pt('Desactivat')" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          :aria-label="t('production.actions.delete')"
          :title="t('production.actions.delete')"
          @click="deleteButton($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import TableFilter from "../../../components/tables/TableFilter.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { usePlantModelStore } from "../store/plantmodel";
import { computed, onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { Workcenter } from "../types";
import { useShiftStore } from "../store/shift";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const plantmodelStore = usePlantModelStore();
const shiftStore = useShiftStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

onMounted(async () => {
  await plantmodelStore.fetchWorkcenters();
  await plantmodelStore.fetchActiveWorkcenterTypes();
  await plantmodelStore.fetchActiveAreas();
  await shiftStore.fetchAllShifts();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: pt("Gestió de màquines"),
  });
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
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/workcenter/${row.data.id}` });
  }
};
const deleteButton = (event: any, entity: Workcenter) => {
  confirm.require({
    target: event.currentTarget,
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

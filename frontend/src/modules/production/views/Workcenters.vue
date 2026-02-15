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
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter">
          <div class="filter-field">
            <label class="block text-900 mb-2">Tipus</label>
            <Select
              v-model="filter.workcenterTypeId"
              :options="plantmodelStore.workcenterTypes"
              optionValue="id"
              optionLabel="name"
              class="w-full"
            />
          </div>
          <div class="filter-field">
            <label class="block text-900 mb-2">Àrea</label>
            <Select
              v-model="filter.areaId"
              :options="plantmodelStore.areas"
              optionValue="id"
              optionLabel="name"
              class="w-full"
            />
          </div>
        </div>
        <div class="datatable-buttons">
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
    <Column field="name" header="Nom" style="width: 20%"></Column>
    <Column field="description" header="Descripció" style="width: 40%"></Column>
    <Column header="Tipus" style="width: 15%">
      <template #body="slotProps">
        {{ getWorkcenterTypeNameById(slotProps.data.workcenterTypeId) }}
      </template>
    </Column>
    <Column header="Area" style="width: 15%">
      <template #body="slotProps">
        {{ getAreaNameById(slotProps.data.areaId) }}
      </template>
    </Column>
    <Column header="Desactivat" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteButton($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
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

onMounted(async () => {
  await plantmodelStore.fetchWorkcenters();
  await plantmodelStore.fetchWorkcenterTypes();
  await plantmodelStore.fetchAreas();
  await shiftStore.fetchAllShifts();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: "Gestió de màquines",
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
  router.push({ path: `/workcenter/${uuidv4()}` });
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
    message: `Está segur que vol eliminar la màquina ${entity.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteWorkcenter(entity.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await plantmodelStore.fetchWorkcenters();
      }
    },
  });
};
</script>

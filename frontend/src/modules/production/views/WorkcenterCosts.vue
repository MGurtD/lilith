<template>
  <DataTable
    :value="filteredData"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sort-field="workcenterName"
    :sort-order="1"
    @row-click="editRow"
    paginator
    :rows="20"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter">
          <div class="filter-field">
            <label class="block text-900 mb-2">Màquina</label>
            <Select
              v-model="filter.workcenterId"
              :options="plantmodelStore.workcenters"
              optionValue="id"
              optionLabel="name"
              class="w-full"
            />
          </div>
          <div class="filter-field">
            <label class="block text-900 mb-2">Cost 0</label>
            <Checkbox :binary="true" v-model="filter.zerocost" />
          </div>
        </div>
        <div class="datatable-buttons">
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.PLUS"
            rounded
            raised
            @click="createButtonClick"
          />
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER_SLASH"
            rounded
            raised
            @click="cleanFilter"
          />
        </div>
      </div>
    </template>
    <Column field="workcenterName" header="Màquina" style="width: 30%" sortable>
    </Column>
    <Column
      field="machineStatusName"
      header="Estat de màquina"
      style="width: 30%"
    >
    </Column>
    <Column field="cost" header="Cost" style="width: 30%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.cost) }}
      </template>
    </Column>
    <Column header="Desactivada" style="width: 10%">
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
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { usePlantModelStore } from "../store/plantmodel";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { WorkcenterCost } from "../types";
import { formatCurrency } from "../../../utils/functions";
import { useUserFilterStore } from "../../../store/userfilter";

const router = useRouter();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const userFilterStore = useUserFilterStore();
const toast = useToast();
const confirm = useConfirm();

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("WorkcenterCosts", "");
  if (userFilter) {
    filter.value = userFilter;
  }
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: "Costs per màquina",
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
  router.push({ path: `/workcentercost/${uuidv4()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/workcentercost/${row.data.id}` });
  }
};

const deleteButton = (event: any, workcentercost: WorkcenterCost) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el cost  ${workcentercost.id}?`,
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
          summary: "Eliminat",
          life: 3000,
        });
        await plantmodelStore.fetchWorkcenterCosts();
      }
    },
  });
};
</script>

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
            <label class="filter-label table-filter-prepend-label"
              >{{ pt("Màquina") }}</label
            >
            <Select
              v-model="filter.workcenterId"
              :options="plantmodelStore.workcenters"
              optionValue="id"
              optionLabel="name"
              class="w-full"
              size="small"
              showClear
            />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--sm"
          >
            <label class="filter-label table-filter-prepend-label"
              >{{ pt("Cost 0") }}</label
            >
            <div class="table-filter-checkbox-field">
              <Checkbox :binary="true" v-model="filter.zerocost" />
            </div>
          </div>
        </template>
      </TableFilter>
    </template>
    <Column field="workcenterName" :header="pt('Màquina')" style="width: 30%" sortable>
    </Column>
    <Column
      field="machineStatusName"
      :header="pt('Estat de màquina')"
      style="width: 30%"
    >
    </Column>
    <Column field="cost" :header="pt('Cost')" style="width: 30%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.cost) }}
      </template>
    </Column>
    <Column :header="pt('Desactivada')" style="width: 10%">
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
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import TableFilter from "../../../components/tables/TableFilter.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { usePlantModelStore } from "../store/plantmodel";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { WorkcenterCost } from "../types";
import { formatCurrency, getNewUuid } from "../../../utils/functions";
import { useUserFilterStore } from "../../../store/userfilter";

const router = useRouter();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const userFilterStore = useUserFilterStore();
const toast = useToast();
const confirm = useConfirm();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

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

<style scoped>
.table-filter-checkbox-field {
  display: flex;
  align-items: center;
  min-height: 2.375rem;
}
</style>

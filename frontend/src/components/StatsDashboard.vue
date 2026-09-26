<template>
  <div class="stats-dashboard">
    <!-- Row 1: filter -->
    <div class="dashboard-filter">
      <div class="dashboard-filter-left">
        <TableFilter
          :config="filterConfig ?? []"
          :model-value="filterValues ?? {}"
          :show-title="false"
          :show-filter-action="true"
          :show-create="false"
          :show-action-labels="true"
          :body-width="filterBodyWidth"
          embedded
          @update:model-value="emit('update:filterValues', $event)"
          @filter="emit('filter')"
          @clear="emit('clear')"
        >
          <!-- Leading content that is not a filter, e.g. a title. -->
          <template v-if="slots.prepend" #prepend>
            <slot name="prepend" />
          </template>
          <!-- Forward the #filter-{key} slots of type "slot" filter fields -->
          <template
            v-for="name in filterSlotNames"
            :key="name"
            #[name]="slotProps"
          >
            <slot :name="name" v-bind="slotProps" />
          </template>
        </TableFilter>
      </div>
    </div>

    <!-- Row 2: KPI line -->
    <div class="dashboard-kpis">
      <slot name="kpis">
        <div v-for="kpi in kpis" :key="kpi.label" class="kpi-card">
          <div class="kpi-label">{{ kpi.label }}</div>
          <div class="kpi-value" :class="kpi.colorClass">{{ kpi.value }}</div>
        </div>
      </slot>
    </div>

    <!-- Row 3: table -->
    <Table
      :columns="columns"
      :items="items"
      :show-filters="false"
      :show-create="false"
      :page="page"
      :paginator="true"
      :rows="rows"
      scrollable
      scroll-height="flex"
      :sort-field="sortField"
      :sort-order="sortOrder"
      striped-rows
      row-hover
      class="small-datatable"
      tableStyle="min-width: 100%"
      @row-click="(e) => emit('row-click', e)"
    >
      <!-- Forward consumer table slots (e.g. body-customerName) to Table -->
      <template v-for="(_, name) in tableSlots" #[name]="slotData">
        <slot :name="name" v-bind="(slotData ?? {}) as any" />
      </template>
    </Table>
  </div>
</template>

<script setup lang="ts">
import { computed, useSlots } from "vue";
import TableFilter, {
  type FilterBodyWidth,
  type FilterConfig,
} from "@/components/tables/TableFilter.vue";
import Table from "@/components/tables/Table.vue";
import type { Column } from "@/components/tables/types";
import type { DataTableRowClickEvent } from "primevue/datatable";

export interface StatKpi {
  label: string;
  value: string | number;
  colorClass?: string;
}

withDefaults(
  defineProps<{
    columns: Column[];
    items: readonly unknown[];
    kpis?: StatKpi[];
    filterConfig?: FilterConfig[];
    filterValues?: Record<string, any>;
    filterBodyWidth?: FilterBodyWidth;
    page?: string;
    rows?: number;
    sortField?: string;
    sortOrder?: number;
  }>(),
  { kpis: () => [], rows: 25 },
);

const emit = defineEmits<{
  (e: "update:filterValues", value: Record<string, any>): void;
  (e: "filter"): void;
  (e: "clear"): void;
  (e: "row-click", event: DataTableRowClickEvent): void;
}>();

// `prepend` is non-filter lead content of the filter bar, `kpis` replaces the
// KPI line, `filter-{key}` renders a `type: "slot"` filter of `filterConfig`
// (props `{ field, value, update }`) and every other slot goes to the Table
// (e.g. body-{field}, props `{ data, index }`).
defineSlots<
  {
    prepend?: () => unknown;
    kpis?: () => unknown;
  } & Record<string, (props: Record<string, any>) => unknown>
>();

const slots = useSlots();
const isFilterSlot = (name: string) => name.startsWith("filter-");
// Slots named filter-{key} render the inputs of `type: "slot"` filters.
const filterSlotNames = computed(() => Object.keys(slots).filter(isFilterSlot));
// Forward only table-facing slots; the filter bar and KPI slots stay here.
const tableSlots = computed(() =>
  Object.fromEntries(
    Object.entries(slots).filter(
      ([name]) => name !== "prepend" && name !== "kpis" && !isFilterSlot(name),
    ),
  ),
);
</script>

<style scoped>
.dashboard-filter {
  display: flex;
  gap: 0.75rem;
  align-items: flex-end;
  justify-content: flex-start;
  flex-wrap: wrap;
}
.dashboard-filter-left {
  flex: 0 1 auto;
  min-width: 20rem;
}

.dashboard-kpis {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(10rem, 1fr));
  gap: 0.75rem;
  margin: 0.75rem 0 1rem;
  align-items: stretch;
}
.kpi-card {
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  padding: 0.75rem 1rem;
  background: var(--p-content-background, var(--p-surface-card, #fff));
}
.kpi-label {
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}
.kpi-value {
  font-size: 1.4rem;
  font-weight: 700;
}

@media only screen and (max-width: 767px) {
  .dashboard-filter {
    gap: 1rem;
  }
  .dashboard-filter-left {
    min-width: 100%;
  }
}
</style>


<script setup lang="ts">
import { computed, useSlots, useAttrs } from "vue";
import TableFilter from "./TableFilter.vue";
import type { FilterConfig, FilterBodyWidth } from "./TableFilter.vue";

export type Aggregation = "sum" | "avg" | "count" | "min" | "max";

export interface Column {
  field: string;
  header: string;
  sortable?: boolean;
  total?: Aggregation;
  totalFormat?: (value: number) => string;
}

defineOptions({ inheritAttrs: false });

const props = withDefaults(
  defineProps<{
    columns: Column[];
    items: readonly any[];
    filterConfig?: FilterConfig[];
    filterValues?: any;
    filterBodyWidth?: FilterBodyWidth;
    showFilters?: boolean;
  }>(),
  { showFilters: true },
);

const emit = defineEmits<{
  (e: "update:filterValues", value: any): void;
  (e: "filter"): void;
  (e: "clear"): void;
  (e: "create"): void;
}>();

const slots = useSlots();
const attrs = useAttrs();

// --- Totals ---

const columnsWithTotal = computed(() => props.columns.filter((c) => c.total));

const hasFooter = computed(
  () =>
    columnsWithTotal.value.length > 0 ||
    props.columns.some((c) => !!slots[`footer-${c.field}`]),
);

function aggregate(
  items: readonly any[],
  field: string,
  kind: Aggregation,
): number {
  const nums = items
    .map((i) => i[field])
    .filter((v): v is number => typeof v === "number");
  if (nums.length === 0) return 0;
  switch (kind) {
    case "sum":
      return nums.reduce((a, b) => a + b, 0);
    case "avg":
      return nums.reduce((a, b) => a + b, 0) / nums.length;
    case "min":
      return Math.min(...nums);
    case "max":
      return Math.max(...nums);
    case "count":
      return items.filter(
        (i) => i[field] !== undefined && i[field] !== null,
      ).length;
  }
}

const totals = computed(() => {
  const map: Record<string, number> = {};
  for (const col of columnsWithTotal.value) {
    map[col.field] = aggregate(props.items, col.field, col.total!);
  }
  return map;
});

function formatTotal(col: Column): string {
  const raw = totals.value[col.field] ?? 0;
  return col.totalFormat ? col.totalFormat(raw) : String(raw);
}

// --- Filter slot helpers ---

function isFilterSlot(name: string | number | symbol): boolean {
  return typeof name === "string" && name.startsWith("filter-");
}

function filterSlotName(name: string | number | symbol): string {
  return typeof name === "string" ? name.slice(7) : "";
}
</script>

<template>
  <DataTable v-bind="attrs" :value="items">
    <!-- TableFilter embedded in DataTable's native header slot -->
    <template
      v-if="showFilters && filterConfig"
      #header
    >
      <TableFilter
        :config="filterConfig"
        :model-value="filterValues"
        :body-width="filterBodyWidth"
        :show-title="false"
        :show-action-labels="false"
        embedded
        @update:model-value="emit('update:filterValues', $event)"
        @filter="emit('filter')"
        @clear="emit('clear')"
        @create="emit('create')"
      >
        <!-- Forward #prepend and #append to TableFilter -->
        <template v-if="slots.prepend" #prepend>
          <slot name="prepend" />
        </template>
        <template v-if="slots.append" #append>
          <slot name="append" />
        </template>
        <!-- Forward #filter-{name} slots -->
        <template
          v-for="(_, name) in slots"
          :key="String(name)"
          #[filterSlotName(name)]
        >
          <template v-if="isFilterSlot(name)">
            <slot :name="name" />
          </template>
        </template>
      </TableFilter>
    </template>

    <!-- Dynamic columns -->
    <Column
      v-for="col in columns"
      :key="col.field"
      :field="col.field"
      :header="col.header"
      :sortable="col.sortable ?? false"
    >
      <template v-if="slots[`body-${col.field}`]" #body="slotProps">
        <slot :name="`body-${col.field}`" v-bind="slotProps" />
      </template>
    </Column>

    <!-- Footer totals row (only if any column has total or footer slot) -->
    <ColumnGroup v-if="hasFooter" type="footer">
      <Row>
        <Column v-for="col in columns" :key="col.field">
          <template #footer>
            <slot
              v-if="slots[`footer-${col.field}`]"
              :name="`footer-${col.field}`"
            />
            <span v-else-if="col.total">{{ formatTotal(col) }}</span>
          </template>
        </Column>
      </Row>
    </ColumnGroup>

    <!-- PrimeVue slot passthrough -->
    <template v-if="slots.empty" #empty>
      <slot name="empty" />
    </template>
    <template v-if="slots.loading" #loading>
      <slot name="loading" />
    </template>
    <template v-if="slots.paginatorstart" #paginatorstart>
      <slot name="paginatorstart" />
    </template>
    <template v-if="slots.paginatorend" #paginatorend>
      <slot name="paginatorend" />
    </template>
  </DataTable>
</template>

<template>
  <div>
    <Table
      class="p-datatable-sm"
      tableStyle="min-width:100%"
      scrollable
      scrollHeight="flex"
      sortField="movementDate"
      :sortOrder="1"
      :items="stockMovementStore.stockMovements ?? []"
      :columns="columns"
      :filter-config="filterConfig"
      v-model:filter-values="filter"
      :filter-labels="filterLabels"
      :filter-value-resolvers="filterValueResolvers"
      :filter-body-width="filterBodyWidth"
      :show-create="false"
      page="StockMovements"
      :paginator="(stockMovementStore.stockMovements?.length ?? 0) > 20"
      :rows="20"
      @filter="filterMovements"
      @clear="cleanFilter"
    >
      <template #filter-append>
        <div class="table-filter-prepend-field table-filter-prepend-field--sm">
          <label class="filter-label table-filter-prepend-label">{{
            t("warehouse.fields.location")
          }}</label>
          <DropdownWarehousesWithLocations
            label=""
            v-model="filter.locationId"
          />
        </div>
      </template>
      <template #body-movementType="{ data }">
        <TagMovementType :movementType="data.movementType" />
      </template>
    </Table>
  </div>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "@/components/tables/TableFilter.vue";
import DropdownWarehousesWithLocations from "../components/DropdownWarehousesWithLocations.vue";
import TagMovementType from "@/components/TagMovementType.vue";
import { useToast } from "primevue/usetoast";
import { useStore } from "@/store";
import { useStockMovementStore } from "../store/stockMovement";
import { useReferenceStore } from "../../shared/store/reference";
import { useExerciseStore } from "../../shared/store/exercise";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { formatDateForQueryParameter } from "@/utils/functions";
import { useWarehouseStore } from "../store/warehouse";

const toast = useToast();
const { t } = useI18n();
const store = useStore();
const stockMovementStore = useStockMovementStore();
const referenceStore = useReferenceStore();
const exerciseStore = useExerciseStore();
const warehouseStore = useWarehouseStore();

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  locationId: undefined as string | undefined,
});

const filterBodyWidth: FilterBodyWidth = {
  desktop: "55%",
  tablet: "70%",
};

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "dates",
    label: t("common.period"),
    type: "date-range",
    placeholder: t("warehouse.placeholders.selectPeriod"),
    size: "sm",
  },
]);

const filterLabels = computed<Record<string, string>>(() => ({
  locationId: t("warehouse.fields.location"),
}));

const filterValueResolvers: Record<string, (value: unknown) => string> = {
  locationId: (value) => {
    if (typeof value !== "string") return "";
    for (const warehouse of warehouseStore.warehouses ?? []) {
      const location = warehouse.locations?.find((item) => item.id === value);
      if (location) return `${warehouse.name} - ${location.description}`;
    }
    return "";
  },
};

const columns = computed<Column[]>(() => [
  {
    field: "movementDate",
    header: t("common.date"),
    sortable: true,
    columnType: ColumnType.DateTime,
    style: "width: 10%",
  },
  {
    field: "referenceId",
    header: t("warehouse.fields.reference"),
    columnType: ColumnType.Lookup,
    resolver: referenceStore.getFullNameById,
    style: "width: 15%",
  },
  {
    field: "location.name",
    header: t("warehouse.fields.location"),
    style: "width: 10%",
  },
  {
    field: "width",
    header: t("warehouse.fields.widthMmAxis"),
    columnType: ColumnType.Number,
    style: "width: 5%; min-width: 8rem",
  },
  {
    field: "length",
    header: t("warehouse.fields.lengthMmAxis"),
    columnType: ColumnType.Number,
    style: "width: 5%; min-width: 8rem",
  },
  {
    field: "height",
    header: t("warehouse.fields.heightMmAxis"),
    columnType: ColumnType.Number,
    style: "width: 5%; min-width: 8rem",
  },
  {
    field: "diameter",
    header: t("warehouse.fields.diameterMm"),
    columnType: ColumnType.Number,
    style: "width: 5%; min-width: 8rem",
  },
  {
    field: "thickness",
    header: t("warehouse.fields.thicknessMm"),
    columnType: ColumnType.Number,
    style: "width: 5%; min-width: 8rem",
  },
  {
    field: "description",
    header: t("common.description"),
    style: "width: 25%",
  },
  {
    field: "movementType",
    header: t("warehouse.fields.movementType"),
    style: "width: 10%",
    truncate: false,
  },
  {
    field: "quantity",
    header: t("warehouse.fields.quantity"),
    columnType: ColumnType.Number,
    style: "width: 10%; min-width: 8rem",
  },
]);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.MAP,
    title: t("warehouse.stockMovements.title"),
  });
  await exerciseStore.fetchAll();
  await referenceStore.fetchReferences();
  setCurrentYear();
});

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

const cleanFilter = () => {
  filter.value.dates = undefined;
  filter.value.locationId = undefined;
};

const filterMovements = async () => {
  if (filter.value.dates && filter.value.dates[1]) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await stockMovementStore.getBetweenDates(
      startTime,
      endTime,
      filter.value.locationId,
    );
  } else {
    toast.add({
      severity: "info",
      summary: t("warehouse.messages.invalidFilter"),
      detail: t("warehouse.messages.selectPeriod"),
      life: 5000,
    });
  }
};
</script>

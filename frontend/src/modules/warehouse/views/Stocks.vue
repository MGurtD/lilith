<template>
  <Table
    class="small-datatable"
    :items="filteredStocks"
    :columns="columns"
    :filter-config="[]"
    v-model:filter-values="filter"
    :filter-labels="filterLabels"
    :filter-value-resolvers="filterValueResolvers"
    :filter-body-width="filterBodyWidth"
    :show-filter-action="false"
    :show-create="false"
    page="Stocks"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortMode="multiple"
    :sortOrder="1"
    :paginator="filteredStocks.length > 20"
    :rows="20"
    @clear="cleanFilter"
  >
    <template #prepend>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">
          {{ t("warehouse.fields.warehouse") }}
        </label>
        <DropdownWarehouses label="" v-model="filter.warehouseId" />
      </div>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">
          {{ t("warehouse.fields.reference") }}
        </label>
        <DropdownReference
          label=""
          :fullName="true"
          :options="stockStore.availableReferences"
          v-model="filter.referenceId"
        />
      </div>
    </template>
    <template #body-lotCode="{ data }">
      <span class="flex align-items-center gap-2">
        {{ data.lotCode || "-" }}
        <Tag
          v-if="data.lotClosedDate"
          severity="secondary"
          :value="t('warehouse.lotTraceability.closed')"
          v-tooltip.top="t('warehouse.lotTraceability.closed')"
        />
      </span>
    </template>
    <template #body-lotTraceability="{ data }">
      <Button
        icon="pi pi-sitemap"
        text
        rounded
        size="small"
        :disabled="!data.lotId"
        v-tooltip.top="t('warehouse.lotTraceability.view')"
        @click="goToLotTraceability(data.referenceId, data.lotId)"
      />
    </template>
  </Table>
</template>

<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import DropdownWarehouses from "../components/DropdownWarehouses.vue";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { useStore } from "../../../store";
import { useStockStore } from "../store/stock";
import { useWarehouseStore } from "../store/warehouse";

const store = useStore();
const router = useRouter();
const { t } = useI18n();
const stockStore = useStockStore();
const warehouseStore = useWarehouseStore();

const filter = ref({
  referenceId: undefined as string | undefined,
  warehouseId: undefined as string | undefined,
});

const filterBodyWidth: FilterBodyWidth = {
  desktop: "55%",
  tablet: "70%",
};

const filterLabels = computed<Record<string, string>>(() => ({
  warehouseId: t("warehouse.fields.warehouse"),
  referenceId: t("warehouse.fields.reference"),
}));

const filterValueResolvers: Record<string, (value: unknown) => string> = {
  warehouseId: (value) =>
    typeof value === "string"
      ? (warehouseStore.warehouses?.find((item) => item.id === value)?.name ??
        "")
      : "",
  referenceId: (value) => {
    if (typeof value !== "string") return "";
    const reference = stockStore.availableReferences.find(
      (item) => item.id === value,
    );
    return reference ? `${reference.code} - ${reference.description}` : "";
  },
};

const columns = computed<Column[]>(() => [
  {
    field: "referenceDisplay",
    header: t("warehouse.fields.reference"),
    sortable: true,
    style: "width: 24%",
  },
  {
    field: "lotCode",
    header: t("common.lot"),
    style: "width: 12%",
    truncate: false,
  },
  {
    field: "warehouseName",
    header: t("warehouse.fields.warehouse"),
    style: "width: 14%",
  },
  {
    field: "locationName",
    header: t("warehouse.fields.location"),
    style: "width: 14%",
  },
  {
    field: "quantity",
    header: t("warehouse.fields.units"),
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
  {
    field: "width",
    header: t("warehouse.fields.widthMmAxis"),
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
  {
    field: "length",
    header: t("warehouse.fields.lengthMmAxis"),
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
  {
    field: "height",
    header: t("warehouse.fields.heightMmAxis"),
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
  {
    field: "diameter",
    header: t("warehouse.fields.diameterMm"),
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
  {
    field: "thickness",
    header: t("warehouse.fields.thicknessMm"),
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
  {
    field: "lotTraceability",
    header: "",
    style: "width: 6%",
    truncate: false,
  },
]);

const filteredStocks = computed(() => {
  if (!stockStore.stocks) return [];

  let result = [...stockStore.stocks];

  if (filter.value.referenceId) {
    result = result.filter(
      (stock) => stock.referenceId === filter.value.referenceId,
    );
  }

  if (filter.value.warehouseId) {
    result = result.filter(
      (stock) => stock.warehouseId === filter.value.warehouseId,
    );
  }

  return result.sort((left, right) =>
    left.referenceDisplay.localeCompare(right.referenceDisplay),
  );
});

const cleanFilter = () => {
  filter.value.referenceId = undefined;
  filter.value.warehouseId = undefined;
};

const goToLotTraceability = (referenceId: string, lotId?: string | null) => {
  if (!lotId) return;
  router.push({
    path: "/lot-traceability",
    query: { referenceId, lotId },
  });
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.BOX,
    title: t("warehouse.stocks.title"),
  });

  await stockStore.fetchStocks();
  await warehouseStore.fetchWarehousesWithLocations();
});
</script>

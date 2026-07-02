<template>
  <div class="mb-3">
    <TableFilter
      :config="filterConfig"
      v-model="filter"
      :show-title="false"
      :show-filter-action="false"
      :show-create="false"
      :show-action-labels="false"
      :body-width="filterBodyWidth"
      embedded
      @clear="clearFilter"
    >
      <template #prepend>
        <div class="table-filter-prepend-field table-filter-prepend-field--lg">
          <label class="filter-label table-filter-prepend-label">Període</label>
          <DatePicker
            v-model="filter.dates"
            selectionMode="range"
            dateFormat="dd/mm/yy"
            placeholder="Selecciona període"
            showIcon
            size="small"
            class="w-full"
          />
        </div>
      </template>
    </TableFilter>
  </div>
  <Tabs value="0" class="dashboard-tabs">
    <TabList>
      <Tab value="0">Gràfics</Tab>
      <Tab value="1">Dades</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <div class="dashboard-container">
          <Chart
            v-if="
              chartData && chartData.datasets && chartData.datasets.length > 0
            "
            type="bar"
            :data="chartData"
            :options="chartOptions"
            class="chart-canvas"
          />
          <div v-else class="empty-state">
            <i :class="PrimeIcons.CHART_BAR" class="empty-icon"></i>
            <p class="empty-message">
              Selecciona un interval de dates i un concepte per visualitzar les
              dades
            </p>
          </div>
        </div>
      </TabPanel>
      <TabPanel value="1">
        <TableProductionCosts :costs="costs" />
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from "vue";
import { useStore } from "../../../store";
import { useProductionCostDashboardStore } from "../store/productioncostdashboard";
import { formatDateForQueryParameter } from "../../../utils/functions";
import { usePlantModelStore } from "../store/plantmodel";
import { PrimeIcons } from "@primevue/core/api";
import TableFilter, {
  type FilterBodyWidth,
  type FilterConfig,
} from "../../../components/tables/TableFilter.vue";
import TableProductionCosts from "../components/TableProductionCosts.vue";
import { ProductionCostDashboardGrouped } from "../types";
import ProductionCostDashboardService from "../services/productioncostdashboard.service";

const store = useStore();
const productionCostStore = useProductionCostDashboardStore();
const plantModelStore = usePlantModelStore();
const productionCostDashboardService = new ProductionCostDashboardService(
  "/ProductionCost",
);

const today = new Date();
const sixMonthsAgo = new Date(today.getFullYear(), today.getMonth() - 6, 1);
const filter = ref({
  dates: [sixMonthsAgo, today] as Array<Date> | undefined,
  consolidatedBy: undefined as string | undefined,
});

const filterConfig = computed<Array<FilterConfig>>(() => [
  {
    key: "consolidatedBy",
    label: "Concepte",
    type: "select",
    options: optionValues.map((option) => ({
      label: option.value,
      value: option.id,
    })),
    placeholder: "Selecciona...",
    size: "xl",
  },
]);

const filterBodyWidth: FilterBodyWidth = {
  desktop: "50%",
  tablet: "75%",
};

const clearFilter = () => {
  filter.value.dates = undefined;
  filter.value.consolidatedBy = "";
};

watch(
  () => filter.value.dates,
  (dates) => {
    if (dates && dates.length === 2 && dates[1]) {
      filterDashboard();
    }
  },
  { deep: true },
);

watch(
  () => filter.value.consolidatedBy,
  (newValue, oldValue) => {
    if (newValue === oldValue) return;
    filterDashboard();
  },
);

type Options = {
  id: string;
  value: string;
};

const optionValues: Options[] = [
  { id: "operator", value: "Operaris" },
  { id: "workcentertype", value: "Tipus de centre de treball" },
  { id: "workcenter", value: "Centre de treball" },
];

const costs = ref([] as Array<ProductionCostDashboardGrouped>);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: "Dashboard costs producció",
  });
  await plantModelStore.fetchOperators();
  await plantModelStore.fetchWorkcenterTypes();
  await plantModelStore.fetchWorkcenters();
});

const filterDashboard = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    var dataResponse: Array<ProductionCostDashboardGrouped> | undefined;
    if (filter.value.consolidatedBy == "operator") {
      await productionCostStore.fetchGroupedByOperator(startTime, endTime);
      dataResponse =
        await productionCostDashboardService.GroupedByMonthAndOperator(
          startTime,
          endTime,
        );
    }
    if (filter.value.consolidatedBy == "workcentertype") {
      await productionCostStore.fetchGroupedByType(startTime, endTime);
      dataResponse =
        await productionCostDashboardService.GetGroupedByMonthAndWorkcenterType(
          startTime,
          endTime,
        );
    }
    if (filter.value.consolidatedBy == "workcenter") {
      await productionCostStore.fetchGroupedByWorkcenter(startTime, endTime);
      dataResponse =
        await productionCostDashboardService.GetGroupedByMonthAndWorkcenter(
          startTime,
          endTime,
        );
    }

    chartData.value = setChartData();
    chartOptions.value = setChartOptions();
    if (dataResponse) costs.value = dataResponse;
  }
};

const chartData = ref();
const chartOptions = ref();

const setChartData = () => {
  const monthNames = [
    "Gener",
    "Febrer",
    "Març",
    "Abril",
    "Maig",
    "Juny",
    "Juliol",
    "Agost",
    "Setembre",
    "Octubre",
    "Novembre",
    "Desembre",
  ];

  interface GroupedData {
    [key: string]: { [key: string]: number };
  }
  interface Dataset {
    type: string;
    label: string;
    backgroundColor: string;
    data: number[];
  }

  const groupedByMonth: GroupedData = {};
  const uniqueEntities = new Set<string>();

  // Build grouped data and collect unique entities from actual data
  productionCostStore.productionCostDashboardGrouped!.forEach((item) => {
    const monthYear = `${monthNames[item.month - 1]} ${item.year}`;
    if (!groupedByMonth[monthYear]) {
      groupedByMonth[monthYear] = {};
    }

    let entityKey = "";
    if (filter.value.consolidatedBy == "operator") {
      entityKey = item.operatorName;
      groupedByMonth[monthYear][entityKey] =
        (groupedByMonth[monthYear][entityKey] || 0) + item.totalCost;
    } else if (filter.value.consolidatedBy == "workcentertype") {
      entityKey = item.workcenterTypeName;
      groupedByMonth[monthYear][entityKey] =
        (groupedByMonth[monthYear][entityKey] || 0) + item.totalCost;
    } else if (filter.value.consolidatedBy == "workcenter") {
      entityKey = item.workcenterName;
      groupedByMonth[monthYear][entityKey] =
        (groupedByMonth[monthYear][entityKey] || 0) + item.totalCost;
    }

    if (entityKey) {
      uniqueEntities.add(entityKey);
    }
  });

  // Sort labels chronologically
  const labels = Object.keys(groupedByMonth).sort((a, b) => {
    const [monthA, yearA] = a.split(" ");
    const [monthB, yearB] = b.split(" ");
    const monthIndexA = monthNames.indexOf(monthA);
    const monthIndexB = monthNames.indexOf(monthB);

    if (yearA !== yearB) {
      return parseInt(yearA) - parseInt(yearB);
    }
    return monthIndexA - monthIndexB;
  });

  // Create datasets only for entities with actual data
  const entityList = Array.from(uniqueEntities).sort();
  const datasets: Dataset[] = entityList.map((entity, index) => {
    return {
      type: "bar",
      label: entity,
      backgroundColor: colors[index % colors.length],
      data: labels.map((monthYear) => groupedByMonth[monthYear][entity] || 0),
    };
  });

  return {
    labels,
    datasets,
  };
};
const setChartOptions = () => {
  const documentStyle = getComputedStyle(document.documentElement);
  const textColor = documentStyle.getPropertyValue("--p-text-color");
  const textColorSecondary = documentStyle.getPropertyValue(
    "--p-text-muted-color",
  );
  const surfaceBorder = documentStyle.getPropertyValue(
    "--p-content-border-color",
  );

  return {
    maintainAspectRatio: false,
    aspectRatio: 0.8,
    interaction: {
      mode: "index",
      intersect: false,
    },
    plugins: {
      tooltip: {
        callbacks: {
          label: function (context: any) {
            let label = context.dataset.label || "";
            if (label) {
              label += ": ";
            }
            if (context.parsed.y !== null) {
              label += new Intl.NumberFormat("ca-ES", {
                style: "currency",
                currency: "EUR",
              }).format(context.parsed.y);
            }
            return label;
          },
          footer: function (tooltipItems: any) {
            let sum = 0;
            tooltipItems.forEach(function (tooltipItem: any) {
              sum += tooltipItem.parsed.y;
            });
            return (
              "Total: " +
              new Intl.NumberFormat("ca-ES", {
                style: "currency",
                currency: "EUR",
              }).format(sum)
            );
          },
        },
      },
      legend: {
        position: "top",
        labels: {
          color: textColor,
          padding: 15,
          font: {
            size: 13,
          },
          usePointStyle: true,
          pointStyle: "rectRounded",
        },
      },
      title: {
        display: false,
      },
    },
    scales: {
      x: {
        stacked: true,
        ticks: {
          color: textColorSecondary,
          font: {
            size: 12,
          },
        },
        grid: {
          display: false,
        },
      },
      y: {
        stacked: true,
        ticks: {
          color: textColorSecondary,
          font: {
            size: 12,
          },
          callback: function (value: any) {
            return new Intl.NumberFormat("ca-ES", {
              style: "currency",
              currency: "EUR",
              notation: "compact",
              compactDisplay: "short",
            }).format(value);
          },
        },
        grid: {
          color: surfaceBorder,
          drawTicks: false,
        },
      },
    },
  };
};

const colors = [
  "#3B82F6", // Blue
  "#EF4444", // Red
  "#10B981", // Green
  "#F59E0B", // Amber
  "#8B5CF6", // Violet
  "#EC4899", // Pink
  "#14B8A6", // Teal
  "#F97316", // Orange
  "#6366F1", // Indigo
  "#84CC16", // Lime
  "#06B6D4", // Cyan
  "#F43F5E", // Rose
  "#8B5CF6", // Purple
  "#22D3EE", // Cyan Light
  "#A855F7", // Purple Light
  "#FBBF24", // Yellow
  "#34D399", // Emerald
  "#FB923C", // Orange Light
  "#60A5FA", // Blue Light
  "#F472B6", // Pink Light
  "#4ADE80", // Green Light
  "#FCD34D", // Amber Light
  "#C084FC", // Violet Light
  "#2DD4BF", // Teal Light
  "#FCA5A5", // Red Light
  "#94A3B8", // Slate
  "#4B5563", // Gray
  "#7C3AED", // Purple Deep
  "#DC2626", // Red Deep
  "#059669", // Green Deep
  "#D97706", // Amber Deep
  "#7C2D12", // Orange Deep
  "#1E40AF", // Blue Deep
  "#BE185D", // Pink Deep
  "#0F766E", // Teal Deep
  "#4338CA", // Indigo Deep
  "#65A30D", // Lime Deep
  "#0E7490", // Cyan Deep
  "#9F1239", // Rose Deep
  "#6D28D9", // Violet Deep
];
</script>
<style scoped>
.dashboard-filter {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.dashboard-filter-left {
  display: flex;
  flex: 1 1 100%;
  min-width: 22rem;
  align-self: center;
}

.dashboard-tabs {
  margin-top: 0;
}

.dashboard-container {
  display: flex;
  flex-direction: column;
  min-height: 600px;
  padding: 1.5rem 0;
}

.chart-canvas {
  width: 100%;
  height: 600px;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  gap: 1rem;
  color: var(--p-text-muted-color);
}

.empty-icon {
  font-size: 4rem;
  opacity: 0.3;
}

.empty-message {
  font-size: 1.1rem;
  text-align: center;
  max-width: 400px;
}

/* Mobile responsive */
@media only screen and (max-width: 767px) {
  .dashboard-filter {
    flex-direction: column;
    align-items: stretch;
  }

  .dashboard-filter-left {
    min-width: 100%;
  }

  .dashboard-container {
    min-height: 400px;
    padding: 1rem 0;
  }

  .chart-canvas {
    height: 400px;
  }

  .empty-icon {
    font-size: 3rem;
  }

  .empty-message {
    font-size: 0.95rem;
  }
}
</style>

<template>
  <div class="statistics-container">
    <div class="cards-row">
      <Card>
        <template #header>
          <div class="card-title">Total Pressuposts</div>
        </template>
        <template #content>
          <div class="total-count">{{ totalBudgetCount }}</div>
        </template>
      </Card>

      <Card>
        <template #header>
          <div class="card-title">Pressuposts</div>
        </template>
        <template #content>
          <div class="budgets-summary">
            <div class="budget-stat">
              <span class="budget-label">Acceptats</span>
              <span class="total-count-accepted">{{
                acceptedBudgetCount
              }}</span>
            </div>
            <div class="budget-divider"></div>
            <div class="budget-stat">
              <span class="budget-label">Rebutjats</span>
              <span class="total-count-refused">{{ rejectedBudgetCount }}</span>
            </div>
          </div>
        </template>
      </Card>

      <Card>
        <template #header>
          <div class="card-title">Total Factures</div>
        </template>
        <template #content>
          <div class="total-count-accepted">{{ totalInvoicesCount }}</div>
        </template>
      </Card>

      <Card>
        <template #header>
          <div class="card-title">Total Facturat (s/ impostos)</div>
        </template>
        <template #content>
          <div class="total-count-accepted">
            {{ formatCurrency(totalInvoicedAmount) }}
          </div>
        </template>
      </Card>
    </div>

    <div class="chart-container">
      <Card class="chart-card">
        <template #header>
          <div class="card-title">Facturació Mensual</div>
        </template>
        <template #content>
          <Chart type="line" :data="chartData" :options="chartOptions" />
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { Budget, SalesInvoice } from "../types";
import { formatCurrency } from "../../../utils/functions";

const props = defineProps<{
  budgets: Budget[];
  salesInvoices: SalesInvoice[];
}>();

const totalBudgetCount = computed(() => props.budgets.length);
const acceptedBudgetCount = computed(
  () => props.budgets.filter((b) => b.acceptanceDate !== null).length
);
const rejectedBudgetCount = computed(
  () => totalBudgetCount.value - acceptedBudgetCount.value
);

const totalInvoicesCount = computed(() => props.salesInvoices.length);
const totalInvoicedAmount = computed(() =>
  props.salesInvoices.reduce((sum, invoice) => sum + invoice.netAmount, 0)
);

const monthlyInvoicing = computed(() => {
  const monthlyTotals: Record<string, number> = {};
  props.salesInvoices.forEach((invoice) => {
    const month = invoice.invoiceDate.substring(0, 7); // 'YYYY-MM'
    monthlyTotals[month] = (monthlyTotals[month] || 0) + invoice.netAmount;
  });
  const months = Object.keys(monthlyTotals).sort();
  const amounts = months.map((m) => monthlyTotals[m]);

  return { months, amounts };
});

const chartData = computed(() => ({
  labels: monthlyInvoicing.value.months,
  datasets: [
    {
      label: "Facturació mensual (s/ impostos)",
      backgroundColor: "#42A5F5",
      data: monthlyInvoicing.value.amounts,
    },
  ],
}));

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  scales: {
    y: {
      beginAtZero: true,
    },
  },
};
</script>

<style scoped>
.statistics-container {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.cards-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
}

.cards-row :deep(.p-card) {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.cards-row :deep(.p-card-body) {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.cards-row :deep(.p-card-content) {
  display: flex;
  flex-direction: column;
  justify-content: center;
  flex: 1;
}

.total-count {
  font-size: 3rem;
  font-weight: bold;
  text-align: center;
}

.total-count-accepted {
  font-size: 2.5rem;
  font-weight: bold;
  text-align: center;
  color: green;
}

.total-count-refused {
  font-size: 2.5rem;
  font-weight: bold;
  text-align: center;
  color: rgb(155, 32, 32);
}

.card-title {
  font-size: 1.25rem;
  text-align: center;
  margin-top: 0.25rem;
  color: #555;
  font-weight: 600;
}

.budgets-summary {
  display: flex;
  align-items: center;
  justify-content: space-around;
  padding: 0.5rem 0;
}

.budget-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.budget-label {
  font-size: 0.9rem;
  color: #666;
  font-weight: 500;
}

.budget-divider {
  width: 1px;
  height: 60px;
  background-color: #e0e0e0;
}

.chart-container {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 350px);
  min-height: 400px;
}

.chart-card {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.chart-card :deep(.p-card-body) {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.chart-card :deep(.p-card-content) {
  flex: 1;
  display: flex;
  min-height: 0;
  padding: 1rem;
}

.chart-card :deep(.p-chart) {
  flex: 1;
  min-height: 0;
}

/* Responsive design */
@media (max-width: 1200px) {
  .cards-row {
    grid-template-columns: repeat(2, 1fr);
  }

  .chart-container {
    height: calc(100vh - 450px);
  }
}

@media (max-width: 768px) {
  .cards-row {
    grid-template-columns: 1fr;
  }

  .total-count,
  .total-count-accepted,
  .total-count-refused {
    font-size: 2rem;
  }

  .card-title {
    font-size: 1rem;
  }

  .budget-label {
    font-size: 0.8rem;
  }

  .chart-container {
    height: calc(100vh - 600px);
  }
}
</style>

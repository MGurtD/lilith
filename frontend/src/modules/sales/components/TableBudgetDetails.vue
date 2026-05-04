<template>
  <DataTable
    @row-click="onEditRow"
    :value="details"
    tableStyle="min-width: 100%"
    class="p-datatable-sm"
    sortMode="single"
    sortField="reference.code"
    selectionMode="single"
    dataKey="id"
    :sortOrder="1"
  >
    <template #header>
      <slot name="header"></slot>
    </template>
    <Column field="quantity" header="Un." style="width: 3%" />
    <Column
      header="Referencia"
      field="reference.code"
      sortable
      style="width: 15%"
    >
      <template #body="slotProps">
        <LinkReference :id="slotProps.data.referenceId" />
      </template>
    </Column>
    <Column field="description" header="Descripció" style="width: 25%" />
    <Column header="Cost intern" style="width: 10%">
      <template #body="slotProps">
        {{
          formatCurrency(
            slotProps.data.totalCost -
              slotProps.data.serviceCost -
              slotProps.data.transportCost,
          )
        }}
      </template>
    </Column>
    <Column header="Cost extern" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.serviceCost) }}
      </template>
    </Column>
    <Column header="Cost transport" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.transportCost) }}
      </template>
    </Column>
    <Column field="totalCost" header="Cost total" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.totalCost) }}
      </template>
    </Column>
    <Column field="profit" header="Benefici" style="width: 10%">
      <template #body="slotProps"> {{ slotProps.data.profit }} % </template>
    </Column>
    <Column field="discount" header="Descompte" style="width: 10%">
      <template #body="slotProps"> {{ slotProps.data.discount }} % </template>
    </Column>
    <Column field="unitPrice" header="Preu un." style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.unitPrice) }}
      </template>
    </Column>
    <Column field="amount" header="Total" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.amount) }}
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="budgetStore.order === undefined"
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
      </template>
    </Column>
    <template #footer>
      <div class="total-footer">
        <span class="total-label">Total</span>
        <span class="total-value">{{ formatCurrency(totalAmount) }}</span>
      </div>
    </template>
  </DataTable>
</template>
<script setup lang="ts">
import LinkReference from "../../shared/components/LinkReference.vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Budget, BudgetDetail } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useBudgetStore } from "../store/budget";
import { computed } from "vue";
import { formatCurrency } from "../../../utils/functions";

const props = defineProps<{
  budget: Budget;
  details: Array<BudgetDetail> | undefined;
}>();

const emit = defineEmits<{
  (e: "edit", detail: BudgetDetail): void;
  (e: "delete", detail: BudgetDetail): void;
}>();

const confirm = useConfirm();
const budgetStore = useBudgetStore();

const totalAmount = computed(() => {
  if (props.details) {
    return props.details.reduce((acc, detail) => acc + detail.amount, 0);
  }
  return 0;
});

const onEditRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, detail: BudgetDetail) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar la línea?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", detail);
    },
  });
};
</script>
<style scoped>
.total-footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 0.5rem;
}

.total-label {
  font-weight: 600;
  color: var(--p-text-muted-color);
  font-size: 0.85rem;
}

.total-value {
  font-weight: 700;
}
</style>

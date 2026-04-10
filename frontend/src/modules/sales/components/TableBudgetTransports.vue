<template>
  <DataTable
    @row-click="onEditRow"
    :value="transports"
    tableStyle="min-width: 100%"
    class="p-datatable-sm"
    sortMode="single"
    sortField="price"
    selectionMode="single"
    dataKey="id"
    :sortOrder="1"
  >
    <template #header>      
        <slot name="header"></slot>
      
    </template>
    <Column field="destination" header="Destinació" style="width: 25%" />
    <Column field="description" header="Descripció" style="width: 20%" />
    <Column field="weight" header="Pes (kg)" style="width: 10%" />
    <Column field="volume" header="Volum (m³)" style="width: 10%" />
    <Column field="distance" header="Distància (km)" style="width: 10%" />
    <Column field="price" header="Preu" style="width: 15%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.price) }}
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
  </DataTable>
</template>
<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Budget, BudgetTransport } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useBudgetStore } from "../store/budget";
import { formatCurrency } from "../../../utils/functions";

const props = defineProps<{
  budget: Budget;
  transports: Array<BudgetTransport> | undefined;
}>();

const emit = defineEmits<{
  (e: "edit", transport: BudgetTransport): void;
  (e: "delete", transport: BudgetTransport): void;
}>();

const confirm = useConfirm();
const toast = useToast();
const budgetStore = useBudgetStore();

const onEditRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, transport: BudgetTransport) => {
  confirm.require({
    target: event.currentTarget,
    message: `Està segur que vol eliminar el transport?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", transport);
    },
  });
};

const onDistributeCosts = async () => {
  const result = await budgetStore.DistributeTransportCosts(props.budget.id);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Costos ponderats",
      detail: "S'han ponderat els costos de transport correctament entre els detalls.",
      life: 5000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error al ponderar",
      detail: "No s'han pogut ponderar els costos (és possible que el pes total sigui 0 o hi hagi un error al servidor).",
      life: 5000,
    });
  }
};
</script>
<style scoped>
.header-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>

<template>
  <DataTable
    :value="workorders"
    tableStyle="min-width: 100%"
    class="p-datatable-sm small-datatable"
    sortField="order"
    :sortOrder="1"
    scrollable
    scrollHeight="flex"
    @rowReorder="onRowReorder"
  >
    <template #header>
      <div class="flex flex-wrap align-items-end justify-content-end gap-2">
        <div class="datatable-buttons">
          <Button :icon="PrimeIcons.SAVE" rounded raised @click="updateOrder" />
        </div>
      </div>
    </template>
    <Column rowReorder headerStyle="width: 3rem" :reorderableColumn="false" />
    <Column field="code" header="Codi">
      <template #body="slotProps">
        <LinkWorkorder :id="slotProps.data.id" :code="slotProps.data.code" />
      </template>
    </Column>
    <Column header="Estat">
      <template #body="slotProps">
        {{ slotProps.data.status?.name }}
      </template>
    </Column>
    <Column header="Client">
      <template #body="slotProps">
        {{ slotProps.data.reference?.customer?.comercialName }}
      </template>
    </Column>
    <Column header="Referència">
      <template #body="slotProps">
        {{
          slotProps.data.reference?.code +
          " - " +
          slotProps.data.reference?.description
        }}
      </template>
    </Column>
    <Column field="plannedDate" header="Data Prevista" style="width: 12%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.plannedDate) }}
      </template>
    </Column>
    <Column field="order" header="Prioritat"></Column>
    <Column field="plannedQuantity" header="Quantitat"></Column>
  </DataTable>
</template>
<script setup lang="ts">
import LinkWorkorder from "../components/LinkWorkorder.vue";
import { onMounted } from "vue";
import { useWorkOrderStore } from "../store/workorder";
import { PrimeIcons } from "@primevue/core/api";
import { computed } from "vue";
import { formatDate } from "../../../utils/functions";
import { WorkOrderOrder } from "../types";
import { useStore } from "@/store";
import { useToast } from "primevue/usetoast";

const store = useStore();
const toast = useToast();
const workorderStore = useWorkOrderStore();

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: false,
    title: "Prioritzar ordres de fabricació",
  });

  await workorderStore.fetchPlannable();
});

const workorders = computed(() => {
  return Array.isArray(workorderStore.workorders)
    ? workorderStore.workorders
    : [];
});

const onRowReorder = async (event: any) => {
  workorderStore.workorders = event.value.map((item: any, index: number) => {
    item.order = index + 1;
    return item;
  });
};

const updateOrder = async () => {
  const payload: Array<WorkOrderOrder> = workorders.value.map((item: any) => {
    return {
      id: item.id,
      order: item.order,
    };
  });
  const response = await workorderStore.priorize(payload);
  console.log(response);
  if (response.result) {
    toast.add({
      severity: "success",
      summary: "Ordres de fabricació actualitzades",
      detail: "L'ordre de les ordres de fabricació s'ha actualitzat.",
      life: 3000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error actualitzant les ordres de fabricació",
      detail:
        response.errors?.join(", ") || "S'ha produït un error desconegut.",
      life: 5000,
    });
  }
};
</script>

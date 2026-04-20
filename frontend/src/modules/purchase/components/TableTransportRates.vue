<template>
  <!-- Dialog Tarifa -->
  <Dialog
    v-model:visible="rateDialogVisible"
    :header="rateFormMode === FormActionMode.CREATE ? 'Nova tarifa de transport' : 'Editar tarifa de transport'"
    :closable="true"
    :modal="true"
    style="width: 600px"
  >
    <FormTransportRate
      v-if="selectedRate"
      :transportRate="selectedRate"
      @submit="submitRateForm"
    />
  </Dialog>

  <!-- Dialog Detall -->
  <Dialog
    v-model:visible="detailDialogVisible"
    :header="detailFormMode === FormActionMode.CREATE ? 'Nou detall de tarifa' : 'Editar detall de tarifa'"
    :closable="true"
    :modal="true"
    style="width: 650px"
  >
    <FormTransportRateDetail
      v-if="selectedDetail"
      :detail="selectedDetail"
      @submit="submitDetailForm"
    />
  </Dialog>

  <div class="flex flex-column" style="gap: 1rem">
    <!-- Taula Superior: Transport Rates -->
    <DataTable
      :value="transportRateStore.transportRates"
      tableStyle="min-width: 100%"
      :scroll-height="'calc(50vh - 100px)'"
      scrollable
      sortField="name"
      :sortOrder="1"
      :rowClass="(row: TransportRate) => row.id === transportRateStore.transportRate?.id ? 'selected-row' : ''"
      class="clickable-rows"
      @row-click="onRateRowClick"
    >
      <template #header>
        <div class="flex flex-wrap align-items-center justify-content-between gap-2">
          <span class="text-l text-900 font-bold">Tarifes de transport</span>
          <Button :icon="PrimeIcons.PLUS" rounded @click="createRate" />
        </div>
      </template>
      <Column header="Nom" field="name" style="width: 20%" sortable />
      <Column header="Descripció" field="description" style="width: 25%" />
      <Column header="Des de" field="validFrom" style="width: 15%">
        <template #body="slotProps">
          {{ formatDate(slotProps.data.validFrom) }}
        </template>
      </Column>
      <Column header="Fins a" field="validTo" style="width: 15%">
        <template #body="slotProps">
          {{ formatDate(slotProps.data.validTo) }}
        </template>
      </Column>
      <Column style="width: 5%">
        <template #body="slotProps">
          <i
            :class="PrimeIcons.PENCIL"
            class="grid_copy_column_button mr-2"
            @click.stop="editRate(slotProps.data)"
          />
          <i
            :class="PrimeIcons.TIMES"
            class="grid_delete_column_button"
            @click.stop="deleteRate($event, slotProps.data)"
          />
        </template>
      </Column>
    </DataTable>

    <!-- Taula Inferior: Detalls de la tarifa seleccionada -->
    <DataTable
      :value="transportRateStore.transportRateDetails"
      tableStyle="min-width: 100%"
      :scroll-height="'calc(50vh - 100px)'"
      scrollable
      class="clickable-rows"
      @row-click="onDetailRowClick"
    >
      <template #header>
        <div class="flex flex-wrap align-items-center justify-content-between gap-2">
          <span class="text-l text-900 font-bold">
            Detalls
            <span v-if="transportRateStore.transportRate" class="text-600 font-normal ml-2">
              — {{ transportRateStore.transportRate.name }}
            </span>
          </span>
          <Button
            :icon="PrimeIcons.PLUS"
            rounded
            :disabled="!transportRateStore.transportRate"
            @click="createDetail"
          />
        </div>
      </template>
      <Column header="Pes mín." field="minWeight" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.minWeight }} kg</template>
      </Column>
      <Column header="Pes màx." field="maxWeight" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.maxWeight }} kg</template>
      </Column>
      <Column header="Vol. mín." field="minVolume" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.minVolume }} m³</template>
      </Column>
      <Column header="Vol. màx." field="maxVolume" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.maxVolume }} m³</template>
      </Column>
      <Column header="Dist. mín." field="minDistance" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.minDistance }} km</template>
      </Column>
      <Column header="Dist. màx." field="maxDistance" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.maxDistance }} km</template>
      </Column>
      <Column header="Preu" field="price" style="width: 12%">
        <template #body="slotProps">{{ formatCurrency(slotProps.data.price) }}</template>
      </Column>
      <Column style="width: 5%">
        <template #body="slotProps">
          <i
            :class="PrimeIcons.PENCIL"
            class="grid_copy_column_button mr-2"
            @click.stop="editDetail(slotProps.data)"
          />
          <i
            :class="PrimeIcons.TIMES"
            class="grid_delete_column_button"
            @click.stop="deleteDetail($event, slotProps.data)"
          />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useTransportRateStore } from "../store/transportRate";
import { TransportRate, TransportRateDetail } from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { DataTableRowClickEvent } from "primevue/datatable";
import { FormActionMode } from "../../../types/component";
import { formatCurrency, formatDate } from "@/utils/functions";
import FormTransportRate from "./FormTransportRate.vue";
import FormTransportRateDetail from "./FormTransportRateDetail.vue";

const props = defineProps<{
  supplierId: string;
}>();

const confirm = useConfirm();
const toast = useToast();
const transportRateStore = useTransportRateStore();

// --- Rate dialogs ---
const rateDialogVisible = ref(false);
const rateFormMode = ref(FormActionMode.CREATE);
const selectedRate = ref<TransportRate | undefined>(undefined);

// --- Detail dialogs ---
const detailDialogVisible = ref(false);
const detailFormMode = ref(FormActionMode.CREATE);
const selectedDetail = ref<TransportRateDetail | undefined>(undefined);

// --- Rates ---
const createRate = () => {
  selectedRate.value = transportRateStore.setNewTransportRate(props.supplierId);
  rateFormMode.value = FormActionMode.CREATE;
  rateDialogVisible.value = true;
};

const editRate = (rate: TransportRate) => {
  selectedRate.value = { ...rate };
  rateFormMode.value = FormActionMode.EDIT;
  rateDialogVisible.value = true;
};

const onRateRowClick = (row: DataTableRowClickEvent) => {
  const target = row.originalEvent.target as HTMLElement;
  if (
    target?.className &&
    typeof target.className === "string" &&
    target.className.includes("grid_delete_column_button")
  )
    return;

  const rate = row.data as TransportRate;
  transportRateStore.fetchTransportRateDetails(rate);
  editRate(rate);
};

const submitRateForm = async (rate: TransportRate) => {
  let result = false;
  if (rateFormMode.value === FormActionMode.CREATE) {
    result = await transportRateStore.createTransportRate(rate);
    if (result) toast.add({ severity: "success", summary: "Tarifa creada", life: 4000 });
  } else {
    result = await transportRateStore.updateTransportRate(rate);
    if (result) toast.add({ severity: "success", summary: "Tarifa actualitzada", life: 4000 });
  }
  if (result) rateDialogVisible.value = false;
};

const deleteRate = (event: Event, rate: TransportRate) => {
  confirm.require({
    target: event.currentTarget as HTMLElement,
    message: "Está segur que vol eliminar la tarifa?",
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const result = await transportRateStore.deleteTransportRate(rate);
      if (result) toast.add({ severity: "success", summary: "Tarifa eliminada", life: 4000 });
    },
  });
};

// --- Details ---
const createDetail = () => {
  if (!transportRateStore.transportRate) return;
  selectedDetail.value = transportRateStore.setNewTransportRateDetail(
    transportRateStore.transportRate.id
  );
  detailFormMode.value = FormActionMode.CREATE;
  detailDialogVisible.value = true;
};

const editDetail = (detail: TransportRateDetail) => {
  selectedDetail.value = { ...detail };
  detailFormMode.value = FormActionMode.EDIT;
  detailDialogVisible.value = true;
};

const onDetailRowClick = (row: DataTableRowClickEvent) => {
  const target = row.originalEvent.target as HTMLElement;
  if (target.className.includes("grid_delete_column_button")) return;
  editDetail(row.data as TransportRateDetail);
};

const submitDetailForm = async (detail: TransportRateDetail) => {
  let result = false;
  if (detailFormMode.value === FormActionMode.CREATE) {
    result = await transportRateStore.createTransportRateDetail(detail);
    if (result) toast.add({ severity: "success", summary: "Detall creat", life: 4000 });
  } else {
    result = await transportRateStore.updateTransportRateDetail(detail);
    if (result) toast.add({ severity: "success", summary: "Detall actualitzat", life: 4000 });
  }
  if (result) detailDialogVisible.value = false;
};

const deleteDetail = (event: Event, detail: TransportRateDetail) => {
  confirm.require({
    target: event.currentTarget as HTMLElement,
    message: "Está segur que vol eliminar el detall?",
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const result = await transportRateStore.deleteTransportRateDetail(detail);
      if (result) toast.add({ severity: "success", summary: "Detall eliminat", life: 4000 });
    },
  });
};
</script>

<style scoped>
:deep(.selected-row) {
  background-color: var(--p-primary-100) !important;
  font-weight: 600;
}
</style>

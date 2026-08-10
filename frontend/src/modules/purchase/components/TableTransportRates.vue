<template>
  <!-- Dialog Tarifa -->
  <Dialog
    v-model:visible="rateDialogVisible"
    :header="rateFormMode === FormActionMode.CREATE ? t('purchase.transportRates.dialogs.createRate') : t('purchase.transportRates.dialogs.editRate')"
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
    :header="detailFormMode === FormActionMode.CREATE ? t('purchase.transportRates.dialogs.createDetail') : t('purchase.transportRates.dialogs.editDetail')"
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
          <span class="text-l text-900 font-bold">{{ t("purchase.transportRates.title") }}</span>
          <Button :icon="PrimeIcons.PLUS" rounded @click="createRate" />
        </div>
      </template>
      <Column :header="t('purchase.transportRates.columns.name')" field="name" style="width: 20%" sortable />
      <Column :header="t('purchase.transportRates.columns.description')" field="description" style="width: 25%" />
      <Column :header="t('purchase.transportRates.columns.validFrom')" field="validFrom" style="width: 15%">
        <template #body="slotProps">
          {{ formatDate(slotProps.data.validFrom) }}
        </template>
      </Column>
      <Column :header="t('purchase.transportRates.columns.validTo')" field="validTo" style="width: 15%">
        <template #body="slotProps">
          {{ formatDate(slotProps.data.validTo) }}
        </template>
      </Column>
      <Column style="width: 5%">
        <template #body="slotProps">
          <Button
            :icon="PrimeIcons.PENCIL"
            text
            rounded
            size="small"
            class="w-2rem h-2rem p-0"
            @click.stop="editRate(slotProps.data)"
          />
        </template>
      </Column>
      <Column style="width: 5%">
        <template #body="slotProps">
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
          <span class="text-l text-900 font-bold flex align-items-center">
            {{ t("purchase.transportRates.details") }}
            <span v-if="transportRateStore.transportRate" class="text-600 font-normal ml-2 flex align-items-center">
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
      <Column :header="t('purchase.transportRates.columns.minimumWeight')" field="minWeight" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.minWeight }} kg</template>
      </Column>
      <Column :header="t('purchase.transportRates.columns.maximumWeight')" field="maxWeight" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.maxWeight }} kg</template>
      </Column>
      <Column :header="t('purchase.transportRates.columns.minimumVolume')" field="minVolume" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.minVolume }} m³</template>
      </Column>
      <Column :header="t('purchase.transportRates.columns.maximumVolume')" field="maxVolume" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.maxVolume }} m³</template>
      </Column>
      <Column :header="t('purchase.transportRates.columns.minimumDistance')" field="minDistance" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.minDistance }} km</template>
      </Column>
      <Column :header="t('purchase.transportRates.columns.maximumDistance')" field="maxDistance" style="width: 12%">
        <template #body="slotProps">{{ slotProps.data.maxDistance }} km</template>
      </Column>
      <Column :header="t('purchase.transportRates.columns.price')" field="price" style="width: 12%">
        <template #body="slotProps">{{ formatCurrency(slotProps.data.price) }}</template>
      </Column>
      <Column style="width: 5%">
        <template #body="slotProps">
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
import { useI18n } from "vue-i18n";

const props = defineProps<{
  supplierId: string;
}>();

const confirm = useConfirm();
const toast = useToast();
const transportRateStore = useTransportRateStore();
const { t } = useI18n();

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
  if (selectedRate.value.validFrom) selectedRate.value.validFrom = new Date(selectedRate.value.validFrom);
  if (selectedRate.value.validTo) selectedRate.value.validTo = new Date(selectedRate.value.validTo);
  
  rateFormMode.value = FormActionMode.EDIT;
  rateDialogVisible.value = true;
};

const onRateRowClick = (row: DataTableRowClickEvent) => {
  const target = row.originalEvent.target as HTMLElement;
  if (
    target?.className &&
    typeof target.className === "string" &&
    (target.className.includes("grid_delete_column_button") || target.className.includes("grid_edit_column_button") || target.className.includes("p-button"))
  )
    return;

  const rate = row.data as TransportRate;
  transportRateStore.fetchTransportRateDetails(rate);
};

const submitRateForm = async (rate: TransportRate) => {
  let result = false;
  if (rateFormMode.value === FormActionMode.CREATE) {
    result = await transportRateStore.createTransportRate(rate);
    if (result) toast.add({ severity: "success", summary: t("purchase.transportRates.messages.rateCreated"), life: 4000 });
  } else {
    result = await transportRateStore.updateTransportRate(rate);
    if (result) toast.add({ severity: "success", summary: t("purchase.transportRates.messages.rateUpdated"), life: 4000 });
  }
  if (result) rateDialogVisible.value = false;
};

const deleteRate = (event: Event, rate: TransportRate) => {
  confirm.require({
    target: event.currentTarget as HTMLElement,
    message: t("purchase.transportRates.messages.confirmDeleteRate"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const result = await transportRateStore.deleteTransportRate(rate);
      if (result) toast.add({ severity: "success", summary: t("purchase.transportRates.messages.rateDeleted"), life: 4000 });
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
  if (
    target?.className &&
    typeof target.className === "string" &&
    target.className.includes("grid_delete_column_button")
  )
    return;
    
  editDetail(row.data as TransportRateDetail);
};

const submitDetailForm = async (detail: TransportRateDetail) => {
  let result = false;
  if (detailFormMode.value === FormActionMode.CREATE) {
    result = await transportRateStore.createTransportRateDetail(detail);
    if (result) toast.add({ severity: "success", summary: t("purchase.transportRates.messages.detailCreated"), life: 4000 });
  } else {
    result = await transportRateStore.updateTransportRateDetail(detail);
    if (result) toast.add({ severity: "success", summary: t("purchase.transportRates.messages.detailUpdated"), life: 4000 });
  }
  if (result) detailDialogVisible.value = false;
};

const deleteDetail = (event: Event, detail: TransportRateDetail) => {
  confirm.require({
    target: event.currentTarget as HTMLElement,
    message: t("purchase.transportRates.messages.confirmDeleteDetail"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const result = await transportRateStore.deleteTransportRateDetail(detail);
      if (result) toast.add({ severity: "success", summary: t("purchase.transportRates.messages.detailDeleted"), life: 4000 });
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

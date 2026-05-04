<template>
  <!-- Dialog Tarifa -->
  <Dialog
    v-model:visible="rateDialogVisible"
    :header="rateFormMode === FormActionMode.CREATE ? 'Nova tarifa de compra' : 'Editar tarifa de compra'"
    :closable="true"
    :modal="true"
    style="width: 600px"
    @hide="selectedRate = undefined"
  >
    <FormPurchaseRate
      v-if="selectedRate"
      :key="selectedRate.id"
      :purchaseRate="selectedRate"
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
    @hide="selectedDetail = undefined"
  >
    <FormPurchaseRateDetail
      v-if="selectedDetail"
      :key="selectedDetail.id"
      :detail="selectedDetail"
      @submit="submitDetailForm"
    />
  </Dialog>

  <!-- Dialog Duplicar -->
  <Dialog
    v-model:visible="duplicateDialogVisible"
    header="Duplicar tarifa"
    :closable="true"
    :modal="true"
    style="width: 500px"
  >
    <div class="flex flex-column gap-3">
      <div>
        <label class="block text-900 mb-2">Nou nom</label>
        <InputText v-model="duplicateData.name" class="w-full" />
      </div>
      <div class="two-columns">
        <div>
          <label class="block text-900 mb-2">Data inici</label>
          <DatePicker v-model="duplicateData.validFrom" class="w-full" dateFormat="dd/mm/yy" />
        </div>
        <div>
          <label class="block text-900 mb-2">Data fi</label>
          <DatePicker v-model="duplicateData.validTo" class="w-full" dateFormat="dd/mm/yy" />
        </div>
      </div>
      <div class="mt-2 text-right">
        <Button label="Duplicar" icon="pi pi-copy" @click="confirmDuplicate" />
      </div>
    </div>
  </Dialog>

  <div class="flex flex-column" style="gap: 1rem">
    <!-- Taula Superior: Purchase Rates -->
    <DataTable
      :value="purchaseRateStore.purchaseRates"
      tableStyle="min-width: 100%"
      :scroll-height="'calc(50vh - 100px)'"
      scrollable
      sortField="validFrom"
      :sortOrder="-1"
      :rowClass="(row: PurchaseRate) => row.id === purchaseRateStore.purchaseRate?.id ? 'selected-row' : ''"
      class="clickable-rows"
      @row-click="onRateRowClick"
    >
      <template #header>
        <div class="flex flex-wrap align-items-center justify-content-between gap-2">
          <span class="text-l text-900 font-bold">Tarifes de compra</span>
          <Button :icon="PrimeIcons.PLUS" rounded @click="createRate" />
        </div>
      </template>
      <Column header="Nom" field="name" style="width: 30%" sortable />
      <Column header="Des de" field="validFrom" style="width: 20%" sortable>
        <template #body="slotProps">
          {{ formatDate(slotProps.data.validFrom) }}
        </template>
      </Column>
      <Column header="Fins a" field="validTo" style="width: 20%">
        <template #body="slotProps">
          {{ formatDate(slotProps.data.validTo) }}
        </template>
      </Column>
      <Column style="width: 10%">
        <template #body="slotProps">
          <div class="flex gap-2">
            <i
              :class="PrimeIcons.COPY"
              class="grid_copy_column_button mr-2"
              title="Duplicar"
              @click.stop="openDuplicateDialog(slotProps.data)"
            />
            <i
              :class="PrimeIcons.TIMES"
              class="grid_delete_column_button"
              @click.stop="deleteRate($event, slotProps.data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <!-- Taula Inferior: Detalls de la tarifa seleccionada -->
    <DataTable
      :value="purchaseRateStore.purchaseRateDetails"
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
            <span v-if="purchaseRateStore.purchaseRate" class="text-600 font-normal ml-2">
              — {{ purchaseRateStore.purchaseRate.name }}
            </span>
          </span>
          <Button
            :icon="PrimeIcons.PLUS"
            rounded
            :disabled="!purchaseRateStore.purchaseRate"
            @click="createDetail"
          />
        </div>
      </template>
      <Column header="Referència" field="reference.code" style="width: 20%">
        <template #body="slotProps">
            {{ referenceStore.getShortNameById(slotProps.data.referenceId) }}
        </template>
      </Column>
      <Column header="Càlcul" field="calculationType" style="width: 15%">
        <template #body="slotProps">
          {{ getCalculationTypeLabel(slotProps.data.calculationType) }}
        </template>
      </Column>
      <Column header="Des de" field="from" style="width: 15%">
        <template #body="slotProps">{{ slotProps.data.from }}</template>
      </Column>
      <Column header="Fins a" field="to" style="width: 15%">
        <template #body="slotProps">{{ slotProps.data.to }}</template>
      </Column>
      <Column header="Preu" field="price" style="width: 15%">
        <template #body="slotProps">{{ formatCurrency(slotProps.data.price) }}</template>
      </Column>
      <Column style="width: 10%">
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
import { ref, onMounted } from "vue";
import { usePurchaseRateStore } from "../store/purchaseRate";
import { useReferenceStore } from "../../shared/store/reference";
import { PurchaseRate, PurchaseRateDetail, CalculationType } from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { DataTableRowClickEvent } from "primevue/datatable";
import { FormActionMode } from "../../../types/component";
import { formatCurrency, formatDate } from "@/utils/functions";
import FormPurchaseRate from "./FormPurchaseRate.vue";
import FormPurchaseRateDetail from "./FormPurchaseRateDetail.vue";

const props = defineProps<{
  supplierId: string;
}>();

const confirm = useConfirm();
const toast = useToast();
const purchaseRateStore = usePurchaseRateStore();
const referenceStore = useReferenceStore();

// --- Rate dialogs ---
const rateDialogVisible = ref(false);
const rateFormMode = ref(FormActionMode.CREATE);
const selectedRate = ref<PurchaseRate | undefined>(undefined);

// --- Detail dialogs ---
const detailDialogVisible = ref(false);
const detailFormMode = ref(FormActionMode.CREATE);
const selectedDetail = ref<PurchaseRateDetail | undefined>(undefined);

// --- Duplicate dialog ---
const duplicateDialogVisible = ref(false);
const duplicateData = ref({
  name: "",
  validFrom: new Date(),
  validTo: new Date(),
});

onMounted(async () => {
    await purchaseRateStore.fetchPurchaseRatesBySupplierId(props.supplierId);
    if (referenceStore.references === undefined) {
        await referenceStore.fetchReferencesByModule("purchase");
    }
});

const getCalculationTypeLabel = (type: CalculationType) => {
  switch (type) {
    case CalculationType.Volume: return "Volum";
    case CalculationType.Weight: return "Pes";
    case CalculationType.Units: return "Unitats";
    default: return "Unitats";
  }
};

// --- Rates ---
const createRate = () => {
  selectedRate.value = purchaseRateStore.setNewPurchaseRate(props.supplierId);
  rateFormMode.value = FormActionMode.CREATE;
  rateDialogVisible.value = true;
};

const editRate = (rate: PurchaseRate) => {
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

  const rate = row.data as PurchaseRate;
  purchaseRateStore.fetchPurchaseRateDetails(rate);
  editRate(rate);
};

const submitRateForm = async (rate: PurchaseRate) => {
  let result = false;
  try {
    if (rateFormMode.value === FormActionMode.CREATE) {
      result = await purchaseRateStore.createPurchaseRate(rate);
      if (result) toast.add({ severity: "success", summary: "Tarifa creada", life: 4000 });
    } else {
      result = await purchaseRateStore.updatePurchaseRate(rate);
      if (result) toast.add({ severity: "success", summary: "Tarifa actualitzada", life: 4000 });
    }
    if (result) rateDialogVisible.value = false;
  } catch (error: any) {
      toast.add({ severity: "error", summary: "Error", detail: error.message || "Error al guardar la tarifa", life: 4000 });
  }
};

const deleteRate = (event: Event, rate: PurchaseRate) => {
  confirm.require({
    target: event.currentTarget as HTMLElement,
    message: "Está segur que vol eliminar la tarifa?",
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const result = await purchaseRateStore.deletePurchaseRate(rate);
      if (result) toast.add({ severity: "success", summary: "Tarifa eliminada", life: 4000 });
    },
  });
};

const openDuplicateDialog = (rate: PurchaseRate) => {
  selectedRate.value = rate;
  duplicateData.value = {
    name: `${rate.name} (copia)`,
    validFrom: new Date(rate.validFrom),
    validTo: new Date(rate.validTo),
  };
  // Increment year by default for duplication
  duplicateData.value.validFrom.setFullYear(duplicateData.value.validFrom.getFullYear() + 1);
  duplicateData.value.validTo.setFullYear(duplicateData.value.validTo.getFullYear() + 1);
  
  duplicateDialogVisible.value = true;
};

const confirmDuplicate = async () => {
  if (!selectedRate.value) return;
  const result = await purchaseRateStore.duplicatePurchaseRate(
    selectedRate.value,
    duplicateData.value.name,
    duplicateData.value.validFrom,
    duplicateData.value.validTo
  );
  if (result) {
    toast.add({ severity: "success", summary: "Tarifa duplicada", life: 4000 });
    duplicateDialogVisible.value = false;
  } else {
    toast.add({ severity: "error", summary: "Error", detail: "No s'ha pogut duplicar la tarifa. Revisa les dates.", life: 4000 });
  }
};

// --- Details ---
const createDetail = () => {
  if (!purchaseRateStore.purchaseRate) return;
  selectedDetail.value = purchaseRateStore.setNewPurchaseRateDetail(
    purchaseRateStore.purchaseRate.id
  );
  detailFormMode.value = FormActionMode.CREATE;
  detailDialogVisible.value = true;
};

const editDetail = (detail: PurchaseRateDetail) => {
  selectedDetail.value = { ...detail };
  detailFormMode.value = FormActionMode.EDIT;
  detailDialogVisible.value = true;
};

const onDetailRowClick = (row: DataTableRowClickEvent) => {
  const target = row.originalEvent.target as HTMLElement;
  if (typeof target.className === "string" && target.className.includes("grid_delete_column_button")) return;
  editDetail(row.data as PurchaseRateDetail);
};

const submitDetailForm = async (detail: PurchaseRateDetail) => {
  let result = false;
  if (detailFormMode.value === FormActionMode.CREATE) {
    result = await purchaseRateStore.createPurchaseRateDetail(detail);
    if (result) toast.add({ severity: "success", summary: "Detall creat", life: 4000 });
  } else {
    result = await purchaseRateStore.updatePurchaseRateDetail(detail);
    if (result) toast.add({ severity: "success", summary: "Detall actualitzat", life: 4000 });
  }
  if (result) detailDialogVisible.value = false;
};

const deleteDetail = (event: Event, detail: PurchaseRateDetail) => {
  confirm.require({
    target: event.currentTarget as HTMLElement,
    message: "Está segur que vol eliminar el detall?",
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const result = await purchaseRateStore.deletePurchaseRateDetail(detail);
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
.two-columns {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
}
</style>

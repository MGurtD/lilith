<template>
  <DataTable
    :value="workOrderStore.workorderPhases"
    class="p-datatable-sm small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :sort-order="1"
    sort-field="date"
    paginator
    :rows="20"
    dataKey="id"
    ><template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Seleccionar fases</span>
        <div class="datatable-filter-3">
          <div class="filter-field">
            <ExerciseDatePicker
              :exercises="sharedStore.exercises"
              @range-selected="fetchWorkOrderPhases"
            />
          </div>
        </div>
        <div class="datatable-buttons">
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER"
            rounded
            raised
            @click="fetchWorkOrderPhases"
          />
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER_SLASH"
            rounded
            raised
            @click="cleanFilter"
          />
          <Button
            :size="'small'"
            label="Crear comandes"
            rounded
            @click="sendData"
          />
        </div>
      </div>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div></div>
      </div>
    </template>
    <!--    <Column selectionMode="multiple" headerStyle="width: 1rem"></Column>-->
    <Column field="workOrder.code" header="Ordre de fabricació"></Column>
    <Column field="description" header="Fase" style="width: 15%"></Column>
    <Column header="Referència">
      <template #body="slotProps">
        {{ getName(slotProps.data.serviceReferenceId) }}
      </template>
    </Column>
    <Column field="workOrder.plannedQuantity" header="Quantitat planificada">
    </Column>
    <Column header="Proveïdor">
      <template #body="slotProps">
        <Select
          v-model="selectedSuppliers[slotProps.data.id]"
          :options="suppliersByReference[slotProps.data.id]"
          placeholder="Selecciona..."
          optionValue="id"
          optionLabel="comercialName"
          @show="() => onSupplierDropdownShow(slotProps.data)"
          @focus="() => onSupplierDropdownShow(slotProps.data)"
          @change="(event) => selectPhase(slotProps.data, event.value)"
          showClear
        >
        </Select>
      </template>
    </Column>
  </DataTable>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useWorkOrderStore } from "../../production/store/workorder";
import { SupplierService } from "../services/suppliers.service";
import { PurchaseOrderFromWO, Supplier } from "../types";
import { useReferenceStore } from "../../shared/store/reference";
import { useSharedDataStore } from "../../shared/store/masterData";
import { WorkOrderPhase } from "../../production/types";
import { useOrderStore } from "../store/order";
import { useUserFilterStore } from "../../../store/userfilter";
import { formatDateForQueryParameter } from "../../../utils/functions";
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";

const router = useRouter();
const store = useStore();
const toast = useToast();
const workOrderStore = useWorkOrderStore();
const supplierService = new SupplierService("/supplier");
const referenceStore = useReferenceStore();
const sharedStore = useSharedDataStore();
const orderStore = useOrderStore();
const userFilterStore = useUserFilterStore();

const selectedPhases = ref<WorkOrderPhase[]>([]);
const suppliersByReference = ref<{ [key: string]: Supplier[] }>({});
const phaseLoading = ref<{ [key: string]: boolean }>({});
const allSuppliers = ref<Supplier[] | null>(null);
const selectedSuppliers = ref<{ [key: string]: string }>({});
const purchaseOrders = ref<PurchaseOrderFromWO[]>([]);

const mostrarToastInfo = (summary: string, detail: string) => {
  toast.add({
    severity: "info",
    summary: summary,
    detail: detail,
    life: 5000,
  });
};

const obtenirDates = () => {
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0],
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);
    return [startTime, endTime];
  }
  return ["", ""];
};
const supplierMapping = ref<{ [key: string]: string }>({});

const selectPhase = (phase: WorkOrderPhase, selectedSupplierId: string) => {
  updatePhase(phase.id, selectedSupplierId);
  if (selectedSupplierId) {
    selectedPhases.value.push(phase);
  } else {
    selectedPhases.value = selectedPhases.value.filter(
      (p, ind) => p.id !== phase.id,
    );
  }
};

const updatePhase = (phaseId: string, selectedSupplierId: string) => {
  const phaseIndex = workOrderStore.workorderPhases?.findIndex(
    (phase) => phase.id === phaseId,
  );

  if (phaseIndex !== -1) {
    supplierMapping.value[phaseId] = selectedSupplierId;
  } else {
    const selectedPhase = purchaseOrders.value.find(
      (phase) => phase.phaseId === phaseId,
    );

    if (selectedPhase) {
      supplierMapping.value[phaseId] = selectedSupplierId;
    }
  }
};

const fetchWorkOrderPhases = async () => {
  if (!store.exercisePicker.dates) {
    mostrarToastInfo("Filtre Invàlid", "Seleccioni un perióde");
    return;
  }
  const [startTime, endTime] = obtenirDates();
  if (endTime == "") {
    return;
  }

  await workOrderStore.fetchExternalPhases(startTime, endTime);
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.SHOPPING_CART,
    title: "Generació de comandes de compra",
  });
  await sharedStore.fetchMasterData();
  await referenceStore.fetchReferences();
  setCurrentYear();
  getUserFilter();
});

// Lazy load suppliers when user opens the dropdown for a phase
const fetchSuppliersForPhase = async (phase: WorkOrderPhase) => {
  if (!phase || !phase.serviceReferenceId) return;
  if (suppliersByReference.value[phase.id] || phaseLoading.value[phase.id]) {
    return; // already loaded or in progress
  }
  try {
    phaseLoading.value[phase.id] = true;
    const suppliers = await supplierService.getSuppliersReferenceById(
      phase.serviceReferenceId,
    );
    if (suppliers === null) {
      // fallback: load all suppliers once if not yet
      if (!allSuppliers.value) {
        const all = await supplierService.getAll();
        allSuppliers.value = all ? all : [];
      }
      if (allSuppliers.value) {
        suppliersByReference.value[phase.id] = allSuppliers.value;
      }
    } else {
      suppliersByReference.value[phase.id] = suppliers;
    }
  } catch (e) {
    // optional: could add a toast here
  } finally {
    phaseLoading.value[phase.id] = false;
  }
};

const onSupplierDropdownShow = (phase: WorkOrderPhase) => {
  fetchSuppliersForPhase(phase);
};

onUnmounted(() => {
  const savedFilter = {
    exercisePicker: store.exercisePicker,
  };

  userFilterStore.addFilter("ExternalPhasesToPurhcaseOrders", "", savedFilter);
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter(
    "ExternalPhasesToPurhcaseOrders",
    "",
  );
  if (userFilter) {
    if (userFilter.exercisePicker) {
      store.exercisePicker.exercise = userFilter.exercisePicker.exercise;
      store.exercisePicker.dates = [
        new Date(userFilter.exercisePicker.dates[0]),
        new Date(userFilter.exercisePicker.dates[1]),
      ];
    }
  }
};

const setCurrentYear = () => {
  const year = new Date().getFullYear().toString();
  const currentExercise = sharedStore.exercises?.find((e) => e.name === year);

  if (currentExercise) {
    store.exercisePicker.exercise = currentExercise;
    store.exercisePicker.dates = [
      new Date(store.exercisePicker.exercise.startDate),
      new Date(store.exercisePicker.exercise.endDate),
    ];
  }
};

const cleanFilter = () => {
  setCurrentYear();

  //filterPhases();
};

const getName = (id: string) => {
  return referenceStore.getFullNameById(id) || "Desconeguda";
};

const sendData = async () => {
  if (selectedPhases.value.length == 0) {
    mostrarToastInfo(
      "OFs no seleccionades",
      "Has de seleccionar una OF com a mínim",
    );
    return;
  }

  for (const phase of selectedPhases.value) {
    purchaseOrders.value.push({
      workorderId: phase.workOrder?.id || "",
      workorderDescription: phase.description || "",
      phaseId: phase.id,
      phaseDescription: phase.description,
      serviceReferenceId: phase.serviceReferenceId || "",
      serviceReferenceName: getName(phase.serviceReferenceId || ""),
      supplierId: supplierMapping.value[phase.id],
      quantity: phase.workOrder?.plannedQuantity || 0, // o l'atribut que estigui adequat
    });
  }

  const result = await orderStore.createFromWo(purchaseOrders.value);
  if (!result.result) {
    toast.add({
      severity: "error",
      summary: "Error creació",
      detail: "Error al crear la comanda",
      life: 5000,
    });
  } else {
    toast.add({
      severity: "success",
      summary: "Creació comandes",
      detail: "Comandes creades correctament",
      life: 5000,
    });
    router.push("/purchase-orders");
  }
};
</script>

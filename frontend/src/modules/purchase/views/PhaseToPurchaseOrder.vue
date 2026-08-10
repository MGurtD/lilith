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
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :body-width="filterBodyWidth"
        embedded
        @filter="fetchWorkOrderPhases"
        @clear="cleanFilter"
      >
        <template #prepend>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >{{ t("purchase.phaseToOrder.filters.period") }}</label
            >
            <DatePicker
              v-model="filter.dates"
              selectionMode="range"
              dateFormat="dd/mm/yy"
              :placeholder="t('purchase.phaseToOrder.placeholders.selectPeriod')"
              showIcon
              class="w-full"
              size="small"
            />
          </div>
        </template>
        <template #append>
          <Button
            :size="'small'"
            :label="t('purchase.phaseToOrder.actions.createOrders')"
            rounded
            @click="sendData"
          />
        </template>
      </TableFilter>
    </template>
    <!--    <Column selectionMode="multiple" headerStyle="width: 1rem"></Column>-->
    <Column field="workOrder.code" :header="t('purchase.phaseToOrder.columns.workOrder')"></Column>
    <Column field="description" :header="t('purchase.phaseToOrder.columns.phase')" style="width: 15%"></Column>
    <Column :header="t('purchase.phaseToOrder.columns.reference')">
      <template #body="slotProps">
        {{ getName(slotProps.data.serviceReferenceId) }}
      </template>
    </Column>
    <Column field="workOrder.plannedQuantity" :header="t('purchase.phaseToOrder.columns.plannedQuantity')">
    </Column>
    <Column :header="t('purchase.phaseToOrder.columns.supplier')">
      <template #body="slotProps">
        <Select
          v-model="selectedSuppliers[slotProps.data.id]"
          :options="suppliersByReference[slotProps.data.id]"
          :placeholder="t('purchase.phaseToOrder.placeholders.selectSupplier')"
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
import TableFilter from "../../../components/tables/TableFilter.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const toast = useToast();
const workOrderStore = useWorkOrderStore();
const supplierService = new SupplierService("/supplier");
const referenceStore = useReferenceStore();
const sharedStore = useSharedDataStore();
const orderStore = useOrderStore();
const userFilterStore = useUserFilterStore();
const { t } = useI18n();

const filterBodyWidth: FilterBodyWidth = { desktop: "33%", tablet: "50%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
});

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
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);
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
  if (
    !filter.value.dates ||
    filter.value.dates.length < 2 ||
    !filter.value.dates[1]
  ) {
    mostrarToastInfo(
      t("purchase.messages.invalidFilter"),
      t("purchase.phaseToOrder.messages.selectPeriod"),
    );
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
    title: t("purchase.phaseToOrder.title"),
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
  userFilterStore.addFilter("ExternalPhasesToPurhcaseOrders", "", filter.value);
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter(
    "ExternalPhasesToPurhcaseOrders",
    "",
  );
  if (userFilter) {
    if (userFilter.dates) {
      filter.value.dates = [
        new Date(userFilter.dates[0]),
        new Date(userFilter.dates[1]),
      ];
    }
  }
};

const setCurrentYear = () => {
  const now = new Date();
  filter.value.dates = [
    new Date(now.getFullYear(), 0, 1),
    new Date(now.getFullYear(), 11, 31),
  ];
};

const cleanFilter = () => {
  setCurrentYear();
};

const getName = (id: string) => {
  return referenceStore.getFullNameById(id) || t("purchase.phaseToOrder.messages.unknownReference");
};

const sendData = async () => {
  if (selectedPhases.value.length == 0) {
    mostrarToastInfo(
      t("purchase.phaseToOrder.messages.noWorkOrdersSelected"),
      t("purchase.phaseToOrder.messages.selectAtLeastOneWorkOrder"),
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
      summary: t("purchase.phaseToOrder.messages.creationError"),
      detail: t("purchase.phaseToOrder.messages.orderCreationError"),
      life: 5000,
    });
  } else {
    toast.add({
      severity: "success",
      summary: t("purchase.phaseToOrder.messages.ordersCreated"),
      detail: t("purchase.phaseToOrder.messages.ordersCreatedSuccessfully"),
      life: 5000,
    });
    router.push("/purchase-orders");
  }
};
</script>

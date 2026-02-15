<template>
  <DataTable
    class="p-datatable-sm"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    dataKey="reference.id"
    v-model:expandedRows="expandedRows"
    :value="filteredOrders"
  >
    <template #header>
      <header class="selector-filter">
        <div class="selector-filter-field">
          <label for="">Buscar</label> &nbsp;
          <InputText
            style="width: 250px; height: 35px"
            placeholder="Comanda, OF o referència"
            v-model="filterReference"
            size="small"
          />
        </div>
        <div>
          <div>
            <Button
              @click="onSelectedClick"
              :size="'small'"
              :icon="PrimeIcons.CHECK_SQUARE"
              label="Afegir"
            ></Button>
          </div>
        </div>
      </header>
    </template>
    <Column expander style="width: 5%" />
    <Column header="" field="number" style="width: 85%">
      <template #body="{ data }">
        <b
          >{{ referenceStore.getFullNameById(data.reference.id) }} |
          {{ data.description }}
        </b>
      </template>
    </Column>
    <Column header="" field="number" style="width: 10%">
      <template #body="{ data }">
        <div>
          <label>Import</label>
          <BaseInput
            :type="BaseInputType.CURRENCY"
            style="width: 250px; height: 35px"
            v-model="data.price"
            placeholder="Import (€)"
          />
        </div>
      </template>
    </Column>
    <template #expansion="{ data }">
      <DataTable
        class="p-datatable-sm"
        :value="data.details"
        v-model:selection="selectedOrderDetails"
      >
        <Column selectionMode="multiple" headerStyle="width: 2%"></Column>
        <Column header="Comanda" field="orderNumber" headerStyle="width: 15%" />
        <Column
          header="D. Prevista"
          field="expectedReceiptDate"
          headerStyle="width: 15%"
        >
          <template #body="{ data }">
            {{ formatDate(data.expectedReceiptDate) }}
          </template>
        </Column>
        <Column header="OF" field="workOrder" headerStyle="width: 60%">
          <template #body="{ data }">
            <span v-if="data.workOrder.length > 0">{{
              `${data.workOrder} - ${data.workOrderPhase}`
            }}</span>
          </template>
        </Column>
        <Column header="Quantitat pendent" headerStyle="width: 10%">
          <template #body="{ data }">
            <BaseInput
              style="width: 250px; height: 35px"
              :type="BaseInputType.NUMERIC"
              :min="0"
              :max="data.pendingQuantity"
              v-model="data.pendingQuantity"
            />
          </template>
        </Column>
      </DataTable>
    </template>
  </DataTable>
</template>
<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import {
  AddReceptionsRequest,
  PurchaseOrderReceiptDetail,
  Receipt,
  ReceiptOrderDetail,
  ReceiptOrderDetailGroup,
} from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { formatDate, getNewUuid } from "../../../utils/functions";
import { useReferenceStore } from "../../shared/store/reference";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { BaseInputType } from "../../../types/component";

const toast = useToast();
const store = useStore();
const referenceStore = useReferenceStore();
const expandedRows = ref({});
const filterReference = ref("");
const selectedOrderDetails = ref([] as Array<ReceiptOrderDetail>);

const props = defineProps<{
  receipt: Receipt;
  groupedOrderDetails: Array<ReceiptOrderDetailGroup> | undefined;
}>();
const emits = defineEmits<{
  (e: "selected", receipts: AddReceptionsRequest): void;
}>();

const filteredOrders = computed(() => {
  var filtered = [] as Array<ReceiptOrderDetailGroup>;
  if (props.groupedOrderDetails) {
    try {
      // Filter orders by reference full name
      filtered = props.groupedOrderDetails.filter((group) =>
        group.reference.code.includes(filterReference.value),
      );
    } catch (error) {
      console.error("Error filtering orders", error);
    }
  }
  return filtered;
});

const expandAll = () => {
  if (!props.groupedOrderDetails) {
    return;
  }

  expandedRows.value = props.groupedOrderDetails.reduce(
    (acc: { [key: string]: boolean }, p) => {
      acc[p.reference.id] = true;
      return acc;
    },
    {},
  );
};

onMounted(() => {
  if (!props.groupedOrderDetails) {
    console.error("No orders to show");
    return;
  }

  expandAll();
});

const validateSelection = () => {
  if (selectedOrderDetails.value.length === 0) {
    toast.add({
      severity: "warn",
      summary: "Selecció inválida",
      detail: "Selecciona alguna línia per afegir-la a l'albarà",
      life: 6000,
    });

    return false;
  }

  if (
    selectedOrderDetails.value.some((detail) => detail.pendingQuantity === 0)
  ) {
    toast.add({
      severity: "warn",
      summary: "Selecció inválida",
      detail: "No es poden afegir línies amb quantitat 0",
      life: 6000,
    });

    return false;
  }

  return true;
};

const onSelectedClick = () => {
  if (!props.groupedOrderDetails) {
    return;
  }

  if (!validateSelection()) {
    return;
  }

  // Recorrer grups i ponderar l'import sobre la quantitat de les linies seleccionades
  const receptionsRequest = {
    receiptId: props.receipt.id,
    receptions: [],
  } as AddReceptionsRequest;

  // Filtrar els detalls seleccionats per cada grup
  const clonedGroupedOrderDetails = JSON.parse(
    JSON.stringify(props.groupedOrderDetails),
  ) as Array<ReceiptOrderDetailGroup>;
  const groupsWithSelectedDetails = clonedGroupedOrderDetails.map((group) => {
    group.details = selectedOrderDetails.value.filter((selectedDetail) =>
      group.details.some((detail) => detail.id === selectedDetail.id),
    );
    return group;
  });

  // Recorrer i ponderar l'import de cada grup
  groupsWithSelectedDetails.forEach((group) => {
    const totalQuantity = group.details.reduce((acc, detail) => {
      return acc + detail.pendingQuantity;
    }, 0);

    console.log("Total quantity", totalQuantity);
    console.log("Price", group.price);

    group.details.forEach((detail) => {
      console.log(
        "Detail price",
        detail.pendingQuantity,
        (group.price / totalQuantity) * detail.pendingQuantity,
      );

      receptionsRequest.receptions.push({
        receiptDetailId: getNewUuid(),
        purchaseOrderDetailId: detail.id,
        quantity: detail.pendingQuantity,
        price:
          totalQuantity > 0
            ? (group.price / totalQuantity) * detail.pendingQuantity
            : 0,
        user: store.user?.username,
      } as PurchaseOrderReceiptDetail);
    });
  });

  emits("selected", receptionsRequest);
};
</script>
<style scoped>
.selector-filter {
  display: grid;
  grid-template-columns: 1fr 0.1fr;
}
</style>

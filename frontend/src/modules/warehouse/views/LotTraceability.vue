<template>
  <div class="lot-traceability">
    <Card class="mb-3">
      <template #content>
        <section class="three-columns">
          <DropdownReference
            label="Referència"
            :fullName="true"
            v-model="filter.referenceId"
          />
          <div class="mb-2">
            <label class="block text-900 mb-2">Lot</label>
            <Select
              showClear
              filter
              :filter-fields="['code']"
              :options="lots"
              :loading="lotsLoading"
              :disabled="!filter.referenceId"
              placeholder="Selecciona un lot..."
              optionValue="id"
              optionLabel="code"
              class="w-full"
              v-model="filter.lotId"
            />
          </div>
          <div class="flex align-items-end mb-2">
            <Button
              label="Informe de recall"
              icon="pi pi-exclamation-triangle"
              severity="warn"
              :disabled="!filter.lotId"
              :loading="lotTraceabilityStore.loadingRecall"
              @click="onRecall"
            />
          </div>
        </section>
      </template>
    </Card>

    <Tabs v-model:value="activeTab">
      <TabList>
        <Tab value="backward">Cap enrere (des d'un lot venut)</Tab>
        <Tab value="forward">Cap endavant (des d'un lot de compra)</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="backward">
          <TreeTable
            :value="backwardTreeData"
            :loading="lotTraceabilityStore.loadingBackward"
            scrollable
            tableStyle="min-width: 60rem"
          >
            <Column expander field="lotCode" header="Lot" style="width: 18%" />
            <Column
              field="referenceCode"
              header="Referència"
              style="width: 14%"
            />
            <Column
              field="referenceDescription"
              header="Descripció"
              style="width: 22%"
            />
            <Column field="quantity" header="Quantitat" style="width: 10%" />
            <Column header="Data" style="width: 10%">
              <template #body="slotProps">
                {{ traceabilityRowDate(slotProps.node.data) }}
              </template>
            </Column>
            <Column header="Origen de compra / moviments" style="width: 26%">
              <template #body="slotProps">
                <span v-if="slotProps.node.data.kind === 'purchase'">
                  {{ slotProps.node.data.supplierName }} · Rebut
                  {{ slotProps.node.data.receiptNumber }}
                </span>
                <span
                  v-else-if="slotProps.node.data.kind === 'movement'"
                  class="movement-row"
                >
                  <TagMovementType :movementType="slotProps.node.data.movementType" />
                  <span>
                    {{ slotProps.node.data.locationName }}
                    <template v-if="slotProps.node.data.description">
                      · {{ slotProps.node.data.description }}
                    </template>
                  </span>
                </span>
              </template>
            </Column>
            <template #empty>
              Selecciona un lot venut per veure'n la traçabilitat cap enrere.
            </template>
          </TreeTable>
        </TabPanel>
        <TabPanel value="forward">
          <TreeTable
            :value="forwardTreeData"
            :loading="lotTraceabilityStore.loadingForward"
            scrollable
            tableStyle="min-width: 60rem"
          >
            <Column expander field="lotCode" header="Lot" style="width: 18%" />
            <Column
              field="referenceCode"
              header="Referència"
              style="width: 14%"
            />
            <Column
              field="referenceDescription"
              header="Descripció"
              style="width: 22%"
            />
            <Column field="quantity" header="Quantitat" style="width: 10%" />
            <Column header="Data" style="width: 10%">
              <template #body="slotProps">
                {{ traceabilityRowDate(slotProps.node.data) }}
              </template>
            </Column>
            <Column header="Destí de venda / moviments" style="width: 26%">
              <template #body="slotProps">
                <span v-if="slotProps.node.data.kind === 'sale'">
                  {{ slotProps.node.data.customerName }} · Albarà
                  {{ slotProps.node.data.deliveryNoteNumber }}
                </span>
                <span
                  v-else-if="slotProps.node.data.kind === 'movement'"
                  class="movement-row"
                >
                  <TagMovementType :movementType="slotProps.node.data.movementType" />
                  <span>
                    {{ slotProps.node.data.locationName }}
                    <template v-if="slotProps.node.data.description">
                      · {{ slotProps.node.data.description }}
                    </template>
                  </span>
                </span>
              </template>
            </Column>
            <template #empty>
              Selecciona un lot de compra per veure'n la traçabilitat cap
              endavant.
            </template>
          </TreeTable>
        </TabPanel>
      </TabPanels>
    </Tabs>

    <Panel
      v-if="lotTraceabilityStore.recall"
      header="Informe de recall"
      toggleable
      class="mt-3"
    >
      <div class="mb-3">
        <strong>Lot:</strong> {{ lotTraceabilityStore.recall.lotCode }} —
        {{ lotTraceabilityStore.recall.referenceCode }} -
        {{ lotTraceabilityStore.recall.referenceDescription }}
      </div>
      <div class="flex gap-2 mb-3">
        <Tag
          severity="warn"
          :value="`${lotTraceabilityStore.recall.totalAffectedDeliveryNotes} albarans afectats`"
        />
        <Tag
          severity="danger"
          :value="`${lotTraceabilityStore.recall.totalAffectedQuantity} unitats afectades`"
        />
      </div>
      <p v-if="lotTraceabilityStore.recall.affectedCustomers.length === 0">
        Aquest lot no ha arribat a cap client.
      </p>
      <Panel
        v-for="customer in lotTraceabilityStore.recall.affectedCustomers"
        :key="customer.customerId"
        :header="customer.customerName"
        toggleable
        class="mb-2"
      >
        <DataTable :value="customer.deliveryNotes" size="small">
          <Column field="deliveryNoteNumber" header="Albarà" />
          <Column header="Data">
            <template #body="slotProps">{{
              formatDate(slotProps.data.deliveryDate)
            }}</template>
          </Column>
          <Column field="lotCode" header="Lot" />
          <Column field="referenceCode" header="Referència" />
          <Column field="quantity" header="Quantitat" />
        </DataTable>
      </Panel>
    </Panel>
  </div>
</template>
<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useReferenceStore } from "../../shared/store/reference";
import { useLotTraceabilityStore } from "../store/lotTraceability";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import TagMovementType from "../../../components/TagMovementType.vue";
import Services from "../services";
import { formatDate, formatDateTime } from "../../../utils/functions";
import { Lot, LotTraceabilityNode } from "../types";

interface TraceabilityTreeRowData {
  lotCode: string;
  referenceCode: string;
  referenceDescription: string;
  quantity: number;
  kind: "node" | "purchase" | "sale" | "movement";
  supplierName?: string;
  receiptNumber?: string;
  receiptDate?: any;
  customerName?: string;
  deliveryNoteNumber?: string;
  deliveryDate?: any;
  movementType?: string;
  movementDate?: any;
  locationName?: string;
  description?: string;
}

interface TraceabilityTreeRow {
  key: string;
  data: TraceabilityTreeRowData;
  children?: TraceabilityTreeRow[];
}

const store = useStore();
const toast = useToast();
const route = useRoute();
const referenceStore = useReferenceStore();
const lotTraceabilityStore = useLotTraceabilityStore();

const activeTab = ref("backward");
const filter = ref({
  referenceId: undefined as string | undefined,
  lotId: undefined as string | undefined,
});

const lots = ref<Lot[]>([]);
const lotsLoading = ref(false);
const pendingLotIdFromQuery = ref<string | undefined>(undefined);

const traceabilityRowDate = (data: TraceabilityTreeRowData): string => {
  switch (data.kind) {
    case "purchase":
      return data.receiptDate ? formatDate(data.receiptDate) : "";
    case "sale":
      return data.deliveryDate ? formatDate(data.deliveryDate) : "";
    case "movement":
      return data.movementDate ? formatDateTime(data.movementDate) : "";
    default:
      return "";
  }
};

const buildMovementRows = (
  node: LotTraceabilityNode,
  parentKey: string,
): TraceabilityTreeRow[] =>
  (node.movements ?? []).map((movement, index) => ({
    key: `${parentKey}-movement-${index}-${movement.movementId}`,
    data: {
      lotCode: node.lotCode,
      referenceCode: node.referenceCode,
      referenceDescription: node.referenceDescription,
      quantity: movement.quantity,
      kind: "movement",
      movementType: movement.movementType,
      movementDate: movement.movementDate,
      locationName: movement.locationName,
      description: movement.description,
    },
  }));

const toBackwardTreeNode = (
  node: LotTraceabilityNode,
  parentKey = "root",
): TraceabilityTreeRow => {
  const key = `${parentKey}-${node.lotId}`;
  const children: TraceabilityTreeRow[] = [];

  node.children?.forEach((child) =>
    children.push(toBackwardTreeNode(child, key)),
  );

  buildMovementRows(node, key).forEach((row) => children.push(row));

  node.purchaseOrigins?.forEach((origin, index) => {
    children.push({
      key: `${key}-purchase-${index}-${origin.receiptId}`,
      data: {
        lotCode: origin.lotCode,
        referenceCode: origin.referenceCode,
        referenceDescription: origin.referenceDescription,
        quantity: origin.quantity,
        kind: "purchase",
        supplierName: origin.supplierName,
        receiptNumber: origin.receiptNumber,
        receiptDate: origin.receiptDate,
      },
    });
  });

  return {
    key,
    data: {
      lotCode: node.lotCode,
      referenceCode: node.referenceCode,
      referenceDescription: node.referenceDescription,
      quantity: node.quantity,
      kind: "node",
    },
    children: children.length > 0 ? children : undefined,
  };
};

const toForwardTreeNode = (
  node: LotTraceabilityNode,
  parentKey = "root",
): TraceabilityTreeRow => {
  const key = `${parentKey}-${node.lotId}`;
  const children: TraceabilityTreeRow[] = [];

  node.children?.forEach((child) =>
    children.push(toForwardTreeNode(child, key)),
  );

  buildMovementRows(node, key).forEach((row) => children.push(row));

  node.salesDestinations?.forEach((destination, index) => {
    children.push({
      key: `${key}-sale-${index}-${destination.deliveryNoteId}`,
      data: {
        lotCode: destination.lotCode,
        referenceCode: destination.referenceCode,
        referenceDescription: destination.referenceDescription,
        quantity: destination.quantity,
        kind: "sale",
        customerName: destination.customerName,
        deliveryNoteNumber: destination.deliveryNoteNumber,
        deliveryDate: destination.deliveryDate,
      },
    });
  });

  return {
    key,
    data: {
      lotCode: node.lotCode,
      referenceCode: node.referenceCode,
      referenceDescription: node.referenceDescription,
      quantity: node.quantity,
      kind: "node",
    },
    children: children.length > 0 ? children : undefined,
  };
};

const backwardTreeData = computed(() =>
  lotTraceabilityStore.backward
    ? [toBackwardTreeNode(lotTraceabilityStore.backward.root)]
    : [],
);

const forwardTreeData = computed(() =>
  lotTraceabilityStore.forward
    ? [toForwardTreeNode(lotTraceabilityStore.forward.root)]
    : [],
);

const notifyLotNotFound = () => {
  toast.add({
    severity: "warn",
    summary: "No s'ha trobat el lot",
    life: 5000,
  });
};

const loadTraceability = async (lotId: string) => {
  const result =
    activeTab.value === "backward"
      ? await lotTraceabilityStore.fetchBackward(lotId)
      : await lotTraceabilityStore.fetchForward(lotId);

  if (!result) notifyLotNotFound();
};

const loadLots = async (referenceId: string) => {
  lotsLoading.value = true;
  lots.value = await Services.Lot.getOpenByReference(referenceId);
  lotsLoading.value = false;
};

watch(
  () => filter.value.referenceId,
  async (referenceId) => {
    filter.value.lotId = undefined;
    lots.value = [];
    lotTraceabilityStore.reset();
    if (referenceId) await loadLots(referenceId);
  },
);

watch(lotsLoading, (loading) => {
  if (loading || !pendingLotIdFromQuery.value) return;

  const lotId = pendingLotIdFromQuery.value;
  pendingLotIdFromQuery.value = undefined;

  if (lots.value.some((lot) => lot.id === lotId)) {
    filter.value.lotId = lotId;
  }
});

watch(
  () => filter.value.lotId,
  async (lotId) => {
    lotTraceabilityStore.reset();
    if (lotId) await loadTraceability(lotId);
  },
);

watch(activeTab, async (tab) => {
  if (!filter.value.lotId) return;
  if (tab === "backward" && !lotTraceabilityStore.backward) {
    await loadTraceability(filter.value.lotId);
  } else if (tab === "forward" && !lotTraceabilityStore.forward) {
    await loadTraceability(filter.value.lotId);
  }
});

const onRecall = async () => {
  if (!filter.value.lotId) return;
  const result = await lotTraceabilityStore.fetchRecall(filter.value.lotId);
  if (!result) notifyLotNotFound();
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.SITEMAP,
    title: "Traçabilitat de lots",
  });

  await referenceStore.fetchReferences();

  const queryReferenceId = route.query.referenceId;
  const queryLotId = route.query.lotId;

  const referenceId =
    typeof queryReferenceId === "string" ? queryReferenceId : undefined;
  const lotId = typeof queryLotId === "string" ? queryLotId : undefined;

  if (referenceId) {
    activeTab.value = "backward";
    if (lotId) pendingLotIdFromQuery.value = lotId;
    filter.value.referenceId = referenceId;
  }
});
</script>

<style scoped>
.movement-row {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}
</style>

<template>
  <div class="lot-traceability">
    <Card class="mb-3">
      <template #content>
        <section class="three-columns">
          <DropdownReference
            :label="t('warehouse.lotTraceability.fields.reference')"
            :fullName="true"
            v-model="filter.referenceId"
          />
          <div class="mb-2">
            <label class="block text-900 mb-2">{{
              t("warehouse.lotTraceability.fields.lot")
            }}</label>
            <Select
              showClear
              filter
              :filter-fields="['code']"
              :options="lots"
              :loading="lotsLoading"
              :disabled="!filter.referenceId"
              :placeholder="
                t('warehouse.lotTraceability.placeholders.selectLot')
              "
              optionValue="id"
              optionLabel="code"
              class="w-full"
              v-model="filter.lotId"
            />
          </div>
          <div class="flex align-items-end mb-2">
            <Button
              :label="t('warehouse.lotTraceability.recall.title')"
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
        <Tab value="backward">{{
          t("warehouse.lotTraceability.tabs.backward")
        }}</Tab>
        <Tab value="forward">{{
          t("warehouse.lotTraceability.tabs.forward")
        }}</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="backward">
          <TreeTable
            :value="backwardTreeData"
            :loading="lotTraceabilityStore.loadingBackward"
            scrollable
            tableStyle="min-width: 60rem"
          >
            <Column
              expander
              field="lotCode"
              :header="t('warehouse.lotTraceability.fields.lot')"
              style="width: 18%"
            />
            <Column
              field="referenceCode"
              :header="t('warehouse.lotTraceability.fields.reference')"
              style="width: 14%"
            />
            <Column
              field="referenceDescription"
              :header="t('warehouse.lotTraceability.fields.description')"
              style="width: 22%"
            />
            <Column
              field="quantity"
              :header="t('warehouse.lotTraceability.fields.quantity')"
              style="width: 10%"
            />
            <Column
              :header="t('warehouse.lotTraceability.fields.date')"
              style="width: 10%"
            >
              <template #body="slotProps">
                {{ traceabilityRowDate(slotProps.node.data) }}
              </template>
            </Column>
            <Column
              :header="
                t('warehouse.lotTraceability.fields.purchaseOriginMovements')
              "
              style="width: 26%"
            >
              <template #body="slotProps">
                <span
                  v-if="slotProps.node.data.kind === 'movement'"
                  class="movement-row"
                >
                  <TagMovementType
                    :movementType="slotProps.node.data.movementType"
                  />
                  <span>
                    {{ slotProps.node.data.locationName }}
                    <template v-if="slotProps.node.data.partnerName">
                      · {{ slotProps.node.data.partnerName }}
                    </template>
                    <template v-if="slotProps.node.data.description">
                      · {{ slotProps.node.data.description }}
                    </template>
                  </span>
                </span>
              </template>
            </Column>
            <template #empty>
              {{ t("warehouse.lotTraceability.empty.backward") }}
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
            <Column
              expander
              field="lotCode"
              :header="t('warehouse.lotTraceability.fields.lot')"
              style="width: 18%"
            />
            <Column
              field="referenceCode"
              :header="t('warehouse.lotTraceability.fields.reference')"
              style="width: 14%"
            />
            <Column
              field="referenceDescription"
              :header="t('warehouse.lotTraceability.fields.description')"
              style="width: 22%"
            />
            <Column
              field="quantity"
              :header="t('warehouse.lotTraceability.fields.quantity')"
              style="width: 10%"
            />
            <Column
              :header="t('warehouse.lotTraceability.fields.date')"
              style="width: 10%"
            >
              <template #body="slotProps">
                {{ traceabilityRowDate(slotProps.node.data) }}
              </template>
            </Column>
            <Column
              :header="
                t('warehouse.lotTraceability.fields.salesDestinationMovements')
              "
              style="width: 26%"
            >
              <template #body="slotProps">
                <span
                  v-if="slotProps.node.data.kind === 'movement'"
                  class="movement-row"
                >
                  <TagMovementType
                    :movementType="slotProps.node.data.movementType"
                  />
                  <span>
                    {{ slotProps.node.data.locationName }}
                    <template v-if="slotProps.node.data.partnerName">
                      · {{ slotProps.node.data.partnerName }}
                    </template>
                    <template v-if="slotProps.node.data.description">
                      · {{ slotProps.node.data.description }}
                    </template>
                  </span>
                </span>
              </template>
            </Column>
            <template #empty>
              {{ t("warehouse.lotTraceability.empty.forward") }}
            </template>
          </TreeTable>
        </TabPanel>
      </TabPanels>
    </Tabs>

    <Panel
      v-if="lotTraceabilityStore.recall"
      :header="t('warehouse.lotTraceability.recall.title')"
      toggleable
      class="mt-3"
    >
      <div class="mb-3">
        <strong>{{ t("warehouse.lotTraceability.fields.lot") }}:</strong>
        {{ lotTraceabilityStore.recall.lotCode }} —
        {{ lotTraceabilityStore.recall.referenceCode }} -
        {{ lotTraceabilityStore.recall.referenceDescription }}
      </div>
      <div class="flex gap-2 mb-3">
        <Tag
          severity="warn"
          :value="
            t('warehouse.lotTraceability.recall.affectedDeliveryNotes', {
              count: lotTraceabilityStore.recall.totalAffectedDeliveryNotes,
            })
          "
        />
        <Tag
          severity="danger"
          :value="
            t('warehouse.lotTraceability.recall.affectedUnits', {
              count: lotTraceabilityStore.recall.totalAffectedQuantity,
            })
          "
        />
      </div>
      <p v-if="lotTraceabilityStore.recall.affectedCustomers.length === 0">
        {{ t("warehouse.lotTraceability.recall.noAffectedCustomers") }}
      </p>
      <Panel
        v-for="customer in lotTraceabilityStore.recall.affectedCustomers"
        :key="customer.customerId"
        :header="customer.customerName"
        toggleable
        class="mb-2"
      >
        <DataTable :value="customer.deliveryNotes" size="small">
          <Column
            field="deliveryNoteNumber"
            :header="t('warehouse.lotTraceability.fields.deliveryNote')"
          />
          <Column :header="t('warehouse.lotTraceability.fields.date')">
            <template #body="slotProps">{{
              formatDate(slotProps.data.deliveryDate)
            }}</template>
          </Column>
          <Column
            field="lotCode"
            :header="t('warehouse.lotTraceability.fields.lot')"
          />
          <Column
            field="referenceCode"
            :header="t('warehouse.lotTraceability.fields.reference')"
          />
          <Column
            field="quantity"
            :header="t('warehouse.lotTraceability.fields.quantity')"
          />
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
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { useReferenceStore } from "../../shared/store/reference";
import { useLotTraceabilityStore } from "../store/lotTraceability";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import TagMovementType from "../../../components/TagMovementType.vue";
import Services from "../services";
import { formatDate, formatDateTime } from "@/utils/functions";
import { Lot, LotTraceabilityNode } from "../types";

interface TraceabilityTreeRowData {
  lotCode: string;
  referenceCode: string;
  referenceDescription: string;
  quantity: number;
  kind: "node" | "movement";
  movementType?: string;
  movementDate?: any;
  locationName?: string;
  description?: string;
  partnerName?: string | null;
  documentNumber?: string | null;
}

interface TraceabilityTreeRow {
  key: string;
  data: TraceabilityTreeRowData;
  children?: TraceabilityTreeRow[];
}

const store = useStore();
const toast = useToast();
const { t, locale } = useI18n();
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

const traceabilityRowDate = (data: TraceabilityTreeRowData): string =>
  data.kind === "movement" && data.movementDate
    ? formatDateTime(data.movementDate)
    : "";

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
      partnerName: movement.partnerName,
      documentNumber: movement.documentNumber,
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
    summary: t("warehouse.lotTraceability.messages.lotNotFound"),
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

const setMenuItem = () => {
  store.setMenuItem({
    icon: PrimeIcons.SITEMAP,
    title: t("warehouse.lotTraceability.title"),
    backButtonVisible: true,
  });
};

watch(locale, setMenuItem);

onMounted(async () => {
  setMenuItem();

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

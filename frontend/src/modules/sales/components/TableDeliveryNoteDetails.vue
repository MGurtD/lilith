<template>
  <div ref="tableRef" class="delivery-note-table-container" :style="{ height: scrollHeight }">
    <DataTable
      :value="treeTableData"
      tableStyle="min-width: 100%"
      class="p-datatable-sm delivery-note-data-table"
      rowGroupMode="subheader"
      groupRowsBy="salesOrderNumber"
      sortMode="multiple"
      :sortOrder="1"
      scrollable
      scrollHeight="flex"
    >
      <template #header>
        <slot name="header"></slot>
      </template>
      <template #groupheader="slotProps">
        <i
          v-if="canDelete"
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
        <span class="vertical-align-middle ml-2 font-bold line-height-3"
          >Comanda {{ slotProps.data.salesOrderNumber }} (Núm. Client:
          {{ slotProps.data.customerNumber }})</span
        >
      </template>
      <Column field="salesOrderNumber" :header="t('sales.components.comanda')" style="width: 5%" />
      <Column field="" header="" style="width: 2%" />
      <Column field="quantity" :header="t('sales.components.quantitat')" style="width: 5%" />
      <Column
        :header="t('sales.components.referencia')"
        field="reference.code"
        sortable
        style="width: 15%"
      >
        <template #body="slotProps">
          <LinkReference :id="slotProps.data.referenceId" />
        </template>
      </Column>
      <Column field="description" :header="t('sales.components.descripcio')" style="width: 30%" />
      <Column field="unitPrice" :header="t('sales.components.preuUn')" style="width: 10%">
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.unitPrice) }}
        </template>
      </Column>
      <Column field="amount" :header="t('sales.components.total')" style="width: 10%">
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.amount) }}
        </template>
      </Column>
      <template #footer>
        <div class="total-footer">
          <span class="total-label">{{ t('sales.components.total') }}</span>
          <span class="total-value">{{ formatCurrency(deliveryNoteAmount) }}</span>
        </div>
      </template>
    </DataTable>
  </div>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { useScrollHeight } from "@/composables/useScrollHeight";
import LinkReference from "../../shared/components/LinkReference.vue";
import { PrimeIcons } from "@primevue/core/api";
import { SalesOrderHeader } from "../types";
import { computed } from "vue";
import { formatCurrency } from "../../../utils/functions";
import { useConfirm } from "primevue/useconfirm";

const { t } = useI18n();
const props = defineProps<{
  orders: Array<SalesOrderHeader> | undefined;
  canDelete: boolean;
}>();

const treeTableData = computed(() => {
  if (!props.orders) return [];

  let salesOrderDetails = [] as any;
  for (let index = 0; index < props.orders.length; index++) {
    if (!props.orders[index].salesOrderDetails) continue;

    const order = props.orders[index];
    order.salesOrderDetails!.forEach((detail) => {
      salesOrderDetails.push({
        salesOrderId: order.id,
        salesOrderNumber: order.number,
        salesOrderDate: order.date,
        customerNumber: order.customerNumber,
        ...detail,
      });
    });
  }
  return salesOrderDetails;
});

const emit = defineEmits<{
  (e: "delete", order: SalesOrderHeader): void;
}>();

const { tableRef, scrollHeight } = useScrollHeight(24);

const confirm = useConfirm();

const onDeleteRow = (event: any, clickedOrder: any) => {
  if (!props.orders) return;
  const order = props.orders?.find((o) => o.id === clickedOrder.salesOrderId);
  if (!order) return;

  confirm.require({
    target: event.currentTarget,
    message: t("sales.componentMessages.unassignOrder", { number: order.number }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", order);
    },
  });
};

const deliveryNoteAmount = computed(() => {
  if (!props.orders) return 0;
  return props.orders.reduce((acc, order) => {
    if (!order.salesOrderDetails) return acc;
    return (
      acc +
      order.salesOrderDetails.reduce((acc, detail) => acc + detail.amount, 0)
    );
  }, 0);
});
</script>
<style scoped>
.delivery-note-table-container {
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.delivery-note-data-table {
  flex: 1;
  min-height: 0;
}

.total-footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 0.5rem;
}

.total-label {
  font-weight: 600;
  color: var(--p-text-muted-color);
  font-size: 0.85rem;
}

.total-value {
  font-weight: 700;
}
</style>

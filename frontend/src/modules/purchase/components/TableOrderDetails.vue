<template>
  <DataTable
    @row-click="onEditRow"
    :value="detailsWithReceptions"
    tableStyle="min-width: 100%"
    class="p-datatable-sm"
    sort-mode="single"
    sort-field="reference.code"
    :sort-order="1"
    dataKey="id"
    v-model:expandedRows="expandedRows"
  >
    <template #header>
      <slot name="header"></slot>
    </template>
    <Column expander style="width: 2%" />
    <Column field="quantity" header="Quantitat" style="width: 5%" />
    <Column field="receivedQuantity" header="Q. Rebuda" style="width: 6%" />
    <Column
      field="reference.code"
      sortable
      header="Referència"
      style="width: 20%"
    >
      <template #body="{ data }">
        <LinkReference
          :id="data.referenceId"
          :category="data.reference.categoryName"
          :fullName="true"
        />
      </template>
    </Column>
    <Column field="description" header="Descripció" style="width: 25%">
    </Column>
    <Column field="statusId" header="Estat" style="width: 7.5%">
      <template #body="{ data }">
        {{ getStatusName(data.statusId) }}
      </template>
    </Column>
    <Column
      field="expectedReceiptDate"
      header="Data Recepció"
      style="width: 7.5%"
    >
      <template #body="{ data }">
        {{ formatDate(data.expectedReceiptDate) }}
      </template>
    </Column>
    <Column field="unitPrice" header="Preu Un." style="width: 5%">
      <template #body="{ data }">
        {{ formatCurrency(data.unitPrice) }}
      </template>
    </Column>
    <Column field="amount" header="Preu" style="width: 5%">
      <template #body="{ data }">
        {{ formatCurrency(data.amount) }}
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="{ data }">
        <i
          v-if="data.receivedQuantity === 0"
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, data)"
        />
      </template>
    </Column>
    <template #expansion="{ data }">
      <div class="expanded-details">
        <DataTable :value="data.receptions">
          <Column field="receiptDetail.receipt.number" header="Albarà">
            <template #body="{ data }">
              <span
                class="link"
                @click="navegateToReference(data.receiptDetail.receipt.id)"
              >
                {{ data.receiptDetail.receipt.number }}
              </span>
            </template>
          </Column>
          <Column field="quantity" header="Quantitat" />
          <Column field="createdOn" header="Data">
            <template #body="{ data }">
              {{ formatDate(data.createdOn) }}
            </template>
          </Column>
          <Column field="user" header="Usuari" />
        </DataTable>
      </div>
    </template>
  </DataTable>
</template>
<script setup lang="ts">
import LinkReference from "../../shared/components/LinkReference.vue";
import { onMounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { PurchaseOrderDetail } from "../types";
import { formatCurrency, formatDate } from "../../../utils/functions";
import { Lifecycle } from "../../shared/types";
import { useOrderStore } from "../store/order";
import sharedServices from "../../shared/services";
import { useReferenceStore } from "../../shared/store/reference";

const props = defineProps<{
  details: Array<PurchaseOrderDetail> | undefined;
}>();
const emit = defineEmits<{
  (e: "edit", detail: PurchaseOrderDetail): void;
  (e: "delete", detail: PurchaseOrderDetail): void;
}>();

const router = useRouter();
const route = useRoute();
const orderStore = useOrderStore();
const lifecycle = ref(undefined as undefined | Lifecycle);
const expandedRows = ref({});
const detailsWithReceptions = ref([] as Array<any>);
const referenceStore = useReferenceStore();

watch(
  () => props.details?.length,
  async (newValue) => {
    if (newValue) {
      await mergeDetailsWithReceptions();
    }
  },
);

onMounted(async () => {
  await orderStore.getReceptions(route.params.id as string);
  if (!orderStore.receptions) {
    return;
  }

  await mergeDetailsWithReceptions();

  lifecycle.value = await sharedServices.Lifecycle.getByName(
    "PurchaseOrderDetail",
  );
});

const mergeDetailsWithReceptions = async () => {
  detailsWithReceptions.value =
    props.details?.map((d) => {
      return {
        ...d,
        receptions: orderStore.receptions?.filter(
          (r) => r.purchaseOrderDetailId === d.id,
        ),
      };
    }) || [];
};

const getStatusName = (statusId: string) => {
  if (!lifecycle.value) return "";
  const status = lifecycle.value.statuses.find((s) => s.id === statusId);
  return status ? status.name : "";
};

const onEditRow = (row: DataTableRowClickEvent) => {
  try {
    if (
      !(row.originalEvent.target as any).className.includes(
        "grid_delete_column_button",
      )
    ) {
      emit("edit", row.data);
    }
  } catch (e) {}
};

const onDeleteRow = (event: any, detail: PurchaseOrderDetail) => {
  emit("delete", detail);
};

const navegateToReference = (id: string) => {
  const path = `/receipts/${id}`;
  router.push(path);
};
</script>
<style scoped>
.expanded-details {
  width: 90%;
  margin: 0 auto;
}
.link {
  text-decoration: none; /* Elimina la subrayado predeterminado */
  color: var(--p-blue-800); /* Color del texto */
  font-weight: bold; /* Fuente en negrita */
  font-size: 0.9rem;
  transition: color 0.3s; /* Agrega una transición suave al color del texto */
}

.link:hover {
  color: var(
    --p-blue-600
  ); /* Cambia el color del texto al pasar el mouse sobre el enlace */
  cursor: pointer;
}
</style>

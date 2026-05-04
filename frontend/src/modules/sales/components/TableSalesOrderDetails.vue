<template>
  <DataTable
    @row-click="onEditRow"
    :value="salesOrderDetails"
    tableStyle="min-width: 100%"
    class="p-datatable-sm"
    selectionMode="single"
    dataKey="id"
    sortMode="single"
    sortField="reference.code"
    :sortOrder="1"
  >
    <template #header>
      <slot name="header"></slot>
    </template>
    <Column field="quantity" header="Un." style="width: 3%" />
    <Column
      header="Referencia"
      field="reference.code"
      sortable
      style="width: 15%"
    >
      <template #body="slotProps">
        <LinkReference :id="slotProps.data.referenceId" />
      </template>
    </Column>
    <Column field="description" header="Descripció" style="width: 25%" />
    <Column field="workOrderId" header="Ordre fabr." style="width: 8%">
      <template #body="slotProps">
        <div
          v-if="workOrderStore.getWorkOrderCodeById(slotProps.data.workOrderId)"
        >
          <Button
            size="small"
            :severity="getStatusColorByWOId(slotProps.data.workOrderId)"
            :label="
              workOrderStore.getWorkOrderCodeById(slotProps.data.workOrderId)
            "
            @click="openWorkOrder(slotProps.data.workOrderId)"
          >
          </Button>
        </div>
        <div v-else>
          <Button
            :disabled="salesOrder.deliveryNoteId !== null"
            size="small"
            :icon="PrimeIcons.PLUS_CIRCLE"
            value="Generar OF"
            @click="onWorkOrderCreateClick(slotProps.data)"
          />
        </div>
      </template>
    </Column>
    <Column field="unitCost" header="Cost un. teo." style="width: 8%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.unitCost) }}
      </template>
    </Column>
    <Column field="totalCost" header="Cost teòric" style="width: 8%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.totalCost) }}
      </template>
    </Column>
    <Column header="Cost un. r." style="width: 8%">
      <template #body="slotProps">
        {{ formatCurrency(getUnitRealCost(slotProps.data)) }}
      </template>
    </Column>
    <Column header="Cost real" style="width: 6%">
      <template #body="slotProps">
        {{
          formatCurrency(
            workOrderStore.getWorkOrderCost(slotProps.data.workOrderId)
          )
        }}
      </template>
    </Column>
    <Column field="discount" header="% Bene." style="width: 6%">
      <template #body="slotProps"> {{ slotProps.data.profit }} % </template>
    </Column>
    <Column field="profit" header="% Desc." style="width: 6%">
      <template #body="slotProps"> {{ slotProps.data.discount }} % </template>
    </Column>
    <Column header="Preu un." style="width: 6%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.unitPrice) }}
      </template>
    </Column>
    <Column field="amount" header="Total" style="width: 6%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.amount) }}
      </template>
    </Column>
    <Column style="width: 5%" v-if="!salesOrder.deliveryNoteId">
      <template #body="slotProps">
        <i
          v-if="!slotProps.data.isDelivered && !slotProps.data.workOrderId"
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
      </template>
    </Column>
    <template #footer>
      <div class="total">
        <span class="total-text">Total {{ formatCurrency(totalAmount) }} </span>
      </div>
    </template>
  </DataTable>

  <Dialog
    :closable="dialogOptions.closable"
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :modal="dialogOptions.modal"
    :style="{ width: '50vw' }"
  >
    <FormCreateWorkorder
      :createWorkOrderDto="createWorkOrderDto"
      :filtered-work-masters="referenceActiveWorkMasters"
      @submit="onWorkOrderCreateSubmit"
    ></FormCreateWorkorder>
  </Dialog>
</template>
<script setup lang="ts">
import LinkReference from "../../shared/components/LinkReference.vue";
import { computed, onMounted, ref } from "vue";
import FormCreateWorkorder from "../../production/components/FormCreateWorkorder.vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  SalesOrderHeader,
  SalesOrderDetail,
  CreateWorkOrderFromSalesOrderDto,
} from "../types";
import {
  CreateWorkOrderDto,
  WorkMaster,
  WorkOrder,
} from "../../production/types";
import { DialogOptions } from "../../../types/component";
import { useConfirm } from "primevue/useconfirm";
import { useWorkMasterStore } from "../../production/store/workmaster";
import { useWorkOrderStore } from "../../production/store/workorder";
import {
  convertDDMMYYYYToDate,
  formatCurrency,
} from "../../../utils/functions";
import { Lifecycle } from "../../shared/types";

const props = defineProps<{
  salesOrder: SalesOrderHeader;
  salesOrderDetails: Array<SalesOrderDetail> | undefined;
  secondaryLifecycle: Lifecycle | undefined;
  workorders: Array<WorkOrder> | undefined;
}>();

const emit = defineEmits<{
  (e: "edit", salesOrderDetail: SalesOrderDetail): void;
  (e: "delete", salesOrderDetail: SalesOrderDetail): void;
  (e: "createWorkOrder", dto: CreateWorkOrderFromSalesOrderDto): void;
  (e: "openWorkOrder", workorderid: string): void;
}>();

const confirm = useConfirm();
const workMasterStore = useWorkMasterStore();
const workOrderStore = useWorkOrderStore();

const totalAmount = computed(() => {
  if (props.salesOrderDetails) {
    return props.salesOrderDetails.reduce(
      (acc, detail) => acc + detail.amount,
      0
    );
  }
  return 0;
});

onMounted(async () => {
  workOrderStore.fetchBySalesOrder(props.salesOrder.id);
});

var selectedDetail = undefined as SalesOrderDetail | undefined;
const dialogOptions = ref({
  closable: true,
  modal: true,
  title: "Generar ordre de fabricació",
  visible: false,
} as DialogOptions);
const referenceActiveWorkMasters = ref([] as Array<WorkMaster>);
const createWorkOrderDto = ref({
  workMasterId: "",
  plannedDate: "",
  plannedQuantity: 0,
  comment: "",
} as CreateWorkOrderDto);

const onEditRow = (row: DataTableRowClickEvent) => {
  if (props.salesOrder.deliveryNoteId) return;

  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, salesOrderDetail: SalesOrderDetail) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar la referència?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", salesOrderDetail);
    },
  });
};

const getUnitRealCost = (detail: SalesOrderDetail) => {
  if (!detail.workOrderId) return 0;
  const cost = workOrderStore.getWorkOrderCost(detail.workOrderId);
  if (cost === 0) return 0;
  return cost / detail.quantity;
};

const onWorkOrderCreateClick = (salesOrderDetail: SalesOrderDetail) => {
  // Get workmasters of the selected reference
  referenceActiveWorkMasters.value = workMasterStore.getByReferenceId(
    salesOrderDetail.referenceId
  );
  // Clear form data
  createWorkOrderDto.value.workMasterId = salesOrderDetail.workMasterId || "";
  if (createWorkOrderDto.value.workMasterId === "") {
    createWorkOrderDto.value.workMasterId =
      referenceActiveWorkMasters.value[0].id;
  }
  createWorkOrderDto.value.plannedQuantity = salesOrderDetail.quantity;
  createWorkOrderDto.value.plannedDate = props.salesOrder.expectedDate
    ? convertDDMMYYYYToDate(props.salesOrder.expectedDate)
    : "";
  createWorkOrderDto.value.comment = "";
  selectedDetail = salesOrderDetail;

  dialogOptions.value.visible = true;
};

type StatusColor =
  | "secondary"
  | "success"
  | "info"
  | "warning"
  | "help"
  | "danger"
  | "contrast";

const getStatusColorByWOId = (workorderId: string): StatusColor | undefined => {
  if (!workorderId) {
    return "contrast";
  }
  const statusId = workOrderStore.getWorkOrderStatusById(workorderId);

  if (statusId) {
    const status = props.secondaryLifecycle?.statuses.find(
      (s) => s.id === statusId
    );
    if (status) {
      return status.color as StatusColor;
    }
  }
  return "contrast";
};

const openWorkOrder = (workorderId: string) => {
  emit("openWorkOrder", workorderId);
};

const onWorkOrderCreateSubmit = () => {
  const workOrder = {
    workOrderDto: createWorkOrderDto.value,
    orderDetail: selectedDetail!,
  } as CreateWorkOrderFromSalesOrderDto;

  dialogOptions.value.visible = false;
  emit("createWorkOrder", workOrder);
};
</script>
<style scoped>
.total {
  display: flex;
  justify-content: flex-end;
}
</style>

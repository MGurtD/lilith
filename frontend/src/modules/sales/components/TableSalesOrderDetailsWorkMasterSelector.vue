<template>
  <DataTable
    @row-click="onEditRow"
    :value="props.salesOrderDetails"
    tableStyle="min-width: 100%"
    class="p-datatable-sm"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-l text-900 font-bold"></span>
        <section>
          <Button :size="'small'" :label="t('sales.components.generar')" class="mr-2" />
        </section>
      </div>
    </template>
    <Column field="quantity" :header="t('sales.components.quantitat')" style="width: 10%" />
    <Column :header="t('sales.components.referencia')" style="width: 15%">
      <template #body="slotProps">
        {{ referenceStore.getShortNameById(slotProps.data.referenceId) }}
      </template>
    </Column>
    <Column field="description" :header="t('sales.components.descripcio')" style="width: 40%" />
    <Column field="unitPrice" :header="t('sales.components.preuUn')" style="width: 10%">
      <template #body="slotProps"> {{ slotProps.data.unitPrice }} € </template>
    </Column>
    <Column :header="t('sales.components.total')" style="width: 10%">
      <template #body="slotProps"> {{ slotProps.data.amount }} € </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { DataTableRowClickEvent } from "primevue/datatable";
import { SalesOrderDetail } from "../types";
import { useReferenceStore } from "../../shared/store/reference";
import { useConfirm } from "primevue/useconfirm";
import { computed } from "vue";
import { CreateWorkOrderDto } from "../../production/types";
import { convertDateTimeToJSON } from "../../../utils/functions";

const { t } = useI18n();
const props = defineProps<{
  salesOrderDetails: Array<SalesOrderDetail> | undefined;
}>();

const emit = defineEmits<{
  (e: "edit", salesOrderDetail: SalesOrderDetail): void;
  (e: "delete", salesOrderDetail: SalesOrderDetail): void;
}>();

const confirm = useConfirm();
const referenceStore = useReferenceStore();

const createWorkOrderDtos = computed(() => {
  props.salesOrderDetails?.filter(d => d.workOrderId === null).map(d => {

    return {
      plannedDate: convertDateTimeToJSON(new Date()),
      plannedQuantity: d.quantity,
      workMasterId: "",
      comment: ""
    } as CreateWorkOrderDto

  })
})

const onEditRow = (row: DataTableRowClickEvent) => {
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
    message: t("sales.componentMessages.deleteReference"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", salesOrderDetail);
    },
  });
};
</script>

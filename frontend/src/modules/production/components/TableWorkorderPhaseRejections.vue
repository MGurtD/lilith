<template>
  <Table
    :items="rejections"
    :columns="columns"
    :filter-config="[]"
    :show-filters="false"
    :show-create="false"
    preset="read-only"
    tableStyle="min-width: 100%"
  >
    <template #prepend>
      <span class="text-900 font-bold">
        {{
          t("production.rejections.total", { quantity: totalRejectedQuantity })
        }}
      </span>
    </template>
  </Table>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";

import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { WorkOrderPhaseRejection } from "../types";

const props = defineProps<{
  rejections: Array<WorkOrderPhaseRejection>;
}>();

const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "createdOn",
    header: t("common.date"),
    columnType: ColumnType.DateTime,
    style: "width: 25%",
  },
  {
    field: "rejectionReasonCode",
    header: t("production.fields.code"),
    style: "width: 15%",
  },
  {
    field: "rejectionReasonName",
    header: t("production.fields.name"),
    style: "width: 40%",
  },
  {
    field: "quantity",
    header: t("production.rejections.quantity"),
    columnType: ColumnType.Number,
    style: "width: 20%",
  },
]);

const totalRejectedQuantity = computed(() =>
  props.rejections.reduce((total, r) => total + r.quantity, 0),
);
</script>

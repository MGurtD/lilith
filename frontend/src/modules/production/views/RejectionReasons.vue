<template>
  <Table
    :items="rejectionReasonStore.rejectionReasons ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    tableStyle="min-width: 100%"
    show-delete-column
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editRow"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{
        t("production.rejectionReasons.title")
      }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useRejectionReasonStore } from "../store/rejectionreason";
import { computed, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { RejectionReason } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const rejectionReasonStore = useRejectionReasonStore();
const confirm = useConfirm();
const toast = useToast();
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "code",
    header: t("production.fields.code"),
    style: "width: 15%",
  },
  {
    field: "name",
    header: t("production.fields.name"),
    style: "width: 25%",
  },
  {
    field: "description",
    header: t("common.description"),
    style: "width: 45%",
  },
  {
    field: "disabled",
    header: t("production.fields.disabled"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

onMounted(async () => {
  await rejectionReasonStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.EXCLAMATION_TRIANGLE,
    title: t("production.rejectionReasons.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/rejectionreason/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/rejectionreason/${row.data.id}` });
};

const deleteButton = (rejectionReason: RejectionReason) => {
  confirm.require({
    message: t("production.messages.confirmDeleteRejectionReason", {
      name: rejectionReason.name,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await rejectionReasonStore.delete(rejectionReason.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("production.messages.deleted"),
          life: 3000,
        });
      }
    },
  });
};
</script>

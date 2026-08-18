<template>
  <Table
    :items="referenceTypeStore.referenceTypes ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    sort-field="name"
    :sort-order="1"
    show-delete-column
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editRow"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{
        t("shared.referenceTypes.title")
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
import { computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { DataTableRowClickEvent } from "primevue/datatable";
import { ReferenceType } from "../types";
import { useReferenceTypeStore } from "../store/referenceType";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const referenceTypeStore = useReferenceTypeStore();
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("shared.referenceTypes.columns.name"),
    sortable: true,
    style: "width: 25%",
  },
  {
    field: "description",
    header: t("shared.referenceTypes.columns.description"),
    sortable: true,
    style: "width: 40%",
  },
  {
    field: "density",
    header: t("shared.referenceTypes.columns.density"),
    columnType: ColumnType.Number,
    style: "width: 15%",
  },
  {
    field: "disabled",
    header: t("shared.referenceTypes.columns.disabled"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

onMounted(async () => {
  await referenceTypeStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.BOX,
    title: t("shared.referenceTypes.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/referencetype/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/referencetype/${row.data.id}` });
};

const deleteButton = (rawmaterialtype: ReferenceType) => {
  confirm.require({
    message: t("shared.referenceTypes.messages.confirmDelete", {
      name: rawmaterialtype.name,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await referenceTypeStore.deleteReferenceType(
        rawmaterialtype.id,
      );

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("shared.referenceTypes.messages.deleted"),
          life: 3000,
        });
        await referenceTypeStore.fetchAll();
      }
    },
  });
};
</script>

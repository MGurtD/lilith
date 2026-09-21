<template>
  <Table
    :items="plantmodelStore.enterprises ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    tableStyle="min-width: 100%"
    show-delete-column
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editEnterprise"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{
        t("production.enterprises.title")
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
import { usePlantModelStore } from "../store/plantmodel";
import { computed, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { Enterprise } from "../types";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const plantmodelStore = usePlantModelStore();
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("production.fields.name"),
    style: "width: 25%",
  },
  {
    field: "description",
    header: t("common.description"),
    style: "width: 50%",
  },
  {
    field: "defaultSiteId",
    header: t("production.enterprises.defaultSite"),
    columnType: ColumnType.Lookup,
    resolver: (value) =>
      typeof value === "string" ? plantmodelStore.getSiteNameById(value) : "",
    style: "width: 15%",
  },
  {
    field: "disabled",
    header: t("production.fields.disabled"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

onMounted(async () => {
  await plantmodelStore.fetchEnterprises();
  if (!plantmodelStore.sites) await plantmodelStore.fetchSites();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: t("production.enterprises.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/enterprise/${getNewUuid()}` });
};

const editEnterprise = (row: DataTableRowClickEvent) => {
  router.push({ path: `/enterprise/${row.data.id}` });
};
const deleteButton = (entity: Enterprise) => {
  confirm.require({
    message: t("production.messages.confirmDeleteEnterprise", {
      name: entity.name,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteEnterprise(entity.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("production.messages.deleted"),
          life: 3000,
        });
        await plantmodelStore.fetchEnterprises();
      }
    },
  });
};
</script>

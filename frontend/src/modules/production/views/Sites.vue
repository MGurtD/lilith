<template>
  <Table
    :items="plantmodelStore.sites ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    tableStyle="min-width: 100%"
    show-delete-column
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editSite"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{ t("production.sites.title") }}</span>
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
import { Site } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
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
    style: "width: 20%",
  },
  {
    field: "description",
    header: t("common.description"),
    style: "width: 30%",
  },
  {
    field: "city",
    header: t("production.sites.city"),
    style: "width: 20%",
  },
  {
    field: "address",
    header: t("production.sites.address"),
    style: "width: 30%",
  },
  {
    field: "disabled",
    header: t("production.fields.disabled"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

onMounted(async () => {
  await plantmodelStore.fetchSites();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: t("production.sites.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/site/${getNewUuid()}` });
};

const editSite = (row: DataTableRowClickEvent) => {
  router.push({ path: `/site/${row.data.id}` });
};

const deleteButton = (entity: Site) => {
  confirm.require({
    message: t("production.messages.confirmDeleteSite", { name: entity.name }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteSite(entity.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("production.messages.deleted"),
          life: 3000,
        });
        await plantmodelStore.fetchSites();
      }
    },
  });
};
</script>

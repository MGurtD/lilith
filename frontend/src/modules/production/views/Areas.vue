<template>
  <Table
    :items="plantmodelStore.areas ?? []"
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
      <span class="text-900 font-bold">{{ t("production.areas.title") }}</span>
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
import { computed, onMounted, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { Area } from "../types";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const plantmodelStore = usePlantModelStore();
const { t, locale } = useI18n();

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
    field: "isVisibleInPlant",
    header: t("production.areas.visibleInPlant"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
  {
    field: "disabled",
    header: t("production.fields.disabled"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

const setMenuTitle = () =>
  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: t("production.areas.menuTitle"),
  });

onMounted(async () => {
  await plantmodelStore.fetchAreas();

  setMenuTitle();
});

const createButtonClick = () => {
  router.push({ path: `/area/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/area/${row.data.id}` });
};
const deleteButton = (area: Area) => {
  confirm.require({
    message: t("production.messages.confirmDeleteArea", { name: area.name }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteArea(area.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("production.messages.deleted"),
          life: 3000,
        });
        await plantmodelStore.fetchAreas();
      }
    },
  });
};

watch(locale, setMenuTitle);
</script>

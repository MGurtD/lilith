<template>
  <Table
    :items="taxStore.taxes ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    tableStyle="min-width: 100%"
    show-delete-column
    @create="createButtonClick"
    @delete="deleteTax"
    @row-click="edit"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{ t("shared.taxes.title") }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { getNewUuid } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { computed, onMounted, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useTaxesStore } from "../store/tax";
import { Tax } from "../types";

const router = useRouter();
const route = useRoute();
const store = useStore();
const taxStore = useTaxesStore();
const confirm = useConfirm();
const toast = useToast();
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("shared.taxes.columns.name"),
    style: "width: 30%",
  },
  {
    field: "percentatge",
    header: t("shared.taxes.columns.percentage"),
    columnType: ColumnType.Number,
    style: "width: 25%",
  },
  {
    field: "isReverseCharge",
    header: t("shared.taxes.columns.reverseCharge"),
    columnType: ColumnType.Boolean,
    showColor: false,
    style: "width: 25%",
  },
  {
    field: "disabled",
    header: t("shared.taxes.columns.disabled"),
    columnType: ColumnType.Boolean,
    showColor: false,
    style: "width: 15%",
  },
]);

const refreshMenu = () => {
  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: t("shared.taxes.menuTitle"),
  });
};

onMounted(async () => {
  await taxStore.fetchAll();
  refreshMenu();
});

// Re-fetch quan es torna a la ruta /taxes des d'una sub-ruta (ex: /tax/:id).
// El RouterView no manté la vista muntada (no hi ha KeepAlive), però Vue
// reutilitza la instància del component quan només canvien els params, de
// manera que onMounted no es torna a executar. Sense aquest watch, els canvis
// fets al formulari (ex: marcar inversió subjecte passiu) no es reflecteixen
// al llistat fins a recarregar la pàgina.
watch(
  () => route.fullPath,
  async (newPath, oldPath) => {
    const cameBackToList =
      newPath === "/taxes" && oldPath?.startsWith("/tax/") === true;
    if (cameBackToList) {
      await taxStore.fetchAll();
    }
  },
);

const createButtonClick = () => {
  router.push({ path: `/tax/${getNewUuid()}` });
};

const edit = (row: DataTableRowClickEvent) => {
  router.push({ path: `/tax/${row.data.id}` });
};

const deleteTax = (tax: Tax) => {
  confirm.require({
    message: t("shared.taxes.messages.confirmDelete"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await taxStore.delete(tax.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("shared.taxes.messages.deleted"),
          life: 3000,
        });
      } else {
        toast.add({
          severity: "error",
          summary: t("shared.taxes.messages.deleteError"),
          life: 4000,
        });
      }
    },
  });
};
</script>

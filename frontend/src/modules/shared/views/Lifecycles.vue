<template>
  <Table
    :items="lifecyclesStore.lifecycles ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    tableStyle="min-width: 100%"
    show-delete-column
    @create="createButtonClick"
    @delete="onDeleteRow"
    @row-click="edit"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{
        t("shared.lifecycles.title")
      }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import type { Column } from "@/components/tables/types";
import { getNewUuid } from "@/utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useStore } from "@/store";
import { useLifecyclesStore } from "../store/lifecycle";
import { Lifecycle } from "../types";
import { useToast } from "primevue/usetoast";

const router = useRouter();
const store = useStore();
const lifecyclesStore = useLifecyclesStore();
const resource = "lifecycle";
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("shared.lifecycles.columns.name"),
    style: "width: 25%",
  },
  {
    field: "description",
    header: t("shared.lifecycles.columns.description"),
    style: "width: 25%",
  },
  {
    field: "initialStatusId",
    header: t("shared.lifecycles.columns.initialStatus"),
    resolver: (_value, data) => getInitialStatusName(data as Lifecycle),
    style: "width: 25%",
  },
]);

onMounted(async () => {
  await lifecyclesStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.REFRESH,
    title: t("shared.lifecycles.menuTitle"),
  });
});

// Generic function to get status name by ID
const getStatusNameById = (
  lifecycle: Lifecycle,
  statusId: string | undefined
) => {
  if (!statusId || statusId.length === 0) {
    return "";
  }

  const status = lifecycle.statuses.find((s) => s.id === statusId);
  return status?.name || "";
};

// Helper functions for better readability
const getInitialStatusName = (lifecycle: Lifecycle) =>
  getStatusNameById(lifecycle, lifecycle.initialStatusId);

const createButtonClick = () => {
  router.push({ path: `/${resource}/${getNewUuid()}` });
};

const edit = (row: DataTableRowClickEvent) => {
  router.push({ path: `/${resource}/${row.data.id}` });
};

const toast = useToast();
const onDeleteRow = async (lifecycle: Lifecycle) => {
  await lifecyclesStore.fetchOne(lifecycle.id);

  if (
    !lifecyclesStore.lifecycle ||
    lifecyclesStore.lifecycle.statuses.length > 0 ||
    lifecyclesStore.transitions.length > 0
  ) {
    toast.add({
      summary: t("shared.lifecycles.messages.deleteTitle"),
      detail: t("shared.lifecycles.messages.deleteHasDependencies", {
        name: lifecyclesStore.lifecycle?.name,
      }),
      severity: "warn",
      life: 5000,
    });
    return;
  }

  await lifecyclesStore.delete(lifecycle.id);
};
</script>

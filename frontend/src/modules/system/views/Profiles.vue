<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useStore } from "@/store";
import { useProfilesStore } from "@/modules/system/store/profiles";
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type { Profile } from "@/types";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import type { DataTableRowClickEvent } from "primevue/datatable";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { getNewUuid } from "@/utils/functions";

const { t } = useI18n();
const router = useRouter();
const globalStore = useStore();
const profiles = useProfilesStore();
const confirm = useConfirm();
const toast = useToast();

const loading = ref(false);

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("profiles.name"),
    sortable: true,
    style: "width: 25%",
  },
  {
    field: "description",
    header: t("profiles.description"),
    style: "width: 40%",
  },
  {
    field: "isSystem",
    header: t("profiles.system"),
    columnType: ColumnType.Boolean,
    showColor: false,
    style: "width: 15%",
  },
]);

const load = async () => {
  loading.value = true;
  try {
    await profiles.fetchAll();
  } finally {
    loading.value = false;
  }
};

const createNew = () => router.push({ path: `/profile/${getNewUuid()}` });
const open = (profile: Profile) => router.push({ path: `/profile/${profile.id}` });
const openRow = (row: DataTableRowClickEvent) => open(row.data as Profile);
const canDelete = (profile: Profile) => !profile.isSystem;
const remove = (profile: Profile) => {
  if (profile.isSystem) return;
  confirm.require({
    message: t("profiles.confirmDelete"),
    header: t("profiles.confirmHeader"),
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      const ok = await profiles.remove(profile.id);
      toast.add({
        severity: ok ? "success" : "error",
        summary: t(ok ? "profiles.deleted" : "profiles.error"),
        life: 3000,
      });
    },
  });
};

onMounted(async () => {
  globalStore.setMenuItem({
    icon: PrimeIcons.USERS,
    title: t("profiles.listTitle"),
  });
  await load();
});
</script>
<template>
  <div class="card">
    <Table
      :items="profiles.items"
      :columns="columns"
      :filter-config="[]"
      :show-filter-actions="false"
      preset="crud-list"
      :loading="loading"
      dataKey="id"
      show-delete-column
      :can-delete="canDelete"
      @row-click="openRow"
      @create="createNew"
      @delete="remove"
      delete-column-width="2%"
    >
      <template #prepend>
        <span class="font-bold">{{ t("profiles.listTitle") }}</span>
      </template>
    </Table>
  </div>
</template>

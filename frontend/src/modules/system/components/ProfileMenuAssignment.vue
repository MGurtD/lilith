<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useProfileMenuSelectionStore } from "@/modules/system/store/profile-menu-selection";
import { useStore } from "@/store";
import TableFilter from "@/components/tables/TableFilter.vue";
import type { FilterConfig } from "@/components/tables/TableFilter.vue";

const props = defineProps<{ profileId: string }>();

const { t } = useI18n();
const toast = useToast();
const confirm = useConfirm();
const selectionStore = useProfileMenuSelectionStore();
const appStore = useStore();

// Search filter
const filter = ref({ search: "" });

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "search",
    label: t("common.search"),
    type: "text",
    placeholder: t("common.search"),
  },
]);

// Computed properties derivadas del store (sin estado local)
const loading = computed(() => selectionStore.loading);

const filteredRows = computed(() => {
  if (!filter.value.search) return selectionStore.rows;

  const searchLower = filter.value.search.toLowerCase();
  return selectionStore.rows.filter(
    (row) =>
      row.title.toLowerCase().includes(searchLower) ||
      row.key.toLowerCase().includes(searchLower),
  );
});

// DataTable necesita v-model bidireccional, usamos computed con getter/setter
const selectionRows = computed({
  get: () => selectionStore.selectedRows,
  set: () => {
    // El setter es llamado por DataTable pero no hace nada -
    // La selección real se maneja via eventos row-select/row-unselect
  },
});

// Event handlers - llaman directamente acciones del store
const onRowSelect = (e: any) => {
  selectionStore.toggle(e.data.id, true);
};

const onRowUnselect = (e: any) => {
  selectionStore.toggle(e.data.id, false);
};

const onSelectAll = () => {
  selectionStore.selectAll();
};

const onUnselectAll = () => {
  selectionStore.unselectAll();
};

const saveSelection = () => {
  confirm.require({
    message: t("profiles.confirmAssignMenus"),
    header: t("common.confirm"),
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      const ok = await selectionStore.save();

      toast.add({
        severity: ok ? "success" : "error",
        summary: ok
          ? t("common.saved")
          : t("common.error"),
        life: ok ? 2500 : 4000,
      });

      // Si el perfil modificado es el del usuario actual, recargar menús
      if (ok && appStore.user?.profileId === props.profileId) {
        await appStore.loadUserMenus(appStore.user);
      }
    },
  });
};

onMounted(async () => {
  await selectionStore.load(props.profileId);
});
</script>

<template>
  <div class="card profile-menu-assignment">
    <DataTable
      scrollable
      scrollHeight="flex"
      :value="filteredRows"
      v-model:selection="selectionRows"
      :loading="loading"
      dataKey="id"
      selectionMode="multiple"
      @row-select="onRowSelect"
      @row-unselect="onRowUnselect"
      @select-all-change="
        ($event) => ($event.checked ? onSelectAll() : onUnselectAll())
      "
    >
      <template #header>
        <TableFilter
          v-model="filter"
          :config="filterConfig"
          embedded
          :show-title="false"
          :show-action-labels="false"
          :show-create="false"
          :show-filter-action="false"
          :show-clear-action="false"
        >
          <template #append>
            <Button
              size="small"
              icon="pi pi-link"
              :label="t('common.assign')"
              @click="saveSelection"
            />
          </template>
        </TableFilter>
      </template>

      <Column selectionMode="multiple" headerStyle="width: 3rem" />

      <Column :header="t('menuItems.title')">
        <template #body="slotProps">
          <div class="flex align-items-center">
            <span :style="{ marginLeft: slotProps.data.level * 16 + 'px' }">{{
              slotProps.data.title
            }}</span>
          </div>
        </template>
      </Column>
      <Column
        field="key"
        :header="t('menuItems.key')"
        style="width: 20%"
      />
      <Column
        field="route"
        :header="t('menuItems.route')"
        style="width: 25%"
      />
      <Column
        field="sortOrder"
        :header="t('menuItems.order')"
        style="width: 8%"
      />
    </DataTable>
  </div>
</template>

<style scoped>
.p-datatable .p-datatable-tbody > tr > td {
  padding-top: 0.3rem;
  padding-bottom: 0.3rem;
}
.profile-menu-assignment {
  display: flex;
  flex-direction: column;
}
.profile-menu-assignment :deep(.p-datatable-wrapper) {
  flex: 1 1 auto;
}
</style>

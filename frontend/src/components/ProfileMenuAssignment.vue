<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useProfileMenuSelectionStore } from "../store/profile-menu-selection";
import { useStore } from "../store";

const props = defineProps<{ profileId: string }>();

const { t } = useI18n();
const toast = useToast();
const confirm = useConfirm();
const selectionStore = useProfileMenuSelectionStore();
const appStore = useStore();

// Search filter
const searchFilter = ref("");

// Computed properties derivadas del store (sin estado local)
const loading = computed(() => selectionStore.loading);

const filteredRows = computed(() => {
  if (!searchFilter.value) return selectionStore.rows;

  const searchLower = searchFilter.value.toLowerCase();
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
    message:
      (t("profiles.confirmAssignMenus") as string) ||
      "Confirmes desar la selecció de menús?",
    header: (t("common.confirm") as string) || "Confirmació",
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      const ok = await selectionStore.save();

      toast.add({
        severity: ok ? "success" : "error",
        summary: ok
          ? (t("common.saved") as string) || "Desat correctament"
          : (t("common.error") as string) || "Error en el procés",
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
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <div class="datatable-filter-1">
            <IconField iconPosition="left">
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText
                v-model="searchFilter"
                :placeholder="t('common.search')"
                class="w-full"
              />
            </IconField>
          </div>
          <div class="datatable-buttons">
            <Button
              size="small"
              icon="pi pi-link"
              :label="t('common.assign') || 'Assignar'"
              @click="saveSelection"
            />
          </div>
        </div>
      </template>

      <Column selectionMode="multiple" headerStyle="width: 3rem" />

      <Column :header="t('menuItems.title') || 'Títol'">
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
        :header="t('menuItems.key') || 'Clau'"
        style="width: 20%"
      />
      <Column
        field="route"
        :header="t('menuItems.route') || 'Ruta'"
        style="width: 25%"
      />
      <Column
        field="sortOrder"
        :header="t('menuItems.order') || 'Ordre'"
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

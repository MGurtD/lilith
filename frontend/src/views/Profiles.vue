<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useStore } from "../store";
import { useProfilesStore } from "../store/profiles";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { v4 as uuidv4 } from "uuid";

const { t } = useI18n();
const router = useRouter();
const globalStore = useStore();
const profiles = useProfilesStore();
const confirm = useConfirm();
const toast = useToast();

const loading = ref(false);

const load = async () => {
  loading.value = true;
  try {
    await profiles.fetchAll();
  } finally {
    loading.value = false;
  }
};

const createNew = () => router.push({ path: `/profile/${uuidv4()}` });
const open = (row: any) => router.push({ path: `/profile/${row.data.id}` });
const remove = (row: any) => {
  if (row.data.isSystem) return; // guard
  confirm.require({
    message: t("profiles.confirmDelete"),
    header: t("profiles.confirmHeader"),
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      const ok = await profiles.remove(row.data.id);
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
    <DataTable
      :value="profiles.items"
      :loading="loading"
      dataKey="id"
      @row-dblclick="open"
      tableStyle="min-width:50rem"
    >
      <template #header>
        <div class="flex justify-content-between align-items-center w-full">
          <span class="font-bold">{{ t("profiles.listTitle") }}</span>
          <Button
            :label="t('profiles.newButton')"
            icon="pi pi-plus"
            @click="createNew"
          />
        </div>
      </template>
      <Column
        field="name"
        :header="t('profiles.name')"
        sortable
        style="width: 25%"
      />
      <Column
        field="description"
        :header="t('profiles.description')"
        style="width: 40%"
      />
      <Column
        field="isSystem"
        :header="t('profiles.system')"
        style="width: 15%"
      >
        <template #body="slotProps">
          <i v-if="slotProps.data.isSystem" class="pi pi-lock text-sm" />
        </template>
      </Column>
      <Column :header="t('profiles.actions')" style="width: 20%">
        <template #body="slotProps">
          <Button
            icon="pi pi-pencil"
            text
            rounded
            @click.stop="open(slotProps)"
          />
          <Button
            icon="pi pi-trash"
            text
            rounded
            severity="danger"
            @click.stop="remove(slotProps)"
            :disabled="slotProps.data.isSystem"
          />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

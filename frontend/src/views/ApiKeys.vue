<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { useApiKeysStore } from "@/store/apiKeys";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { v4 as uuidv4 } from "uuid";
import FormApiKey from "@/components/forms/FormApiKey.vue";
import type { CreateApiKeyResponse } from "@/types";

const { t } = useI18n();
const globalStore = useStore();
const store = useApiKeysStore();
const confirm = useConfirm();
const toast = useToast();

// New key dialog
const showCreateDialog = ref(false);
const newKeyId = ref<string>("");
const formInitialData = ref<{
  id: string;
  name: string;
  description?: string;
  scopes?: string;
  expiresOn?: string | null;
}>({ id: "", name: "" });

// Show-once dialog
const generatedKey = ref<CreateApiKeyResponse | null>(null);
const showKeyDialog = ref(false);
const copied = ref(false);

const openCreateDialog = () => {
  newKeyId.value = uuidv4();
  formInitialData.value = { id: newKeyId.value, name: "" };
  showCreateDialog.value = true;
};

const handleCreate = async (data: {
  id?: string;
  name?: string;
  description?: string;
  scopes?: string;
  expiresOn?: string | null;
}) => {
  const result = await store.create({
    id: data.id ?? newKeyId.value,
    name: data.name ?? "",
    description: data.description,
    scopes: data.scopes,
    expiresOn: data.expiresOn ?? null,
  });

  if (result) {
    showCreateDialog.value = false;
    generatedKey.value = result;
    copied.value = false;
    showKeyDialog.value = true;
  } else {
    toast.add({
      severity: "error",
      summary: t("apiKeys.toasts.createError"),
      life: 4000,
    });
  }
};

const copyKey = async () => {
  if (!generatedKey.value?.apiKey) return;
  await navigator.clipboard.writeText(generatedKey.value.apiKey);
  copied.value = true;
  toast.add({
    severity: "success",
    summary: t("apiKeys.toasts.copied"),
    life: 2000,
  });
};

const closeKeyDialog = () => {
  showKeyDialog.value = false;
  generatedKey.value = null;
  copied.value = false;
};

const confirmDisable = (row: any) => {
  confirm.require({
    message: t("apiKeys.disable.confirmMessage", { name: row.data.name }),
    header: t("apiKeys.disable.confirmHeader"),
    icon: "pi pi-exclamation-triangle",
    acceptClass: "p-button-danger",
    acceptLabel: t("apiKeys.disable.confirmAccept"),
    rejectLabel: t("apiKeys.disable.confirmReject"),
    accept: async () => {
      const ok = await store.disable(row.data.id);
      toast.add({
        severity: ok ? "success" : "error",
        summary: ok ? t("apiKeys.toasts.disableSuccess") : t("apiKeys.toasts.disableError"),
        life: 3000,
      });
    },
  });
};

onMounted(async () => {
  globalStore.setMenuItem({
    icon: PrimeIcons.KEY,
    title: t("apiKeys.pageTitle"),
  });
  await store.fetchAll();
});
</script>

<template>
  <div class="card">
    <DataTable
      :value="store.items"
      :loading="store.loading"
      dataKey="id"
      tableStyle="min-width:50rem"
    >
      <template #header>
        <div class="flex justify-content-between align-items-center w-full">
          <span class="font-bold">{{ t('apiKeys.pageTitle') }}</span>
          <Button
            :label="t('apiKeys.newButton')"
            icon="pi pi-plus"
            @click="openCreateDialog"
          />
        </div>
      </template>

      <Column field="name" :header="t('apiKeys.table.columns.name')" sortable style="width: 20%" />
      <Column field="description" :header="t('apiKeys.table.columns.description')" style="width: 25%" />
      <Column field="keyPrefix" :header="t('apiKeys.table.columns.prefix')" style="width: 12%">
        <template #body="slotProps">
          <code class="text-sm">{{ slotProps.data.keyPrefix }}</code>
        </template>
      </Column>
      <Column field="scopes" :header="t('apiKeys.table.columns.scopes')" style="width: 18%" />
      <Column field="expiresOn" :header="t('apiKeys.table.columns.expires')" style="width: 12%">
        <template #body="slotProps">
          <span v-if="slotProps.data.expiresOn">
            {{ new Date(slotProps.data.expiresOn).toLocaleDateString("ca-ES") }}
          </span>
          <span v-else class="text-color-secondary">{{ t('apiKeys.table.never') }}</span>
        </template>
      </Column>
      <Column field="disabled" :header="t('apiKeys.table.columns.status')" style="width: 8%">
        <template #body="slotProps">
          <Tag
            :value="slotProps.data.disabled ? t('apiKeys.table.statusInactive') : t('apiKeys.table.statusActive')"
            :severity="slotProps.data.disabled ? 'danger' : 'success'"
          />
        </template>
      </Column>
      <Column :header="t('apiKeys.table.columns.actions')" style="width: 5%">
        <template #body="slotProps">
          <Button
            icon="pi pi-ban"
            text
            rounded
            severity="danger"
            :disabled="slotProps.data.disabled"
            @click.stop="confirmDisable(slotProps)"
            v-tooltip.left="t('apiKeys.disable.tooltip')"
          />
        </template>
      </Column>
    </DataTable>
  </div>

  <!-- Create dialog -->
  <Dialog
    v-model:visible="showCreateDialog"
    :header="t('apiKeys.createDialog.header')"
    :modal="true"
    :style="{ width: '50rem' }"
    :closable="true"
  >
    <FormApiKey
      :initialData="formInitialData"
      :submitting="store.saving"
      @submit="handleCreate"
    />
  </Dialog>

  <!-- Show-key-once dialog -->
  <Dialog
    v-model:visible="showKeyDialog"
    :header="t('apiKeys.showKeyDialog.header')"
    :modal="true"
    :closable="false"
    :style="{ width: '40rem' }"
    @hide="closeKeyDialog"
  >
    <div class="flex flex-column gap-3">
      <Message severity="warn" :closable="false">
        {{ t('apiKeys.showKeyDialog.warning') }}
      </Message>

      <div>
        <label class="block mb-1 font-semibold">{{ t('apiKeys.showKeyDialog.fieldName') }}</label>
        <span>{{ generatedKey?.name }}</span>
      </div>
      <div>
        <label class="block mb-1 font-semibold">{{ t('apiKeys.showKeyDialog.fieldPrefix') }}</label>
        <code>{{ generatedKey?.keyPrefix }}</code>
      </div>
      <div>
        <label class="block mb-1 font-semibold">{{ t('apiKeys.showKeyDialog.fieldApiKey') }}</label>
        <div class="flex gap-2 align-items-center">
          <InputText
            :value="generatedKey?.apiKey"
            readonly
            class="w-full font-mono text-sm"
          />
          <Button
            :icon="copied ? 'pi pi-check' : 'pi pi-copy'"
            :severity="copied ? 'success' : 'secondary'"
            @click="copyKey"
            v-tooltip.left="t('apiKeys.showKeyDialog.copyTooltip')"
          />
        </div>
      </div>
    </div>

    <template #footer>
      <Button
        :label="t('apiKeys.showKeyDialog.dismissButton')"
        icon="pi pi-check"
        @click="closeKeyDialog"
      />
    </template>
  </Dialog>
</template>

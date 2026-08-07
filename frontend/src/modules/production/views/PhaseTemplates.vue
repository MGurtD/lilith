<template>
  <DataTable
    :value="phaseTemplateStore.phaseTemplates"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sort-field="name"
    :sort-order="1"
    @row-click="editRow"
    paginator
    :rows="20"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">{{ t("phaseTemplates.title") }}</span>
        <div class="datatable-buttons">
          <Button
            :icon="PrimeIcons.PLUS"
            :aria-label="t('phaseTemplates.actions.create')"
            :title="t('phaseTemplates.actions.create')"
            rounded
            raised
            @click="createButtonClick"
          />
        </div>
      </div>
    </template>
    <Column
      field="name"
      sortable
      :header="t('phaseTemplates.fields.name')"
      style="width: 30%"
    ></Column>
    <Column
      field="description"
      :header="t('common.description')"
      style="width: 50%"
    ></Column>
    <Column :header="t('phaseTemplates.columns.disabled')" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column style="width: 10%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          :aria-label="t('phaseTemplates.actions.delete')"
          :title="t('phaseTemplates.actions.delete')"
          @click="deleteButton($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="t('phaseTemplates.dialogs.createTitle')"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <div>
      <BaseInput
        :label="t('phaseTemplates.fields.name')"
        v-model="phaseTemplateStore.phaseTemplate!.name"
        class="w-full mb-2"
      />
    </div>
    <div>
      <BaseInput
        :label="t('common.description')"
        v-model="phaseTemplateStore.phaseTemplate!.description"
        class="w-full mb-2"
      />
    </div>
    <br />
    <div>
      <Button
        :label="t('phaseTemplates.actions.create')"
        style="float: right"
        @click="onCreateSubmit"
      ></Button>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { onMounted, reactive, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { usePhaseTemplateStore } from "../store/phasetemplate";
import { PhaseTemplate } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import BaseInput from "../../../components/BaseInput.vue";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const phaseTemplateStore = usePhaseTemplateStore();
const { t, locale } = useI18n();

const dialogOptions = reactive({
  visible: false,
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const setMenuTitle = () => {
  store.setMenuItem({
    icon: PrimeIcons.LIST,
    title: t("phaseTemplates.menuTitle"),
  });
};

onMounted(async () => {
  setMenuTitle();
  await phaseTemplateStore.fetchAll();
});

watch(locale, () => setMenuTitle());

const createButtonClick = () => {
  const newId = getNewUuid();
  phaseTemplateStore.setNew(newId);
  dialogOptions.visible = true;
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/phasetemplate/${row.data.id}` });
  }
};

const onCreateSubmit = async () => {
  if (!phaseTemplateStore.phaseTemplate) return;

  const created = await phaseTemplateStore.create(
    phaseTemplateStore.phaseTemplate,
  );
  if (created)
    router.push({
      path: `/phasetemplate/${phaseTemplateStore.phaseTemplate.id}`,
    });
};

const deleteButton = (event: any, phaseTemplate: PhaseTemplate) => {
  confirm.require({
    target: event.currentTarget,
    message: t("phaseTemplates.messages.confirmDelete", {
      name: phaseTemplate.name,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await phaseTemplateStore.delete(phaseTemplate.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("phaseTemplates.messages.deleted"),
          life: 3000,
        });
        await phaseTemplateStore.fetchAll();
      }
    },
  });
};
</script>

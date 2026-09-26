<template>
  <Table
    phone-layout="cards"
    :card-layout="cardLayout"
    :items="phaseTemplateStore.phaseTemplates ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    tableStyle="min-width: 100%"
    sort-field="name"
    :sort-order="1"
    show-delete-column
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editRow"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{ t("phaseTemplates.title") }}</span>
    </template>
  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="t('phaseTemplates.dialogs.createTitle')"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <Form
      v-if="phaseTemplateStore.phaseTemplate"
      :rows="createRows"
      :initial-values="phaseTemplateStore.phaseTemplate"
      @submit="onCreateSubmit"
      @cancel="dialogOptions.visible = false"
    />
  </Dialog>
</template>

<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { stringValue } from "@/components/forms/value-utils";
import Table from "@/components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "@/components/tables/types";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { computed, onMounted, reactive, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { usePhaseTemplateStore } from "../store/phasetemplate";
import { PhaseTemplate } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const phaseTemplateStore = usePhaseTemplateStore();
const { t, locale } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("phaseTemplates.fields.name"),
    sortable: true,
    style: "width: 30%",
  },
  {
    field: "description",
    header: t("common.description"),
    style: "width: 50%",
  },
  {
    field: "disabled",
    header: t("phaseTemplates.columns.disabled"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

const cardLayout: CardLayout = {
  title: "name",
  subtitle: "description",
  meta: ["disabled"],
};

const createRows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 1 },
    fields: [
      {
        name: "name",
        label: t("phaseTemplates.fields.name"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("phaseTemplates.validation.nameRequired"),
        ),
      },
      {
        name: "description",
        label: t("common.description"),
        type: FormFieldType.Text,
      },
    ],
  },
]);

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
  router.push({ path: `/phasetemplate/${row.data.id}` });
};

const onCreateSubmit = async (values: FormValues) => {
  const source = phaseTemplateStore.phaseTemplate;
  if (!source) return;

  const model: PhaseTemplate = {
    ...source,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
  };

  const created = await phaseTemplateStore.create(model);
  if (created)
    router.push({
      path: `/phasetemplate/${model.id}`,
    });
};

const deleteButton = (phaseTemplate: PhaseTemplate) => {
  confirm.require({
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

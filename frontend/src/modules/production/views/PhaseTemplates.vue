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
        <span class="text-900 font-bold">Plantilles de fase</span>
        <div class="datatable-buttons">
          <Button
            :icon="PrimeIcons.PLUS"
            rounded
            raised
            @click="createButtonClick"
          />
        </div>
      </div>
    </template>
    <Column field="name" sortable header="Nom" style="width: 30%"></Column>
    <Column
      field="description"
      header="Descripció"
      style="width: 50%"
    ></Column>
    <Column header="Desactivada" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column style="width: 10%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteButton($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <div>
      <BaseInput
        label="Nom"
        v-model="phaseTemplateStore.phaseTemplate!.name"
        class="w-full mb-2"
      />
    </div>
    <div>
      <BaseInput
        label="Descripció"
        v-model="phaseTemplateStore.phaseTemplate!.description"
        class="w-full mb-2"
      />
    </div>
    <br />
    <div>
      <Button
        label="Crear"
        style="float: right"
        @click="onCreateSubmit"
      ></Button>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { onMounted, reactive } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { usePhaseTemplateStore } from "../store/phasetemplate";
import { PhaseTemplate } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import BaseInput from "../../../components/BaseInput.vue";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const phaseTemplateStore = usePhaseTemplateStore();

const dialogOptions = reactive({
  visible: false,
  title: "Crear plantilla de fase",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.LIST,
    title: "Gestió de plantilles de fase",
  });

  await phaseTemplateStore.fetchAll();
});

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
    message: `Està segur que vol eliminar la plantilla ${phaseTemplate.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await phaseTemplateStore.delete(phaseTemplate.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminada",
          life: 3000,
        });
        await phaseTemplateStore.fetchAll();
      }
    },
  });
};
</script>

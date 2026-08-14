<template>
  <div class="mt-4">
    <DataTable
      :value="reasons"
      class="p-datatable-sm small-last-column clickable-rows"
      tableStyle="min-width: 100%"
      paginator
      :rows="20"
      stripedRows
      :rowHover="true"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <span class="text-900 font-bold">{{ t("production.components.motius") }}</span>
          <Button icon="pi pi-plus" :label="t('production.components.afegirMotiu')" @click="onAdd" />
        </div>
      </template>
      <Column field="code" :header="t('production.components.codi')" sortable>
        <template #body="slotProps">
          {{ slotProps.data.code }}
        </template>
      </Column>
      <Column field="name" :header="t('production.components.nom')" sortable>
        <template #body="slotProps">
          {{ slotProps.data.name }}
        </template>
      </Column>
      <Column field="description" :header="t('production.components.descripcio')" sortable>
        <template #body="slotProps">
          {{ slotProps.data.description }}
        </template>
      </Column>
      <Column field="color" :header="t('production.components.color')" sortable>
        <template #body="slotProps">
          <ColorColumn :value="slotProps.data.color" />
        </template>
      </Column>
      <Column field="icon" :header="t('production.components.icona')" sortable>
        <template #body="slotProps">
          <IconColumn :value="slotProps.data.icon" />
        </template>
      </Column>
      <Column>
        <template #body="slotProps">
          <div class="flex justify-content-end gap-2">
            <Button
              icon="pi pi-pencil"
              text
              rounded
              @click="onEdit(slotProps.data)"
            />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              @click="onDelete(slotProps.data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import { ref } from "vue";
import { MachineStatusReason } from "../types";
import { getNewUuid } from "../../../utils/functions";
import ColorColumn from "../../../components/tables/ColorColumn.vue";
import IconColumn from "../../../components/tables/IconColumn.vue";

const props = defineProps<{
  reasons: Array<MachineStatusReason>;
  machineStatusId: string;
}>();

const emit = defineEmits<{
  (e: "add", reason: MachineStatusReason): void;
  (e: "edit", reason: MachineStatusReason): void;
  (e: "delete", id: string): void;
}>();

const filter = ref("");

const onAdd = () => {
  const newReason: MachineStatusReason = {
    id: getNewUuid(),
    code: "",
    name: "",
    description: "",
    color: "#000000",
    icon: "",
    machineStatusId: props.machineStatusId,
    disabled: false,
  };
  emit("add", newReason);
};

const onEdit = (reason: MachineStatusReason) => {
  emit("edit", reason);
};

const onDelete = (reason: MachineStatusReason) => {
  emit("delete", reason.id);
};
</script>

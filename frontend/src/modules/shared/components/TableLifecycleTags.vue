<template>
  <section class="card">
    <DataTable :value="tags" stripedRows>
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <span class="text-900 font-bold">{{ $t('shared.lifecycleTags.title') }}</span>
          <Button :icon="PrimeIcons.PLUS" rounded raised @click="addTag" />
        </div>
      </template>

      <Column field="name" :header="$t('shared.lifecycleTags.columns.name')" style="min-width: 12rem"></Column>
      <Column
        field="description"
        :header="$t('shared.lifecycleTags.columns.description')"
        style="min-width: 15rem"
      ></Column>
      <Column field="color" :header="$t('shared.lifecycleTags.columns.color')" style="width: 10rem">
        <template #body="slotProps">
          <Tag
            v-if="slotProps.data.color"
            :severity="slotProps.data.color as any"
          >
            {{ slotProps.data.name }}
          </Tag>
          <span v-else>-</span>
        </template>
      </Column>
      <Column field="icon" :header="$t('shared.lifecycleTags.columns.icon')" style="width: 8rem">
        <template #body="slotProps">
          <i v-if="slotProps.data.icon" :class="slotProps.data.icon"></i>
          <span v-else>-</span>
        </template>
      </Column>
      <Column :header="$t('shared.lifecycleTags.columns.actions')" style="width: 10rem">
        <template #body="slotProps">
          <Button
            icon="pi pi-pencil"
            severity="info"
            text
            rounded
            @click="editTag(slotProps.data)"
          />
          <Button
            icon="pi pi-trash"
            severity="danger"
            text
            rounded
            @click="$emit('delete', slotProps.data)"
          />
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-4 text-500">
          {{ $t('shared.lifecycleTags.empty') }}
        </div>
      </template>
    </DataTable>
  </section>
</template>

<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";
import { LifecycleTag } from "../types";

defineProps<{
  tags: LifecycleTag[];
}>();

const emit = defineEmits<{
  (e: "add", tag: LifecycleTag): void;
  (e: "edit", tag: LifecycleTag): void;
  (e: "delete", tag: LifecycleTag): void;
}>();

const addTag = () => {
  emit("add", {
    id: crypto.randomUUID(),
    name: "",
    description: "",
    color: "",
    icon: "",
    lifecycleId: "",
    disabled: false,
    createdOn: new Date(),
    updatedOn: new Date(),
  });
};

const editTag = (tag: LifecycleTag) => {
  emit("edit", { ...tag });
};
</script>

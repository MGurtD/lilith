<template>
  <main class="container">
    <Button label="Guardar" class="grid_add_row_button" @click="submitForm" />
    <section class="section_lifecycle mt-5">
      <FormLifecycle v-if="lifecycle" :lifecycle="lifecycle" />
    </section>

    <Tabs v-if="formMode === FormActionMode.EDIT && lifecycle" value="0">
      <TabList>
        <Tab value="0">Estats i Transicions</Tab>
        <Tab value="1">Etiquetes</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <section class="section_status">
            <div class="section_status_list">
              <TableStatuses
                v-if="lifecycle"
                :lifecycle-id="lifecycle.id"
                :statuses="lifecycle.statuses"
                :transitions="lifecycleStore.transitions"
                @add="(s) => openStatus(FormActionMode.CREATE, s)"
                @edit="(s) => openStatus(FormActionMode.EDIT, s)"
                @delete="deleteStatus"
              />
            </div>
            <div class="section_statustransition_list">
              <TableStatusTransitions
                v-if="lifecycle"
                :statuses="lifecycle.statuses"
                :transitions="lifecycleStore.transitions"
                @add="(t) => openStatusTransition(FormActionMode.CREATE, t)"
                @edit="(t) => openStatusTransition(FormActionMode.EDIT, t)"
                @delete="deleteStatusTransition"
              />
            </div>
          </section>
        </TabPanel>

        <TabPanel value="1">
          <TableLifecycleTags
            :tags="lifecycle.tags || []"
            @add="(t) => openTag(FormActionMode.CREATE, t)"
            @edit="(t) => openTag(FormActionMode.EDIT, t)"
            @delete="deleteTag"
          />
        </TabPanel>
      </TabPanels>
    </Tabs>

    <section v-else class="section_status">
      <div class="section_status_list">
        <TableStatuses
          v-if="lifecycle"
          :lifecycle-id="lifecycle.id"
          :statuses="lifecycle.statuses"
          :transitions="lifecycleStore.transitions"
          @add="(s) => openStatus(FormActionMode.CREATE, s)"
          @edit="(s) => openStatus(FormActionMode.EDIT, s)"
          @delete="deleteStatus"
        />
      </div>
      <div class="section_statustransition_list">
        <TableStatusTransitions
          v-if="lifecycle"
          :statuses="lifecycle.statuses"
          :transitions="lifecycleStore.transitions"
          @add="(t) => openStatusTransition(FormActionMode.CREATE, t)"
          @edit="(t) => openStatusTransition(FormActionMode.EDIT, t)"
          @delete="deleteStatusTransition"
        />
      </div>
    </section>
  </main>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <FormStatus
      v-if="selectedStatus"
      :status="selectedStatus"
      :formAction="auxiliarFormAction"
      @submit="onStatusSubmit"
    />
    <FormStatusTransition
      v-if="selectedStatusTransition"
      :transition="selectedStatusTransition"
      :statuses="lifecycle!.statuses"
      :formAction="auxiliarFormAction"
      @submit="onStatusTransitionSubmit"
    />
    <FormLifecycleTag
      v-if="selectedTag"
      :tag="selectedTag"
      :formAction="auxiliarFormAction"
      @submit="onTagSubmit"
      @cancel="
        () => {
          dialogOptions.visible = false;
          selectedTag = undefined;
        }
      "
    />
  </Dialog>
</template>
<script setup lang="ts">
import { onMounted, reactive, ref } from "vue";
import TableStatuses from "../components/TableStatuses.vue";
import TableStatusTransitions from "../components/TableStatusTransitions.vue";
import TableLifecycleTags from "../components/TableLifecycleTags.vue";
import FormLifecycle from "../components/FormLifecycle.vue";
import FormStatus from "../components/FormStatus.vue";
import FormStatusTransition from "../components/FormStatusTransition.vue";
import FormLifecycleTag from "../components/FormLifecycleTag.vue";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
import { useLifecyclesStore } from "../store/lifecycle";
import { PrimeIcons } from "@primevue/core/api";
import { FormActionMode, DialogOptions } from "../../../types/component";
import { Lifecycle, Status, StatusTransition, LifecycleTag } from "../types";
import SharedServices from "../services";

const formMode = ref(FormActionMode.EDIT);
const router = useRouter();
const route = useRoute();
const store = useStore();
const lifecycleStore = useLifecyclesStore();
const { lifecycle, transitions } = storeToRefs(lifecycleStore);
const auxiliarFormAction = ref(FormActionMode.CREATE);
const selectedStatus = ref(undefined as Status | undefined);
const selectedStatusTransition = ref(undefined as StatusTransition | undefined);
const selectedTag = ref(undefined as LifecycleTag | undefined);

const dialogOptions = reactive({
  visible: false,
  title: "",
  closable: true,
  position: "bottom",
  modal: true,
} as DialogOptions);

const loadView = async () => {
  await lifecycleStore.fetchOne(route.params.id as string);

  // Comprovar existencia del proveïdor
  let pageTitle = "";
  if (!lifecycle.value) {
    formMode.value = FormActionMode.CREATE;
    lifecycleStore.setNew(route.params.id as string);
    pageTitle = "Alta cicle de vida";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Cicle de vida ${lifecycle.value.name}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.REFRESH,
    backButtonVisible: true,
    title: pageTitle,
  });
};
onMounted(async () => {
  await loadView();
});

// Status
const openStatus = (action: FormActionMode, status: Status) => {
  if (formMode.value === FormActionMode.CREATE) return;

  auxiliarFormAction.value = action;
  selectedStatus.value = status;
  selectedStatusTransition.value = undefined;
  selectedTag.value = undefined;

  dialogOptions.visible = true;
  dialogOptions.title = "Introducció d'estats";
};

const deleteStatus = async (status: Status) => {
  const exist = transitions.value.find(
    (t) => t.statusId === status.id || t.statusToId === status.id,
  );
  if (exist) {
    toast.add({
      severity: "warn",
      summary: "Estat dependent",
      detail: `L'estat ${status.name} forma part d'una transició`,
      life: 5000,
    });
    return;
  }

  await lifecycleStore.deleteStatus(status.id);
};

const onStatusSubmit = async (
  status: Status,
  tagChanges: { assign: string[]; remove: string[] },
) => {
  let success = false;

  if (auxiliarFormAction.value == FormActionMode.CREATE) {
    success = await lifecycleStore.createStatus(status);
  } else {
    success = await lifecycleStore.updateStatus(status.id, status);
  }

  if (success) {
    // Apply tag changes
    for (const tagId of tagChanges.assign) {
      await lifecycleStore.assignTagToStatus(status.id, tagId);
    }
    for (const tagId of tagChanges.remove) {
      await lifecycleStore.removeTagFromStatus(status.id, tagId);
    }

    dialogOptions.visible = false;
    selectedStatus.value = undefined;
    await loadView();
  } else {
    dialogOptions.visible = false;
  }
};

// StatusTransition
const openStatusTransition = (
  action: FormActionMode,
  transition: StatusTransition,
) => {
  if (formMode.value === FormActionMode.CREATE) return;

  auxiliarFormAction.value = action;
  selectedStatus.value = undefined;
  selectedStatusTransition.value = transition;
  selectedTag.value = undefined;

  dialogOptions.visible = true;
  dialogOptions.title = "Introducció de transicions";
};

const deleteStatusTransition = async (transition: StatusTransition) =>
  await lifecycleStore.deleteTransition(transition.id);

const onStatusTransitionSubmit = async (transition: StatusTransition) => {
  let asyncAction;
  if (auxiliarFormAction.value == FormActionMode.CREATE) {
    asyncAction = lifecycleStore.createTransition(transition);
  } else {
    asyncAction = lifecycleStore.updateTransition(transition.id, transition);
  }
  const success = await asyncAction;

  if (success) {
    dialogOptions.visible = false;
    selectedStatusTransition.value = undefined;
    await loadView();
  }
};

// LifecycleTag
const openTag = (action: FormActionMode, tag: LifecycleTag) => {
  if (formMode.value === FormActionMode.CREATE) return;

  auxiliarFormAction.value = action;
  selectedStatus.value = undefined;
  selectedStatusTransition.value = undefined;
  selectedTag.value = { ...tag, lifecycleId: lifecycle.value!.id };

  dialogOptions.visible = true;
  dialogOptions.title =
    action === FormActionMode.CREATE ? "Nova etiqueta" : "Editar etiqueta";
};

const deleteTag = async (tag: LifecycleTag) => {
  const result = await lifecycleStore.deleteTag(tag.id);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Etiqueta eliminada correctament",
      life: 4000,
    });
    await loadView();
  }
};

const onTagSubmit = async (tag: LifecycleTag) => {
  let result;
  if (auxiliarFormAction.value === FormActionMode.CREATE) {
    result = await lifecycleStore.createTag(lifecycle.value!.id, tag);
  } else {
    result = await lifecycleStore.updateTag(tag);
  }

  if (result) {
    toast.add({
      severity: "success",
      summary: "Etiqueta desada correctament",
      life: 4000,
    });
    dialogOptions.visible = false;
    selectedTag.value = undefined;
    await loadView();
  }
};

// Lifecycle submit
const toast = useToast();
const submitForm = async () => {
  const data = lifecycle.value as Lifecycle;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await lifecycleStore.create(data);
    message = "Cicle de vida creat correctament";
  } else {
    result = await lifecycleStore.update(data.id, data);
    message = "Cicle de vida actualizat correctament";
  }

  if (result) {
    toast.add({
      severity: "success",
      summary: message,
      life: 5000,
    });

    if (formMode.value === FormActionMode.EDIT) {
      router.back();
    } else {
      await loadView();
    }
  }
};
</script>

<style scoped>
.container {
  display: grid;
  grid-template-rows: 0.15fr 0.85fr;
  height: 100%;
}

.section_status {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
  width: 100%;
}
</style>

# Component Templates

Complete copy-paste templates for the most common component patterns.

---

## List View

**File**: `src/modules/<domain>/views/YourEntities.vue`

```vue
<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useRouter } from "vue-router";
import { useYourEntityStore } from "../store/yourentity";
import { useUserFilterStore } from "@/store/userfilter";
import { getNewUuid } from "@/utils/functions";
import FormYourEntity from "../components/FormYourEntity.vue";

const router = useRouter();
const store = useYourEntityStore();
const filterStore = useUserFilterStore();

const showCreateDialog = ref(false);
const newEntityId = ref("");

const filters = ref({
  search: filterStore.getFilter("yourEntities", "search") ?? "",
});

const filtered = computed(() =>
  store.yourEntities.filter((e) =>
    !filters.value.search ||
    e.number?.toLowerCase().includes(filters.value.search.toLowerCase())
  )
);

onMounted(() => store.fetchAll());

function onRowClick(row: any) {
  router.push({ name: "yourentity", params: { id: row.data.id } });
}

function clearFilters() {
  filters.value.search = "";
  filterStore.clearFilters("yourEntities");
}

function openCreate() {
  newEntityId.value = getNewUuid();
  store.setNew(newEntityId.value);
  showCreateDialog.value = true;
}

async function onCreated() {
  showCreateDialog.value = false;
  router.push({ name: "yourentity", params: { id: newEntityId.value } });
}
</script>

<template>
  <div class="p-3">
    <div class="flex gap-2 mb-3 align-items-center flex-wrap">
      <ExerciseDatePicker />
      <InputText v-model="filters.search" placeholder="Cercar..." class="p-inputtext-sm" />
      <Button icon="pi pi-times" class="p-button-text p-button-sm" @click="clearFilters" />
      <Button label="Nova" icon="pi pi-plus" class="p-button-sm ml-auto" @click="openCreate" />
    </div>

    <DataTable
      :value="filtered"
      :rows="20"
      paginator
      row-hover
      striped-rows
      @row-click="onRowClick"
    >
      <Column field="number" header="Número" sortable />
      <Column field="date" header="Data" sortable />
      <Column field="status.name" header="Estat" />
    </DataTable>

    <Dialog v-model:visible="showCreateDialog" header="Nova entitat" modal style="width: 40rem">
      <FormYourEntity :is-new="true" @saved="onCreated" @cancelled="showCreateDialog = false" />
    </Dialog>
  </div>
</template>
```

---

## Detail View

**File**: `src/modules/<domain>/views/YourEntity.vue`

```vue
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { useYourEntityStore } from "../store/yourentity";
import { useConfirm } from "primevue/useconfirm";
import FormYourEntity from "../components/FormYourEntity.vue";
import TableYourEntityLines from "../components/TableYourEntityLines.vue";

const props = defineProps<{ id: string }>();
const router = useRouter();
const store = useYourEntityStore();
const confirm = useConfirm();

const formRef = ref<InstanceType<typeof FormYourEntity>>();

onMounted(() => store.fetchOne(props.id));

const menuItems = [
  {
    label: "Eliminar",
    icon: "pi pi-trash",
    command: () => confirmDelete(),
  },
];

async function save() {
  await formRef.value?.submit();
}

function confirmDelete() {
  confirm.require({
    message: "Estàs segur que vols eliminar aquesta entitat?",
    header: "Confirmació",
    icon: "pi pi-exclamation-triangle",
    acceptLabel: "Eliminar",
    rejectLabel: "Cancel·lar",
    accept: async () => {
      const ok = await store.remove(props.id);
      if (ok) router.push({ name: "YourEntities" });
    },
  });
}
</script>

<template>
  <div class="p-3" v-if="store.yourEntity">
    <div class="flex justify-content-between align-items-center mb-3">
      <h2 class="m-0">{{ store.yourEntity.number }}</h2>
      <SplitButton
        label="Guardar"
        icon="pi pi-save"
        :model="menuItems"
        @click="save"
      />
    </div>

    <TabView>
      <TabPanel header="Dades generals">
        <FormYourEntity ref="formRef" />
      </TabPanel>
      <TabPanel header="Línies">
        <TableYourEntityLines :entity-id="props.id" />
      </TabPanel>
    </TabView>
  </div>
</template>
```

---

## Form Component

**File**: `src/modules/<domain>/components/FormYourEntity.vue`

```vue
<script setup lang="ts">
import { ref, watch } from "vue";
import * as yup from "yup";
import { FormValidation } from "@/utils/form-validator";
import { globalToast } from "@/utils/global-toast";
import { convertDateTimeToJSON } from "@/utils/functions";
import { useYourEntityStore } from "../store/yourentity";
import type { YourEntity } from "../types";

const props = defineProps<{
  isNew?: boolean;
}>();

const emit = defineEmits<{
  (e: "saved", entity: YourEntity): void;
  (e: "cancelled"): void;
}>();

const store = useYourEntityStore();
const model = ref({ ...store.yourEntity! });
const validationErrors = ref<Record<string, string[]>>({});

// Watch store changes (e.g. after fetchOne)
watch(
  () => store.yourEntity,
  (newVal) => { if (newVal) model.value = { ...newVal }; }
);

const schema = yup.object({
  number: yup.string().required("El número és obligatori"),
  date: yup.mixed().required("La data és obligatòria"),
});

async function submit() {
  const validation = new FormValidation(schema).validate(model.value);
  if (!validation.result) {
    validationErrors.value = validation.errors;
    globalToast.warn(Object.values(validation.errors).flat().join("\n"));
    return;
  }
  validationErrors.value = {};

  // Normalize all dates before API call
  model.value.date = convertDateTimeToJSON(model.value.date);

  let ok: boolean;
  if (props.isNew) {
    ok = await store.create(model.value);
  } else {
    ok = await store.update(model.value.id, model.value);
  }

  if (ok) {
    globalToast.success("Entitat guardada correctament");
    emit("saved", model.value);
  }
}

defineExpose({ submit });
</script>

<template>
  <form @submit.prevent="submit">
    <div class="four-columns">

      <div class="field">
        <label for="number">Número *</label>
        <InputText
          id="number"
          v-model="model.number"
          :class="{ 'p-invalid': validationErrors.number }"
          fluid
        />
        <small class="p-error">{{ validationErrors.number?.[0] }}</small>
      </div>

      <div class="field">
        <label for="date">Data *</label>
        <DatePicker
          id="date"
          v-model="model.date"
          date-format="dd/mm/yy"
          :class="{ 'p-invalid': validationErrors.date }"
          fluid
        />
        <small class="p-error">{{ validationErrors.date?.[0] }}</small>
      </div>

      <div class="field">
        <label for="description">Descripció</label>
        <InputText id="description" v-model="model.description" fluid />
      </div>

    </div>
  </form>
</template>
```

---

## Table Component

**File**: `src/modules/<domain>/components/TableYourEntityLines.vue`

```vue
<script setup lang="ts">
import { computed, ref } from "vue";
import { useConfirm } from "primevue/useconfirm";
import { formatCurrency, getNewUuid } from "@/utils/functions";
import { FormActionMode } from "@/types/component";
import { useYourEntityStore } from "../store/yourentity";
import { Services } from "../services";
import type { YourEntityLine } from "../types";
import FormYourEntityLine from "./FormYourEntityLine.vue";

const props = defineProps<{ entityId: string }>();

const store = useYourEntityStore();
const confirm = useConfirm();

const showDialog = ref(false);
const dialogMode = ref<FormActionMode>(FormActionMode.CREATE);
const editingLine = ref<YourEntityLine | null>(null);

const lines = computed(() => store.yourEntity?.lines ?? []);

function openCreate() {
  dialogMode.value = FormActionMode.CREATE;
  editingLine.value = {
    id: getNewUuid(),
    yourEntityId: props.entityId,
    quantity: 0,
    unitPrice: 0,
    description: "",
  };
  showDialog.value = true;
}

function openEdit(line: YourEntityLine) {
  dialogMode.value = FormActionMode.EDIT;
  editingLine.value = { ...line }; // Clone!
  showDialog.value = true;
}

async function onSaved() {
  showDialog.value = false;
  await store.fetchOne(props.entityId);
}

function confirmDelete(line: YourEntityLine) {
  confirm.require({
    message: "Vols eliminar aquesta línia?",
    header: "Confirmació",
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      await Services.YourEntityLine.delete(line.id);
      await store.fetchOne(props.entityId);
    },
  });
}
</script>

<template>
  <div>
    <div class="flex justify-content-end mb-2">
      <Button label="Afegir línia" icon="pi pi-plus" class="p-button-sm" @click="openCreate" />
    </div>

    <DataTable :value="lines" striped-rows>
      <Column field="description" header="Descripció" />
      <Column field="quantity" header="Quantitat" style="width: 8rem" />
      <Column field="unitPrice" header="Preu">
        <template #body="{ data }">{{ formatCurrency(data.unitPrice) }}</template>
      </Column>
      <Column header="" style="width: 6rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" text rounded @click="openEdit(data)" />
          <Button icon="pi pi-trash" text rounded severity="danger" @click="confirmDelete(data)" />
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="showDialog"
      :header="dialogMode === FormActionMode.CREATE ? 'Nova línia' : 'Editar línia'"
      modal
      style="width: 35rem"
    >
      <FormYourEntityLine
        v-if="editingLine"
        :model="editingLine"
        :mode="dialogMode"
        @saved="onSaved"
        @cancelled="showDialog = false"
      />
    </Dialog>
  </div>
</template>
```

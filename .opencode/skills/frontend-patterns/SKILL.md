---
name: frontend-patterns
description: Vue 3 component patterns and templates for Lilith ERP frontend (PrimeVue 4, Pinia, TypeScript). Use when (1) Building list views with DataTable, filters, and exercise date picker, (2) Creating detail views with SplitButton actions and tabbed forms, (3) Implementing dialog-based CRUD for nested entities (lines, phases, etc.), (4) Adding form validation with Yup and PrimeVue inputs, (5) Creating table components with edit/delete confirmation, (6) Adding dropdown/selector components for foreign keys, (7) Downloading reports from the reports microservice, (8) Understanding how existing patterns work before modifying them.
---

# Frontend Patterns

Reference patterns for Vue 3 components in Lilith ERP.

## Overview of Available Patterns

| Pattern | When to use |
|---------|------------|
| [List View](#1-list-view) | Main page showing a DataTable of all entities |
| [Detail View](#2-detail-view) | Single entity page with form and nested tables |
| [Dialog CRUD](#3-dialog-crud) | Inline create/edit for nested entities via Dialog |
| [Form Validation](#4-form-validation) | Yup validation with PrimeVue inputs |
| [Nested Table](#5-nested-table) | Table component for child entities with edit/delete |
| [Dropdown Selectors](#6-dropdown-selectors) | Foreign key dropdowns (Lifecycle, Status, custom) |
| [Report Download](#7-report-download) | Download PDF/Excel from reports microservice |

See [references/PRIMEVUE_PATTERNS.md](references/PRIMEVUE_PATTERNS.md) for PrimeVue-specific component configs.  
See [references/SERVICE_PATTERNS.md](references/SERVICE_PATTERNS.md) for advanced service layer patterns.

---

## 1. List View

**File**: `src/modules/<domain>/views/YourEntities.vue`  
**Real example**: `src/modules/sales/views/Budgets.vue`

```vue
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { useYourEntityStore } from "../store/yourentity";
import { useStore } from "@/store";
import { useUserFilterStore } from "@/store/userfilter";

const router = useRouter();
const store = useYourEntityStore();
const appStore = useStore();
const filterStore = useUserFilterStore();

// Filter state
const filters = ref({
  search: filterStore.getFilter("yourEntities", "search") ?? "",
  statusId: filterStore.getFilter("yourEntities", "statusId") ?? null,
});

// Dialog for creation
const showCreateDialog = ref(false);
const newEntityId = ref("");

onMounted(() => loadData());

async function loadData() {
  await store.fetchAll();
}

function onRowClick(row: any) {
  router.push({ name: "yourentity", params: { id: row.data.id } });
}

function clearFilters() {
  filters.value.search = "";
  filters.value.statusId = null;
  filterStore.clearFilters("yourEntities");
}

function openCreateDialog() {
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
  <div>
    <!-- Filters bar -->
    <div class="flex gap-2 mb-3 align-items-center flex-wrap">
      <ExerciseDatePicker />  <!-- date range picker, always present -->
      
      <InputText
        v-model="filters.search"
        placeholder="Cercar..."
        class="p-inputtext-sm"
      />
      
      <DropdownLifecycle
        v-model="filters.statusId"
        lifecycle="YourEntity"
        placeholder="Estat"
      />
      
      <Button
        icon="pi pi-times"
        class="p-button-text p-button-sm"
        @click="clearFilters"
      />
      
      <Button
        label="Nova entitat"
        icon="pi pi-plus"
        class="p-button-sm ml-auto"
        @click="openCreateDialog"
      />
    </div>

    <!-- Data table -->
    <DataTable
      :value="store.yourEntities"
      :rows="20"
      paginator
      row-hover
      striped-rows
      @row-click="onRowClick"
    >
      <Column field="number" header="Número" sortable />
      <Column field="date" header="Data">
        <template #body="{ data }">
          {{ formatDate(data.date) }}
        </template>
      </Column>
      <Column field="amount" header="Import">
        <template #body="{ data }">
          {{ formatCurrency(data.amount) }}
        </template>
      </Column>
      <Column field="status.name" header="Estat" />
    </DataTable>

    <!-- Creation dialog -->
    <Dialog v-model:visible="showCreateDialog" header="Nova entitat" modal>
      <FormYourEntity :is-new="true" @saved="onCreated" />
    </Dialog>
  </div>
</template>
```

**Key points:**
- `ExerciseDatePicker` is always in the filter bar for date-range filtering
- `useUserFilterStore` persists filter state across navigation
- Row click navigates to detail view
- Creation via Dialog, then redirect to detail

---

## 2. Detail View

**File**: `src/modules/<domain>/views/YourEntity.vue`  
**Real example**: `src/modules/sales/views/Budget.vue`

```vue
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { useYourEntityStore } from "../store/yourentity";
import { useConfirm } from "primevue/useconfirm";
import { globalToast } from "@/utils/global-toast";

const props = defineProps<{ id: string }>();
const router = useRouter();
const store = useYourEntityStore();
const confirm = useConfirm();

// Ref to form component to trigger submit from parent
const formRef = ref();

onMounted(() => store.fetchOne(props.id));

// SplitButton: primary action = Save, secondary = menu items
const menuItems = [
  {
    label: "Acceptar",
    icon: "pi pi-check",
    command: () => handleAccept(),
  },
  {
    label: "Eliminar",
    icon: "pi pi-trash",
    command: () => confirmDelete(),
  },
];

async function save() {
  formRef.value?.submit();
}

async function handleAccept() {
  // Call specific workflow action via store/service
}

function confirmDelete() {
  confirm.require({
    message: "Estàs segur que vols eliminar aquesta entitat?",
    header: "Confirmació",
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      const ok = await store.remove(props.id);
      if (ok) router.push({ name: "YourEntities" });
    },
  });
}
</script>

<template>
  <div v-if="store.yourEntity">
    <!-- Header with SplitButton -->
    <div class="flex justify-content-between align-items-center mb-3">
      <h2>{{ store.yourEntity.number }}</h2>
      <SplitButton
        label="Guardar"
        icon="pi pi-save"
        :model="menuItems"
        @click="save"
      />
    </div>

    <!-- Tabbed content -->
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

**Key points:**
- `formRef.value?.submit()` — parent triggers child form save
- `SplitButton` for save + extra actions in dropdown
- `TabView` for organizing complex entities
- Always check `v-if="store.yourEntity"` before rendering

---

## 3. Dialog CRUD

**For nested entities** (e.g., adding a line to a Budget)  
**Real example**: `TableBudgetDetails.vue` dialog flow

```vue
<script setup lang="ts">
import { ref } from "vue";
import { FormActionMode } from "@/types/component";
import type { YourEntityLine } from "../types";

const props = defineProps<{ entityId: string }>();

const showDialog = ref(false);
const dialogMode = ref<FormActionMode>(FormActionMode.CREATE);
const editingLine = ref<YourEntityLine | null>(null);

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
  editingLine.value = { ...line }; // Clone before editing!
  showDialog.value = true;
}

async function onSaved() {
  showDialog.value = false;
  // Re-fetch parent to refresh all state
  await store.fetchOne(props.entityId);
}
</script>

<template>
  <!-- Dialog for create/edit -->
  <Dialog v-model:visible="showDialog" :header="dialogMode === FormActionMode.CREATE ? 'Nova línia' : 'Editar línia'" modal>
    <FormYourEntityLine
      v-if="editingLine"
      :model="editingLine"
      :mode="dialogMode"
      @saved="onSaved"
      @cancelled="showDialog = false"
    />
  </Dialog>
</template>
```

**`FormActionMode` enum** (`src/types/component.ts`):
```typescript
export enum FormActionMode {
  CREATE = "CREATE",
  EDIT = "EDIT",
}
```

**Critical**: Always `{ ...line }` clone before passing to dialog. Never mutate store state directly.

---

## 4. Form Validation

**Real example**: `FormBudget.vue`

```vue
<script setup lang="ts">
import * as yup from "yup";
import { FormValidation } from "@/utils/form-validator";
import { globalToast } from "@/utils/global-toast";
import { convertDateTimeToJSON } from "@/utils/functions";

const store = useYourEntityStore();
const model = ref({ ...store.yourEntity! }); // Clone

// Yup schema
const schema = yup.object({
  number: yup.string().required("El número és obligatori"),
  date: yup.mixed().required("La data és obligatòria"),
  amount: yup.number().min(0, "L'import ha de ser positiu"),
});

// Expose submit for parent (Detail view) to call
defineExpose({ submit });

async function submit() {
  const validation = new FormValidation(schema).validate(model.value);
  if (!validation.result) {
    globalToast.warn(Object.values(validation.errors).flat().join("\n"));
    return;
  }

  // Normalize dates before API call
  model.value.date = convertDateTimeToJSON(model.value.date);

  const isNew = !store.yourEntity?.createdOn;
  if (isNew) {
    await store.create(model.value);
  } else {
    await store.update(model.value.id, model.value);
  }
}
</script>

<template>
  <form @submit.prevent="submit">
    <div class="four-columns">  <!-- or three-columns, two-columns -->
      
      <div class="field">
        <label>Número</label>
        <InputText
          v-model="model.number"
          :class="{ 'p-invalid': validationErrors.number }"
          fluid
        />
        <small class="p-error">{{ validationErrors.number }}</small>
      </div>

      <div class="field">
        <label>Data</label>
        <DatePicker
          v-model="model.date"
          date-format="dd/mm/yy"
          fluid
        />
      </div>

      <div class="field">
        <label>Import</label>
        <InputNumber
          v-model="model.amount"
          mode="currency"
          currency="EUR"
          locale="ca-ES"
          fluid
        />
      </div>
      
    </div>
  </form>
</template>
```

**Grid classes** (from `src/assets/styles.scss`):
- `.four-columns` — 4-column grid for dense forms
- `.three-columns` — 3-column grid
- `.two-columns` — 2-column grid

**Validation error display:**
```typescript
// After FormValidation, access errors per field
const validationErrors = ref<Record<string, string[]>>({});

async function submit() {
  const validation = new FormValidation(schema).validate(model.value);
  if (!validation.result) {
    validationErrors.value = validation.errors;
    return;
  }
  validationErrors.value = {};
  // ... proceed
}
```

---

## 5. Nested Table

**File**: `src/modules/<domain>/components/TableYourEntityLines.vue`  
**Real example**: `src/modules/sales/components/TableBudgetDetails.vue`

```vue
<script setup lang="ts">
import { useConfirm } from "primevue/useconfirm";
import { formatCurrency } from "@/utils/functions";

const props = defineProps<{ entityId: string }>();
const emit = defineEmits<{
  (e: "edit", line: YourEntityLine): void;
}>();

const confirm = useConfirm();
const store = useYourEntityStore();

// Lines come from parent entity store
const lines = computed(() => store.yourEntity?.lines ?? []);

function editLine(line: YourEntityLine) {
  emit("edit", line);
}

function confirmDelete(line: YourEntityLine) {
  confirm.require({
    message: `Vols eliminar la línia?`,
    header: "Confirmació",
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      await Services.YourEntityLine.delete(line.id);
      await store.fetchOne(props.entityId); // Re-fetch parent
    },
  });
}
</script>

<template>
  <div>
    <!-- Add button in header slot -->
    <template #header>
      <Button label="Afegir línia" icon="pi pi-plus" @click="$emit('add')" />
    </template>

    <DataTable :value="lines" striped-rows>
      <Column field="description" header="Descripció" />
      <Column field="quantity" header="Quantitat" />
      <Column field="unitPrice" header="Preu unitari">
        <template #body="{ data }">{{ formatCurrency(data.unitPrice) }}</template>
      </Column>
      <Column header="Accions" style="width: 8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" text @click="editLine(data)" />
          <Button icon="pi pi-trash" text severity="danger" @click="confirmDelete(data)" />
        </template>
      </Column>
    </DataTable>
  </div>
</template>
```

---

## 6. Dropdown Selectors

### Lifecycle Dropdown (existing component)

```vue
<DropdownLifecycle
  v-model="model.statusId"
  lifecycle="Budget"  <!-- StatusConstants.Lifecycles name -->
  placeholder="Selecciona estat"
/>
```

Available lifecycles: `Budget`, `SalesOrder`, `SalesInvoice`, `DeliveryNote`, `PurchaseOrder`, `PurchaseInvoice`, `WorkOrder`, etc.

### Custom Dropdown for Foreign Key

```vue
<script setup lang="ts">
const store = useYourRelatedEntityStore();
onMounted(() => store.fetchAll());
</script>

<template>
  <Select
    v-model="model.relatedEntityId"
    :options="store.relatedEntities"
    option-label="name"
    option-value="id"
    placeholder="Selecciona..."
    fluid
  />
</template>
```

---

## 7. Report Download

```typescript
import { ReportService } from "@/services/report.service";
import { globalToast } from "@/utils/global-toast";

const reportService = new ReportService();

async function downloadReport() {
  try {
    await reportService.download(
      "your-report-name",  // Report identifier
      { id: props.id }     // Query params
    );
  } catch {
    globalToast.error("Error en descarregar l'informe");
  }
}
```

---

## Toast Conventions

```typescript
import { globalToast } from "@/utils/global-toast";

globalToast.success("Entitat guardada correctament");
globalToast.warn("Camps obligatoris buits");
globalToast.error("Error en guardar l'entitat");
```

All toast messages in **Catalan**. `globalToast` deduplicates repeated messages automatically.

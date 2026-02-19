# PrimeVue Patterns

Configuraciones comunes de componentes PrimeVue 4 en Lilith ERP.

## DataTable

```vue
<!-- Basic DataTable with pagination, hover, striped -->
<DataTable
  :value="items"
  :rows="20"
  :rows-per-page-options="[10, 20, 50]"
  paginator
  row-hover
  striped-rows
  @row-click="onRowClick"
>
  <Column field="number" header="Número" sortable style="min-width: 8rem" />
  
  <!-- Custom body slot -->
  <Column field="amount" header="Import">
    <template #body="{ data }">{{ formatCurrency(data.amount) }}</template>
  </Column>
  
  <!-- Boolean column -->
  <Column field="active" header="Actiu">
    <template #body="{ data }"><BooleanColumn :value="data.active" /></template>
  </Column>
  
  <!-- Actions column (no header, fixed width) -->
  <Column header="" style="width: 6rem">
    <template #body="{ data }">
      <Button icon="pi pi-pencil" text rounded @click="edit(data)" />
      <Button icon="pi pi-trash" text rounded severity="danger" @click="del(data)" />
    </template>
  </Column>
</DataTable>
```

**Row click navigation:**
```typescript
function onRowClick(row: DataTableRowClickEvent) {
  router.push({ name: "entity", params: { id: row.data.id } });
}
```

## Dialog

```vue
<!-- Controlled with v-model:visible -->
<Dialog
  v-model:visible="showDialog"
  header="Títol del diàleg"
  modal
  style="width: 40rem"
  :draggable="false"
>
  <template #header>
    <span class="font-semibold">Títol personalitzat</span>
  </template>
  
  <!-- Content -->
  
  <template #footer>
    <Button label="Cancel·lar" text @click="showDialog = false" />
    <Button label="Guardar" @click="save" />
  </template>
</Dialog>
```

## SplitButton

```vue
<!-- Primary action + dropdown menu -->
<SplitButton
  label="Guardar"
  icon="pi pi-save"
  :model="menuItems"
  @click="primaryAction"
/>

<!-- menuItems format: -->
const menuItems = [
  { label: "Acceptar", icon: "pi pi-check", command: () => accept() },
  { separator: true },
  { label: "Eliminar", icon: "pi pi-trash", command: () => confirmDelete() },
];
```

## Toast (via globalToast)

```typescript
import { globalToast } from "@/utils/global-toast";

// Severities
globalToast.success("Operació completada");      // green
globalToast.info("Informació addicional");       // blue
globalToast.warn("Atenció: camps obligatoris");  // orange
globalToast.error("Error en l'operació");        // red

// With custom life (ms)
globalToast.success("Guardat", 2000);
```

**Note**: `globalToast` deduplicates messages with same content within 1 second.

## Confirm Dialog

```typescript
import { useConfirm } from "primevue/useconfirm";

const confirm = useConfirm();

// Must have <ConfirmDialog /> in app root (already registered globally)
confirm.require({
  message: "Estàs segur que vols eliminar?",
  header: "Confirmació",
  icon: "pi pi-exclamation-triangle",
  acceptLabel: "Eliminar",
  rejectLabel: "Cancel·lar",
  acceptClass: "p-button-danger",
  accept: async () => { /* do delete */ },
  reject: () => { /* do nothing */ },
});
```

## InputNumber (currency)

```vue
<InputNumber
  v-model="model.amount"
  mode="currency"
  currency="EUR"
  locale="ca-ES"
  :min-fraction-digits="2"
  fluid
/>
```

## DatePicker

```vue
<DatePicker
  v-model="model.date"
  date-format="dd/mm/yy"
  :show-icon="true"
  :class="{ 'p-invalid': errors.date }"
  fluid
/>
```

**Important**: Always normalize before sending to API:
```typescript
model.value.date = convertDateTimeToJSON(model.value.date);
```

## Select (dropdown)

```vue
<!-- Foreign key dropdown -->
<Select
  v-model="model.relatedId"
  :options="options"
  option-label="name"
  option-value="id"
  placeholder="Selecciona una opció"
  :show-clear="true"
  fluid
/>
```

## TabView

```vue
<TabView v-model:active-index="activeTab">
  <TabPanel header="Dades generals">
    <!-- content -->
  </TabPanel>
  <TabPanel header="Línies">
    <!-- content -->
  </TabPanel>
  <TabPanel header="Historial">
    <!-- content -->
  </TabPanel>
</TabView>
```

## BaseInput (project global component)

```vue
<!-- Wraps InputText, InputNumber, Password -->
<BaseInput
  v-model="model.name"
  label="Nom"
  :required="true"
  :error="errors.name"
/>

<!-- Types: text (default), numeric, currency, password -->
<BaseInput type="currency" v-model="model.price" label="Preu" />
```

## Grid Layout Classes

From `src/assets/styles.scss`:

```vue
<!-- 4-column grid (most forms) -->
<div class="four-columns">
  <div class="field">...</div>
  <div class="field">...</div>
</div>

<!-- 3-column grid -->
<div class="three-columns">...</div>

<!-- 2-column grid -->
<div class="two-columns">...</div>

<!-- Field that spans full width -->
<div class="field col-span-all">
  <Textarea v-model="model.observations" rows="3" fluid />
</div>
```

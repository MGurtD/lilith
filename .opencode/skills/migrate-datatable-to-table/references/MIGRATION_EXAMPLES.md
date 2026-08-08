# Ejemplos reales de migración

Ejemplos tomados literalmente del repo. Úsalos como referencia de patrón, no los copies ciegamente — adapta campos/tipos al componente que estés migrando.

## Tabla de contenidos

- [CRUD simple con filtro de texto y borrado](#crud-simple-con-filtro-de-texto-y-borrado)
- [Antes de migrar: patrón típico sin Table.vue](#antes-de-migrar-patr%C3%B3n-t%C3%ADpico-sin-tablevue)
- [Columna Lookup con resolver](#columna-lookup-con-resolver)

## CRUD simple con filtro de texto y borrado

Real: `frontend/src/modules/sales/views/Customers.vue`. Cubre: título vía `store.setMenuItem`, `filterConfig` con un campo de texto, `preset="crud-list"`, `showDeleteColumn`, `page` para vistas guardadas, columna `Lookup`, columna `Boolean`.

```vue
<template>
  <Table
    :columns="customerColumns"
    :items="filteredData"
    :filter-config="customerFilterConfig"
    v-model:filter-values="customerFilter"
    :filter-body-width="customerFilterBodyWidth"
    preset="crud-list"
    page="Customers"
    sort-field="comercialName"
    :sort-order="1"
    showDeleteColumn
    :canDelete="() => true"
    @clear="cleanCustomerFilter"
    @create="createCustomer"
    @delete="deleteCustomer"
    @row-click="editCustomer"
  />
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import type { Column } from "@/components/tables/types";
import { ColumnType } from "@/components/tables/types";
import type { FilterConfig, FilterBodyWidth } from "@/components/tables/TableFilter.vue";

const customerFilterBodyWidth: FilterBodyWidth = { desktop: "33%", tablet: "50%" };

const customerFilterConfig: FilterConfig[] = [
  { key: "code", label: "Nom comercial", type: "text", placeholder: "Nom comercial", size: "md" },
];

const customerColumns = ref<Column[]>([
  { field: "comercialName", header: "Nom comercial", sortable: true, style: "width: 20%" },
  { field: "taxName", header: "Nom Fiscal", style: "width: 20%" },
  {
    field: "customerTypeId",
    header: "Tipus",
    columnType: ColumnType.Lookup,
    resolver: customerStore.getCustomerTypeNameById,
    style: "width: 20%",
  },
  { field: "disabled", header: "Desactivat", sortable: true, columnType: ColumnType.Boolean, style: "width: 20%" },
]);

const customerFilter = ref({ code: "" });

// El filtrado sigue siendo un computed local — Table.vue no impone cómo filtrar `items`,
// solo consume el array final. filterConfig + v-model:filterValues solo pilotan la UI del filtro.
const filteredData = computed(() => {
  if (!customerStore.customers) return [];
  if (customerFilter.value.code.length > 0) {
    return customerStore.customers.filter((r) =>
      r.comercialName.toLowerCase().includes(customerFilter.value.code.toLowerCase()),
    );
  }
  return customerStore.customers;
});

const cleanCustomerFilter = () => { customerFilter.value.code = ""; };
const createCustomer = () => router.push({ path: `/customers/${getNewUuid()}` });
const deleteCustomer = (customer) => {
  confirm.require({
    message: `Está segur que vol eliminar el client ${customer.comercialName}?`,
    accept: async () => {
      const deleted = await customerStore.deleteCustomer(customer.id);
      if (deleted) await customerStore.fetchCustomers();
    },
  });
};
const editCustomer = (row) => router.push({ path: `/customers/${row.data.id}` });

onMounted(async () => {
  await customerStore.fetchCustomers();
  store.setMenuItem({ title: "Clients", icon: PrimeIcons.HASHTAG }); // el título va aquí, no en la tabla
});
</script>
```

Tabla sin filtros reales (solo botón "crear"), en el mismo archivo — nótese `filter-config="[]"`:

```vue
<Table
  :columns="customerTypeColumns"
  :items="customerStore.customerTypes ?? []"
  :filter-config="[]"
  v-model:filter-values="emptyFilter"
  preset="crud-list"
  page="CustomerTypes"
  showDeleteColumn
  :canDelete="() => true"
  @create="createCustomerType"
  @delete="deleteCustomerType"
  @row-click="editCustomerType"
/>
```

## Antes de migrar: patrón típico sin Table.vue

Real (aún no migrado): `frontend/src/modules/warehouse/views/Warehouses.vue`. Este es el tipo de componente candidato — título estático + botón crear + columna booleana + columna de borrado, sin filtros:

```vue
<DataTable :value="warehouseStore.warehouses" scrollable scrollHeight="flex" @row-click="editRow">
  <template #header>
    <div class="flex justify-content-between">
      <span class="text-900 font-bold">Magatzem</span>
      <Button :icon="PrimeIcons.PLUS" rounded raised @click="createButtonClick" />
    </div>
  </template>
  <Column field="name" header="Nom" style="width: 25%"></Column>
  <Column field="description" header="Descripció" style="width: 50%"></Column>
  <Column header="Desactivada" style="width: 20%">
    <template #body="slotProps"><BooleanColumn :value="slotProps.data.disabled" /></template>
  </Column>
  <Column>
    <template #body="slotProps">
      <i :class="PrimeIcons.TIMES" class="grid_delete_column_button" @click="deleteButton($event, slotProps.data)" />
    </template>
  </Column>
</DataTable>
```

Migración directa (aplicando el mismo patrón que `Customers.vue`):

```vue
<Table
  :columns="warehouseColumns"
  :items="warehouseStore.warehouses"
  :filter-config="[]"
  v-model:filter-values="emptyFilter"
  preset="crud-list"
  showDeleteColumn
  :canDelete="() => true"
  @create="createButtonClick"
  @delete="deleteButton"
  @row-click="editRow"
/>
```

```typescript
const warehouseColumns = ref<Column[]>([
  { field: "name", header: "Nom", style: "width: 25%" },
  { field: "description", header: "Descripció", style: "width: 50%" },
  { field: "disabled", header: "Desactivada", columnType: ColumnType.Boolean, style: "width: 20%" },
]);
const emptyFilter = ref({});

onMounted(async () => {
  await warehouseStore.fetchWarehouses();
  store.setMenuItem({ icon: PrimeIcons.BOX, title: "Gestió de magatzems" }); // ya existía, se mantiene
});
```

El `deleteButton` deja de recibir `event` (ya no hace falta `event.currentTarget` para el `confirm.require` con `target`, salvo que se quiera mantener el popup posicionado — en ese caso ajustar la firma del handler y usar el evento nativo del click si Table.vue lo expone via slot, o simplificar a un `confirm.require` sin `target`).

## Columna Lookup con resolver

Cuando una columna original hacía algo como:

```vue
<Column field="customerTypeId" header="Tipus">
  <template #body="slotProps">{{ customerStore.getCustomerTypeNameById(slotProps.data.customerTypeId) }}</template>
</Column>
```

Se convierte en:

```typescript
{ field: "customerTypeId", header: "Tipus", columnType: ColumnType.Lookup, resolver: customerStore.getCustomerTypeNameById }
```

# Matriz de compatibilidad — DataTable (PrimeVue) → Table.vue

Basada en el código real de `frontend/src/components/tables/Table.vue`, `TableFilter.vue`, `TableViewConfig.vue` y `types.ts`, y en un grep exhaustivo de los ~100 usos de `<DataTable>` crudo en `frontend/src`. Si `Table.vue` cambia, vuelve a leer su código fuente antes de confiar ciegamente en esta tabla.

## Tabla de contenidos

- [Soportado — mapeo directo](#soportado--mapeo-directo)
- [Soportado con adaptación de patrón](#soportado-con-adaptaci%C3%B3n-de-patr%C3%B3n)
- [Gaps confirmados (bloquean la migración)](#gaps-confirmados-bloquean-la-migraci%C3%B3n)
- [Cómo detectar cada gap con grep](#c%C3%B3mo-detectar-cada-gap-con-grep)

## Soportado — mapeo directo

| Feature PrimeVue original | Prop/slot en Table.vue | Notas |
|---|---|---|
| `<Column field header sortable style>` | `columns: Column[]` | `field`, `header`, `sortable`, `style` iguales |
| `<Column>` con `<template #body>` formateando fecha | `columnType: ColumnType.Date` / `.DateTime` / `.Time` | Usa `formatDate`/`formatDateTime`/`formatTime` internamente |
| `<Column>` con `<template #body>` formateando moneda | `columnType: ColumnType.Currency` | Usa `formatCurrency` internamente |
| `<Column>` con `<BooleanColumn :value>` | `columnType: ColumnType.Boolean`, opcional `showColor` | Igual componente `BooleanColumn` por debajo |
| `<Column>` resolviendo un id a texto (lookup) | `columnType: ColumnType.Lookup`, `resolver: (id) => string` | |
| `<ProgressColumn>` custom | `columnType: ColumnType.ProgressBar`, `props: { showValue, cap, overrunSeverity, tooltip }` | |
| `<Column>` con `<template #body>` totalmente custom (botones, links, badges) | slot `#body-{field}` en `<Table>`, recibe los mismos `slotProps` | No hay pérdida de flexibilidad |
| Columna con icono de borrar + `confirm.require` | `showDeleteColumn`, `:canDelete="(item) => boolean"`, `@delete="(item) => ..."` | El `confirm.require` se mueve al handler `@delete` del padre |
| Columna de icono de adjuntos + dialog de archivos | `attachmentConfig: { entity, title?, titleField? }` | Reusa `FileService.GetEntityFiles` + `FileViewer` |
| `@row-click` para navegación | `@row-click="(event: DataTableRowClickEvent) => ..."` | Firma idéntica |
| Truncado de celda con tooltip en overflow | Automático por defecto; opt-out con `truncate: false` por columna | No hacía nada especial en el DataTable crudo — es una mejora, no rompe nada |
| Fila de totales por columna | `Column.total: "sum"\|"avg"\|"min"\|"max"\|"count"`, opcional `totalFormat` | Reemplaza un `<template #footer>` que solo mostraba un total simple **si el total es por una sola columna alineada con esa columna**; para un footer combinado/global ver gaps |
| Botón "crear" + inputs de filtro (texto/select/número/checkbox/multiselect) | `filterConfig: FilterConfig[]` + `v-model:filterValues` + `@create` | Ver tipos exactos en `TableFilter.vue` (`FilterConfig`) |
| Título estático en el header de la tabla | Eliminar del template; usar `store.setMenuItem({ title, icon })` en `onMounted` | El título real de la página no vive en el DataTable |
| Filtros con date picker de rango u otro widget no listado en `FilterConfig` | slot `#prepend` / `#append` (reenviados por `<Table>` a `TableFilter`) | Patrón ya usado en el repo — ver comentarios en `TableViewConfig.vue` |
| `paginator`, `:rows`, `scrollable`, `scrollHeight` | Props homónimas, o heredadas de `preset` | `paginator` prop es `boolean | null`; `null` = "usa el preset" |
| `sortField`/`sortOrder`/`sortMode="single"` | Props homónimas | Soportado |
| `sortMode="multiple"` | Pasado vía `v-bind` (attrs) a `resolvedDataTableProps`; usar `sortField`/`sortOrder` para un único sort inicial | `Table.vue` adapta esto a `multiSortMeta` internamente |
| Vistas guardadas por usuario (columnas visibles/orden + sort + filtros persistidos) | Prop `page: string` (opt-in) | Nueva feature respecto al DataTable crudo; requiere backend `UserTableView` ya existente, no requiere cambios de backend |
| Slots nativos `#empty`, `#loading`, `#paginatorstart`, `#paginatorend` | Mismos nombres, reenviados tal cual | |

## Soportado con adaptación de patrón

| Feature original | Qué cambiar |
|---|---|
| `filterDisplay="row"` / `filterDisplay="menu"` + `:globalFilterFields` | Reemplazar por `filterConfig` + `v-model:filterValues` — el propósito (filtrar filas) es el mismo, el mecanismo cambia |
| Filtro de búsqueda libre calculado con un `computed()` local sobre `props.items` | Puede mantenerse igual (alimentando `items` con el array ya filtrado) O migrarse a `filterConfig` tipo `"text"` — evaluar caso a caso, ninguna de las dos es un gap |
| `class="p-datatable-sm"` u otras clases de densidad | Pasan igual vía atributos no declarados (`v-bind="resolvedDataTableProps"` incluye `attrs`) |

## Gaps confirmados (bloquean la migración)

Si el archivo auditado usa cualquiera de estas features, el veredicto es **Bloqueado**. No implementar workarounds — redactar el issue con [ISSUE_TEMPLATE.md](ISSUE_TEMPLATE.md).

| Gap | Por qué está bloqueado hoy | Archivos reales afectados (a fecha de esta auditoría) |
|---|---|---|
| Selección de filas (`v-model:selection`, `selectionMode="multiple"` en una columna) | `Table.vue` no expone `selection` como v-model ni genera una columna de checkbox; solo pasa `selectionMode` como string a `resolvedDataTableProps` sin cablear el binding | `WorkOrderLoader.vue`, `WorkcenterDocumentation.vue`, `PhaseTemplateLoader.vue`, `PurchaseInvoicesByDates.vue`, `SalesInvoicesByDates.vue`, `SelectorReceipts.vue`, `SelectorOrdersDetailsToReceipt.vue`, `ProfileMenuAssignment.vue`, `SelectorOrders.vue`, `SelectorOrderDetails.vue`, `SelectorDeliveryNotes.vue`, `WorkcenterWorkOrderSelector.vue` (`selectionKeys`), `MenuItems.vue` (`selectionKeys`) |
| Expansión de filas (`v-model:expandedRows` de dos vías + `<template #expansion>`) | `Table.vue` solo acepta `expandedRows` como prop de una vía (`any[] | null`), sin slot `#expansion` ni eventos `row-expand`/`row-collapse` | `TableOrderDetails.vue`, `SelectorOrdersDetailsToReceipt.vue` |
| Footer global no ligado a una columna (`<template #footer>` con un div/resumen que no es un total por columna) | `Table.vue` solo soporta `footer-{field}` por columna dentro de un `ColumnGroup`; no hay slot de footer de toda la tabla | `TableSalesOrderDetails.vue` (`.total-footer`), `Expenses.vue` (`.expenses-footer-total`) |
| Paginación server-side (`:lazy`, `:totalRecords`, `@page`) | `Table.vue` calcula `paginator`/`rows` sobre `props.items.length` (array completo en cliente); no hay soporte de lazy loading | Ninguno detectado en uso actual, pero vigilar si aparece en tablas de gran volumen |
| `contextMenu` / `@row-contextmenu` | No reenviado por `Table.vue` | Ninguno detectado en uso actual |
| Edición de celda (`editMode`, `cellEditComplete`, `<template #editor>`) | No reenviado por `Table.vue`; el slot `#body-{field}` es de solo lectura visual | Ninguno detectado en uso actual |
| Columnas congeladas (`frozen`) | No reenviado como concepto de columna individual (solo pasa attrs genéricos al `<DataTable>`, no a `<Column>` por columna) | `CustomerSalesRankingDashboard.vue` (dashboard analítico, caso aislado) |

## Cómo detectar cada gap con grep

Ejecuta estos patrones sobre el archivo objetivo antes de dar el veredicto (usa la herramienta Grep, no leas el archivo entero de nuevo si ya lo hiciste en el paso 2):

```
v-model:selection|v-model:selectionKeys|selectionMode="multiple"
v-model:expandedRows|#expansion|@row-expand|@row-collapse
<template #footer>            (revisar si es un total por columna o un resumen global)
:lazy=|:totalRecords|@page=
contextMenu|@row-contextmenu
editMode|cellEditComplete|#editor
frozen
```

Si algún patrón aparece, confirma manualmente que es exactamente ese uso (no un falso positivo, ej. `frozen` como nombre de variable no relacionado) antes de bloquear.

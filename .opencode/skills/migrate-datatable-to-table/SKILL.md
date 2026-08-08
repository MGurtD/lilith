---
name: migrate-datatable-to-table
description: Migra una vista o componente Vue que usa `<DataTable>` de PrimeVue directamente al componente base interno `Table.vue` (con `TableFilter.vue` y `TableViewConfig.vue`) del frontend de Lilith ERP. Use when (1) el usuario pide migrar/convertir/actualizar una tabla o vista concreta a `Table.vue`, (2) auditar si un componente con DataTable es candidato viable para el componente base, (3) detectar que una tabla necesita una feature no soportada por `Table.vue` y redactar el plan de un issue de GitHub para cubrirla en otra sesión, (4) revisar de forma sistemática columnas, filtros, selección, expansión, footer y paginación de una tabla antes de tocarla.
---

# Migrar DataTable a Table.vue

Workflow para migrar UN componente/vista Vue por invocación desde `<DataTable>` de PrimeVue crudo al componente base `frontend/src/components/tables/Table.vue`. No se ejecuta en modo batch: el usuario indica el archivo objetivo.

**Regla de oro**: si el análisis detecta una feature en uso que `Table.vue` no soporta (ver lista de gaps abajo), la migración se **bloquea por completo** — no se toca el código del componente. En su lugar se redacta el contenido de un issue de GitHub (nunca se ejecuta `gh issue create` automáticamente).

Ver también:
- [references/FEATURE_MATRIX.md](references/FEATURE_MATRIX.md) — matriz de compatibilidad completa (features soportadas, gaps confirmados, cómo detectarlas).
- [references/MIGRATION_EXAMPLES.md](references/MIGRATION_EXAMPLES.md) — ejemplos reales before/after ya migrados en el repo.
- [references/ISSUE_TEMPLATE.md](references/ISSUE_TEMPLATE.md) — plantilla para redactar el plan de una feature faltante.

## Workflow

### 1. Identificar el objetivo

Confirma la ruta exacta del archivo `.vue` a migrar. Si el usuario da un nombre ambiguo, usa `codegraph_explore` o `glob` para localizarlo antes de continuar. Trabaja sobre un único archivo por sesión.

### 2. Auditoría (leer el archivo completo)

Lee el componente entero (`<template>` + `<script setup>`) y construye un inventario explícito de:

- **Columnas**: `field`, `header`, tipo implícito (texto/booleano/fecha/moneda/lookup/progreso), si tienen `<template #body>` custom, si son `sortable`, anchos (`style="width: X%"`).
- **Header**: título estático, botón "crear", inputs de filtro (texto/select/multiselect/checkbox/número), date pickers de rango.
- **Footer**: `<template #footer>` global vs. totales visuales por columna.
- **Selección de filas**: `v-model:selection`, `selectionMode`, columnas con `selectionMode="multiple"`.
- **Expansión de filas**: `v-model:expandedRows`, `<template #expansion>`.
- **Acciones por fila**: borrar (icono + `confirm.require`), editar, navegación en `@row-click`.
- **Paginación/scroll**: `paginator`, `:rows`, `scrollable`, `scrollHeight`, `:lazy`, `totalRecords`, `@page`.
- **Sort**: `sortField`/`sortOrder`/`sortMode`.
- **Otras features nativas de PrimeVue**: `contextMenu`, `editMode`/`cellEditComplete`, `filterDisplay`/`globalFilterFields`, `frozen`, `reorderableColumns`, `virtualScroller` sobre las filas del propio DataTable (no confundir con `virtualScrollerOptions` de un `Select`/`MultiSelect`, que es irrelevante aquí).
- **Adjuntos**: columna de icono de clip que abre un visor de archivos por entidad.

### 3. Match contra la matriz de compatibilidad

Usa [references/FEATURE_MATRIX.md](references/FEATURE_MATRIX.md) para clasificar cada elemento del inventario como **Soportado** (mapeo directo), **Soportado con adaptación** (mismo resultado, patrón distinto) o **Gap confirmado** (no existe hoy en `Table.vue`/`TableFilter.vue`/`TableViewConfig.vue`).

Presenta esta tabla al usuario antes de decidir el veredicto:

| Feature detectada | Uso actual en el archivo | Soporte en Table.vue | Acción |
|---|---|---|---|

### 4. Veredicto

- **Viable**: todo mapea directo → ir a paso 5a.
- **Viable con adaptación**: hay diferencias de patrón pero ningún gap confirmado (ej. `globalFilterFields` a reemplazar por `filterConfig`, título estático a eliminar porque ya lo pone `store.setMenuItem()`) → ir a paso 5a.
- **Bloqueado**: al menos un gap confirmado en uso (selección con checkbox, expansión de filas, footer global no-por-columna, paginación lazy/server-side, context menu, cell editing, frozen columns) → ir a paso 5b. **No migres parcialmente ni improvises un workaround.**

### 5a. Ejecutar la migración (Viable / Viable con adaptación)

1. Define `columns: Column[]` (importar `Column`, `ColumnType` de `@/components/tables/types`) reemplazando cada `<Column>` manual. Usa `columnType` en vez de reimplementar formato de fecha/moneda/booleano/lookup a mano.
2. Slots custom que no encajen en ningún `ColumnType` se preservan tal cual con `#body-{field}` en el `<Table>` (Table.vue reenvía `slotProps` igual que PrimeVue).
3. Header:
   - Título estático → eliminar del template; confirmar que `store.setMenuItem({ title, icon })` ya existe en `onMounted` (si no existe, añadirlo, no inventar un título nuevo en el header de la tabla).
   - Botón "crear" → `filterConfig` (usar `[]` si no hay campos de filtro reales) + `@create`.
   - Inputs de filtro estándar (texto/select/número/checkbox/multiselect) → `FilterConfig[]` + `v-model:filterValues`.
   - Date pickers de rango u otros widgets no cubiertos por `FilterConfig` → slot `#prepend` (patrón ya usado en el repo, ver TableViewConfig.vue).
4. Elegir `preset` (`crud-list`, `read-only`, `detail-lines`, `selector`) según el propósito de la tabla; solo sobreescribir props concretas (`rows`, `scrollHeight`, etc.) si el original difiere del preset.
5. Borrado → `showDeleteColumn` + `:canDelete` + `@delete`, moviendo el `confirm.require(...)` al handler del evento `delete` en el padre.
6. Adjuntos → `attachmentConfig` si el componente tenía una columna de clip.
7. Totales por columna → `Column.total` (+ `totalFormat` si el formato no es el default).
8. `page` (persistencia de vista por usuario): es **opt-in**. Pregunta al usuario si quiere activarla; si el módulo ya tiene vistas hermanas migradas con `page`, sigue esa misma convención de nombre (normalmente el nombre de la ruta/entidad en PascalCase, ej. `"Customers"`).
9. Elimina imports que queden muertos (`Column`, `Row`, `ColumnGroup`, `DataTableRowClickEvent` si ya no se usa el tipo nativo, etc.) y añade `import Table from "@/components/tables/Table.vue"` + tipos de `TableFilter.vue`/`types.ts` según haga falta.

### 5b. Bloqueado — redactar el issue (no tocar código)

Usa [references/ISSUE_TEMPLATE.md](references/ISSUE_TEMPLATE.md) para redactar (no crear) el contenido completo del issue: título, problema, diseño de API propuesto para `Table.vue`/`TableFilter.vue`/`TableViewConfig.vue`, archivos afectados (incluye el componente auditado y cualquier otro archivo del repo que comparta el mismo gap, detectable con grep), criterios de aceptación. Entrégalo al usuario en la respuesta; no ejecutes `gh issue create` salvo que el usuario lo pida explícitamente en ese momento.

### 6. Verificación

Tras migrar (paso 5a), exige siempre:

```bash
cd frontend
pnpm run typecheck   # debe pasar con 0 errores
```

No hay tests automatizados en frontend. Entrega al usuario este checklist de verificación manual en `pnpm run dev` (localhost:8100) antes de dar la migración por terminada:

- [ ] Los filtros aplican y limpian (`Filtrar` / `Netejar`) igual que antes.
- [ ] El ordenamiento por columna funciona en las mismas columnas que antes.
- [ ] Paginación/scroll se comporta igual (mismo número de filas visibles, mismo scroll).
- [ ] El click de fila navega/emite igual que antes.
- [ ] Borrado + confirmación funciona y el icono solo aparece cuando `canDelete` es cierto.
- [ ] Los slots custom (badges, botones, links) se ven idénticos.
- [ ] Los totales de columna (si había) coinciden con los valores previos.
- [ ] La columna de adjuntos (si aplica) abre el visor correctamente.
- [ ] Si se activó `page`: guardar/cambiar columnas visibles y filtros persiste al recargar la página.

## Notas importantes

- `Table.vue` no expone hoy `v-model:selection`, `v-model:expandedRows`/`#expansion`, footer global (`#footer` de toda la tabla), paginación `:lazy`/`totalRecords`/`@page`, `contextMenu`, `editMode` de celda, ni `frozen` columns. Confirmar en el propio código de `Table.vue` antes de asumir que se ha añadido soporte nuevo, porque este archivo puede quedar desactualizado si el componente evoluciona.
- `filterDisplay`/`globalFilterFields` nativos de PrimeVue no son un gap: se reemplazan por `filterConfig` + `v-model:filterValues`.
- El título visual de página va por `store.setMenuItem()` (barra superior de la app), nunca por el header interno de la tabla — no intentes replicarlo dentro de `Table.vue`.

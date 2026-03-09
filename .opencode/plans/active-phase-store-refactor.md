# Plan: Separar ActivePhase Store + Pestaña Materiales

## Resumen
Extraer la gestión de la fase activa del `workcenter.store.ts` a un nuevo `activePhase.store.ts`, y crear una nueva pestaña "Materials" en `WorkcenterDetail.vue` para visualizar los materiales (BOM) de la fase cargada.

## Archivos a crear

### 1. `frontend/src/modules/plant/store/activePhase.store.ts`
Nuevo store que encapsula toda la lógica de la fase activa.

**Estado:**
- `workOrderReferenceDocuments: File[]`
- `nextAvailablePhase: NextPhaseInfo | null`
- `phaseTimeMetrics: PhaseTimeMetrics | undefined`
- `billOfMaterials: BillOfMaterialsItem[]` (NUEVO)

**Getters:**
- `activeWorkOrder` → lee `workcenterStore.loadedWorkOrdersPhases[0]`
- `activePhase` → lee `activeWorkOrder.phases[0]`
- `hasBillOfMaterials` → `billOfMaterials.length > 0`

**Acciones migradas del workcenter.store:**
- `syncWithLoadedPhases()` — orquesta la carga de docs, métricas y BOM
- `fetchWorkInstructionDocuments(referenceId)`
- `fetchPhaseTimeMetrics()`
- `fetchBillOfMaterials()` — NUEVA: usa `GetWorkOrderPhasesDetailed(workOrderId)` y filtra por fase activa para obtener `BillOfMaterialsItem[]`
- `fetchNextPhaseForWorkcenter()`
- `getPhaseExitStatusId(closePhase)`
- `validatePhaseQuantity(quantity)`
- `updatePhaseComment(phaseId, comment)` — tras update, llama `workcenterStore.refreshLoadedWorkOrders()`
- `updatePhaseQuantities(counterOk, counterKo)` — tras update, llama `workcenterStore.refreshLoadedWorkOrders()`
- `clearActivePhase()` — limpia todo el estado

### 2. `frontend/src/modules/plant/components/workcenter-detail/WorkcenterMaterials.vue`
Componente read-only con tabla PrimeVue DataTable.

**Columnas:**
- Referència (referenceCode)
- Descripció (referenceDescription)
- Quantitat (quantity)

**Props:** ninguna (lee directamente del `usePlantActivePhaseStore`)

## Archivos a modificar

### 3. `frontend/src/modules/plant/store/workcenter.store.ts`
**Eliminar:**
- Estado: `workOrderReferenceDocuments`, `nextAvailablePhase`, `phaseTimeMetrics`
- Acciones: `fetchWorkInstructionDocuments`, `fetchPhaseTimeMetrics`, `fetchNextPhaseForWorkcenter`, `getPhaseExitStatusId`, `validatePhaseQuantity`, `updatePhaseComment`, `updatePhaseQuantities`

**Modificar:**
- `fetchLoadedWorkOrders()` → delegar a `activePhaseStore.syncWithLoadedPhases()` en vez de llamar directamente a docs/metrics
- `connectToWorkcenter()` → en el callback de statusChanged, llamar `activePhaseStore.fetchPhaseTimeMetrics()` en vez de `this.fetchPhaseTimeMetrics()`
- `clearWorkcenter()` → llamar `activePhaseStore.clearActivePhase()` además de limpiar su propio estado

**Añadir:**
- `refreshLoadedWorkOrders()` — nueva acción pública que re-fetch usando `_lastLoadedPhaseIds` (para que activePhase store pueda forzar refresh tras update de cantidades/comentarios)

### 4. `frontend/src/modules/plant/store/index.ts`
Añadir export: `export * from "./activePhase.store";`

### 5. `frontend/src/modules/plant/views/WorkcenterDetail.vue`
**Importar:** `usePlantActivePhaseStore` del barrel
**Pestaña nueva "Materials":**
- Tab con `v-if="activePhaseStore.hasBillOfMaterials"` y `PrimeIcons.BOX`
- TabPanel con `<WorkcenterMaterials />`

**Cambios en tabs existentes:**
- Las refs a `workcenterStore.loadedWorkOrdersPhases` en la template siguen iguales (el workcenter store mantiene esa data)
- Tab "Documentació" sigue igual

### 6. Componentes que migran al activePhase store

| Componente | Antes (workcenterStore) | Después (activePhaseStore) |
|---|---|---|
| **WorkOrderUnloader.vue** | `nextAvailablePhase`, `fetchNextPhaseForWorkcenter`, `validatePhaseQuantity`, `getPhaseExitStatusId` | Mismas propiedades/acciones desde `usePlantActivePhaseStore` |
| **WorkOrderPhaseQuantities.vue** | `validatePhaseQuantity`, `updatePhaseQuantities` | Mismas acciones desde `usePlantActivePhaseStore` |
| **WorkcenterRealtimePanel.vue** | `phaseTimeMetrics` | Leer desde `usePlantActivePhaseStore` |
| **WorkcenterDocumentation.vue** | `workOrderReferenceDocuments` | Leer desde `usePlantActivePhaseStore` |
| **WorkcenterCommentEditor.vue** | `updatePhaseComment` | Llamar desde `usePlantActivePhaseStore` |

## Estrategia BOM
- Fetch separado: llamar `ProductionServices.WorkOrderPhase.GetWorkOrderPhasesDetailed(workOrderId)` cuando se carga una fase
- Filtrar la fase activa por `phaseId` para extraer su `billOfMaterials: BillOfMaterialsItem[]`
- Sin cambios en backend necesarios

## Orden de ejecución
1. Crear `activePhase.store.ts`
2. Simplificar `workcenter.store.ts` (eliminar lo migrado + añadir `refreshLoadedWorkOrders` + delegar a activePhase)
3. Actualizar `store/index.ts`
4. Crear `WorkcenterMaterials.vue`
5. Actualizar `WorkcenterDetail.vue` (nuevo store + nueva tab)
6. Actualizar 5 componentes consumidores
7. `pnpm run typecheck`

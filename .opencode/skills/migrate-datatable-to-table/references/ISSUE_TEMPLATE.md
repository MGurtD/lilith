# Plantilla de issue — feature faltante en Table.vue

Rellena esta plantilla cuando el veredicto de la auditoría sea **Bloqueado**. Entrega el resultado en la respuesta al usuario en formato Markdown listo para copiar a GitHub. No ejecutes `gh issue create` salvo petición explícita del usuario en ese momento.

---

## Título

`[Table.vue] Soporte para <feature> (bloquea migración de <ComponenteAuditado>.vue)`

## Problema

Describe en 2-4 frases:
- Qué hace hoy el componente auditado con `<DataTable>` crudo que `Table.vue` no reproduce.
- Por qué no se puede migrar sin esta feature (qué se perdería funcionalmente).

## Componentes afectados

Lista el componente auditado + cualquier otro archivo del repo que use el mismo gap (búscalo con los patrones de grep de `FEATURE_MATRIX.md` antes de escribir el issue, para dimensionar el impacto real):

- `frontend/src/modules/.../Componente.vue` (auditado en esta sesión)
- ... otros archivos con el mismo gap, si los hay

## Diseño de API propuesto

Propuesta concreta de props/eventos/slots nuevos en `Table.vue` (y `TableFilter.vue`/`TableViewConfig.vue` si aplica), consistente con las convenciones ya existentes del componente (props `withDefaults`, emits tipados, slots reenviados con `useSlots()`). Ejemplo de formato:

```typescript
// Nuevas props en Table.vue
selection?: any | any[] | null;
selectionMode?: "single" | "multiple" | "checkbox";

// Nuevo emit
(e: "update:selection", value: any | any[]): void;
```

```vue
<!-- Nuevo uso esperado por el consumidor -->
<Table
  v-model:selection="selectedRows"
  selectionMode="checkbox"
  ...
/>
```

Indica también qué debe pasar con features ya existentes (presets, vistas guardadas, etc.) para no romperlas.

## Fuera de alcance

Deja explícito qué NO cubre este issue (para no mezclar varios gaps en uno solo si la auditoría detectó más de uno — preferir un issue por gap).

## Criterios de aceptación

- [ ] `Table.vue` soporta `<feature>` con la API propuesta.
- [ ] `pnpm run typecheck` pasa sin errores.
- [ ] El componente `<ComponenteAuditado>.vue` puede migrarse completamente a `Table.vue` sin perder funcionalidad, usando esta skill (`migrate-datatable-to-table`) en una sesión posterior.
- [ ] Se añade el mapeo correspondiente a `references/FEATURE_MATRIX.md` de esta skill (mover el gap de la sección "Gaps confirmados" a "Soportado").

## Notas técnicas adicionales

Cualquier detalle relevante encontrado durante la auditoría (ej. estructura de datos específica, casos borde, interacción con `TableViewConfig.vue` o con las vistas guardadas por `page`).

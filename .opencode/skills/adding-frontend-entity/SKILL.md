---
name: adding-frontend-entity
description: Complete 10-step workflow for adding new entity CRUD to the Vue 3 frontend of Lilith ERP. Use when (1) Creating new views with DataTable listings for an entity, (2) Adding services extending BaseService<T>, (3) Creating Pinia stores for new entities, (4) Building forms with Yup validation and PrimeVue components, (5) Adding lazy-loaded routes to module router, (6) Synchronizing TypeScript types with C# backend entities, (7) Troubleshooting "module not found", "route not registered", "store not defined", or TypeScript type errors after adding a new entity.
---

# Adding Frontend Entity

Complete 10-step workflow for adding CRUD to the Vue 3 frontend.

**Estimated time**: 45-60 minutes  
**Verification**: `pnpm run typecheck` must pass after all steps

## Workflow Overview

```
types/index.ts → service → store → List view → Detail view → Form component → routes.ts → router.ts
```

## Checklist

```
- [ ] 1. Define TypeScript interface in types/index.ts          ⏱ 5 min
- [ ] 2. Create service extending BaseService<T>                ⏱ 5 min
- [ ] 3. Register service in module index.ts                    ⏱ 2 min
- [ ] 4. Create Pinia store with CRUD actions                   ⏱ 10 min
- [ ] 5. Create List view (DataTable + filters)                 ⏱ 15 min
- [ ] 6. Create Detail view (SplitButton + Form + Tabs)         ⏱ 15 min
- [ ] 7. Create Form component (Yup validation)                 ⏱ 20 min
- [ ] 8. Create Table component (for nested entities, optional) ⏱ 10 min
- [ ] 9. Add routes in module routes.ts                         ⏱ 3 min
- [ ] 10. Register routes in src/router.ts                      ⏱ 2 min

Total: ~87 minutes (with optional Table component)
```

**Verification checkpoint**: After step 10, run `pnpm run typecheck` — must pass with 0 errors.

## Before You Start

Find an analogous entity to use as a reference. Most patterns are identical:
- **Sales module reference**: `Budget` (has lifecycle status, lines, complex form)
- **Production module reference**: `WorkOrder` (complex, multiple tabs)
- **Simple entity reference**: Any entity in `shared/` module

## Detailed Steps

## 1. Define TypeScript Interface

Location: `src/modules/<domain>/types/index.ts`

```typescript
export interface YourEntity {
  id: string;               // Guid → string
  number: string;
  date: any;                // DateTime → any (normalize with convertDateTimeToJSON)
  description: string;
  amount: number;           // decimal/int → number
  disabled: boolean;
  createdOn: any;
  updatedOn: any;
  
  // Foreign keys
  statusId: string | null;
  status: Status | null;
  
  // Collections (nested entities)
  lines: YourEntityLine[];
}

export interface YourEntityLine {
  id: string;
  yourEntityId: string;
  quantity: number;
  unitPrice: number;
  description: string;
}
```

**C# → TypeScript type conversion:**
| C# | TypeScript |
|----|-----------|
| `Guid` | `string` |
| `DateTime` / `DateTime?` | `any` |
| `decimal` / `int` / `double` | `number` |
| `bool` | `boolean` |
| `string` | `string` |
| `ICollection<T>` | `Array<T>` |
| `T?` (nullable) | `T \| null` |
| Property `Name` (PascalCase) | `name` (camelCase) |

**Key rules:**
- Numeric defaults: `0` (never `undefined`)
- UUID fields: `string` type
- Date fields: `any` (always normalize before API calls)
- Navigation properties: nullable (`T | null`)

## 2. Create Service

Location: `src/modules/<domain>/services/<entity>.service.ts`

```typescript
import { BaseService } from "@/api/base.service";
import type { YourEntity } from "../types";

export class YourEntityService extends BaseService<YourEntity> {
  constructor() {
    super("yourentity"); // Must match backend route: api/yourentity
  }

  // Add custom methods if needed
  async getByStatus(statusId: string): Promise<YourEntity[] | null> {
    return this.getCustom(`bystatus/${statusId}`);
  }

  async getBetweenDates(from: string, to: string): Promise<YourEntity[] | null> {
    return this.getCustom(`betweendates?from=${from}&to=${to}`);
  }
}
```

**BaseService<T> provides (inherited):**
- `getAll(): Promise<T[] | null>`
- `getById(id: string): Promise<T | null>`
- `create(entity: T): Promise<boolean>`
- `update(id: string, entity: T): Promise<boolean>`
- `delete(id: string): Promise<boolean>`

**Only add custom methods** if the backend has non-standard endpoints.

## 3. Register Service in Module Index

Location: `src/modules/<domain>/services/index.ts`

```typescript
import { YourEntityService } from "./yourentity.service";

// Add to existing exports
export const Services = {
  // ... existing services
  YourEntity: new YourEntityService(),
};
```

If the module doesn't have a services `index.ts`, check how other services are imported and follow the same pattern.

## 4. Create Pinia Store

Location: `src/modules/<domain>/store/<entity>.ts`

```typescript
import { defineStore } from "pinia";
import { Services } from "../services";
import type { YourEntity } from "../types";
import { getNewUuid } from "@/utils/functions";

export const useYourEntityStore = defineStore("yourEntity", {
  state: () => ({
    yourEntities: [] as YourEntity[],
    yourEntity: null as YourEntity | null,
  }),

  actions: {
    async fetchAll() {
      const result = await Services.YourEntity.getAll();
      if (result) this.yourEntities = result;
    },

    async fetchOne(id: string) {
      const result = await Services.YourEntity.getById(id);
      if (result) this.yourEntity = result;
    },

    setNew(id?: string) {
      this.yourEntity = {
        id: id ?? getNewUuid(),
        number: "",
        date: null,
        description: "",
        amount: 0,
        disabled: false,
        createdOn: null,
        updatedOn: null,
        statusId: null,
        status: null,
        lines: [],
      };
    },

    async create(model: YourEntity) {
      const result = await Services.YourEntity.create(model);
      if (result) await this.fetchOne(model.id);
      return result;
    },

    async update(id: string, model: YourEntity) {
      const result = await Services.YourEntity.update(id, model);
      if (result) await this.fetchOne(id);
      return result;
    },

    async remove(id: string) {
      return await Services.YourEntity.delete(id);
    },
  },
});
```

**Key rules:**
- After create/update: always `fetchOne()` to sync UI with DB state
- `setNew()` initializes blank entity for creation dialog/form
- Use `getNewUuid()` for client-generated IDs
- Numeric defaults: `0`, not `null` or `undefined`

## 5. Create List View

Location: `src/modules/<domain>/views/YourEntities.vue`

See [references/COMPONENT_TEMPLATES.md](references/COMPONENT_TEMPLATES.md#list-view) for the complete template.

**Structure summary:**
```
<template>
  <!-- Filters row: ExerciseDatePicker + dropdowns + search + clear + create button -->
  <!-- DataTable with row-click navigation -->
  <!-- Creation Dialog (if entities are created inline) -->
</template>
```

**Key patterns:**
- Import store: `const store = useYourEntityStore()`
- Load on mount: `onMounted(() => store.fetchAll())`
- Row click: `router.push({ name: 'yourentity', params: { id: row.id } })`
- Filter persistence: `useUserFilterStore()`

## 6. Create Detail View

Location: `src/modules/<domain>/views/YourEntity.vue`

See [references/COMPONENT_TEMPLATES.md](references/COMPONENT_TEMPLATES.md#detail-view) for the complete template.

**Structure summary:**
```
<template>
  <!-- SplitButton: Save (default) + additional actions dropdown -->
  <!-- TabView: Tab 1 = FormYourEntity | Tab 2 = TableYourEntityLines | ... -->
</template>
```

**Key patterns:**
- Props: `defineProps<{ id: string }>()`
- Load on mount: `store.fetchOne(props.id)`
- Save via form ref: `formRef.value?.submit()`
- SplitButton items for workflow actions (change status, delete, etc.)

## 7. Create Form Component

Location: `src/modules/<domain>/components/FormYourEntity.vue`

See [references/COMPONENT_TEMPLATES.md](references/COMPONENT_TEMPLATES.md#form-component) for the complete template.

**Key patterns:**
```typescript
// Expose submit function for parent to call
defineExpose({ submit });

// Clone before editing (prevent premature state mutation)
const model = ref({ ...store.yourEntity! });

// Validate with Yup before save
async function submit() {
  const validation = new FormValidation(schema).validate(model.value);
  if (!validation.result) { /* show toast */ return; }
  
  // Normalize dates before API call
  model.value.date = convertDateTimeToJSON(model.value.date);
  
  await store.update(model.value.id, model.value);
}
```

## 8. Create Table Component (optional, for nested entities)

Location: `src/modules/<domain>/components/TableYourEntityLines.vue`

Only needed if the entity has nested detail lines (e.g., BudgetLines, SalesOrderLines).

See [references/COMPONENT_TEMPLATES.md](references/COMPONENT_TEMPLATES.md#table-component) for the complete template.

**Key patterns:**
- Emit `edit` and `delete` events to parent
- Use `useConfirm()` for delete confirmation dialog
- Use `formatCurrency()` for money columns

## 9. Add Routes in Module Router

Location: `src/modules/<domain>/routes.ts`

```typescript
import type { RouteRecordRaw } from "vue-router";

export default [
  // ... existing routes
  {
    path: "/yourentity",
    name: "YourEntities",
    component: () => import("./views/YourEntities.vue"), // lazy-load
  },
  {
    path: "/yourentity/:id",
    name: "yourentity",
    component: () => import("./views/YourEntity.vue"),
    props: true,
  },
] as Array<RouteRecordRaw>;
```

**Rules:**
- ALWAYS lazy-load: `() => import(...)`
- List route: PascalCase plural name (`YourEntities`)
- Detail route: camelCase singular name (`yourentity`)
- Detail route: `props: true` to receive `:id` as prop

## 10. Register Routes in Central Router

Location: `src/router.ts`

```typescript
import yourEntityRoutes from "@/modules/<domain>/routes";

// Add inside createRouter({ routes: [...] })
...yourEntityRoutes,
```

If the module routes are already spread into the router (e.g., `...salesRoutes`), the new routes are automatically included — no changes needed here.

## Common Pitfalls

### ❌ Forgetting convertDateTimeToJSON before API calls

```typescript
// ❌ BAD: Sending raw Date object
await store.update(model.id, model.value);

// ✅ GOOD: Normalize dates first
model.value.date = convertDateTimeToJSON(model.value.date);
model.value.deliveryDate = convertDateTimeToJSON(model.value.deliveryDate);
await store.update(model.id, model.value);
```

### ❌ Mutating store state directly in component

```typescript
// ❌ BAD: Direct mutation
store.yourEntity!.name = "new name";

// ✅ GOOD: Clone and work with local copy
const model = ref({ ...store.yourEntity! });
```

### ❌ Not re-fetching after nested entity changes

```typescript
// ❌ BAD: Only updating lines, header state stale
await Services.YourEntityLine.create(line);

// ✅ GOOD: Re-fetch entire entity to sync all state
await Services.YourEntityLine.create(line);
await store.fetchOne(props.id);
```

### ❌ Using non-lazy route imports

```typescript
// ❌ BAD: Eager load (breaks code splitting)
import YourEntities from "./views/YourEntities.vue";
component: YourEntities

// ✅ GOOD: Lazy load
component: () => import("./views/YourEntities.vue")
```

### ❌ Missing `props: true` on detail route

```typescript
// ❌ BAD: id not available as prop
{ path: "/yourentity/:id", component: ... }

// ✅ GOOD: id available as prop
{ path: "/yourentity/:id", props: true, component: ... }
```

## Verification

After completing all 10 steps:

```bash
cd frontend
pnpm run typecheck  # Must pass with 0 errors
```

Common TypeScript errors after adding entity:
- `Property 'x' does not exist on type 'Y'` → Check interface definition in types/index.ts
- `Cannot find module '...'` → Check import paths use `@/` alias
- `Type 'null' is not assignable` → Add `| null` to the type or initialize with correct default

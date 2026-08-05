# AGENTS.md - Lilith Frontend

Guidelines for AI coding agents working in this Vue 3 ERP frontend codebase.

## Quick Reference

| Item            | Value                                                |
| --------------- | ---------------------------------------------------- |
| Stack           | Vue 3 + TypeScript + Vite + Pinia + PrimeVue + Axios |
| Package Manager | pnpm v10+ (required)                                 |
| Dev Server      | `pnpm run dev` (port 8100)                           |
| Type Check      | `pnpm run typecheck`                                 |
| Build           | `pnpm run build`                                     |
| Tests           | Not configured                                       |
| Linting         | Not configured (Prettier installed)                  |

## Build & Development Commands

```bash
pnpm install              # Install dependencies (use pnpm, not npm/yarn)
pnpm run dev              # Start dev server at http://localhost:8100
pnpm run typecheck        # Type check without building (vue-tsc --noEmit)
pnpm run build            # Production build: typecheck + vite build to dist/
pnpm run build-development # Dev mode build to dist-test/
pnpm run preview          # Preview production build
```

**No test framework configured** - there are no test scripts or test files in this project.

## Project Structure

```
src/
  api/                 # Axios clients & BaseService<T>
  components/          # Global reusable components
  modules/             # Domain modules (sales, production, warehouse, etc.)
    <domain>/
      routes.ts        # RouteRecordRaw[] with lazy-loaded views
      components/      # Domain components
      services/        # API services extending BaseService<T>
      store/           # Pinia stores
      types/           # TypeScript interfaces
      views/           # Page components
  services/            # Global services (user, auth, reports)
  store/               # Global Pinia stores
  types/               # Shared TypeScript interfaces
  utils/               # Utility functions
  views/               # Top-level pages
```

## Code Style Guidelines

### TypeScript

- **Strict mode enabled** - no implicit `any`
- Use explicit types over inference for function parameters and return types
- Prefer `Type | undefined` over optional chaining for nullable types
- Avoid `any` - use `unknown` if type is truly unknown
- Path alias: `@/` maps to `./src/`

### Vue Components

- **Always use Composition API with `<script setup>`**
- PascalCase for component file names: `WorkOrderDetail.vue`
- Keep components under ~200 lines; extract subcomponents when larger
- Type emitted events explicitly:

```typescript
const emit = defineEmits<{
  (e: "saved", entity: MyEntity): void;
  (e: "cancelled"): void;
}>();
```

### Naming Conventions

| Type           | Convention         | Example                           |
| -------------- | ------------------ | --------------------------------- |
| Vue components | PascalCase         | `WorkOrderDetail.vue`             |
| Utility files  | kebab-case         | `form-validator.ts`               |
| Pinia stores   | `use<Entity>Store` | `useWorkOrderStore`               |
| Services       | `<Entity>Service`  | `WorkOrderService`                |
| Interfaces     | PascalCase         | `WorkOrder`, `PhaseDetail`        |
| Functions      | camelCase          | `fetchWorkOrders`, `handleSubmit` |

### Imports

- Use path alias `@/` for all src imports: `import { useStore } from "@/store"`
- Lazy-load all route views: `() => import('./views/MyView.vue')`
- Group imports: Vue/external libs first, then internal modules

### Error Handling

- Validate before API calls using Yup schemas + `FormValidation` class
- Display errors via PrimeVue toast with severity levels
- All user-facing messages in **Catalan**
- Use early returns for validation failures

```typescript
const validation = new FormValidation(schema).validate(model);
if (!validation.result) {
  toast.add({
    severity: "warn",
    summary: Object.values(validation.errors).flat().join("\n"),
  });
  return;
}
```

### State Management (Pinia)

- Stores manage state + orchestrate service calls
- **Mutate state only in actions**
- After create/update/delete: re-fetch to sync UI
- Use `setNew(id?)` action to initialize blank entities with `getNewUuid()`

```typescript
// Standard store pattern
async create(model: Entity) {
  const result = await Services.Entity.create(model);
  if (result) await this.fetchOne(model.id);
  return result;
}
```

### Service Layer

- All HTTP via service classes extending `BaseService<T>`
- Inherited CRUD: `getAll()`, `getById(id)`, `create()`, `update()`, `delete()`
- Add custom queries as needed: `GetBetweenDates()`, `GetByStatus()`

```typescript
export class WorkOrderService extends BaseService<WorkOrder> {
  constructor() {
    super("workorder");
  }
}
```

### Date Handling

`Date.prototype.toJSON` is globally overridden in `utils/functions.ts` to adjust timezone before serialization. This means `JSON.stringify` (used by Axios) automatically converts any `Date` object to ISO format. **This only works for native `Date` objects, not strings.**

**The correct pattern for dates with PrimeVue DatePicker:**

1. **Store `GetById`**: Convert API date strings to `Date` objects immediately after fetching:

```typescript
async GetById(id: string) {
  const data = await Service.getById(id);
  if (data) {
    if (data.date) data.date = new Date(data.date) as any;
  }
  this.entity = data;
}
```

2. **Component `onMounted`**: Do NOT re-format dates. The store already provides `Date` objects that PrimeVue DatePicker accepts natively.

3. **Component save/update**: Send the entity directly to the store. `Date.prototype.toJSON` handles serialization automatically. No need to call `convertDateTimeToJSON()`.

```typescript
// ✅ Correct — Date objects serialize automatically
const updated = await store.Update(entity.value);

// ❌ WRONG — formatDate() converts Date to string "dd/MM/yyyy",
//    breaking both DatePicker binding and JSON serialization
entity.value.date = formatDate(entity.value.date);

// ❌ WRONG — mutating a reactive ref triggers re-render race conditions
entity.value.date = convertDateTimeToJSON(entity.value.date);
```

**Key rules:**
- `formatDate()` returns a **string** — use it only for display (columns, labels), never to set DatePicker model values
- `convertDateTimeToJSON()` is for non-reactive contexts only (e.g., building query parameters on a local variable)
- Never mutate date fields on a `storeToRefs` reactive ref before navigation — it triggers re-renders that race with `router.back()` and cause DOM crashes (`Cannot read properties of null (reading 'parentNode')`)
- Use `formatDateForQueryParameter()` for URL query strings in list view filters

### UI Conventions

- PrimeVue components are globally registered
- Use PrimeVue `FileUpload` instead of raw `<input type="file">` for future file uploads
- Toast messages in Catalan with appropriate severity and life duration
- Clone objects before editing in dialogs to prevent premature state mutation
- Numeric defaults should be `0`, not `undefined`

### Contextual Help Documentation

- When adding or updating contextual help, first analyze the real behavior of the view instead of documenting from route names alone
- Keep help content in Catalan and align terminology with the business flow: `pressupost`, `comanda`, `albara`, `factura`, `client`, `referencia`
- Use the mandatory section order defined in `frontend/docs/help-module.md`: purpose, actions, typical flow, important notes, common errors, and basic Mermaid process
- If a new help document raises the quality bar, review sibling help files in the same module for consistency

## Utilities (utils/functions.ts)

Use these instead of reimplementing:

- `getNewUuid()` - Generate client-side UUIDs
- `convertDateTimeToJSON(date)` - Normalize dates for API
- `formatDateForQueryParameter(date)` - Format for URL queries
- `createBlobAndDownloadFile(name, blob)` - Download files
- `formatCurrency(value)` - Currency formatting

## Table Attachments

`Table.vue` supports an optional read-only attachment column through `attachmentConfig`:

```vue
<Table
  :attachment-config="{
    entity: 'SalesOrder',
    formats: ['.pdf', '.jpg', '.jpeg', '.png'],
    title: 'Adjunts de la comanda',
    titleField: 'number',
  }"
/>
```

It reuses `FileService.GetEntityFiles` and `FileViewer`; keep uploads and deletion in the detail view's `FileEntityPicker`. When `formats` is omitted, only PDF and image formats supported by `FileViewer` are shown. `titleField` identifies the opened row in the dialog title.
## Anti-Patterns to Avoid

- Hard-coding API URLs (use service layer)
- Duplicating validation logic across components
- Mixing languages in UI (maintain Catalan)
- Mutating store state outside actions
- Using `any` type
- Forgetting to re-fetch after nested entity changes
- Creating new utility functions that already exist
- Using `formatDate()` to set DatePicker model values (use `new Date()` in store `GetById` instead)
- Mutating reactive ref date fields before `router.back()` (causes DOM unmount crashes)

## Route Pattern

```typescript
// modules/<domain>/routes.ts
export default [
  {
    path: "/workorder",
    name: "Workorders",
    component: () => import("./views/Workorders.vue"),
  },
  {
    path: "/workorder/:id",
    name: "workorder",
    component: () => import("./views/Workorder.vue"),
    props: true,
  },
] as Array<RouteRecordRaw>;
```

## Adding New Features

1. Find analogous entity module and mirror patterns
2. Create service extending `BaseService<T>`
3. Create Pinia store with CRUD actions
4. Add routes with lazy-loaded views
5. Use `getNewUuid()` for client IDs
6. Provide Catalan messages & toasts
7. Register routes in `src/router.ts`

## Environment Variables

Use `.env` files with `VITE_` prefix:

- `VITE_API_BASE_URL` - Main backend URL
- `VITE_REPORTS_BASE_URL` - Reports microservice URL

## Additional Documentation

See `.github/copilot-instructions.md` for comprehensive patterns including:

- Detailed service layer examples
- Dialog CRUD patterns for nested entities
- Report download patterns
- Exercise picker usage
- Authentication flow

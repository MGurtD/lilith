# Copilot Project Instructions (Lilith Frontend)

Vue 3 SPA for Lilith ERP - manufacturing management platform with domain-driven modules, Pinia state, and PrimeVue UI.

## Quick Start Reference

**Stack**: Vue 3 (Composition API) + TypeScript + Vite + Pinia + PrimeVue + Axios  
**Package Manager**: pnpm (v10+) - Fast, disk-efficient package manager  
**Structure**: Domain modules in `src/modules/<domain>/` with `routes.ts`, `store/`, `services/`, `views/`, `components/`, `types/`  
**State**: Pinia stores (`use<Entity>Store`) with CRUD actions calling service layer  
**Services**: Extend `BaseService<T>` from `src/api/base.service.ts`, wrap Axios calls  
**Validation**: Yup schemas + `FormValidation` class from `utils/form-validator.ts`  
**Utilities**: `utils/functions.ts` - `getNewUuid()`, `convertDateTimeToJSON()`, `formatCurrency()`, `createBlobAndDownloadFile()`  
**Language**: Catalan UI text throughout (some i18n structure exists but most strings are inline)  
**Reports**: Fetch via service `GetReportDataById()` → download blob with `createBlobAndDownloadFile()`  
**When uncertain**: Find analogous entity module and mirror patterns

## Dev Workflow

```bash
pnpm install              # Install dependencies
pnpm run dev              # Start dev server (port 8100)
pnpm run typecheck        # Type check without build
pnpm run build            # Production build to dist/
pnpm run build-development # Dev mode build to dist-test/
pnpm run preview          # Preview production build
```

**Environment**: Use `.env` files with `VITE_` prefix (e.g., `VITE_API_BASE_URL`, `VITE_REPORTS_BASE_URL`)  
**Docker**: Expects pre-built `dist/` - run `pnpm run build` before `docker build`  
**PWA**: Enabled via `vite-plugin-pwa` with auto-update strategy

## Architecture Overview

### Module Structure (Domain-Driven)

Each business domain (sales, production, purchase, warehouse, etc.) lives in `src/modules/<domain>/`:

```
modules/
  <domain>/
    routes.ts          # RouteRecordRaw[] exported, lazy-loaded views
    components/        # Domain-specific components
    services/          # API clients extending BaseService<T>
    store/             # Pinia stores for domain entities
    types/             # TypeScript interfaces for domain models
    views/             # Page components (List + Detail patterns)
```

**Route Registration**: `src/router.ts` imports and spreads all domain route arrays  
**Naming**: Stores use `use<Entity>Store`, services `<Entity>Service extends BaseService<T>`

### Service Layer Pattern

All HTTP communication through service classes:

```typescript
// Example: src/modules/production/services/workorder.service.ts
export class WorkOrderService extends BaseService<WorkOrder> {
  constructor() {
    super("workorder");
  }

  async GetBetweenDatesAndStatus(
    start: string,
    end: string,
    statusId?: string,
  ) {
    const response = await this.apiClient.get(
      `${this.resource}/betweenDates?startTime=${start}&endTime=${end}&statusId=${statusId}`,
    );
    return response.data;
  }
}
```

**Base CRUD**: `getAll()`, `getById(id)`, `create(model)`, `update(id, model)`, `delete(id)` inherited  
**Custom queries**: Add methods like `GetBetweenDates()`, `GetBySalesOrderId()`, etc.  
**Returns**: Boolean for simple CRUD, typed data or `GenericResponse<T>` for complex operations

### Pinia Store Pattern

Stores manage state + orchestrate service calls + refresh strategies:

```typescript
export const useWorkOrderStore = defineStore({
  id: "workorder",
  state: () => ({
    workorder: undefined as WorkOrder | undefined,
    workorders: undefined as Array<WorkOrder> | undefined,
  }),
  actions: {
    setNew(id: string) {
      this.workorder = { id /* ...defaults */ } as WorkOrder;
    },
    async fetchAll() {
      this.workorders = await Services.WorkOrder.getAll();
    },
    async fetchOne(id: string) {
      this.workorder = await Services.WorkOrder.getById(id);
    },
    async create(model: WorkOrder) {
      const result = await Services.WorkOrder.create(model);
      if (result) await this.fetchOne(model.id);
      return result;
    },
    async update(id: string, model: WorkOrder) {
      const result = await Services.WorkOrder.update(id, model);
      if (result) await this.fetchAll(); // or fetchOne(id)
      return result;
    },
  },
});
```

**Key Conventions**:

- Mutate state only in actions
- After create/update/delete: re-fetch parent or list to sync UI
- Nested entities (phases, details): separate actions that re-fetch parent
- `setNew(id?)` action initializes blank entity with `getNewUuid()` or provided ID

### Global State (`src/store/`)

**Core stores**:

- `useStore()` (index.ts): Auth, JWT, user, sidebar menus, language, exercise picker
- `useUserFilterStore()`: Persisted user-specific filters (table states, etc.)
- `useApiStore()` (backend.ts): Loading state, error messages
- `useExerciseStore()`: Fiscal periods with date ranges

**Exercise Picker**: Many list views require `exercisePicker.dates` (Date[]) for filtering - validate before queries  
**Authentication**: JWT stored in `authorization`, decoded for user ID, menus loaded dynamically via `AppProfileService.GetUserMenu()`  
**Language**: Catalan default, stored in localStorage, headers set in Axios interceptors (`Accept-Language`)

## Critical Integration Points

### API Clients & Interceptors

**Two Axios instances**:

1. `apiClient` (api.client.ts): Main backend at `VITE_API_BASE_URL`
2. `reportsClient` (reports.client.ts): Reports microservice at `VITE_REPORTS_BASE_URL`

Both have identical interceptor patterns:

- **Request**: Set `Accept-Language` from `useStore().language.current`, add `?culture=X` override if set, toggle `useApiStore().isWaiting`
- **Response**: Clear loading state, log errors, show toast on failures
- **Status validation**: Accept <500 status codes (handle 4xx in business logic)

```typescript
// Language is automatically sent with every request
const appStore = useStore();
// Headers: { "Accept-Language": "ca" }
// Query params may include: ?culture=ca
```

### Reports Pattern

1. Service method fetches report data: `GetReportDataById(id: string)`
2. Use `ReportService` from shared module to generate blob
3. Download with utility: `createBlobAndDownloadFile(fileName, blob)`

```typescript
// Typical usage in views
const downloadReport = async (id: string) => {
  const data = await service.GetReportDataById(id);
  if (!data) {
    toast.add({ severity: "warn", summary: "No s'han trobat dades" });
    return;
  }
  const blob = await reportService.Download(data, REPORTS.ORDER, "comanda.pdf");
  if (blob) createBlobAndDownloadFile("comanda.pdf", blob);
};
```

**Report types**: Defined in shared constants (budgets, orders, invoices, delivery notes)

### PWA Behavior

Configured via `vite-plugin-pwa` with:

- **Auto-update strategy**: Background service worker updates
- **Runtime caching**: Network-first for API, cache-first for assets/CDN
- **Redirect logic**: PWA standalone mode redirects `/` to `/plant` (shop floor module)
- **Manifest**: Landscape orientation, custom icons in `public/icons/`

Check PWA mode: `window.matchMedia("(display-mode: standalone)").matches`

## Component & Form Patterns

### DataTable Interaction Patterns

**Row Hover & Click Styling**:

- For clickable tables (with `@row-click` event), add `clickable-rows` class to apply pointer cursor
- Always include `stripedRows` for better row distinction
- Add `:rowHover="true"` to enable hover highlighting (explicit is better)

```vue
<DataTable
  :value="items"
  @row-click="onEditRow"
  class="p-datatable-sm clickable-rows"
  stripedRows
  :rowHover="true"
>
  <!-- columns -->
</DataTable>
```

**Non-clickable tables** (view-only or with button-only actions):

- Include `stripedRows` and `:rowHover="true"` for visual feedback
- Omit `clickable-rows` class - no pointer cursor needed

### Typical Detail View Flow

1. Read route param (`route.params.id`)
2. Fetch entity; if new ID or missing, initialize with `store.setNew(id)`
3. Configure header via `appStore.setMenuItem({ icon, title, backButtonVisible: true })`
4. On save: validate → normalize dates → call store action → toast result → optionally navigate

```typescript
// Example detail view pattern
const route = useRoute();
const store = useWorkOrderStore();
const appStore = useStore();

onMounted(async () => {
  const id = route.params.id as string;
  await store.fetchOne(id);

  if (!store.workorder) {
    store.setNew(id);
  }

  appStore.setMenuItem({
    icon: PrimeIcons.BOOK,
    title: store.workorder?.code || "Nova ordre",
    backButtonVisible: true,
  });
});
```

### Dialog CRUD Pattern

For nested entities (phase details, BOM items):

```typescript
const dialog = reactive({ visible: false, title: "" });
const temp = ref<PhaseDetail | undefined>();
const formMode = ref<FormActionMode>(FormActionMode.CREATE);

const openCreate = () => {
  temp.value = {
    id: getNewUuid(),
    workOrderPhaseId: currentPhase.value!.id,
    // ...defaults with numeric fields = 0 not undefined
  };
  formMode.value = FormActionMode.CREATE;
  dialog.title = "Crear detall";
  dialog.visible = true;
};

const openEdit = (item: PhaseDetail) => {
  temp.value = { ...item }; // Clone to avoid direct mutation
  formMode.value = FormActionMode.EDIT;
  dialog.title = "Editar detall";
  dialog.visible = true;
};

const save = async () => {
  // Validate if needed
  const result =
    formMode.value === FormActionMode.CREATE
      ? await store.createDetail(temp.value!)
      : await store.updateDetail(temp.value!.id, temp.value!);

  if (result) {
    dialog.visible = false;
    toast.add({ severity: "success", summary: "Desat correctament" });
  }
};
```

**Key rules**:

- Always clone objects when editing to prevent premature state mutation
- Numeric defaults should be `0`, not `undefined` (avoids NaN in calculations)
- Re-fetch parent entity after nested entity changes

## Validation & Error Handling

### Yup Validation Pattern

```typescript
import * as Yup from "yup";
import { FormValidation, FormValidationResult } from "@/utils/form-validator";

const schema = Yup.object({
  name: Yup.string().required("El nom és obligatori"),
  quantity: Yup.number()
    .min(1, "La quantitat ha de ser superior a 0")
    .required(),
  price: Yup.number().min(0, "El preu no pot ser negatiu"),
});

const validation = ref<FormValidationResult>({ result: false, errors: {} });

const submit = async () => {
  const validator = new FormValidation(schema);
  validation.value = validator.validate(model.value);

  if (!validation.value.result) {
    const errorMessages = Object.values(validation.value.errors)
      .flat()
      .join("\n");
    toast.add({ severity: "warn", summary: errorMessages, life: 8000 });
    return;
  }

  // Proceed with save
  const success = await store.create(model.value);
  if (success) {
    toast.add({ severity: "success", summary: "Creat correctament" });
  }
};
```

**Validation rules**:

- All error messages in Catalan
- Use `FormValidation` class from `utils/form-validator.ts`
- Display errors with toast (severity: "warn")
- Life: 4000-8000ms depending on message length

### GenericResponse Pattern

Backend returns `GenericResponse<T>` for complex operations:

```typescript
interface GenericResponse<T> {
  result: boolean;
  errors: string[];
  content?: T;
}

// Handling in services/stores
const response = await service.ComplexOperation(payload);
if (!response.result) {
  toast.add({
    severity: "error",
    summary: response.errors[0] || "Error desconegut",
  });
  return false;
}
// Use response.content if needed
```

## Utilities (Use Instead of Rewriting)

`utils/functions.ts` provides essential helpers - always use these instead of reimplementing:

- **`getNewUuid()`**: Generate client-side UUIDs for new entities
- **`convertDateTimeToJSON(date)`**: Normalize Date objects for API submission
- **`formatDateForQueryParameter(date)`**: Format dates for URL query strings
- **`createBlobAndDownloadFile(name, blob)`**: Download files from blob responses
- **`formatCurrency(value)`**: Format numbers as currency (€)
- **`formatDate(date)`, `formatDateTime(dateTime)`**: Display formatting
- **`extractTime(isoString)`**: Extract time portion from ISO datetime
- **`calculateDuration(startTime)`**: Calculate elapsed time from start

**Extension rules**:

- Add new utilities only if reusable across ≥2 modules
- Keep functions pure and generic (no domain-specific logic)
- Include JSDoc comments for new utilities

## Date & Time Handling

**Critical rules**:

- Always normalize dates before API calls: `convertDateTimeToJSON(dateValue)`
- Clone date objects if applying multiple transformations
- Use `formatDateForQueryParameter()` for filtering queries
- Avoid scattering `.toLocaleString()` - use central helpers
- Default date format in UI: `dd/MM/yyyy` (Catalan convention)

```typescript
// Before API submission
const workorder = {
  ...formData,
  plannedDate: convertDateTimeToJSON(formData.plannedDate),
  startTime: convertDateTimeToJSON(formData.startTime),
};
await store.create(workorder);
```

## Production Entities & Hierarchy

**WorkMaster / WorkOrder → Phases → Details & BOM Items**

The production module uses a hierarchical structure:

- **WorkMaster/WorkOrder**: Top-level manufacturing definitions/instances
- **Phases**: Steps in the manufacturing process
- **Phase Details**: Specific operations within a phase
- **BOM Items**: Bill of materials - components consumed in the phase

**CRUD rules for nested entities**:

- Child entity changes must re-fetch parent phase
- Use consistent property naming: `workMasterPhaseId`, `workOrderPhaseId`
- Provide proper defaults on creation (numeric fields = `0`, not `undefined`)
- Avoid partial updates - re-fetch full hierarchy after changes

```typescript
// Creating a phase detail
async createDetail(model: WorkOrderPhaseDetail) {
  const result = await Services.WorkOrderPhaseDetail.create(model);
  if (result) await this.fetchPhaseById(model.workOrderPhaseId); // Re-fetch parent
  return result;
}
```

## Exercise Picker & Date Filtering

Many list views depend on `exercisePicker.dates` from global store for date range filtering.

**Before fetching filtered data**:

```typescript
const appStore = useStore();
if (!appStore.exercisePicker.dates) {
  toast.add({
    severity: "warn",
    summary: "Selecciona un exercici per continuar",
    life: 4000,
  });
  return;
}

// Safe to proceed
await store.fetchFiltered(
  formatDateForQueryParameter(appStore.exercisePicker.dates[0]),
  formatDateForQueryParameter(appStore.exercisePicker.dates[1]),
  statusId,
);
```

**Initialize exercise picker**:

```typescript
// Set current year automatically
appStore.setCurrentYear();
```

## Authentication & Navigation

**Authentication flow**:

- JWT stored in `authorization` property of `useStore()`
- Decoded with `jwt-decode` to extract user ID and locale
- User object loaded via `UserService.GetById()`
- Menus loaded dynamically via `AppProfileService.GetUserMenu()` (fallback to role-based menus)
- Language preference from JWT `locale` field or localStorage

**Setting page headers**:

```typescript
const appStore = useStore();
appStore.setMenuItem({
  icon: PrimeIcons.BOOK,
  title: "Nova ordre de treball",
  backButtonVisible: true,
});
```

**Menu structure**: Dynamic menu loaded per user profile, transformed from backend `MenuNode[]` hierarchy

## Lifecycle & Status Management

Entity lifecycle controls workflow states:

- Each major entity type has named lifecycle (e.g., "WorkOrder", "SalesOrder", "Budget")
- Status transitions managed by backend
- Frontend uses lifecycle/status for guards and UI state

**Status-based guards**:

```typescript
// Compute permission guards based on lifecycle state
const canModifyWorkOrder = computed(() => {
  return (
    store.workorder?.statusId !== "completed" &&
    store.workorder?.statusId !== "cancelled"
  );
});
```

**Disable controls instead of hiding them** - provide visual feedback via tooltip or disabled state

## Toast Usage Conventions

```typescript
// Success
toast.add({ severity: "success", summary: "Creat correctament", life: 4000 });

// Warning (validation, missing data)
toast.add({ severity: "warn", summary: "Selecciona un exercici", life: 6000 });

// Error (API failures)
toast.add({ severity: "error", summary: "Error en el procés", life: 8000 });

// Info
toast.add({ severity: "info", summary: "Processant...", life: 3000 });
```

**Rules**:

- All messages in Catalan
- Life: 3-8 seconds (longer for errors/validation)
- Show first backend error message when available: `response.errors[0]`

## Naming & Code Style

**File naming**:

- Vue SFC components: PascalCase (e.g., `WorkOrderDetail.vue`)
- Utilities/services: camelCase files (e.g., `form-validator.ts`)
- Stores: `use<Entity>Store` with id matching entity name

**Component conventions**:

- Use Composition API with `<script setup>`
- Function names: camelCase
- Avoid very large components (> ~200 lines) — extract subcomponents when possible
- Type emitted events explicitly:

```typescript
const emit = defineEmits<{
  (e: "saved", entity: MyEntity): void;
  (e: "cancelled"): void;
}>();
```

**TypeScript**:

- Prefer explicit types over `var` for clarity
- Use nullable reference types appropriately (`Type | undefined`)
- Avoid `any` - use `unknown` if type truly unknown

## Route Module Pattern

Each domain's `routes.ts` exports `RouteRecordRaw[]`:

```typescript
import { RouteRecordRaw } from "vue-router";

const Workorders = () => import("./views/Workorders.vue");
const Workorder = () => import("./views/Workorder.vue");

export default [
  {
    path: "/workorder",
    name: "Workorders",
    component: Workorders,
  },
  {
    path: "/workorder/:id",
    name: "workorder",
    component: Workorder,
    props: true, // Pass route params as props when useful
  },
] as Array<RouteRecordRaw>;
```

**Conventions**:

- Lazy imports for all views: `() => import('./views/MyView.vue')`
- Consistent naming: lowercase path segments matching plural/singular entity names
- Use `props: true` when binding route params to component props

## Reusable Code Patterns

### Complete Detail View Setup

```typescript
const route = useRoute();
const store = useWorkOrderStore();
const appStore = useStore();

onMounted(async () => {
  const id = route.params.id as string;
  await store.fetchOne(id);

  const title = store.workorder
    ? `Editar ${store.workorder.code}`
    : "Alta nova ordre";

  appStore.setMenuItem({
    icon: PrimeIcons.BOOK,
    backButtonVisible: true,
    title,
  });

  if (!store.workorder) {
    store.setNew(id);
  }
});
```

### Form Submit Handler

```typescript
const submit = async () => {
  const entity = store.current!;

  const ok =
    formMode.value === FormActionMode.CREATE
      ? await store.create(entity)
      : await store.update(entity.id, entity);

  if (ok) {
    toast.add({ severity: "success", summary: "Desat correctament" });
    // Optional: navigate away
    // router.push({ name: "EntityList" });
  }
};
```

### Report Download

```typescript
const downloadReport = async (id: string) => {
  const data = await service.GetReportDataById(id);
  if (!data) {
    toast.add({
      severity: "warn",
      summary: "Sense dades per generar el document",
    });
    return;
  }

  const blob = await new ReportService().Download(
    data,
    REPORTS.ORDER,
    "comanda.pdf",
  );

  if (!blob) {
    toast.add({ severity: "warn", summary: "Error al generar el document" });
    return;
  }

  createBlobAndDownloadFile("comanda.pdf", blob);
};
```

### Dialog CRUD for Nested Items

```typescript
const dialog = reactive({ visible: false, title: "" });
const temp = ref<PhaseDetail | undefined>();

const openEdit = (row: PhaseDetail) => {
  temp.value = { ...row }; // Clone
  dialog.visible = true;
  formMode.value = FormActionMode.EDIT;
  dialog.title = "Editar detall";
};

const saveDetail = async () => {
  const ok =
    formMode.value === FormActionMode.CREATE
      ? await phaseStore.createDetail(temp.value!)
      : await phaseStore.updateDetail(temp.value!.id, temp.value!);

  if (ok) {
    await phaseStore.fetchPhaseById(parentId); // Re-fetch parent
    dialog.visible = false;
    toast.add({ severity: "success", summary: "Desat" });
  }
};
```

## Development Guidelines

### Adding New Domain Module (Checklist)

1. Create folder `src/modules/<domain>/` with subfolders: `components/`, `services/`, `store/`, `types/`, `views/`, and `routes.ts`
2. Define entity interfaces in `types/`
3. Implement service class extending `BaseService<T>`
4. Create Pinia store (state, CRUD actions, refresh strategy)
5. Add routes in `routes.ts` with lazy-loaded views
6. Build list & detail views using established patterns
7. Use `getNewUuid()` for client IDs; convert dates with `convertDateTimeToJSON`
8. Provide Catalan messages & toasts
9. If reports needed, extend report constants & follow download pattern
10. Import and spread routes in `src/router.ts`

### Extension Rules for Utilities

Add new utilities only if:

- Generic (not domain-specific)
- Reused by ≥2 components
- Include concise JSDoc for new utilities

### Code Quality Standards

Generated code should:

- Type-check (no implicit `any`)
- Avoid unused imports
- Functions < ~40 lines (split if larger)
- Use early returns for validation failures
- Null/undefined guard nested access

- Type-check (no implicit `any`)
- Avoid unused imports
- Functions < ~40 lines (split if larger)
- Use early returns for validation failures
- Null/undefined guard nested access

- Type-check (no implicit `any`)
- Avoid unused imports
- Functions < ~40 lines (split if larger)
- Use early returns for validation failures
- Null/undefined guard nested access

## Common Anti-Patterns to Avoid

**DON'T**:

- Hard-code API URLs in components (use service layer)
- Duplicate validation logic across components
- Mix languages in UI (maintain Catalan)
- Mutate store arrays outside actions
- Rewrite existing utility functions
- Use `any` type (prefer explicit types or `unknown`)
- Forget to re-fetch after nested entity changes

**DO**:

- Reuse helpers & naming conventions
- Keep business logic in store actions
- Validate before all API calls
- Keep UI copy in Catalan
- Round numbers only at display layer
- Clone objects before editing in dialogs

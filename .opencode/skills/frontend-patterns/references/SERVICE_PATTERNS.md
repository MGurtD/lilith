# Service Layer Patterns

Advanced patterns for the API service layer in Lilith ERP frontend.

## BaseService<T> API

All services extend `BaseService<T>` from `src/api/base.service.ts`.

**Inherited methods:**

| Method | HTTP | Returns |
|--------|------|---------|
| `getAll()` | `GET /api/{resource}` | `Promise<T[] \| null>` |
| `getById(id)` | `GET /api/{resource}/{id}` | `Promise<T \| null>` |
| `create(entity)` | `POST /api/{resource}` | `Promise<boolean>` |
| `update(id, entity)` | `PUT /api/{resource}/{id}` | `Promise<boolean>` |
| `delete(id)` | `DELETE /api/{resource}/{id}` | `Promise<boolean>` |

**Protected helpers for custom methods:**

| Helper | Purpose |
|--------|---------|
| `getCustom(path)` | `GET /api/{resource}/{path}` → returns `T \| null` |
| `getManyCustom(path)` | `GET /api/{resource}/{path}` → returns `T[] \| null` |
| `postCustom(path, body)` | `POST /api/{resource}/{path}` |

## Custom Service Methods

```typescript
export class BudgetService extends BaseService<Budget> {
  constructor() {
    super("budget"); // maps to: api/budget
  }

  // GET /api/budget/betweendates?from=...&to=...
  async getBetweenDates(from: string, to: string): Promise<Budget[] | null> {
    return this.getManyCustom(`betweendates?from=${from}&to=${to}`);
  }

  // GET /api/budget/{id}/withlines
  async getWithLines(id: string): Promise<Budget | null> {
    return this.getCustom(`${id}/withlines`);
  }

  // POST /api/budget/{id}/accept
  async accept(id: string): Promise<boolean> {
    return this.postCustom(`${id}/accept`, {});
  }
}
```

## Service Registration

Each module has a `services/index.ts` that exports singleton instances:

```typescript
// src/modules/sales/services/index.ts
import { BudgetService } from "./budget.service";
import { SalesOrderService } from "./salesorder.service";

export const Services = {
  Budget: new BudgetService(),
  SalesOrder: new SalesOrderService(),
};
```

Usage in stores and components:
```typescript
import { Services } from "../services";

const result = await Services.Budget.getAll();
```

## Three Axios Clients

The project has 3 separate Axios instances. Services use the correct one:

| Client | File | Used for |
|--------|------|---------|
| Main API | `api/api.client.ts` | All standard CRUD (default in BaseService) |
| Actions | `api/actions.client.ts` | Production actions, real-time (plant module) |
| Reports | `api/reports.client.ts` | PDF/Excel downloads |

All clients share the same interceptor pattern:
- Request: adds `Accept-Language` header from current locale
- Response: handles loading state via `useApiStore`
- Error: parses RFC 7807 errors and shows toast

## Report Download Service

```typescript
import { ReportService } from "@/services/report.service";

const reportService = new ReportService();

// Download with query parameters
async function downloadPdf() {
  await reportService.download(
    "budget-report",         // Report name (matches backend report identifier)
    {
      id: props.id,          // Query parameters
      format: "pdf"
    }
  );
}
```

The `ReportService` uses `reports.client.ts` (different base URL from main API), configured via `VITE_REPORTS_BASE_URL`.

## Error Handling in Stores

Errors are handled automatically by Axios interceptors — the store doesn't need to catch errors for standard operations:

```typescript
async create(model: Budget) {
  // Errors automatically shown as toast via interceptor
  const result = await Services.Budget.create(model);
  if (result) await this.fetchOne(model.id);
  return result; // true = success, false/null = error (already displayed)
}
```

For custom error handling:
```typescript
import { parseError } from "@/utils/error-parser";
import { globalToast } from "@/utils/global-toast";

try {
  const result = await Services.Budget.accept(id);
} catch (error) {
  const parsed = parseError(error);
  globalToast.error(parsed.message);
}
```

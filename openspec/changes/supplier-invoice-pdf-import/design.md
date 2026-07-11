# Design: Supplier Invoice PDF Import

## Goals & Non-Goals

Reference: `openspec/changes/supplier-invoice-pdf-import/spec.md` — the WHAT.
This document is the HOW.

**MVP locks (already agreed, not re-opened here):**
- One endpoint `POST /api/PurchaseInvoice/Ingest` — avoids the existing `/api/PurchaseInvoice/Import` tax-import route (confirmed at `backend/src/Api/Controllers/Purchase/PurchaseInvoiceController.cs:126`).
- LlamaParse via direct `IHttpClientFactory`-built `HttpClient`; the `AddHttpClient<IInvoiceIngestionService, LlamaParseInvoiceIngestionService>()` shape mirrors `GeolocalizationService` (`backend/src/Application/Services/Geolocalization/GeolocalizationService.cs:10`).
- Env-vars only — `Ingestion__ApiKey` / `Ingestion__BaseUrl` (default `https://api.cloud.llamaindex.ai`) / `Ingestion__DefaultModel` (default `llama-parse`) / `Ingestion__TimeoutSeconds` (default 90). No new `appsettings*.json` files committed.
- Prefill `PurchaseInvoiceImports` from per-tax-rate `taxBreakdown[]`; `form.calcAmounts()` invoked once after prefill.
- Backend validates `taxRate` against the `Tax` catalog (exact match on `Tax.Percentatge`); no auto-create. Unknown rate → HTTP 422 `UnknownTaxRate`. Surcharge → HTTP 422 `SurchargeNotSupportedInScope`.
- No test infra; verification = Swagger smoke + manual UI walk-through, recorded in `apply-progress`.

## Architecture Overview

The MVP adds a thin **Ingestion vertical slice** that touches three layers:

```
                  ┌──────────────────────────────┐
                  │ PurchaseInvoiceController   │  (Api)
                  │   POST /api/.../Ingest       │
                  └────────────┬─────────────────┘
                               │ IInvoiceIngestionService (Application.Contracts)
                  ┌────────────▼─────────────────┐
                  │ LlamaParseInvoiceIngestionService │  (Application)
                  │   + LlamaParsePayloadMapper  │
                  │   + HttpClient (IHttpClientFactory) │
                  └────────────┬─────────────────┘
                               │ HttpClient → api.cloud.llamaindex.ai
                               ▼
                       LlamaParse REST
```

Key architectural choices:

- **Vertical-slice layout (`Ingestion/`)** instead of nesting under `Purchase/`. The two folders house both the contract (`Application.Contracts/Ingestion/`) and the implementation (`Application/Ingestion/`). This deviates from `Geolocalization/` only in that *interfaces* sit in `Application.Contracts/Ingestion/` rather than `Application.Contracts/Services/<Domain>/`; the deviation is deliberate so future ingestion targets (supplier statements, expense receipts) drop into the same folder without a rename.
- **`AddHttpClient<TInterface, TImpl>()`** is the registration shape, identical to `IGeolocalizationService` / `IGeoapifyService` / `IGitHubProxyService`. The typed `HttpClient` is built by the factory so `HttpClient.Timeout` is configurable per-deployment via `Ingestion__TimeoutSeconds`.
- **No new NuGet**. Direct `HttpClient` + `System.Net.Http.Json` already shipped.

## Backend Design

### File layout (new + modified, concrete paths under `backend/src/...`)

| File | Action | LoC | Purpose |
|------|--------|-----|---------|
| `Application.Contracts/Ingestion/IInvoiceIngestionService.cs` | Create | ~25 | One-method contract (`IngestAsync`). |
| `Application.Contracts/Ingestion/IngestPurchaseInvoiceResponse.cs` | Create | ~45 | Header DTO + `taxBreakdown[]` + `confidenceMap`. |
| `Application.Contracts/Ingestion/TaxBreakdownRow.cs` | Create | ~30 | `taxRate` / `baseAmount` / `taxAmount` / `taxId` / `confidence`. |
| `Application.Contracts/Ingestion/ConfidenceMap.cs` | Create | ~15 | Two dictionaries: `headers`, per-row line confidence (placeholder list kept empty — we only ship header confidence today). |
| `Application.Contracts/Configuration/AppSettings.cs` | Modify | +12 | Nested `IngestionSettings?` with `Validate()` guarded for null ApiKey. |
| `Application/Ingestion/LlamaParseInvoiceIngestionService.cs` | Create | ~180 | HTTP flow: upload → poll → extract → map. |
| `Application/Ingestion/LlamaParsePayloadMapper.cs` | Create | ~80 | DTO ↔ provider JSON, plus `Tax` resolution. |
| `Application/Ingestion/IngestionException.cs` | Create | ~25 | Discriminated failure modes (`ProviderAuthFailed`, `ProviderUnparseable`, `ProviderUnavailable`, `ProviderNotConfigured`). |
| `Application/Ingestion/LlamaParseContracts.cs` | Create | ~60 | Internal DTOs that mirror the LlamaParse `extract` / `parsing` shapes (not exposed). |
| `Api/Setup/ApplicationServicesSetup.cs` | Modify | +2 | `services.AddHttpClient<IInvoiceIngestionService, LlamaParseInvoiceIngestionService>(...);`. |
| `Api/Controllers/Purchase/PurchaseInvoiceController.cs` | Modify | +55 | `[HttpPost("Ingest")]` + multipart binding + status mapping. |
| `Api/Resources/LocalizationService/ca.json`+`es.json`+`en.json` | Modify | +12 each | Keys: `InvalidFileType`, `ProviderAuthFailed`, `ProviderUnparseable`, `ProviderUnavailable`, `ProviderNotConfigured`, `UnknownTaxRate`, `SurchargeNotSupportedInScope`. |

**Estimated backend total: ~607 lines.**

### Interface contract

```csharp
// Application.Contracts/Ingestion/IInvoiceIngestionService.cs
namespace Application.Contracts.Ingestion;

public interface IInvoiceIngestionService
{
    Task<IngestPurchaseInvoiceResponse> IngestAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken ct = default);
}
```

Naming chosen over `IPurchaseInvoiceIngestionService` to allow ingestion of other vendor documents in future (statements, dunning).

### DTO contract

```csharp
// IngestPurchaseInvoiceResponse.cs
public class IngestPurchaseInvoiceResponse
{
    public string? SupplierVatNumber { get; set; }     // matched client-side from local suppliers
    public string? SupplierName { get; set; }
    public string? InvoiceNumber { get; set; }          // PDF's own number → maps to supplierNumber
    public DateTime? IssueDate { get; set; }
    public decimal? BaseAmount { get; set; }
    public decimal? TransportAmount { get; set; }
    public decimal? Subtotal { get; set; }              // BaseAmount + TransportAmount
    public decimal? TaxAmount { get; set; }
    public decimal? GrossAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? ExtraTaxPercentatge { get; set; }   // recàrconc/IRPF (may be null)
    public decimal? ExtraTaxAmount { get; set; }
    public List<TaxBreakdownRow> TaxBreakdown { get; set; } = new();
    public ConfidenceMap Confidence { get; set; } = new();
}

public class TaxBreakdownRow
{
    public decimal TaxRate { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public Guid TaxId { get; set; }                     // resolved server-side
    public decimal Confidence { get; set; }             // 0.0–1.0
}

public class ConfidenceMap
{
    public Dictionary<string, decimal> Headers { get; set; } = new();
    public List<decimal> Lines { get; set; } = new();   // empty in MVP (header-only)
}
```

### LlamaParse integration

**HttpClient bootstrap** (in `ApplicationServicesSetup.AddApplicationServices`):
```csharp
services.AddHttpClient<IInvoiceIngestionService, LlamaParseInvoiceIngestionService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value.Ingestion;
    client.BaseAddress = new Uri(settings?.BaseUrl ?? "https://api.cloud.llamaindex.ai");
    client.Timeout = TimeSpan.FromSeconds(settings?.TimeoutSeconds ?? 90);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

**Request flow** (inside `LlamaParseInvoiceIngestionService.IngestAsync`):

```
1. POST /api/files/upload      (multipart, Bearer ApiKey)            → file_id
2. POST /api/extraction/run    ({ file_id, schema: <invoice_schema> }) → structured JSON
                            OR (fallback) POST /api/parsing/create → GET /api/parsing/job/{id} (poll ≤90s)
3. Map → IngestPurchaseInvoiceResponse (LlamaParsePayloadMapper)
4. For each taxBreakdown[].taxRate:
       tax = unitOfWork.Taxes.Find(t => t.Percentatge == rate).FirstOrDefault();
       if (tax == null) throw IngestionException(UnknownTaxRate, rate);
       row.TaxId = tax.Id;
5. Return DTO
```

**Schemas attempted** (in priority order, logged to ILogger):

1. **POST `/api/extraction/run`** with a JSON schema describing supplier invoice fields. Returns the parsed fields directly.
2. **Fallback: POST `/api/parsing/create`** then poll `/api/parsing/job/{id}` until status `SUCCESS`, returning a Markdown block. Apply a local regex heuristic to extract `BaseAmount` / `TaxAmount` / `NetAmount` / `InvoiceDate` / `SupplierVatNumber` / `SupplierName` / `InvoiceNumber`. Regex shapes recorded in `apply-progress`.

Apply phase MUST run a POC against both endpoints with a real supplier PDF and commit the decision in `apply-progress` before wiring the controller action. Spec is agnostic about which wins.

### Settings binding

```csharp
// AppSettings.cs (added)
public class IngestionSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.cloud.llamaindex.ai";
    public string DefaultModel { get; set; } = "llama-parse";
    public int TimeoutSeconds { get; set; } = 90;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("Ingestion:ApiKey configuration key is required");
        if (TimeoutSeconds <= 0)
            throw new ArgumentException("Ingestion:TimeoutSeconds must be greater than zero");
        // BaseUrl + DefaultModel have safe defaults; not validated.
    }
}
```

Override precedence: env-var `Ingestion__ApiKey` (double underscore) beats `appsettings.Development.json`. ASP.NET handles this — no extras. The new section is **NOT** added to any committed JSON file; absence is intentional and the orchestrator/spec already lock this.

A doc comment in `PurchaseInvoiceController.Ingest` lists the env-var names — that's the operator-facing breadcrumb.

### Error mapping (controller → HTTP)

| LlamaParse / internal status | Backend HTTP | Localized key | Body shape |
|------------------------------|--------------|---------------|------------|
| 200                          | 200          | —             | `IngestPurchaseInvoiceResponse` |
| 401 upstream                 | 502          | `ProviderAuthFailed` | `GenericResponse(false, msg)` |
| 422 upstream                 | 422          | `ProviderUnparseable` | `GenericResponse(false, msg)` |
| 5xx / timeout                | 502          | `ProviderUnavailable` | `GenericResponse(false, msg)` |
| `Ingestion__ApiKey` missing  | 503          | `ProviderNotConfigured` | `GenericResponse(false, msg)` |
| Non-PDF / missing file       | 400          | `InvalidFileType` | `GenericResponse(false, msg)` |
| `taxRate` not in `Tax` catalog | 422        | `UnknownTaxRate` | `GenericResponse(false, msg, offendingRates: decimal[])` |
| `surchargeRate` present      | 422          | `SurchargeNotSupportedInScope` | `GenericResponse(false, msg)` |

All messages are pulled through `ILocalizationService` so they honor the user's culture.

### Tax catalog resolution (server-side)

Read path: `unitOfWork.Taxes.Find(t => t.Percentatge == rate)` inside `LlamaParsePayloadMapper`. If multiple `Tax` rows share the same `Percentatge`, we take the first by `Id` order — operator can fix manually afterwards. Recàrrec (any `taxBreakdown` row with non-null `surchargeRate`) short-circuits the whole response: return `IngestionException(SurchargeNotSupportedInScope)` before any row is built.

**Anti-create policy**: even if a supplier has VAT `X` and an unknown tax rate shows up, we do NOT insert a `Tax` row. The orchestrator-locked spec is explicit on this.

### Happy-path sequence

```
Browser → POST /api/PurchaseInvoice/Ingest (multipart IFormFile pdfFile)
       → PurchaseInvoiceController.Ingest([FromForm] IFormFile pdfFile)
       → IInvoiceIngestionService.IngestAsync(stream, filename)
           → /api/files/upload          → file_id
           → /api/extraction/run       → JSON (preferred)
              OR  /api/parsing/create + poll → markdown
           → LlamaParsePayloadMapper.Map(...)
              → resolve Tax.Id by Percentatge
              → throw IngestionException(UnknownTaxRate) on miss
       ← 200 OK with IngestPurchaseInvoiceResponse

ImportPurchaseInvoice.vue
   → reads payload
   → for each taxBreakdown row: append PurchaseInvoiceImport to purchaseInvoice.value.purchaseInvoiceImports
   → purchaseStore.setFromIngestion(payload)   ← new store action
   → formRef.value.calcAmounts()              ← runs once
   → operator edits (optional) → "Acceptar i crear" → existing form.submitForm() → PurchaseInvoiceService.Create
```

### Routes & controller action shape

```csharp
// PurchaseInvoiceController.cs (additions; existing constructor untouched —
// IInvoiceIngestionService and IUnitOfWork are added as new DI params)
[HttpPost("Ingest")]
[Consumes("multipart/form-data")]
[RequestSizeLimit(20 * 1024 * 1024)]  // 20 MB; aligns with spec's "~20 MB"
[ProducesResponseType(typeof(IngestPurchaseInvoiceResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(GenericResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(GenericResponse), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(GenericResponse), StatusCodes.Status502BadGateway)]
[ProducesResponseType(typeof(GenericResponse), StatusCodes.Status503ServiceUnavailable)]
public async Task<IActionResult> Ingest([FromForm] IFormFile pdfFile, CancellationToken ct)
{
    if (pdfFile is null || pdfFile.Length == 0
        || !string.Equals(pdfFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        return BadRequest(new GenericResponse(false, localizationService.GetLocalizedString("InvalidFileType")));

    await using var stream = pdfFile.OpenReadStream();
    try
    {
        var result = await ingestionService.IngestAsync(stream, pdfFile.FileName, ct);
        return Ok(result);
    }
    catch (IngestionException ex) when (ex.Kind == IngestionFailureKind.NotConfigured)
    {
        return StatusCode(503, new GenericResponse(false, ex.Message));
    }
    catch (IngestionException ex) when (ex.Kind == IngestionFailureKind.UnknownTaxRate)
    {
        return UnprocessableEntity(new GenericResponse(false, ex.Message));
    }
    catch (IngestionException ex) when (ex.Kind == IngestionFailureKind.SurchargeUnsupported)
    {
        return UnprocessableEntity(new GenericResponse(false, ex.Message));
    }
    catch (IngestionException ex)
    {
        return StatusCode(502, new GenericResponse(false, ex.Message));
    }
}
```

DI params added to the existing primary constructor: `IInvoiceIngestionService ingestionService, IUnitOfWork unitOfWork` (the latter is needed if we want backend-side supplier resolution by VatNumber — currently we leave that to the frontend, so the param is **not required** for MVP; see Decisions § below).

> XML-doc-style comment on the action lists operator env-vars: `// Configure Ingestion__ApiKey, Ingestion__BaseUrl, Ingestion__DefaultModel, Ingestion__TimeoutSeconds as env-vars; no appsettings entry exists.`

## Frontend Design

### File layout (new + modified, paths under `frontend/src/...`)

| File | Action | LoC | Purpose |
|------|--------|-----|---------|
| `modules/purchase/types/index.ts` | Modify | +12 | Add `IngestPurchaseInvoiceResponse`, `TaxBreakdownRow`, `ConfidenceMap`. |
| `modules/purchase/services/invoice-ingestion.service.ts` | Create | ~35 | Class with `Ingest(file): Promise<IngestPurchaseInvoiceResponse>` (multipart POST). NOT extending `BaseService<T>` because payload is binary — mirrors `FileService.upload()`. |
| `modules/purchase/services/index.ts` | Modify | +2 | Register `PurchaseInvoiceIngestion: new PurchaseInvoiceIngestionService()`. |
| `modules/purchase/store/purchaseInvoices.ts` | Modify | +35 | Add `setFromIngestion(payload)` action: `setNewPurchaseInvoice(getNewUuid())` then mutate `purchaseInvoiceImports[]` from payload's `taxBreakdown[]`. |
| `modules/purchase/views/ImportPurchaseInvoice.vue` | Create | ~260 | File input + review form + accept/cancel. |
| `modules/purchase/views/PurchaseInvoices.vue` | Modify | +5 | Toolbar button "Importar factura (PDF)" → `router.push({ name: 'PurchaseInvoiceImport' })`. |
| `modules/purchase/routes.ts` | Modify | +9 | Lazy route `/purchaseinvoice/import`, name `PurchaseInvoiceImport`. |

**Estimated frontend total: ~360 lines.**

### Service contract

`PurchaseInvoiceIngestionService` (NOT extends `BaseService<T>`):

```typescript
// modules/purchase/services/invoice-ingestion.service.ts
import apiClient, { logException } from "@/api/api.client";
import type {
  IngestPurchaseInvoiceResponse,
} from "../types";

export class PurchaseInvoiceIngestionService {
  async ingest(file: File): Promise<IngestPurchaseInvoiceResponse | undefined> {
    const form = new FormData();
    form.append("pdfFile", file);
    try {
      const response = await apiClient.post(
        "/PurchaseInvoice/Ingest",
        form,
        { headers: { "Content-Type": "multipart/form-data" } },
      );
      if (response.status === 200) {
        return response.data as IngestPurchaseInvoiceResponse;
      }
    } catch (err) {
      logException(err);
    }
    return undefined;
  }
}
```

Registered in `services/index.ts`:
```typescript
import { PurchaseInvoiceIngestionService } from "./invoice-ingestion.service";
// ...
export default {
  // ... existing keys
  PurchaseInvoiceIngestion: new PurchaseInvoiceIngestionService(),
};
```

### Store action

```typescript
// store/purchaseInvoices.ts (append to actions)
setFromIngestion(payload: IngestPurchaseInvoiceResponse): PurchaseInvoice | undefined {
  const id = getNewUuid();
  this.setNewPurchaseInvoice(id);                       // existing action, blank slate
  if (!this.purchaseInvoice) return undefined;
  this.purchaseInvoice.supplierNumber  = payload.invoiceNumber ?? "";
  this.purchaseInvoice.purchaseInvoiceDate = payload.issueDate
    ? new Date(payload.issueDate) as any : new Date();
  this.purchaseInvoice.transportAmount = payload.transportAmount ?? 0;
  this.purchaseInvoice.extraTaxPercentatge = payload.extraTaxPercentatge ?? 0;
  this.purchaseInvoice.discountPercentage = payload.discountPercentage ?? 0;
  this.purchaseInvoice.purchaseInvoiceImports = payload.taxBreakdown.map((row) => ({
    id: getNewUuid(),
    baseAmount: row.baseAmount,
    taxAmount: row.taxAmount,
    netAmount: row.baseAmount + row.taxAmount,
    purchaseInvoiceId: id,
    taxId: row.taxId,
    disabled: false,
    createdOn: null,
    updatedOn: null,
  } as PurchaseInvoiceImport));
  // SupplierId left "" — operator picks manually (front-end guide §domains).
  return this.purchaseInvoice;
},
```

> **Supplier auto-match is deferred to the frontend in MVP** because the master-data store (`usePurchaseMasterDataStore`) already has `suppliers` in memory; resolving by VatNumber there is one O(n) lookup. Backend doesn't need `IUnitOfWork` for MVP — confirmed by reading the proposal's locked decisions.

### View: `ImportPurchaseInvoice.vue`

Layout:
```
┌─────────────────────────────────────────────────┐
│ Toolbar: [Importar factura (PDF)] ← disabled until file selected │
├─────────────────────────────────────────────────┤
│ <input type="file" accept="application/pdf">    │
│  +  drag-and-drop area                           │
├─────────────────────────────────────────────────┤
│ Review pane (visible after success):            │
│   <FormPurchaseInvoice ref="formRef" ...>       │
│   — header fields show parsed values             │
│   — bottom shows pre-filled import rows          │
├─────────────────────────────────────────────────┤
│ [Acceptar i crear]    [Cancel·lar]               │
└─────────────────────────────────────────────────┘
```

Key implementation points:

- **Plain `<input type="file" accept="application/pdf">`** — NOT `FileEntityPicker` (which requires a parent entity id, confirmed against `frontend/src/components/FileEntityPicker.vue`).
- Calls `purchaseInvoiceIngestion.ingest(file)`. While uploading: spinner on the button via PrimeVue `Button.loading`.
- Maps response: calls `purchaseInvoiceStore.setFromIngestion(payload)` then `formRef.value.calcAmounts()` once (`FormPurchaseInvoice.vue:251`).
- "Acceptar i crear" → triggers `formRef.value.submitForm()` — that triggers Yup validation, normalizes dates, calls `store.Create(...)`, and the existing PostCreate handler navigates to `/purchaseinvoice/:newId`.
- "Cancel·lar" → `router.push({ name: 'PurchaseInvoices' })`.
- All toasts use `globalToast` with Catalan strings; error mapping (502 vs 422 vs 503) read from the `GenericResponse.errors[0]` so the operator sees the localized key from the backend verbatim.

### Routes & navigation

`modules/purchase/routes.ts` (insertion near `purchaseinvoice` list/detail):

```typescript
const ImportPurchaseInvoice = () => import("./views/ImportPurchaseInvoice.vue");

// inside the export default [...]
{
  path: "/purchaseinvoice/import",
  name: "PurchaseInvoiceImport",
  component: ImportPurchaseInvoice,
  meta: { helpKey: "purchase/purchaseinvoice/import" },
},
```

Toolbar in `views/PurchaseInvoices.vue` (alongside `TableFilter`):
```vue
<Button
  label="Importar factura (PDF)"
  icon="pi pi-file-pdf"
  class="p-button-sm ml-2"
  @click="$router.push({ name: 'PurchaseInvoiceImport' })"
/>
```

## Commit Strategy

| # | Type | Subject | Why now |
|---|------|---------|---------|
| 1 | chore(backend) | `add IInvoiceIngestionService contract + DTOs + IngestionSettings` | Contract-first; build is green without impl. |
| 2 | feat(backend) | `add LlamaParseInvoiceIngestionService + payload mapper` | Impl + POC decision in commit message body. |
| 3 | feat(backend) | `wire Ingest endpoint into PurchaseInvoiceController + localization keys + DI` | Endpoint becomes reachable; Swagger shows it. |
| 4 | feat(frontend) | `add PurchaseInvoiceIngestionService + setFromIngestion store action + types` | Data layer first; view not yet wired. |
| 5 | feat(frontend) | `add ImportPurchaseInvoice view + lazy route + toolbar button` | UI ships; manual smoke possible. |

**Commit count: 5. Estimated total diff: ~667 lines.** Well under the 800 line budget; no chained-PR slicing required.

## Decisions Locked in This Phase (not in proposal/spec)

| Decision | Rationale |
|----------|-----------|
| **Mapper lives in `Application/` (not `Application/Ingestion/`)** so service + mapper share the same project and don't cross the contracts boundary. | Follows `GeolocalizationService` precedent. |
| **Folder layout under `Ingestion/`** rather than `Services/Purchase/Ingestion/`. | Future ingestion targets (statements, expenses) reuse the folder without rename; isolates the new vertical slice's blast radius. |
| **`HttpClient.Timeout` set per-deployment** via `Ingestion__TimeoutSeconds`, not hard-coded. | Operators can raise above 90 s for slow document sources. |
| **Ingestion failure modeled as a single `IngestionException`** with a `Kind` enum, mapped by the controller. | Cleaner than 4 separate exception types; matches existing service patterns. |
| **Tax catalog lookup is by `Percentatge` exact match, not fuzzy / range**. | Spec is explicit. If multiple rows share a percentage, take the lowest `Id`; operator can correct. |
| **Header `taxBreakdown[]` rounding not normalized** in MVP — pass through the provider's decimals as-is and let `calcAmounts()` recompute the header sum from the rows. | Backend does not pre-derive `TaxAmount` from `BaseAmount × Percentatge` — only verifies the row exists in `Tax`. The form's existing `getTaxAmountFromImports` is the source of truth for `taxAmount` on the header. |
| **Surcharge check is whole-response, not per-row**. The moment any `taxBreakdown[i].surchargeRate` is present, return 422. | Maps cleanly to the scenario ("spec is honest that `PurchaseInvoiceImport` carries no surcharge column"). |
| **SupplierId stays empty after prefill** — frontend resolves by VatNumber against `purchaseMasterData.masterData.suppliers` in-memory and sets it pre-save if found. | Avoids backend coupling and matches the proposal's "no auto-create; user picks" stance. |
| **HelpKey `purchase/purchaseinvoice/import` registered** in the new route, but the actual help doc lives in `frontend/docs/purchase/purchaseinvoice/import.md` and is **out of scope for this PR** — placeholder meta only. | Helps prevent a forgotten entry once docs land. |

## Risks Specific to Implementation

| Risk | Mitigation |
|------|------------|
| **Sync-over-async** in the ASP.NET request thread while polling `parsing/job/{id}` for up to 90 s. | Configure `HttpClient.Timeout = 90 s`; backend awaits via `await httpClient.SendAsync(...)` (truly async). If `Kestrel` warnings surface during apply, **defer to** an async job pattern — explicitly listed as Out of MVP in the proposal. |
| **OCelot sync warnings on Kestrel** | Not currently configured; confirm via `Program.cs` reading during apply. |
| **LlamaParse `/extract` returns arrays inside arrays** that our flat DTO can't absorb. | Fallback path (`/parsing/create` + Markdown heuristic) is in scope from day 1; mapper routes JSON shape via runtime type checks. |
| **Provider returns non-numeric amounts** (e.g. `"1.234,56"` Spanish locale). | Mapper rounds to `decimal` culture-invariant; if parsing fails → 422 `ProviderUnparseable`. |
| **Frontend `purchaseInvoiceImports` mutation during an open dialog race**. | Apply flow uses `storeToRefs` + `setFromIngestion` (single mutation point) before opening the review, never mid-edit. |
| **`FormPurchaseInvoice` `hasBeenMounted` gate** on `calcAmounts` (line 252) silently skips if called too early. | Set a 500 ms `setTimeout` in `ImportPurchaseInvoice.vue.onMounted` before calling `calcAmounts()` — or, better, add an explicit `expose({ calcAmounts: () => { hasBeenMounted = true; return calcAmountsImpl(); }})` wrapper. Decision left to apply phase (prefer the wrapper for testability). |
| **Backend route attribute casing** — controllers use `[Route("api/[controller]")]` so the URL is `api/PurchaseInvoice` (controller class is PascalCase). Confirmed `service.ts` already posts to `/PurchaseInvoice/Ingest` (camelCase removed) — the prefix is `/PurchaseInvoice` literal. Apply phase must verify the actual URL the controller uses by reading `Program.cs`/route attribute once. | Resolved before apply starts. |

## References

- `openspec/changes/supplier-invoice-pdf-import/spec.md` — the WHAT (authoritative for behavior).
- `openspec/changes/supplier-invoice-pdf-import/proposal.md` — the WHY.
- Engram:
  - `sdd/supplier-invoice-pdf-import/explore` (id 458)
  - `sdd/supplier-invoice-pdf-import/proposal` (id 459)
  - `sdd/supplier-invoice-pdf-import/spec` (id 461)
  - `architecture/purchase-invoice-data-model` (id 462) — `PurchaseInvoiceImports` is the line-item equivalent.
  - `architecture/purchase-invoice-recalc-flow` (id 463) — totals are recomputed client-side via `form.calcAmounts()`.
- Existing patterns confirmed: `GeolocalizationService` (HttpClient wiring), `PurchaseInvoiceService.Create` (no recalc), `FormPurchaseInvoice.calcAmounts()` (client-side totals), `PurchaseInvoiceController.Create` (existing route shape).

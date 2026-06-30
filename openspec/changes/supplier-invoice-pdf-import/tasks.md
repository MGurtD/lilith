# Tasks: Supplier Invoice PDF Import

> Change slug: `supplier-invoice-pdf-import` — single PR, 5 work-unit commits, ~667 LOC target (session review budget 800 lines).

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~667 LOC target; ~935 LOC upper estimate (PrimeVue/JSON boilerplate) |
| 400-line budget risk | Low (target 667 < session budget 800; well under default 400 too — but spec/design locked OTel + supplier auto-create + tests out) |
| Chained PRs recommended | No |
| Suggested split | single PR with 5 work-unit commits |
| Delivery strategy | single-pr |
| Chain strategy | size-exception (session budget = 800, target 667 fits) |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: size-exception
400-line budget risk: Low

### Suggested Work Units

| Unit | Goal | Likely PR | Notes |
|------|------|-----------|-------|
| 1 | Ingestion contract, DTOs, settings, mapper, impl, exception, contracts, DI, controller action, localization keys (backend) | PR 1 (commits 1–3 inside) | Three backend work-unit commits |
| 2 | Frontend data layer + view + route + toolbar | PR 1 (commits 4–5 inside) | Two frontend work-unit commits |

## Pre-Implementation Steps (apply phase MUST do these BEFORE T2)

These are NOT commits. They are preflight gates whose decisions propagate into T2 (provider wiring) and T5 (form wrapper).

### POC-1: LlamaParse endpoint selection

- **Question**: does `/api/extraction/run` (with structured invoice schema) return invoice-shaped fields, or do we fall back to `/api/parsing/create` + Markdown + regex?
- **Action**: with a real `Ingestion__ApiKey`, run a 5-minute console POC against both endpoints with one Spanish supplier invoice PDF. Document the response shapes.
- **Decision rule**: prefer `/extract` if it returns the fields our DTO expects. Otherwise fall back to `/parsing/create` + heuristic regex.
- **Where decision lives**: `openspec/changes/supplier-invoice-pdf-import/apply-progress.md` (created by apply phase BEFORE T2 commits).
- **Why pre-T2**: T2 hard-codes whichever path wins; ~180 LOC depend on this.

### POC-2: FormPurchaseInvoice `calcAmounts` wrapper exposure

- **Question**: how do we call `form.calcAmounts()` after prefill when it's gated by `hasBeenMounted` (false for 500 ms)?
- **Action**: in `frontend/src/modules/purchase/components/FormPurchaseInvoice.vue`, add to the existing `defineExpose({...})` block:
  ```ts
  calcAmountsNow(): void {
    hasBeenMounted.value = true;
    return calcAmounts();
  }
  ```
- **Where decision lives**: code comment in the form component explaining why the wrapper exists.
- **Why pre-T5**: T5 calls `formRef.value.calcAmountsNow()` after prefill; without it, header totals stay at 0 silently and the operator sees a broken-looking form.

## Sanity-Check Notes (apply phase verifies before T1)

- `AppSettings` shape (verified): optional nested sections with `Validate()` per section. Add `IngestionSettings? Ingestion` property (mirrors `Geolocalization`, `Geoapify`, `GitHub`, `OpenTelemetry`).
- `PurchaseInvoiceController` URL (verified): `[Route("api/[controller]")]` → endpoint becomes `/api/PurchaseInvoice/Ingest` (the class is `PurchaseInvoiceController`, not camelCased). DO NOT add a leading slash.
- Namespace (verified by precedent): the contract deviates from `Services/<Domain>/` and lives in `Application.Contracts/Ingestion/` (vertical slice). Implementation lives in `Application/Ingestion/`.
- Apply phase must also confirm `frontend/src/services/file.service.ts` binary upload pattern (NOT `BaseService<T>`) before writing T4.

## Task Index

| # | ID | Type | Subject | Files | LoC est. | Verification |
|---|----|------|---------|-------|----------|--------------|
| 1 | T1 | chore(backend) | IInvoiceIngestionService contract + DTOs + IngestionSettings | 5 new | ~140 | `dotnet build` green |
| 2 | T2 | feat(backend) | LlamaParseInvoiceIngestionService + payload mapper + IngestionException | 4 new | ~345 | `dotnet build` green; POC-1 recorded |
| 3 | T3 | feat(backend) | Wire Ingest endpoint + DI + localization keys | 5 mod | ~85 | Swagger shows endpoint; 200/400/422/502/503 smoke |
| 4 | T4 | feat(frontend) | PurchaseInvoiceIngestionService + setFromIngestion store action + types | 1 new + 3 mod | ~85 | typecheck + build green |
| 5 | T5 | feat(frontend) | ImportPurchaseInvoice view + lazy route + toolbar button | 1 new + 2 mod | ~280 | typecheck + build green; UI smoke recorded |

**Total: 5 commits, ~935 LOC upper, ~667 LOC target.** Upper estimate includes blank lines, imports, PrimeVue/JSON boilerplate; target is the design's stated floor.

## Dependency Graph

```
POC-1 (preflight) ──> T2 ──> T3 ──┐
                                    ├──> T5 ──> (single PR merge)
POC-2 (preflight) ─────────────────┘
                                   ↑
                                   └─> T4 (independent, ordered before T5 for review focus)
```

Order rationale: T1 is a no-op build-wise (interface only); merge it first so subsequent commits compile against the contract. T2 hard-codes POC-1. T3 makes the endpoint reachable. T4 lays frontend data. T5 ties it all together (depends on POC-2 wrapper).

## Tasks

### T1 — Backend: ingestion contract + DTOs + settings

**Files (new):**
- `backend/src/Application.Contracts/Ingestion/IInvoiceIngestionService.cs` — one method `IngestAsync(Stream, string, CancellationToken)`
- `backend/src/Application.Contracts/Ingestion/IngestPurchaseInvoiceResponse.cs` — header + `TaxBreakdown` + `Confidence`
- `backend/src/Application.Contracts/Ingestion/TaxBreakdownRow.cs` — `TaxRate`, `BaseAmount`, `TaxAmount`, `TaxId`, `Confidence`
- `backend/src/Application.Contracts/Ingestion/ConfidenceMap.cs` — `Headers` dict + `Lines` list (empty in MVP)
- `backend/src/Application.Contracts/Configuration/IngestionSettings.cs` — `ApiKey`, `BaseUrl` (default), `DefaultModel` (default), `TimeoutSeconds` (default 90), `Validate()`

**Files (modified):**
- `backend/src/Application.Contracts/Configuration/AppSettings.cs` — add `IngestionSettings? Ingestion { get; set; }` and call `Ingestion?.Validate()` inside `Validate()`

**Out of scope:** any `PurchaseInvoiceLines`-style collection. The DTO carries `TaxBreakdown[]` only.

**Verification:** `dotnet build` from `backend/` returns zero errors.

**Commit subject:** `chore(backend): add IInvoiceIngestionService contract + DTOs + IngestionSettings`

### T2 — Backend: LlamaParse implementation + payload mapper + exception type

**Pre-condition:** POC-1 result documented in `openspec/changes/supplier-invoice-pdf-import/apply-progress.md`.

**Files (new):**
- `backend/src/Application/Ingestion/LlamaParseInvoiceIngestionService.cs` — HTTP flow per POC-1 winner. Constructor: `(HttpClient httpClient, IOptions<AppSettings> options, ILogger<LlamaParseInvoiceIngestionService> logger)`. Calls `/api/files/upload` then either `/api/extraction/run` or `/api/parsing/create` + polling.
- `backend/src/Application/Ingestion/LlamaParsePayloadMapper.cs` — provider JSON → `IngestPurchaseInvoiceResponse`. Resolves `TaxId` server-side via `unitOfWork.Taxes.Find(t => t.Percentatge == rate)`. Throws `IngestionException(UnknownTaxRate)` on miss. Throws `IngestionException(SurchargeUnsupported)` when any row has `surchargeRate`.
- `backend/src/Application/Ingestion/IngestionException.cs` — class with `IngestionFailureKind` enum (`ProviderAuthFailed`, `ProviderUnparseable`, `ProviderUnavailable`, `ProviderNotConfigured`, `UnknownTaxRate`, `SurchargeUnsupported`) and `Message`.
- `backend/src/Application/Ingestion/LlamaParseContracts.cs` — internal DTOs that mirror the provider's JSON shape (NOT exposed via `Application.Contracts`).

**POC-1 branching:**
- If `/extract` works → `LlamaParseContracts.cs` models the structured response directly; `LlamaParsePayloadMapper.cs` flattens it.
- If `/parsing/create` → `LlamaParseContracts.cs` models the Markdown response (a single block of text); `LlamaParsePayloadMapper.cs` extracts fields with regex.

**Verification:**
- `dotnet build` zero errors.
- POC console output: `apply-progress.md` shows the raw response from one real PDF, with the parsed fields the mapper produces.
- Unit-style smoke (no test project): a `Console.WriteLine` block in `Program.cs` (temporarily, removed before commit) calls the service with a real PDF and prints the `IngestPurchaseInvoiceResponse`.

**Commit subject:** `feat(backend): add LlamaParseInvoiceIngestionService + payload mapper + IngestionException`

### T3 — Backend: wire endpoint + DI + localization keys

**Files (modified):**
- `backend/src/Api/Setup/ApplicationServicesSetup.cs` (+2 LOC) — `services.AddHttpClient<IInvoiceIngestionService, LlamaParseInvoiceIngestionService>(...)` with `BaseAddress` and `Timeout` configured from `IOptions<AppSettings>.Ingestion`.
- `backend/src/Api/Controllers/Purchase/PurchaseInvoiceController.cs` (+55 LOC) — add `IInvoiceIngestionService` to the primary constructor, add `Ingest` action:
  ```csharp
  [HttpPost("Ingest")]
  [Consumes("multipart/form-data")]
  [RequestSizeLimit(20 * 1024 * 1024)]
  public async Task<IActionResult> Ingest([FromForm] IFormFile pdfFile, CancellationToken ct)
  ```
  Maps `IngestionFailureKind` to the HTTP status codes per `design.md` §Error mapping table.
- `backend/src/Api/Resources/LocalizationService/ca.json` + `es.json` + `en.json` (+12 keys each) — `InvalidFileType`, `ProviderAuthFailed`, `ProviderUnparseable`, `ProviderUnavailable`, `ProviderNotConfigured`, `UnknownTaxRate`, `SurchargeNotSupportedInScope`, plus `InvoiceIngestProviderNotConfigured` if needed for the missing-API-key path.

**Code comment on the action:** lists the env-vars (`Ingestion__ApiKey`, `Ingestion__BaseUrl`, `Ingestion__DefaultModel`, `Ingestion__TimeoutSeconds`) so future operators can find them.

**Verification:**
- `dotnet build` zero errors.
- `dotnet run --project src/Api` starts cleanly without `Ingestion__ApiKey` (no startup crash).
- Swagger UI shows `POST /api/PurchaseInvoice/Ingest`.
- Manual `curl` with a real PDF returns 200 + populated DTO.
- Manual `curl` with a non-PDF returns 400 `InvalidFileType`.
- Manual `curl` without `Ingestion__ApiKey` returns 503 `ProviderNotConfigured`.

**Commit subject:** `feat(backend): wire Ingest endpoint into PurchaseInvoiceController + DI + localization keys`

### T4 — Frontend: data layer + store action + types

**Files (new):**
- `frontend/src/modules/purchase/services/invoice-ingestion.service.ts` (~35 LOC) — `PurchaseInvoiceIngestionService` class. Single method `ingest(file: File): Promise<IngestPurchaseInvoiceResponse | undefined>`. POSTs `multipart/form-data` to `/PurchaseInvoice/Ingest`. Does NOT extend `BaseService<T>` (mirrors `FileService.upload()` pattern because payload is binary).

**Files (modified):**
- `frontend/src/modules/purchase/types/index.ts` (+12 LOC) — add `IngestPurchaseInvoiceResponse`, `TaxBreakdownRow`, `ConfidenceMap` interfaces.
- `frontend/src/modules/purchase/services/index.ts` (+2 LOC) — register `PurchaseInvoiceIngestion: new PurchaseInvoiceIngestionService()`.
- `frontend/src/modules/purchase/store/purchaseInvoices.ts` (+35 LOC) — add `setFromIngestion(payload)` action:
  1. `this.setNewPurchaseInvoice(getNewUuid())` (existing action; returns blank draft)
  2. Set header: `supplierNumber`, `purchaseInvoiceDate`, `transportAmount`, `extraTaxPercentatge`, `discountPercentage` from payload
  3. For each `payload.taxBreakdown` row: push `PurchaseInvoiceImport` with `id: getNewUuid()`, `taxId: row.taxId`, `baseAmount: row.baseAmount`, `taxAmount: row.taxAmount`, `netAmount: row.baseAmount + row.taxAmount`
  4. Leave `supplierId` empty; the view may optionally auto-match by `VatNumber` against `purchaseMasterData.masterData.suppliers`

**Verification:**
- `pnpm run typecheck` zero errors.
- `pnpm run build` zero errors.
- One-off dev-tools exercise: `await store.setFromIngestion(samplePayload)` from the browser console confirms the prefill mapping produces the right rows.

**Commit subject:** `feat(frontend): add PurchaseInvoiceIngestionService + setFromIngestion store action + types`

### T5 — Frontend: view + lazy route + navigation entry

**Pre-condition:** POC-2 wrapper merged (already in place from preflight).

**Files (new):**
- `frontend/src/modules/purchase/views/ImportPurchaseInvoice.vue` (~280 LOC) — view structure:
  ```
  <template>
    <Toolbar><Button label="Importar factura (PDF)" loading=isUploading /></Toolbar>
    <input type="file" accept="application/pdf" @change=onFileSelected />
    <FormPurchaseInvoice ref="formRef" :purchaseInvoice="purchaseInvoice" v-if="result">
      ... header fields + pre-filled imports table ...
    </FormPurchaseInvoice>
    <Button label="Acceptar i crear" @click=onAccept />
    <Button label="Cancel·lar" @click=onCancel severity="secondary" />
  </template>
  ```
  - Calls `store.uploadPdf(file)` → store calls `service.ingest(file)` → maps error to toast via `globalToast`.
  - On success: calls `store.setFromIngestion(payload)` then `formRef.value.calcAmountsNow()`.
  - "Acceptar i crear" → `formRef.value.submitForm()` (existing) → existing `PurchaseInvoiceService.Create` → navigation to `/purchaseinvoice/:newId`.
  - "Cancel·lar" → `router.push({ name: 'PurchaseInvoices' })`.

**Files (modified):**
- `frontend/src/modules/purchase/routes.ts` (+9 LOC) — lazy route:
  ```ts
  {
    path: "/purchaseinvoice/import",
    name: "PurchaseInvoiceImport",
    component: () => import("./views/ImportPurchaseInvoice.vue"),
    meta: { helpKey: "purchase/purchaseinvoice/import" },
  }
  ```
- `frontend/src/modules/purchase/views/PurchaseInvoices.vue` (+5 LOC) — toolbar button "Importar factura (PDF)" routing to `PurchaseInvoiceImport`.

**Verification:**
- `pnpm run typecheck` zero errors.
- `pnpm run build` zero errors.
- Manual UI smoke (record in `apply-progress.md`):
  1. Navigate to `/purchaseinvoice/import`.
  2. Select a real PDF.
  3. Confirm error toast on failure.
  4. Confirm `purchaseInvoiceImports[]` populated on success.
  5. Confirm header `BaseAmount` / `TaxAmount` / `NetAmount` non-zero immediately (proves POC-2 wrapper works).
  6. Click "Acceptar i crear" → existing save flow runs → navigation to `/purchaseinvoice/:newId` with a localized success toast.
  7. Click "Cancel·lar" → no PurchaseInvoice created; navigation back.

**Commit subject:** `feat(frontend): add ImportPurchaseInvoice view + lazy route + toolbar button`

## Risk-Adjusted Verification Checklist (apply phase runs at the end)

- [ ] `dotnet build` zero errors after each backend commit
- [ ] `dotnet ef database update --project src/Infrastructure/` no-op (no migrations introduced)
- [ ] `pnpm run typecheck` zero errors after each frontend commit
- [ ] `pnpm run build` zero errors at the end
- [ ] Swagger shows `POST /api/PurchaseInvoice/Ingest`
- [ ] Real PDF → 200 + populated DTO + header amounts visible in form post-prefill
- [ ] Non-PDF file → 400 with localized `InvalidFileType`
- [ ] Missing `Ingestion__ApiKey` env-var → 503 `ProviderNotConfigured` (no startup crash)
- [ ] Tax rate not in catalog → 422 `UnknownTaxRate` listing offending rates
- [ ] `git log --oneline` shows exactly 5 commits in dependency order
- [ ] `git diff --stat main` shows ≤ 800 lines changed

## Out of Scope (deferred)

- Chunkr / second provider
- Side-by-side PDF review screen
- FileEntityPicker linking
- Verifactu hashing
- OCR scanned PDFs
- Batch upload
- Tests (TDD off, no infra)
- OTel Authorization redaction (removed per scope-decisions #468)
- Surcharge (recàrrec) — schema change deferred
- Supplier auto-create — operator picks manually
- Helper text beyond what's already in the spec
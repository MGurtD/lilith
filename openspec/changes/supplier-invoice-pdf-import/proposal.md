# Proposal: Supplier Invoice PDF Import

> Change slug: `supplier-invoice-pdf-import` — issue #78 — MVP PDF ingestion for purchase invoices.

## Why

Operators currently retype supplier invoice data by hand from PDFs into the `PurchaseInvoice` form. Each invoice has ~12 numeric and date fields plus supplier matching, so transcription errors are common and the `Number` counter is allocated only after a manual save. Two open problems amplify this: #58 (purchase invoice supplier matching) and #59 (purchase invoice amount reconciliation) both rely on clean header data, which is exactly what a PDF parser can deliver.

The MVP ships a single end-to-end pipeline: user uploads a PDF, the backend calls LlamaParse to extract a structured header, the frontend pre-fills the existing reactive `FormPurchaseInvoice`, the operator reviews and saves. No new infrastructure, no schema migrations, no extra providers.

## What Changes

### In Scope (MVP)

- Backend: `IInvoiceIngestionService` contract (Application.Contracts) + `LlamaParseInvoiceIngestionService` impl (Application) registered via `AddHttpClient<TInterface, TService>()` in `ApplicationServicesSetup.cs`, mirroring `GeolocalizationService` / `GitHubProxyService` / `GeoapifyService`.
- Backend: DTOs `IngestPurchaseInvoiceRequest` (PDF bytes + filename + content-type) and `IngestPurchaseInvoiceResponse` (header fields + per-field confidence + raw provider metadata).
- Backend: `POST /api/PurchaseInvoice/Ingest` endpoint on the existing `PurchaseInvoiceController` — **NOT `/Import`** (that route is already mapped to the tax-record import flow on `PurchaseInvoiceImport`).
- Config via env-vars only (`Ingestion__ApiKey` / `Ingestion__BaseUrl` / `Ingestion__DefaultModel`). No new `appsettings.json` files.
- LlamaParse strategy: try `/extract` (schema-driven, preferred); fall back to `/parsing/create` + Markdown + local regex heuristic if the structured endpoint shape is wrong. POC decision during apply, documented in `apply-progress`.
- Frontend: `ImportPurchaseInvoice.vue` view with a plain `<input type="file" accept="application/pdf">` upload, calls the ingest endpoint, and pre-fills `FormPurchaseInvoice` reactively when the operator accepts.
- Frontend: small `PurchaseInvoiceIngestionService` (raw axios call, NOT extending `BaseService<T>` because the payload is binary), registered in `frontend/src/modules/purchase/services/index.ts`.
- Frontend: lazy route `/purchaseinvoice/import` in `frontend/src/modules/purchase/routes.ts`; toolbar button in `PurchaseInvoices.vue` with Catalan label "Importar factura (PDF)".
- Localization keys for the ingest path added to all three files (`ca.json`, `es.json`, `en.json`).

### Out of Scope (deferred)

- Chunkr or any second provider
- Side-by-side review screen with PDF viewer + bounding-box / confidence highlighting
- `FileEntityPicker` linking (the PDF is uploaded with the request, not persisted after save)
- Verifactu hashing or any Verifactu integration
- OCR for paper scans (scanned PDFs still won't work — must be digital PDFs)
- Batch / bulk upload
- Tests (TDD off, no test infra project-wide — verification via Swagger + manual UI walk-through)
- Supplier auto-creation when LlamaParse returns an unknown VatNumber — leave `SupplierId` null, user picks manually
- Auto-counter pre-fill — `Number` is still assigned by `IExerciseService.GetNextCounter` on save to avoid collisions

## Impact

### Affected Files (anticipated — refined during design phase)

**Backend — new files**

- `backend/src/Application.Contracts/Services/Purchase/IInvoiceIngestionService.cs`
- `backend/src/Application.Contracts/Services/Purchase/IngestPurchaseInvoiceRequest.cs`
- `backend/src/Application.Contracts/Services/Purchase/IngestPurchaseInvoiceResponse.cs` (+ `IngestedLineItem` DTO + `FieldConfidence` DTO if needed)
- `backend/src/Application.Contracts/Configuration/IngestionSettings.cs`
- `backend/src/Application/Services/Purchase/LlamaParseInvoiceIngestionService.cs`

**Backend — modified files**

- `backend/src/Application.Contracts/Configuration/AppSettings.cs` — add optional `IngestionSettings` section with `Validate()`.
- `backend/src/Api/Setup/ApplicationServicesSetup.cs` — add `services.AddHttpClient<IInvoiceIngestionService, LlamaParseInvoiceIngestionService>();`
- `backend/src/Api/Controllers/Purchase/PurchaseInvoiceController.cs` — add `[HttpPost("Ingest")]` action.
- `backend/src/Api/Resources/LocalizationService/ca.json` + `es.json` + `en.json` — add ingest keys.

**Frontend — new files**

- `frontend/src/modules/purchase/services/invoice-ingestion.service.ts`
- `frontend/src/modules/purchase/views/ImportPurchaseInvoice.vue`

**Frontend — modified files**

- `frontend/src/modules/purchase/services/index.ts` — register `PurchaseInvoiceIngestionService`.
- `frontend/src/modules/purchase/store/purchaseInvoices.ts` — add `setFromIngestion(result)` action.
- `frontend/src/modules/purchase/types/index.ts` — add `IngestPurchaseInvoiceResponse` interface.
- `frontend/src/modules/purchase/routes.ts` — add lazy route `purchaseinvoice/import`.
- `frontend/src/modules/purchase/views/PurchaseInvoices.vue` — toolbar button "Importar factura (PDF)".

### Dependencies

- **No new NuGet packages**: `HttpClient` ships with `Backend.Api`. LlamaParse has no .NET SDK.
- **No new pnpm packages**.
- Optional future: `Microsoft.Extensions.Http.Polly` for resilience — out of MVP.

### Configuration (env-vars only, no new files)

| Env var | Required | Default | Purpose |
|---------|----------|---------|---------|
| `Ingestion__ApiKey` | yes | — | LlamaParse bearer token |
| `Ingestion__BaseUrl` | no | `https://api.cloud.llamaindex.ai` | Provider base URL |
| `Ingestion__DefaultModel` | no | `llama-parse` | Model identifier |
| `Ingestion__TimeoutSeconds` | no | `90` | HTTP client + request budget |

`.NET` env-var convention: `Ingestion__ApiKey` reads the `Ingestion:ApiKey` config key.

### Migration / Database

None. MVP writes to no new tables. (File attachment is part of out-of-scope.)

## Alternatives Considered

1. **Chunkr as primary provider** — rejected for MVP; licensing overhead for an unproven integration. Revisit when bbox / review work begins.
2. **Docling self-hosted** — rejected; adds a service to operate with no native schema extraction.
3. **Mistral OCR-only** — rejected; OCR is necessary but not sufficient — we need structured fields, not text.
4. **Manual paste + browser extension** — rejected; the goal is "upload PDF, get a draft", not assistive copy/paste.
5. **Reusing `PurchaseInvoiceImport` controller action** — rejected; route is already mapped to tax-record import flow.
6. **Multi-provider abstraction up front** (`IInvoiceIngestionProvider` + Chunkr slot) — rejected; YAGNI for a one-provider MVP. Replace the impl later when Chunkr lands.
7. **Async job pattern** (`/Ingest` returns 202 + jobId, `/Ingest/{jobId}` polls) — rejected for MVP; doubles client complexity. Adopt only if Kestrel sync-over-async warnings surface during apply.

## Approach

Reuse the existing `AddHttpClient<TInterface, TService>` pattern verbatim from `GeolocalizationService` (canonical reference) — typed `HttpClient` injected by the factory, `IOptions<AppSettings>` for the optional config section, `ILogger<T>` for traces. The constructor signature is `(HttpClient httpClient, IOptions<AppSettings> options, ILogger<LlamaParseInvoiceIngestionService> logger)`. The service is registered in `ApplicationServicesSetup.cs` alongside the three existing external services.

Request flow per ingest call:

```
Browser -> POST /api/PurchaseInvoice/Ingest (multipart, IFormFile)
        -> PurchaseInvoiceController.Ingest()
        -> IInvoiceIngestionService.IngestAsync(file)
                -> LlamaParse: POST /files/create  (upload bytes)
                -> LlamaParse: POST /extract       (structured fields) — preferred
                   OR fallback: POST /parsing/create + GET /parsing/jobs/{id} (poll, Markdown)
                -> map response to IngestPurchaseInvoiceResponse
        <- 200 OK with header + confidence JSON
Frontend parses JSON -> calls store.setFromIngestion(result) -> navigates to /purchaseinvoice/:newId
```

`PurchaseInvoice` has no line-item collection (confirmed by reading the entity — only `PurchaseInvoiceDueDates` for installments and `PurchaseInvoiceImports` for tax breakdowns), so the DTO carries header fields only.

## Capabilities

### New Capabilities

- `supplier-invoice-pdf-import`: end-to-end PDF ingestion of supplier invoices — backend service contract, controller endpoint, frontend view + service + store action, and the pre-fill-to-save flow.

### Modified Capabilities

- None at the spec level. The existing `table-component` spec is unaffected. No existing `PurchaseInvoice` spec exists in `openspec/specs/`; this change introduces the first one.

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Route collision with `/api/PurchaseInvoice/Import` | Low | Use `/Ingest` (confirmed not used). |
| `/extract` endpoint doesn't return invoice-shaped fields | Medium | POC during apply; fall back to `/parsing/create` + Markdown regex. Decision logged in `apply-progress`. |
| LlamaParse latency exceeds 90s default | Medium | Configure `HttpClient.Timeout = TimeSpan.FromSeconds(90)`; raise via env-var if needed. Document limit. |
| `appsettings.json` base file absent in repo | Low | Operators inject env-vars; README note in apply-phase docs commit. |
| Supplier auto-match by VatNumber fails | High (expected) | Leave `SupplierId` null; operator picks manually. No auto-create in MVP. |
| Sync-over-async in ASP.NET request thread | Low | Acceptable for MVP; monitor in apply. If warnings surface, refactor to job polling — out of MVP scope unless triggered. |
| `appsettings.Development.json` lacks Ingestion section | Low | Env-vars only — `IOptions<AppSettings>.Ingestion` returns null when section absent; service returns 503-style localized error. |

## Rollback Plan

1. Revert commits in the single PR. Order is additive; any single commit can be reverted without breaking the others.
2. Remove `AddHttpClient<IInvoiceIngestionService, LlamaParseInvoiceIngestionService>()` registration — service disappears from DI.
3. `PurchaseInvoiceController.Ingest` action removed — no route collision remains.
4. Frontend route `/purchaseinvoice/import` removed — toolbar button vanishes. No other view depends on it.
5. **No database migration to revert** (zero DB impact).

## Dependencies

- LlamaParse cloud account (free tier is enough for POC; ~10K credits/month).
- Bearer token delivered via `Ingestion__ApiKey` env-var in dev and prod environments.

## Success Criteria

- [ ] `dotnet build` passes with zero errors after each commit in the PR.
- [ ] `pnpm run typecheck` passes with zero errors.
- [ ] `POST /api/PurchaseInvoice/Ingest` accepts a real PDF and returns a structured JSON header in under 30s on the happy path.
- [ ] Loading a PDF into `ImportPurchaseInvoice.vue` populates at least the invoice date, supplier name/VAT, base amount, tax amount, net amount, and supplier invoice number into the `FormPurchaseInvoice` reactive model.
- [ ] `SupplierId` is auto-resolved by VatNumber match when the supplier exists; left null when it doesn't (operator picks).
- [ ] If `/extract` fails, the service falls back to `/parsing/create` and still returns a populated response (or a clearly localized 502 if both fail).
- [ ] If `Ingestion__ApiKey` is missing, the endpoint returns a localized `InvoiceIngestProviderNotConfigured` error (NOT a 500 stack trace).
- [ ] Manual UI walk-through recorded in `apply-progress`: upload sample PDF → review form → save → invoice persisted with assigned `Number`.

## Commit Strategy (work-unit commits, hint for tasks phase)

Approximately 5-6 commits inside the single PR (verify file counts in design phase):

1. `chore(backend): add IInvoiceIngestionService contract + DTOs + IngestionSettings`
2. `feat(backend): add LlamaParseInvoiceIngestionService implementation`
3. `feat(backend): wire Ingest endpoint into PurchaseInvoiceController + DI + localization keys`
4. `feat(frontend): add purchase-invoice-ingestion service + store action + types`
5. `feat(frontend): add ImportPurchaseInvoice view + lazy route + toolbar button`
6. `docs: document Ingestion__* env-vars in operator notes`

Each commit should compile + typecheck on its own. The split follows the work-unit-commits skill: contract first, then implementation, then wiring, then frontend in two slices (data layer, then view), then docs.
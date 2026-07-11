# Spec: Supplier Invoice PDF Import

## Purpose
Ingest a supplier invoice PDF, return a structured draft that pre-fills the existing `FormPurchaseInvoice` with: (a) header fields, (b) one `PurchaseInvoiceImport` row per distinct tax rate in the invoice (because `PurchaseInvoiceImports` is the tax-grouped line-item representation — there is no separate `PurchaseInvoiceLines` collection on the entity), and (c) a per-field `confidenceMap`. The form's `calcAmounts()` (already triggered when an import row is added/removed) SHALL also run once after prefill so header totals are coherent immediately, not zeroed. The operator reviews, edits, accepts, and saves through the existing `PurchaseInvoiceController.Create` flow. **Out of MVP**: Chunkr, side-by-side PDF review, `FileEntityPicker`, Verifactu, OCR scans, batch upload, supplier auto-create, automated tests.

## Requirements

### Requirement: Ingest endpoint accepts a PDF and returns a structured draft
The system SHALL expose `POST /api/PurchaseInvoice/Ingest` accepting `multipart/form-data` with field `pdfFile` (max ~20 MB, `application/pdf`). It SHALL return `IngestPurchaseInvoiceResponse` with header fields matching the `PurchaseInvoice` entity (supplier identity, `PurchaseInvoiceDate`, monetary fields `BaseAmount`..`ExtraTaxAmount`) plus a per-field `confidenceMap` (0.0–1.0). Errors SHALL be localized via `ILocalizationService`.

#### Scenario: valid PDF
- **GIVEN** `pdfFile` is `application/pdf` and `Ingestion__ApiKey` is valid
- **WHEN** the operator submits the upload
- **THEN** HTTP 200 with `IngestPurchaseInvoiceResponse`; `FormPurchaseInvoice` is pre-filled (editable)

#### Scenario: invalid file
- **GIVEN** `pdfFile` is `image/png`, `text/plain`, empty, or missing
- **WHEN** the operator submits the upload
- **THEN** HTTP 400 with the localized `InvalidFileType` key

#### Scenario: provider misbehaves
- **GIVEN** the provider returns 401/422/5xx or `Ingestion__TimeoutSeconds` (90 s default) elapses
- **WHEN** the provider responds
- **THEN** HTTP 502 (401/5xx/timeout) or 422 (unparseable) using localized keys `ProviderAuthFailed` / `ProviderUnparseable` / `ProviderUnavailable`; a log records upstream status + correlation id, never the API key

#### Scenario: `Ingestion__ApiKey` missing
- **GIVEN** the env-var is null, empty, or whitespace
- **WHEN** the operator submits the upload
- **THEN** HTTP 503 with the localized `ProviderNotConfigured` key; app does NOT crash on startup

### Requirement: Operator reviews and saves via the existing flow
After ingest, the operator MAY accept (calls `PurchaseInvoiceController.Create` → `PurchaseInvoiceService.Create`: `Number` is assigned by `IExerciseService.GetNextCounter(exerciseId, "purchaseinvoice")`, entity persisted with supplied header amounts, result is a `GenericResponse`) OR cancel.

#### Scenario: operator accepts and saves
- **GIVEN** a draft with valid header fields, pre-filled `purchaseInvoiceImports` rows (one per distinct tax rate), and a `SupplierId` selected
- **WHEN** the operator submits the existing form's save action
- **THEN** `PurchaseInvoiceService.Create` runs, `Number` is assigned by `IExerciseService.GetNextCounter(exerciseId, "purchaseinvoice")`, the invoice (header + `PurchaseInvoiceImports`) is persisted, `GenericResponse.Result == true`; the frontend navigates to `/purchaseinvoice/:id` with a localized success toast

#### Scenario: no `Supplier` matches the parsed VatNumber
- **GIVEN** the parsed `supplierVatNumber` does NOT match any `Supplier.VatNumber`
- **WHEN** the operator tries to save without picking a supplier
- **THEN** the form does NOT submit until a `Supplier` is selected; no auto-create happens

#### Scenario: operator cancels
- **GIVEN** a draft pre-filled from the provider
- **WHEN** the operator clicks "Cancel·lar"
- **THEN** no `PurchaseInvoice` is created

### Requirement: Ingest pre-fills the `PurchaseInvoiceImports` collection from the per-tax-rate breakdown

Because `PurchaseInvoiceImports` (rows of `BaseAmount` + `TaxAmount` + `NetAmount` keyed by `TaxId`) is the tax-grouped line-item representation in this entity, the ingest response SHALL include a `taxBreakdown[]` array — one element per distinct `taxRate` — each with `taxRate` (numeric, e.g. `21.0`), `baseAmount` (aggregate base for that rate), `taxAmount` (aggregate quota for that rate), and optional `surchargeRate`/`surchargeAmount` when the invoice applies recàrrec. The frontend SHALL pre-populate `purchaseInvoiceImports` by mapping each `taxBreakdown` element to the local `Tax` whose `Percentatge == taxRate`, then creating a `PurchaseInvoiceImport` row with `BaseAmount = baseAmount`, `TaxAmount = taxAmount`, `NetAmount = baseAmount + taxAmount`, and `TaxId` set to the resolved `Tax` id. After populating, the import view SHALL invoke the form's `calcAmounts()` once so header totals (`BaseAmount`, `TransportAmount`, `Subtotal`, `GrossAmount`, `NetAmount`, `ExtraTaxAmount`) are non-zero and visible to the operator immediately. The MVP SHALL NOT auto-create new `Tax` rows.

#### Scenario: provider returns one tax rate that exists locally
- **GIVEN** `taxBreakdown: [{ taxRate: 21, baseAmount: 100, taxAmount: 21 }]`
- **AND** a `Tax` with `Percentatge == 21` exists locally
- **WHEN** the ingest response is rendered
- **THEN** exactly one `PurchaseInvoiceImport` row is added, mapped to the matching `Tax`
- **AND** `form.calcAmounts()` runs once, populating the header `BaseAmount = 100` and `TaxAmount = 21` immediately

#### Scenario: provider returns multiple distinct tax rates
- **GIVEN** `taxBreakdown: [{ taxRate: 21, baseAmount: 100, taxAmount: 21 }, { taxRate: 10, baseAmount: 50, taxAmount: 5 }]`
- **AND** both rates exist as `Tax` rows
- **WHEN** the ingest response is rendered
- **THEN** exactly two `PurchaseInvoiceImport` rows appear, each mapped to the matching `Tax`
- **AND** `form.calcAmounts()` sums them, so header `BaseAmount = 150` and `TaxAmount = 26`

#### Scenario: provider returns a tax rate that has no matching `Tax` in the local catalog
- **GIVEN** `taxBreakdown` includes a rate not present in the local `Tax` catalog (e.g. `2.0` recàrrec shorthand)
- **WHEN** the ingest response is parsed before mapping
- **THEN** the ingest endpoint returns HTTP 422 with the localized `UnknownTaxRate` key, listing the offending rate(s) in the response body
- **AND** the form is NOT pre-filled (no half-state) and the operator sees a clear localized error

#### Scenario: provider returns surcharge (recàrrec) data
- **GIVEN** a `taxBreakdown` element includes `surchargeRate` and `surchargeAmount`
- **WHEN** the row is mapped
- **THEN** the ingest endpoint returns HTTP 422 with localized `SurchargeNotSupportedInScope`
- **AND** the spec is HONEST that `PurchaseInvoiceImport` carries no surcharge column and persisting recàrrec is a schema change deferred from MVP

#### Scenario: operator deletes a pre-filled import row
- **GIVEN** `purchaseInvoiceImports` has one pre-filled row from ingest
- **WHEN** the operator clicks the row's delete control
- **THEN** the existing `deleteInvoiceImport` flow runs, the row is removed
- **AND** `closeDialogAndCalcAmounts` calls `form.calcAmounts()`, recomputing header totals to reflect the new collection state

### Requirement: Configuration is delivered via env-vars only
The required configuration SHALL be delivered through environment variables. No new `Ingestion` section SHALL be committed to any `appsettings*.json` file (the base `appsettings.json` is absent from this repo). A code comment in `PurchaseInvoiceController.Ingest` SHALL list the env-var names.

#### Scenario: env-var overrides appsettings
- **GIVEN** `appsettings.Development.json` defines `Ingestion:DefaultModel: "llama-parse"` AND env sets `Ingestion__DefaultModel=llama-parse-v2`
- **WHEN** the runtime reads settings
- **THEN** `llama-parse-v2` is used; missing env-vars fall back to defaults (`https://api.cloud.llamaindex.ai`, `llama-parse`, `90`)

#### Scenario: integrator opens committed appsettings
- **WHEN** the integrator opens `appsettings.Development.json`
- **THEN** there is NO `Ingestion` section
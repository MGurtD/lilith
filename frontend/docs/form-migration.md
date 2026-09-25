# Form.vue Migration Tracker

> **Status**: In progress. L1–L6 in review as stacked PRs; next batch L7 (system and complex shared).
> **Created**: 2026-09-24 · **Owner**: mgurt
> **Procedure**: `.claude/skills/frontend-form/SKILL.md` ("Migrate A Legacy Form")

## Summary

The frontend has two form implementations:

- **`Form.vue`** (`src/components/forms/Form.vue`): declarative rows, Yup
  validation with inline errors, typed submit payload. Contract and limits are in
  `src/components/forms/README.md`.
- **Legacy**: hand-written PrimeVue markup validated by
  `src/utils/form-validator.ts` (`FormValidation`), manual error refs, an
  "invalid form" toast, and direct prop or Pinia mutation. Two outliers use other
  patterns: `RegisterForm.vue` (Vuelidate) and `LoginForm.vue` (manual checks).

Goal: every in-scope form uses `Form.vue`, `form-validator.ts` is deleted, and
Vuelidate is no longer needed.

This document is the single record of that migration. Section 2 tracks every
form; section 3 tracks every capability added to `Form.vue` and how it was
verified. Both sections are meant to be enough to write commit messages and PR
descriptions without rereading code.

## 1. Rules

### Stop rule

When a form needs a behavior that `Form.vue` cannot express with its current
contract or with an established pattern, **the migration stops**:

1. Mark the form `blocked (F-xx)` and the feature `confirmed`.
2. Implement the capability generically in `Form.vue`, `types.ts` and
   `README.md`, with no feature-specific business logic.
3. Migrate the blocking form in the same PR. It is the validation case for the
   feature.
4. Verify (see "Verification") and fill in the feature's entry in section 3.
5. Add the pattern to "Proven Patterns" in the `frontend-form` skill, remove the
   limitation from the README, and resume the batch.

Established patterns are **not** new features. These already cover:

| Need | Existing solution | Reference |
| --- | --- | --- |
| Feature selector, ColorPicker, IconPicker, markdown editor, `Dropdown*` | `Custom` field + `#field-<name>` slot | `FormLifecycleTag.vue` |
| Synchronous derived fields | Source-field `onChange` + `setFieldValue` | `FormReceiptDetail.vue` |
| Async lookups and cascades | Callback snapshot, request sequence, callback suppression | `FormReceiptDetail.vue` |
| Parent-owned collections next to the form | Stable scalar snapshot, merge at submit | `FormPurchaseInvoice.vue` |
| Reusable multi-field control | Row `section` with registered flat fields | `FormReceiptDetail.vue` |
| Enabled state from current values | `disabled: (values) => boolean` | README |
| Time-only dates | `Date` field with `props: { timeOnly: true, hourFormat: "24" }` (props pass through to `DatePicker`) | `Form.vue` `controlProps` |
| Simple cross-field rules | `Yup.ref` in field schemas | `FormTransportRate.vue` |

### Decisions

- **Inline validation replaces the toast in every migration.** Inline field
  errors replace the legacy "invalid form" toast and `FormValidation`. This is
  accepted globally and does not need to be re-approved per form.
- **Scope** covers business `Form*.vue` components, raw dialog forms inside
  tables and views, raw field blocks inside views, and the login/register forms.
- **Out of scope**: plant operator touch screens, grid/matrix editors, selection
  grids, utilities, read-only panels and filter bars (listed in section 2.9 with
  reasons).
- **PR granularity**: one commit per form. Forms that need no new feature ship in
  one PR per batch. Every new feature ships in its own PR together with the form
  that motivated it.
- **Language**: this document, the README and the skill are written in English.

### Per-form procedure

1. Set the row to `in progress`. For `med`/`high` forms, work through
   `src/components/forms/MIGRATION_TEMPLATE.md` (scratch audit, not committed
   unless it still holds open decisions).
2. Classify every behavior as supported, adaptable (slot, section, `onChange`)
   or unsupported. Unsupported → stop rule.
3. Migrate with the `frontend-form` skill: parents consume the submit payload,
   hidden fields and native dates are preserved, and `FormValidation`, the toast
   and dead imports are removed.
4. Verify, set the row to `migrated`, record the commit, and commit.
5. At the end of the batch run `pnpm run build`, open the batch PR, and add a
   line to the status log.

### Verification

- Every form: `pnpm run i18n:check`, `pnpm run typecheck`, and a
  `lilith-ui-tester` run covering create, edit, invalid submit, correction after
  invalid submit, valid save, cancel/dialog close, and a locale change, with
  screenshots. Writes are allowed against the staging database.
- Every feature: the above, plus `pnpm run build` and a regression run on
  `FormLifecycleTag` (Lifecycle screen) and `FormReceiptDetail` (Receipt screen).
- Every batch: `pnpm run build`.

## 2. Inventory

Status values: `pending` · `in progress` · `blocked (F-xx)` · `migrated` ·
`excluded`. Difficulty: `low` · `med` · `high`. Paths are relative to
`frontend/src/modules/` unless they start with `components/`.

### 2.1 Batch L1 — Shared masters

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SH-01 | `shared/components/FormPaymentMethod.vue` | `PaymentMethod.vue` | low | — | migrated | `f506840` | Flat fields; UI verified |
| SH-02 | `shared/components/FormTax.vue` | `Tax.vue` | low | — | migrated | `6206465`, fix `fddf84d` | UI verified after the fix; exposed the InputNumber double-binding bug fixed in `fddf84d` |
| SH-03 | `shared/components/FormReferenceType.vue` | `ReferenceType.vue` | low | — | migrated | `fc393c1` | UI verified (density PUT keeps colors); 2 ColorPickers via `Custom` slot; `sales/components/FormReference.vue` imports it but never renders it (dead import) |
| SH-04 | `shared/components/FormLifecycle.vue` | `Lifecycle.vue` | low | F-03 | migrated | `7251692` / #135 | Parent's own save replaced by `page-actions`; statuses kept out of the snapshot and merged at submit |
| SH-05 | `shared/components/FormExercise.vue` | `Exercise.vue` | med | — | migrated | `fdf0406` | UI verified (date-range error; PUT dates `2022-01-01T00:00:00.000Z`/`2022-12-31T00:00:00.000Z`); `Yup.ref` date range; submit-time date mutation removed (`toJSON` serializes identically) |

### 2.2 Batch L2 — Production plant model

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PR-01 | `production/components/FormEnterprise.vue` | `Enterprise.vue` | low | — | migrated | `eed879d` | |
| PR-02 | `production/components/FormArea.vue` | `Area.vue` | low | — | migrated | `11fb59b` | Reads Pinia via `storeToRefs` |
| PR-03 | `production/components/FormSite.vue` | `Site.vue` | med | — | migrated | `e76b64e` | `LocationFields` multi-field control → section; email rules |
| PR-04 | `production/components/FormWorkcenter.vue` | `Workcenter.vue` | low | — | migrated | `d8312bd` | Page form |
| PR-05 | `production/components/FormWorkcenterType.vue` | `WorkcenterType.vue` | low | — | migrated | `6a5e748` | Pinia state; % suffix |
| PR-06 | `production/components/FormWorkcenterCost.vue` | `WorkcenterCost.vue` | low | — | migrated | `e87e2c4` | Pinia state; currency |
| PR-07 | `production/components/TableWorkcenterLocations.vue` (add dialog) | `Workcenter.vue` | low | — | migrated | `8ccd24d` | 1 selector; manual checks + toast; duplicate check |
| PR-08 | `production/components/TableWorkcenterProfitPercentage.vue` (add dialog) | `Workcenter.vue` | low | — | migrated | `ac77bda` | 1 InputNumber; manual checks + toast; duplicate check |
| PR-09 | `production/components/FormOperator.vue` | `Operator.vue` | low | — | migrated | `5a84aa3` | |
| PR-10 | `production/components/FormOperatorType.vue` | `OperatorType.vue` | low | — | migrated | `1302dd8` | Pinia state; currency |
| PR-11 | `production/components/FormMachineStatus.vue` | `MachineStatus.vue` | med | — | migrated | `a1c574e` | ColorPicker + IconPicker slots, 6 checkboxes |
| PR-12 | `production/components/FormMachineStatusReason.vue` | `MachineStatuses.vue`, `TableMachineStatusReasons.vue` | med | — | migrated | `63926f1` | Yup `.test` unique code across records |
| PR-13 | `production/components/FormShift.vue` | `Shifts.vue` | low | — | migrated | `6f1e460` | |
| PR-14 | `production/components/FormShiftDetail.vue` | `Shifts.vue` | med | — | migrated | `6f1e460` | Time-only dates via `Date` props; `extractTime` at submit; empty legacy schema |
| PR-15 | `production/components/FormPhaseTemplate.vue` | `PhaseTemplate.vue` | low | — | migrated | `2f864d1` | |
| PR-16 | `production/components/FormPhaseTemplateDetail.vue` | `TablePhaseTemplateDetails.vue` | low | — | migrated | `2f864d1` | |
| PR-17 | `production/views/PhaseTemplates.vue` (create dialog) | — | low | — | migrated | `45a69f4` | 2 raw fields, no validation today |

### 2.3 Batch L3 — Warehouse and purchase leftovers

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| WH-01 | `warehouse/components/FormWarehouse.vue` | `Warehouse.vue` | low | — | migrated | `7fcaff0` | Computed Yup schema |
| WH-02 | `warehouse/components/FormLocation.vue` | `TableLocations.vue` | low | — | migrated | `3fdc510` | Computed Yup schema |
| WH-03 | `warehouse/components/FormInventoryNewMovements.vue` | `Inventory.vue` | med | — | migrated | `4403872` | `DropdownReference`, `DropdownWarehousesWithLocations`, `SelectorLot` keyed on reference |
| PU-01 | `purchase/components/TablePurchaseRates.vue` (duplicate dialog) | `Supplier.vue` | low | — | migrated | `356fc9a` | 1 text + 2 dates, no validation today |

### 2.4 Batch L4 — Production routings and work orders

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PR-18 | `production/components/FormWorkmaster.vue` | `Workmaster.vue` | med | F-02? | migrated | `beceff8` | SplitButton "calculate cost" next to Save |
| PR-19 | `production/views/Workmasters.vue` (create dialog) | — | low | — | migrated | `6003a06` | 1 selector |
| PR-20 | `production/views/Workmasters.vue` (copy dialog) | — | high | F-01 | migrated | `6003a06` | RadioButton switches between existing and new reference fields |
| PR-21 | `production/components/FormWorkmasterPhase.vue` | `WorkmasterPhase.vue`, `TableWorkmasterPhases.vue` | high | F-03? | migrated | `03e62ce` | Cascades (workcenter type/workcenter/external work/service reference), async profit lookup |
| PR-22 | `production/components/FormWorkmasterPhaseDetail.vue` | `WorkmasterPhase.vue` | low | — | migrated | `03e62ce` | |
| PR-23 | `production/components/FormWorkmasterPhaseBomItem.vue` | `WorkmasterPhase.vue` | low | — | migrated | `03e62ce` | `DropdownReference` slot |
| PR-24 | `production/components/FormWorkorder.vue` | `Workorder.vue` | med | F-02? | migrated | `7b9e7f1` | SplitButton exports; computed date range; lifecycle status dropdown |
| PR-25 | `production/components/FormCreateWorkorder.vue` | `Workorders.vue`, `sales/components/TableSalesOrderDetails.vue` | high | F-01 | migrated | `c2f9c94` | Lot code field shown when the selected work master's reference requires a lot |
| PR-26 | `production/components/FormWorkorderPhase.vue` | `WorkorderPhase.vue`, `TableWorkorderPhases.vue` | high | F-03? | migrated | `76225d4` | Same cascade pattern as PR-21 |
| PR-27 | `production/components/FormWorkorderPhaseDetail.vue` | `WorkorderPhase.vue` | low | — | migrated | `76225d4` | |
| PR-28 | `production/components/FormWorkorderPhaseBomItem.vue` | `WorkorderPhase.vue` | low | — | migrated | `76225d4` | `DropdownReference` slot |
| PR-29 | `production/components/FormProductionPart.vue` | `ProductionParts.vue` | med | — | migrated | `3b9bc7f` | Work order selection fills 3 dependent fields |
| PR-30 | `production/components/FormWorkOrderProductionPart.vue` | `Workorder.vue` | med | — | migrated | `7b9e7f1` | Filtered options; async `getWorkOrders` on change |

### 2.5 Batch L5 — Sales masters

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SA-01 | `sales/components/FormCustomerType.vue` | `CustomerType.vue` | low | — | migrated | `335755a` | |
| SA-02 | `sales/components/FormCustomerAddress.vue` | `TableCustomerAddresses.vue` | low | — | migrated | `a1fc991` | Dialog; clone row on edit |
| SA-03 | `sales/components/FormCustomerContact.vue` | `TableCustomerContacts.vue` | low | — | migrated | `22c17dd` | Dialog; clone row on edit |
| SA-04 | `sales/components/FormCustomer.vue` | `Customer.vue` | med | — | migrated | `387454d` | Pinia state |
| SA-05 | `sales/components/FormReference.vue` | `Reference.vue` | med | — | migrated | `dd91809` | Tax store options |

### 2.6 Batch L6 — Sales documents

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SA-06 | `sales/components/FormBudget.vue` | `Budget.vue` | med | — | migrated | `840531f` | Pinia state; lifecycle transitions dropdown |
| SA-07 | `sales/views/Budget.vue` (notes block) | — | low | — | migrated | `840531f` | Editable notes + read-only auto notes outside `FormBudget` |
| SA-08 | `sales/components/FormBudgetOrderDetail.vue` | `Budget.vue`, `SalesOrder.vue` | high | — | migrated | `840531f` | Embeds `TableWorkmasterProfit` (parent-owned collection) |
| SA-09 | `sales/components/FormBudgetTransport.vue` | `Budget.vue` | med | — | migrated | `840531f` | Async supplier/rate lookups |
| SA-10 | `sales/components/FormSalesOrder.vue` | `SalesOrder.vue` | high | — | migrated | `abcbe27` | 4 stores |
| SA-11 | `sales/components/FormSalesOrderTransport.vue` | `SalesOrder.vue` | med | — | migrated | `abcbe27` | Async lookups |
| SA-12 | `sales/components/FormCreateOrderOrInvoice.vue` | `SalesOrders.vue`, `DeliveryNotes.vue`, `Budgets.vue`, `SalesInvoices.vue` | med | — | migrated | `62614eb` | Reused by 4 screens; test every caller |
| SA-13 | `sales/components/FormDeliveryNote.vue` | `DeliveryNote.vue` | med | — | migrated | `7c1ab76` | `lockHeader`/`lockStatus` flags → disabled predicates |
| SA-14 | `sales/components/FormSalesInvoice.vue` | `SalesInvoice.vue` | med | — | migrated | `a007a24` | Lifecycle transitions dropdown |
| SA-15 | `sales/views/SalesInvoice.vue` (fiscal data block) | — | low | — | migrated | `a007a24` | 7 text fields + `DropdownCountry` outside `FormSalesInvoice` |
| SA-16 | `sales/components/FormSalesInvoiceDetail.vue` | `SalesInvoice.vue` | med | — | migrated | `a007a24` | |
| SA-17 | `sales/components/FormRectificativeInvoice.vue` | `SalesInvoice.vue` | med | — | migrated | `a007a24` | Quantity limited by `maximumQuantity` prop |

### 2.7 Batch L7 — System and complex shared

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SY-01 | `system/components/FormProfile.vue` | `Profile.vue` | low | — | pending | | |
| SY-02 | `system/components/FormApiKey.vue` | `ApiKeys.vue` | low | — | pending | | Date string ↔ `Date` boundary |
| SY-03 | `system/components/CreateUserForm.vue` | `Users.vue` | med | — | pending | | Password confirmation via `Yup.ref` |
| SY-04 | `system/components/FormUser.vue` | `User.vue` | high | F-02? | pending | | Mutates prop; 2 emits; embedded password sub-form; SplitButton actions |
| SY-05 | `system/components/FormMenuItem.vue` | `MenuItem.vue` | high | F-05 | pending | | One field per language, loaded asynchronously; IconPicker |
| SY-06 | `system/components/FormApplicationBranding.vue` | `ApplicationBranding.vue` | high | F-04? | pending | | 2 self-submitting `FileUpload` logos + brand name + palette radio |
| SH-06 | `shared/components/FormStatusTransition.vue` | `Lifecycle.vue` | med | — | pending | | Manual cross-field check outside the schema |
| SH-07 | `shared/components/FormStatus.vue` | `Lifecycle.vue` | high | — | pending | | Loads tags on mount; 2-argument emit with tag diff |
| SH-08 | `shared/components/FormSupportRequest.vue` | `TheSidebar.vue` | high | — | pending | | Markdown toolbar + live preview via slot; calls Pinia directly |
| SH-09 | `shared/views/ReferenceManagement.vue` | — | high | F-03?, F-04? | pending | | 3 tabbed sub-forms with separate submits + supplier dialog; `FileEntityPicker` |

### 2.8 Batch L8 — Authentication

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AU-01 | `components/forms/LoginForm.vue` | `Login.vue` | low | — | pending | | Manual empty checks + toast |
| AU-02 | `components/forms/RegisterForm.vue` | `Login.vue` | med | — | pending | | Vuelidate; password match via `Yup.ref` |

### 2.9 Excluded

| Component | Reason |
| --- | --- |
| `plant/components/workcenter-detail/*` (`MaterialConsumptionDialog`, `PhaseRejectionReasons`, `AvailableStockDialog`, `WorkcenterCommentEditor`, `PhaseQuantityForm`, `PhaseTemplateLoader`, `WorkOrderUnloader`), `plant/components/MachineStatusSelector.vue`, `plant/views/OperatorClockIn.vue` | Plant operator touch screens; out of scope by decision |
| `sales/components/TableWorkmasterProfit.vue` | Per-row grid editor, not a single-entity form |
| `purchase/views/PurchaseInvoice.vue` (due-date amount cell) | Per-row cell editing |
| `warehouse/views/Inventory.vue` (new quantity column) | Per-row bulk editing |
| `system/components/MenuItemTranslationMatrixDialog.vue` | Rows × languages matrix editor |
| `components/tables/TableViewConfig.vue` | Dynamic column list with drag-and-drop |
| `system/components/ProfileMenuAssignment.vue` | Selection grid, no fields |
| `system/views/DataMigration.vue` | Import/export utility |
| `system/views/ApiKeys.vue` (show-key dialog) | Read-only display |
| `sales/components/FormCustomerStatistic.vue` | Read-only statistics |
| Analytics dashboards, list views, `TableFilter.vue`, `shared/components/TableReferences.vue`, `Selector*.vue` | Filter bars and selectors, no entity submit |

### 2.10 Already migrated

| Component | Notes |
| --- | --- |
| `shared/components/FormLifecycleTag.vue` | Reference implementation |
| `production/components/FormRejectionReason.vue` | |
| `purchase/components/`: `FormCreatePurchaseDocument`, `FormExpense`, `FormExpenseType`, `FormInvoiceSerie`, `FormMaterial`, `FormOrder`, `FormOrderDetail`, `FormPurchaseInvoice`, `FormPurchaseInvoiceImport`, `FormPurchaseRate`, `FormPurchaseRateDetail`, `FormReceipt`, `FormReceiptDetail`, `FormSupplier`, `FormSupplierContact`, `FormSupplierReference`, `FormSupplierType`, `FormTransportRate`, `FormTransportRateDetail` | Commit `8f19c59` |

### 2.11 Totals

| Pending | Migrated | Excluded |
| --- | --- | --- |
| 68 (58 `FormValidation`/Vuelidate files + 10 raw forms) | 21 | see 2.9 |

Reconcile the inventory with:

```bash
# from frontend/src — legacy validator users (must shrink to 0, plus form-validator.ts itself)
grep -rlE "FormValidation|form-validator|useVuelidate" --include=*.vue --include=*.ts .
# Form.vue consumers
grep -rl "components/forms/Form.vue" --include=*.vue .
```

## 3. Form.vue Features

Status values: `candidate` (possible need, not proven) · `confirmed` (a form
is blocked on it) · `implemented` (merged and verified) · `rejected` (an
established pattern turned out to be enough).

| F | Capability | Motivated by | Status | Files | Verification | PR |
| --- | --- | --- | --- | --- | --- | --- |
| F-01 | Field visibility from current values, with matching conditional validation | PR-25, PR-20 | rejected | — | Section row rendered from slot `values` + Yup `.test` on `this.parent` (README pattern) covered both cases | — |
| F-02 | Secondary actions next to Save (split button) and `cancel` exposed to the `actions` slot | PR-18, PR-24 | rejected for screens | — | With `page-actions` the `actions` slot renders inside PageActions and receives `submit`; the SplitButton lives there (secondary action via a pending-action ref). Re-check for SY-04 (dialog/cancel needs) | — |
| F-03 | Unique native field IDs for simultaneous forms | SH-04 | implemented | `Form.vue`, `README.md`, custom slots in purchase forms and `FormRejectionReason.vue` | See entry below | `770c1ae` |
| F-04 | File upload field, or a documented pattern for self-submitting uploads next to a form | SY-06, SH-09 | candidate | | | |
| F-05 | Repeatable or dynamically generated field groups | SY-05 | candidate | | | |
| F-07 | Read current (unvalidated) values so a feature can persist in-progress edits with another save | PR-21, PR-26 | implemented | `Form.vue` (`getValues`), README, skill; `currentPhase()` in both phase forms | See entry below | `7cb2e10`, `f79be93` |

Candidates come from the inventory and are hypotheses. Confirm one only when
migrating its form proves that the patterns in section 1 are not enough;
otherwise mark it `rejected` and note the pattern used.

### F-03 — Per-instance native field IDs

- **Motivated by**: SH-04. `Lifecycle.vue` opens the `FormLifecycleTag` dialog
  over the migrated `FormLifecycle` screen form. Both have `name` and
  `description` fields, and `Form.vue` built ids from the field name alone
  (`form-field-name`), so the document had duplicate ids and the dialog labels
  targeted the screen inputs. The same collision already existed on Receipt and
  Order screens with their line dialogs.
- **Design**: `Form.vue` prefixes ids with Vue's `useId()`
  (`form-<instance>-<field>`) and passes the id to field slots as `inputId`.
  Custom selectors bind it with `:input-id="inputId"`; the shared `Dropdown*`
  components forward it to their `Select` through `$attrs`. No feature logic.
- **Files**: `src/components/forms/Form.vue`, `src/components/forms/README.md`
  (slot props, limitation removed), `FormRejectionReason.vue` (previously
  hardcoded `form-field-color`), custom slots in `FormOrder`, `FormOrderDetail`,
  `FormReceipt`, `FormReceiptDetail`, `FormPurchaseInvoice`,
  `FormPurchaseRateDetail`, `FormSupplierReference`; `frontend-form` skill.
- **Verification** (2026-09-24, `lilith-ui-tester`, staging DB):
  - `typecheck`, `i18n:check`, `build`: pass (i18n warnings pre-existing).
  - Validation case, Lifecycle "Budget": header form renders with a single
    header Save; empty name shows the inline error in ca/es and sends no
    request; editing the description sends a PUT whose body keeps the
    `statuses` array, then the value was restored. With the tag dialog open,
    screen and dialog `name` fields have different ids (`form-v-384-name`,
    `form-v-475-name`), no duplicates, and clicking the dialog label focuses the
    dialog input.
  - Regression: Rejection reason color label resolves to the ColorPicker input
    and no `form-field-*` ids remain; Receipt 26156 with the line dialog open
    and Purchase order 26130 have every `label[for]` resolved and no duplicate
    ids; status selectors still list transitions. No console errors.
  - Known platform behavior: clicking a label does not focus a PrimeVue
    `Select` because its combobox is a `span`; unchanged by this feature.
- **Docs updated**: README limitation removed · skill custom-field step and
  Proven Patterns (`FormLifecycle`, `FormRejectionReason`).
- **PR**: shipped with batch L1 (branch `forms/l1-shared-masters`).

### F-07 — Current values for in-progress saves

- **Motivated by**: PR-21/PR-26. On the route and work-order phase screens the
  user often edits the header and then manages steps or materials. Those saves
  reload the phase; the legacy form edited the store entity, so the step save
  also persisted the header edits. With `Form.vue` they were no longer sent and
  were lost on reload. The user asked to keep the legacy behaviour.
- **Design**: `Form.vue` exposes `getValues()`, a detached copy of the current
  values without validation. Each phase form exposes a typed `currentPhase()`
  built with the same narrowing as submit; the screens pass it to
  `updatePhase` in the four step/material handlers. The parent never reads raw
  form values.
- **Files**: `src/components/forms/Form.vue`, `README.md` (limitation removed,
  usage rule), `frontend-form` skill, `FormWorkmasterPhase.vue`,
  `FormWorkorderPhase.vue`, `WorkmasterPhase.vue`, `WorkorderPhase.vue`.
- **Verification** (2026-09-24, `lilith-ui-tester`): route "4020-207-1", phase
  "10 - Serrar material": header description edited without saving, then an
  existing step saved from its dialog. The phase PUT carried
  `"description":"Serrar material (test)"` with the `details` array, the header
  kept the value after the reload, and the API returned it after a full reload.
  Test data restored afterwards.
- **Found, not caused by the migration**: loading a phase URL directly does not
  render the header, because `WorkmasterPhase.vue` only fetches the phase while
  the header requires the work master (same `v-if` before the migration). The
  step detail PUT returned 404 "No s'ha trobat el cost del centre de treball"
  for this staging record (backend data).

### Form.vue fixes found during migration

Defects in `Form.vue` itself are not features, but they change behavior for
every consumer, so they are recorded here.

| Fix | Found by | Commit | Verification |
| --- | --- | --- | --- |
| PrimeVue inputs also registered themselves with the injected PrimeVue form and synced their value from it. A cleared `InputNumber` reverted to its initial value on blur, so an empty required number was saved. Controls now render in a scope that hides `$pcForm`/`$pcFormField`; `modelValue` + `setFieldValue` is the only binding. Affects every Number/Currency field, including migrated purchase forms. | SH-02 UI test (tax 21%: empty percentage saved) | `fddf84d` | Tax 21%: cleared percentage stays empty after blur, Save shows the inline required error and sends no PUT; retyping 21 works. Exercise date-range error still blocks submit. Purchase regression: receipt line dialog recalculates on quantity change, a cleared quantity stays empty, closing the dialog saves nothing. Expense screen not reachable (`/expenses` falls back to Home, menu link is `#`), still unchecked |

### Feature entry template

Copy for each feature once it is `confirmed`:

```markdown
### F-xx — <capability>

- **Motivated by**: <form ID and the behavior that blocked it>
- **Design**: <API added to Form.vue/types.ts; why it is generic>
- **Files**: <paths>
- **Verification**:
  - typecheck / i18n:check / build: <result>
  - Validation case (<form ID>): <scenarios run, result, screenshots>
  - Regression (FormLifecycleTag, FormReceiptDetail): <result>
- **Docs updated**: README limitation removed · skill Proven Patterns entry
- **PR**: <link>
```

## 4. Behavior Changes

| Scope | Change | Approval |
| --- | --- | --- |
| All migrated forms | Inline field errors replace the "invalid form" toast and `FormValidation` | Accepted globally (2026-09-24) |
| SH-02 Tax | The percentage validation message said "frequency"; it now names the percentage in ca/es/en | Bug fix |
| SH-05 Exercise | Dates are no longer rewritten on the store object at submit; the request body is serialized by `Date.prototype.toJSON`, which applies the same local-time conversion | Equivalent request |
| PR-07, PR-08 Workcenter dialogs | Required, duplicate and range checks move from warning toasts to inline errors | Global decision |
| PR-10 Operator type | Cost input blocks negatives; the min rule has a localized message (was Yup's English default) | Bug fix |
| PR-13, PR-14 Shifts | Shift detail times convert between `HH:mm:ss` and `Date` at the form boundary (legacy parsed the raw string and produced NaN times for new details); time labels now render; seconds are still saved as `00` | Bug fix |
| PR-15 Phase template | Header form keeps a scalar snapshot, so adding/editing a detail no longer discards unsaved header edits | Improvement |
| PR-16, PR-17 Phase templates | Detail and create dialogs use the default Save label instead of "Guardar detall"/"Crear"; create now requires a name and no longer edits the store while typing | Global decision |
| PR-03 Site, PR-09 Operator | Hardcoded "CIF"/"NIF" labels replaced by `production.fields.companyVatNumber` / `personalVatNumber` | i18n |
| WH-01 Warehouse | Locations are kept out of the form snapshot and merged from the prop at submit | Parent-owned collection pattern |
| WH-02 Location | Edits work on a copy of the row (closing no longer leaves the table changed); location type options localized; a new location without type sends `locationType: null` | Bug fix |
| WH-03 Inventory movement | Changing the reference clears the selected lot (legacy could submit a lot from another reference); hardcoded Catalan messages replaced by keys | Bug fix |
| PU-01 Purchase rate duplicate | Name and both dates now required, end date on or after start; confirm button reads Save instead of Duplicate | Global decision |
| i18n | `common.lot` was referenced by inventory, stock and stock movement screens but missing from every locale; added | Bug fix |
| PR-21..PR-28 Phase screens (workmaster and workorder) | Saving or deleting a step/material still persists unsaved header edits (kept on user request, F-07); header edits also survive the reload | Kept legacy |
| PR-21 Workmaster phase | Stale profit-percentage responses are ignored; a missing workcenter type no longer crashes | Bug fix |
| PR-18, PR-19, PR-20 Workmasters | Split-button save kept; create dialog requires a reference; copy dialog radios sit above the chosen group; footer reads Save instead of Crear/Copiar | Global decision |
| PR-24 Workorder | Clearing the status falls back to the saved status; execution period gets its own row | Layout |
| PR-25 Create workorder | Lot code is submitted only when the route's reference requires a lot (hidden stale codes no longer sent); still optional | Bug fix |
| PR-29 Production part | Changing the workcenter clears the selected work order detail (stale phase could be submitted); integer/greater-than-zero rules now have localized messages | Bug fix |
| SA-04 Customer | Customer type is now required (backend rejects a customer without one; approved); unsaved header edits survive contact/address changes | Approved |
| SA-02, SA-03 Customer address/contact | Edits work on a copy of the row (cancel no longer leaks into the table); location errors show per field; phone error highlights its field | Bug fix |
| SA-05 Reference | Dead `FormReferenceType` import and unused `defaultCustomerId` prop removed; rule on the hidden `cost` field dropped (not editable) | Cleanup |
| i18n (production) | Twelve `production.ui` keys containing an apostrophe or a dot never resolved in vue-i18n and rendered raw (source route label, delete confirmations, ticket table headers and messages); moved to camelCase `production.detail` keys | Bug fix |
| SA-06..SA-09 Budget | Notes block moved into the header form (the separate Notes tab is gone; approved); header keeps a scalar snapshot so line/transport reloads keep unsaved edits; create order sends the header as edited | Approved |
| SA-09, SA-11 Transport dialogs | The distance saved for the final customer is the one shown (legacy showed the real distance but saved 0; approved) | Bug fix |
| SA-12 Create order/invoice | Date serialized once (legacy shifted it twice, sending the next day after ~22:00); double submit blocked | Bug fix |
| SA-13 Delivery note | Only Save is disabled when the note is locked; Word/PDF downloads stay available (approved) | Bug fix |
| SA-14, SA-15 Sales invoice | Header schema (date, payment method) now actually runs; fiscal data keeps its own save in the tab with the propagation check (approved) | Global decision |
| SH-04 Lifecycle | Name and description are now validated (required, max 250). The legacy schema existed but was never run because the screen saved the store ref directly | Covered by the global validation decision |
| All `Form.vue` forms | Native field ids change from `form-field-<name>` to `form-<instance>-<name>` (F-03) | Internal; no consumer depended on the old ids |

Record per-form changes here (cancel now discards edits, validation added where
there was none, and so on).

## 5. Status Log

| Date | Event |
| --- | --- |
| 2026-09-24 | Tracker created with full inventory and candidate features |
| 2026-09-24 | L1 stopped at SH-04 by F-03; F-03 implemented and verified with SH-04 (`770c1ae`, `7251692`) |
| 2026-09-24 | L1 SH-01, SH-02, SH-03, SH-05 migrated; `Form.vue` InputNumber binding fix (`fddf84d`) |
| 2026-09-24 | L1 UI verification complete; purchase regression for `fddf84d` passes (Expenses screen unreachable) |
| 2026-09-24 | L1 rebased onto `dev` and opened as #135 |
| 2026-09-24 | L2 (17 forms) migrated in `eed879d`..`e91d2e7`; no new Form.vue feature needed |
| 2026-09-24 | L2 UI verification complete: enterprise, area, site, workcenter and its dialogs, operators, operator type, machine status, shifts, phase templates. Unverifiable with staging data: duplicate reason code (no status has reasons) |
| 2026-09-24 | L3 (4 forms) migrated in `7fcaff0`..`6a8c993`; no new Form.vue feature needed |
| 2026-09-24 | L3 UI: warehouse, location dialog copy-on-edit, inventory movement validation pass; lot reset unverifiable (tested reference has no lots) |
| 2026-09-24 | L4 (13 forms) migrated in `beceff8`..`c2f9c94`; F-01 and F-02 rejected in favour of section rows and the actions slot |
| 2026-09-24 | L4 stopped on the phase screens: keeping unsaved header edits needed F-07 (`getValues`), implemented in `7cb2e10`/`f79be93` and verified |
| 2026-09-24 | Pre-existing cold-load bugs fixed: phase screens load their parent (`9bcb6ec`) and the work order phase loads the WorkOrder lifecycle so the status shows (`ba7d69e`); verified by loading the four URLs directly |
| 2026-09-24 | Stacked PRs opened: #136 (L2 → L1), #137 (L3 → L2), #138 (L4 → L3) |
| 2026-09-24 | L4 UI verification complete: workorder phase header and step dialog, workmaster copy dialog (both modes), purchase rate duplicate (L3) pass. Unverifiable with staging data: lot code field (no lot-tracked route). Pre-existing: the copy dialog label `production.ui.Ruta d'origen` renders as a raw key (same call before the migration) |
| 2026-09-25 | i18n keys with an apostrophe or dot fixed in `cf11060` and `be09c01` (#138) |
| 2026-09-25 | L5 (5 forms) migrated in `7487304`..`dd91809`; no new Form.vue feature needed; customer type made required |
| 2026-09-25 | L5 UI verification complete: customer header, address dialog (cancel keeps the row, unsaved header edits survive), contact add dialog, customer type, sales reference |
| 2026-09-25 | L6 (12 forms) migrated in `b3f4a77`..`a007a24`; no new Form.vue feature needed; four product decisions applied (fiscal save, downloads, transport distance, budget notes) |
| 2026-09-25 | L6 UI verification complete: budget (create dialog, header with notes, line recalculation and cancel, transport), sales order (header, line, transport rate required), delivery note (locked note keeps downloads; status required), sales invoice (date required, fiscal tab with its own save, line amount 3 × 10 = 30.00 €, rectificative dialog limits). Unverifiable with staging data: final-customer transport distance (customer address not geocoded) |

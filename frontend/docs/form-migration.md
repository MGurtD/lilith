# Form.vue Migration Tracker

> **Status**: In progress. F-03 implemented; batch L1 migrated and verified, awaiting review.
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
| SH-01 | `shared/components/FormPaymentMethod.vue` | `PaymentMethod.vue` | low | — | migrated | `2283365` | Flat fields; UI verified |
| SH-02 | `shared/components/FormTax.vue` | `Tax.vue` | low | — | migrated | `27b4725`, fix `b060c1c` | UI verified after the fix; exposed the InputNumber double-binding bug fixed in `b060c1c` |
| SH-03 | `shared/components/FormReferenceType.vue` | `ReferenceType.vue` | low | — | migrated | `7c202db` | UI verified (density PUT keeps colors); 2 ColorPickers via `Custom` slot; `sales/components/FormReference.vue` imports it but never renders it (dead import) |
| SH-04 | `shared/components/FormLifecycle.vue` | `Lifecycle.vue` | low | F-03 | migrated | `5c0a9a0` / F-03 PR | Parent's own save replaced by `page-actions`; statuses kept out of the snapshot and merged at submit |
| SH-05 | `shared/components/FormExercise.vue` | `Exercise.vue` | med | — | migrated | `884f211` | UI verified (date-range error; PUT dates `2022-01-01T00:00:00.000Z`/`2022-12-31T00:00:00.000Z`); `Yup.ref` date range; submit-time date mutation removed (`toJSON` serializes identically) |

### 2.2 Batch L2 — Production plant model

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PR-01 | `production/components/FormEnterprise.vue` | `Enterprise.vue` | low | — | pending | | |
| PR-02 | `production/components/FormArea.vue` | `Area.vue` | low | — | pending | | Reads Pinia via `storeToRefs` |
| PR-03 | `production/components/FormSite.vue` | `Site.vue` | med | — | pending | | `LocationFields` multi-field control → section; email rules |
| PR-04 | `production/components/FormWorkcenter.vue` | `Workcenter.vue` | low | — | pending | | Page form |
| PR-05 | `production/components/FormWorkcenterType.vue` | `WorkcenterType.vue` | low | — | pending | | Pinia state; % suffix |
| PR-06 | `production/components/FormWorkcenterCost.vue` | `WorkcenterCost.vue` | low | — | pending | | Pinia state; currency |
| PR-07 | `production/components/TableWorkcenterLocations.vue` (add dialog) | `Workcenter.vue` | low | — | pending | | 1 selector; manual checks + toast; duplicate check |
| PR-08 | `production/components/TableWorkcenterProfitPercentage.vue` (add dialog) | `Workcenter.vue` | low | — | pending | | 1 InputNumber; manual checks + toast; duplicate check |
| PR-09 | `production/components/FormOperator.vue` | `Operator.vue` | low | — | pending | | |
| PR-10 | `production/components/FormOperatorType.vue` | `OperatorType.vue` | low | — | pending | | Pinia state; currency |
| PR-11 | `production/components/FormMachineStatus.vue` | `MachineStatus.vue` | med | — | pending | | ColorPicker + IconPicker slots, 6 checkboxes |
| PR-12 | `production/components/FormMachineStatusReason.vue` | `MachineStatuses.vue`, `TableMachineStatusReasons.vue` | med | — | pending | | Yup `.test` unique code across records |
| PR-13 | `production/components/FormShift.vue` | `Shifts.vue` | low | — | pending | | |
| PR-14 | `production/components/FormShiftDetail.vue` | `Shifts.vue` | med | — | pending | | Time-only dates via `Date` props; `extractTime` at submit; empty legacy schema |
| PR-15 | `production/components/FormPhaseTemplate.vue` | `PhaseTemplate.vue` | low | — | pending | | |
| PR-16 | `production/components/FormPhaseTemplateDetail.vue` | `TablePhaseTemplateDetails.vue` | low | — | pending | | |
| PR-17 | `production/views/PhaseTemplates.vue` (create dialog) | — | low | — | pending | | 2 raw fields, no validation today |

### 2.3 Batch L3 — Warehouse and purchase leftovers

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| WH-01 | `warehouse/components/FormWarehouse.vue` | `Warehouse.vue` | low | — | pending | | Computed Yup schema |
| WH-02 | `warehouse/components/FormLocation.vue` | `TableLocations.vue` | low | — | pending | | Computed Yup schema |
| WH-03 | `warehouse/components/FormInventoryNewMovements.vue` | `Inventory.vue` | med | — | pending | | `DropdownReference`, `DropdownWarehousesWithLocations`, `SelectorLot` keyed on reference |
| PU-01 | `purchase/components/TablePurchaseRates.vue` (duplicate dialog) | `Supplier.vue` | low | — | pending | | 1 text + 2 dates, no validation today |

### 2.4 Batch L4 — Production routings and work orders

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PR-18 | `production/components/FormWorkmaster.vue` | `Workmaster.vue` | med | F-02? | pending | | SplitButton "calculate cost" next to Save |
| PR-19 | `production/views/Workmasters.vue` (create dialog) | — | low | — | pending | | 1 selector |
| PR-20 | `production/views/Workmasters.vue` (copy dialog) | — | high | F-01 | pending | | RadioButton switches between existing and new reference fields |
| PR-21 | `production/components/FormWorkmasterPhase.vue` | `WorkmasterPhase.vue`, `TableWorkmasterPhases.vue` | high | F-03? | pending | | Cascades (workcenter type/workcenter/external work/service reference), async profit lookup |
| PR-22 | `production/components/FormWorkmasterPhaseDetail.vue` | `WorkmasterPhase.vue` | low | — | pending | | |
| PR-23 | `production/components/FormWorkmasterPhaseBomItem.vue` | `WorkmasterPhase.vue` | low | — | pending | | `DropdownReference` slot |
| PR-24 | `production/components/FormWorkorder.vue` | `Workorder.vue` | med | F-02? | pending | | SplitButton exports; computed date range; lifecycle status dropdown |
| PR-25 | `production/components/FormCreateWorkorder.vue` | `Workorders.vue`, `sales/components/TableSalesOrderDetails.vue` | high | F-01 | pending | | Lot code field shown when the selected work master's reference requires a lot |
| PR-26 | `production/components/FormWorkorderPhase.vue` | `WorkorderPhase.vue`, `TableWorkorderPhases.vue` | high | F-03? | pending | | Same cascade pattern as PR-21 |
| PR-27 | `production/components/FormWorkorderPhaseDetail.vue` | `WorkorderPhase.vue` | low | — | pending | | |
| PR-28 | `production/components/FormWorkorderPhaseBomItem.vue` | `WorkorderPhase.vue` | low | — | pending | | `DropdownReference` slot |
| PR-29 | `production/components/FormProductionPart.vue` | `ProductionParts.vue` | med | — | pending | | Work order selection fills 3 dependent fields |
| PR-30 | `production/components/FormWorkOrderProductionPart.vue` | `Workorder.vue` | med | — | pending | | Filtered options; async `getWorkOrders` on change |

### 2.5 Batch L5 — Sales masters

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SA-01 | `sales/components/FormCustomerType.vue` | `CustomerType.vue` | low | — | pending | | |
| SA-02 | `sales/components/FormCustomerAddress.vue` | `TableCustomerAddresses.vue` | low | — | pending | | Dialog; clone row on edit |
| SA-03 | `sales/components/FormCustomerContact.vue` | `TableCustomerContacts.vue` | low | — | pending | | Dialog; clone row on edit |
| SA-04 | `sales/components/FormCustomer.vue` | `Customer.vue` | med | — | pending | | Pinia state |
| SA-05 | `sales/components/FormReference.vue` | `Reference.vue` | med | — | pending | | Tax store options |

### 2.6 Batch L6 — Sales documents

| ID | Component | Consumers | Diff. | Features | Status | Commit / PR | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SA-06 | `sales/components/FormBudget.vue` | `Budget.vue` | med | — | pending | | Pinia state; lifecycle transitions dropdown |
| SA-07 | `sales/views/Budget.vue` (notes block) | — | low | — | pending | | Editable notes + read-only auto notes outside `FormBudget` |
| SA-08 | `sales/components/FormBudgetOrderDetail.vue` | `Budget.vue`, `SalesOrder.vue` | high | — | pending | | Embeds `TableWorkmasterProfit` (parent-owned collection) |
| SA-09 | `sales/components/FormBudgetTransport.vue` | `Budget.vue` | med | — | pending | | Async supplier/rate lookups |
| SA-10 | `sales/components/FormSalesOrder.vue` | `SalesOrder.vue` | high | — | pending | | 4 stores |
| SA-11 | `sales/components/FormSalesOrderTransport.vue` | `SalesOrder.vue` | med | — | pending | | Async lookups |
| SA-12 | `sales/components/FormCreateOrderOrInvoice.vue` | `SalesOrders.vue`, `DeliveryNotes.vue`, `Budgets.vue`, `SalesInvoices.vue` | med | — | pending | | Reused by 4 screens; test every caller |
| SA-13 | `sales/components/FormDeliveryNote.vue` | `DeliveryNote.vue` | med | — | pending | | `lockHeader`/`lockStatus` flags → disabled predicates |
| SA-14 | `sales/components/FormSalesInvoice.vue` | `SalesInvoice.vue` | med | — | pending | | Lifecycle transitions dropdown |
| SA-15 | `sales/views/SalesInvoice.vue` (fiscal data block) | — | low | — | pending | | 7 text fields + `DropdownCountry` outside `FormSalesInvoice` |
| SA-16 | `sales/components/FormSalesInvoiceDetail.vue` | `SalesInvoice.vue` | med | — | pending | | |
| SA-17 | `sales/components/FormRectificativeInvoice.vue` | `SalesInvoice.vue` | med | — | pending | | Quantity limited by `maximumQuantity` prop |

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
| F-01 | Field visibility from current values, with matching conditional validation | PR-25, PR-20 | candidate | | | |
| F-02 | Secondary actions next to Save (split button) and `cancel` exposed to the `actions` slot. First check whether a Save `SplitButton` in `PageActions` driven through an external form ref (frontend save convention) is enough | PR-18, PR-24, SY-04 | candidate | | | |
| F-03 | Unique native field IDs for simultaneous forms | SH-04 | implemented | `Form.vue`, `README.md`, custom slots in purchase forms and `FormRejectionReason.vue` | See entry below | `fe59b6b` |
| F-04 | File upload field, or a documented pattern for self-submitting uploads next to a form | SY-06, SH-09 | candidate | | | |
| F-05 | Repeatable or dynamically generated field groups | SY-05 | candidate | | | |

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
- **PR**: pending (branch `forms/f03-unique-field-ids`).

### Form.vue fixes found during migration

Defects in `Form.vue` itself are not features, but they change behavior for
every consumer, so they are recorded here.

| Fix | Found by | Commit | Verification |
| --- | --- | --- | --- |
| PrimeVue inputs also registered themselves with the injected PrimeVue form and synced their value from it. A cleared `InputNumber` reverted to its initial value on blur, so an empty required number was saved. Controls now render in a scope that hides `$pcForm`/`$pcFormField`; `modelValue` + `setFieldValue` is the only binding. Affects every Number/Currency field, including migrated purchase forms. | SH-02 UI test (tax 21%: empty percentage saved) | `b060c1c` | Tax 21%: cleared percentage stays empty after blur, Save shows the inline required error and sends no PUT; retyping 21 works. Exercise date-range error still blocks submit. Purchase Number fields: regression pending |

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
| SH-04 Lifecycle | Name and description are now validated (required, max 250). The legacy schema existed but was never run because the screen saved the store ref directly | Covered by the global validation decision |
| All `Form.vue` forms | Native field ids change from `form-field-<name>` to `form-<instance>-<name>` (F-03) | Internal; no consumer depended on the old ids |

Record per-form changes here (cancel now discards edits, validation added where
there was none, and so on).

## 5. Status Log

| Date | Event |
| --- | --- |
| 2026-09-24 | Tracker created with full inventory and candidate features |
| 2026-09-24 | L1 stopped at SH-04 by F-03; F-03 implemented and verified with SH-04 (`fe59b6b`, `5c0a9a0`) |
| 2026-09-24 | L1 SH-01, SH-02, SH-03, SH-05 migrated; `Form.vue` InputNumber binding fix (`b060c1c`) |
| 2026-09-24 | L1 UI verification complete; purchase Number regression for `b060c1c` queued |

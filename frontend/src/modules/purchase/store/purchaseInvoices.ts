import { defineStore } from "pinia";
import PurchaseService from "../services";
import {
  IngestPurchaseInvoiceResponse,
  PurchaseInvoice,
  PurchaseInvoiceImport,
  PurchaseInvoiceUpdateStatues,
  PurchaseInvoiceDueDate,
} from "../types";
import { getNewUuid } from "@/utils/functions";
import { globalToast } from "@/utils/global-toast";
import { useSuppliersStore } from "./suppliers";

// Normalize a VatNumber for fuzzy equality when auto-resolving SupplierId:
// - Uppercase
// - Drop spaces and dashes
// - Strip "ES" country prefix
// - Strip leading "B" (NIF/CIF marker in supplier codes like "B09680521")
// Returns an empty string for null/undefined/empty input.
function normalizeVatNumber(input: string | null | undefined): string {
  if (!input) return "";
  return input
    .toUpperCase()
    .replace(/\s+/g, "")
    .replace(/-/g, "")
    .replace(/^ES/, "")
    .replace(/^B/, "");
}

// Toast shown when the auto-resolver finds 2+ suppliers sharing a normalized
// VatNumber. Operator must pick manually because Supplier.VatNumber is NOT
// uniquely constrained in the DB (only ComercialName is — see
// SupplierBuilder.cs:76-77). Catalan message per frontend AGENTS.md.
const AMBIGUOUS_SUPPLIER_TOAST = (vatNumber: string): string =>
  `S'han trobat múltiples proveïdors amb el mateix NIF/CIF (${vatNumber}). Selecciona'l manualment.`;

export const usePurchaseInvoiceStore = defineStore({
  id: "purchaseInvoices",
  state: () => ({
    purchaseInvoice: undefined as PurchaseInvoice | undefined,
    purchaseInvoices: undefined as Array<PurchaseInvoice> | undefined,
  }),
  getters: {},
  actions: {
    setNewPurchaseInvoice(id: string) {
      this.purchaseInvoice = {
        id: id,
        number: "0",
        supplierNumber: "--",
        purchaseInvoiceDate: new Date(),
        baseAmount: 0.0,
        transportAmount: 0.0,
        subtotal: 0.0,
        taxAmount: 0.0,
        grossAmount: 0.0,
        netAmount: 0.0,
        discountPercentage: 0.0,
        discountAmount: 0.0,
        supplierId: "",
        taxId: "",
        exerciceId: "",
        purchaseInvoiceSerieId: "",
        paymentMethodId: "",
        statusId: "",
        extraTaxAmount: 0,
        extraTaxPercentatge: 0,
        purchaseInvoiceDueDates: [],
        purchaseInvoiceImports: [],
      } as PurchaseInvoice;
    },
    // Prefills a draft PurchaseInvoice from an LlamaParse ingestion result.
    // Seeds a fresh id, then mutates the header + populates purchaseInvoiceImports
    // from payload.taxBreakdown (taxId is already resolved server-side).
    // SupplierId is auto-resolved by normalized VatNumber match against the
    // useSuppliersStore in-memory list. If no match, leaves supplierId empty
    // so the operator can pick manually.
    setFromIngestion(payload: IngestPurchaseInvoiceResponse) {
      const id = getNewUuid();
      this.setNewPurchaseInvoice(id);
      if (!this.purchaseInvoice) return undefined;

      this.purchaseInvoice.supplierNumber = payload.invoiceNumber ?? "--";
      if (payload.issueDate) {
        this.purchaseInvoice.purchaseInvoiceDate =
          new Date(payload.issueDate) as any;
      }
      this.purchaseInvoice.transportAmount = payload.transportAmount ?? 0;
      this.purchaseInvoice.extraTaxPercentatge =
        payload.extraTaxPercentatge ?? 0;
      this.purchaseInvoice.discountPercentage =
        payload.discountPercentage ?? 0;

      this.purchaseInvoice.purchaseInvoiceImports = (payload.taxBreakdown ?? []).map(
        (row) =>
          ({
            id: getNewUuid(),
            taxId: row.taxId,
            baseAmount: row.baseAmount,
            taxAmount: row.taxAmount,
            netAmount: row.baseAmount + row.taxAmount,
            purchaseInvoiceId: id,
          } as PurchaseInvoiceImport),
      );

      // Auto-resolve SupplierId by normalized VatNumber match.
      // NOTE: Supplier.VatNumber is NOT unique in the DB (only ComercialName
      // is unique — see SupplierBuilder.cs:76-77), so multiple suppliers may
      // match the same normalized VatNumber.
      //   1 match  → set supplierId automatically.
      //   2+ match → leave supplierId empty, warn operator via globalToast
      //              so they pick manually (auto-picking is non-deterministic
      //              because VatNumber is not unique).
      //   0 match  → silent, supplierId stays empty; operator picks manually.
      const normalized = normalizeVatNumber(payload.supplierVatNumber);
      if (normalized) {
        const supplierStore = useSuppliersStore();
        const suppliers = supplierStore.suppliers ?? [];
        const matches = suppliers.filter(
          (s) => normalizeVatNumber(s.vatNumber) === normalized,
        );
        if (matches.length === 1) {
          this.purchaseInvoice.supplierId = matches[0].id;
        } else if (matches.length > 1) {
          globalToast.warn(
            AMBIGUOUS_SUPPLIER_TOAST(payload.supplierVatNumber ?? ""),
          );
        }
      }

      return this.purchaseInvoice;
    },
    async Create(purchaseInvoice: PurchaseInvoice) {
      const created =
        await PurchaseService.PurchaseInvoice.create(purchaseInvoice);
      return created;
    },
    async GetById(id: string) {
      const data = await PurchaseService.PurchaseInvoice.getById(id);
      if (data) {
        // Convert ISO date string to Date object for PrimeVue 4 DatePicker
        if (data.purchaseInvoiceDate) {
          data.purchaseInvoiceDate = new Date(data.purchaseInvoiceDate) as any;
        }
      }
      this.purchaseInvoice = data;
    },
    async GetFiltered(
      startDate: string,
      endDate: string,
      statusId?: string,
      excludeStatusId?: string,
      supplierId?: string,
      paymentMethodId?: string,
      dueDateStartTime?: string,
      dueDateEndTime?: string,
      accountNumber?: string,
    ) {
      this.purchaseInvoices =
        await PurchaseService.PurchaseInvoice.GetFiltered(
          startDate,
          endDate,
          supplierId,
          statusId,
          excludeStatusId,
          paymentMethodId,
          dueDateStartTime,
          dueDateEndTime,
          accountNumber,
        );
    },
    async Update(purchaseInvoice: PurchaseInvoice) {
      const updated = await PurchaseService.PurchaseInvoice.update(
        purchaseInvoice.id,
        purchaseInvoice,
      );
      return updated;
    },
    async Delete(id: string): Promise<boolean> {
      const deleted = await PurchaseService.PurchaseInvoice.delete(id);
      return deleted;
    },

    async GetDueDates(purchaseInvoice: PurchaseInvoice) {
      const result =
        await PurchaseService.PurchaseInvoice.GetDueDates(purchaseInvoice);
      if (result) return result;
    },

    async UpdateInvoicesStatus(
      invoiceImport: PurchaseInvoiceUpdateStatues,
    ): Promise<boolean> {
      const updated =
        await PurchaseService.PurchaseInvoice.UpdateStatuses(invoiceImport);
      return updated;
    },

    async CreateInvoiceImport(
      invoiceImport: PurchaseInvoiceImport,
    ): Promise<boolean> {
      const created =
        await PurchaseService.PurchaseInvoice.CreateImport(invoiceImport);
      return created;
    },
    async UpdateInvoiceImport(
      invoiceImport: PurchaseInvoiceImport,
    ): Promise<boolean> {
      const created =
        await PurchaseService.PurchaseInvoice.UpdateImport(invoiceImport);
      return created;
    },
    async DeleteInvoiceImport(
      invoiceImport: PurchaseInvoiceImport,
    ): Promise<boolean> {
      const created =
        await PurchaseService.PurchaseInvoice.DeleteImport(invoiceImport);
      return created;
    },

    async AddDueDates(
      dueDates: Array<PurchaseInvoiceDueDate>,
    ): Promise<boolean> {
      const added = await PurchaseService.PurchaseInvoice.AddDueDates(dueDates);
      return added;
    },
    async RemoveDueDates(ids: Array<string>): Promise<boolean> {
      const removed = await PurchaseService.PurchaseInvoice.RemoveDueDates(ids);
      return removed;
    },
    async ReplaceDueDates(
      newDueDates: Array<PurchaseInvoiceDueDate>,
    ): Promise<boolean> {
      if (!this.purchaseInvoice) return false;
      // remove existing first
      const existingIds = this.purchaseInvoice.purchaseInvoiceDueDates.map(
        (d) => d.id,
      );
      if (existingIds.length) await this.RemoveDueDates(existingIds);
      const created = await this.AddDueDates(newDueDates);
      if (created) {
        this.purchaseInvoice.purchaseInvoiceDueDates = newDueDates;
      }
      return created;
    },
  },
});

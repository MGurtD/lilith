import { defineStore } from "pinia";
import { ReferenceService } from "../services/reference.service";
import { SupplierReference } from "../../purchase/types";
import {
  Reference,
  ReferenceCategory,
  ReferenceCategoryEnum,
  ReferenceFormat,
} from "../types";

const referenceService = new ReferenceService("/reference");
export const useReferenceStore = defineStore({
  id: "reference",
  state: () => ({
    reference: undefined as Reference | undefined,
    references: undefined as Array<Reference> | undefined,
    referenceFormats: undefined as Array<ReferenceFormat> | undefined,
    referenceSuppliers: undefined as Array<SupplierReference> | undefined,
    referenceCategories: [
      { code: "Service", description: "Servei" },
      { code: "Tool", description: "Eina" },
      { code: "Material", description: "Material" },
    ] as ReferenceCategory[],
    module: "" as string,
  }),
  getters: {
    getByCategory: (state) => {
      return (category: ReferenceCategoryEnum) => {
        return state.references?.filter((r) => r.categoryName === category);
      };
    },
    getFullName: (state) => {
      return (reference: Reference) => {
        if (!reference) return "";

        if (reference.sales) {
          return `${reference.code} (v. ${reference.version}) - ${reference.description}`;
        } else {
          return `${reference.code} - ${reference.description}`;
        }
      };
    },
    getShortName: (state) => {
      return (reference: Reference) => {
        if (!reference) return "";

        if (reference.sales) {
          return `${reference.code} (v. ${reference.version})`;
        } else {
          return `${reference.code}`;
        }
      };
    },
  },
  actions: {
    getFullNameById(id: string) {
      const ref = this.references?.find((r) => r.id === id);
      if (!ref) return "";
      return this.getFullName(ref);
    },
    getShortNameById(id: string) {
      const ref = this.references?.find((r) => r.id === id);
      if (!ref) return "";
      return this.getShortName(ref);
    },
    setNewReference(id: string, category: ReferenceCategoryEnum) {
      this.reference = {
        id: id,
        code: "",
        description: "",
        cost: 0.0,
        price: 0.0,
        disabled: false,
        version: "1",
        sales: this.module === "sales",
        purchase: this.module === "purchase",
        production: this.module === "production",
        referenceTypeId: null,
        referenceFormatId: null,
        categoryName: category,
        transportAmount: 0,
        density: 0,
        lastPurchaseCost: 0,
        isService: false,
        lastCost: 0,
        workMasterCost: 0,
        customer: null,
        customerId: null,
        areaId: null,
      } as Reference;
    },
    async getReferencesByModuleAndCategory(
      module: string,
      category: ReferenceCategoryEnum
    ) {
      return await referenceService.getByModule(module, category);
    },
    async getPrice(referenceId: string, supplierId: string) {
      return await referenceService.getPrice(referenceId, supplierId);
    },
    async fetchReferences() {
      this.references = await referenceService.getAll();

      if (!this.referenceFormats)
        this.referenceFormats = await referenceService.getReferenceFormats();
    },
    async fetchReferencesByModule(module: string) {
      this.module = module;
      this.references = await referenceService.getByModule(module);
      if (!this.referenceFormats)
        this.referenceFormats = await referenceService.getReferenceFormats();
    },
    async fetchReferencesByCustomer(customerId: string) {
      this.module = "sales";
      this.references =
        await referenceService.getSalesReferencesByCustomer(customerId);
      if (!this.referenceFormats)
        this.referenceFormats = await referenceService.getReferenceFormats();
    },
    async fetchReference(id: string) {
      this.reference = await referenceService.getById(id);
    },
    async createReference(reference: Reference) {
      const result = await referenceService.create(reference);
      if (result) await this.fetchReferencesByModule(this.module);
      return result;
    },
    async updateReference(id: string, reference: Reference) {
      const result = await referenceService.update(id, reference);
      if (result) await this.fetchReferencesByModule(this.module);
      return result;
    },
    async deleteReference(id: string) {
      const response = await referenceService.deleteReference(id);
      if (response.result) {
        await this.fetchReferencesByModule(this.module);
      }
      return response;
    },

    // Suppliers
    async fetchReferenceSuppliers(referenceId: string) {
      this.referenceSuppliers =
        await referenceService.getReferenceSuppliers(referenceId);
    },
    async addSupplier(model: SupplierReference) {
      const response = await referenceService.addSupplier(model);
      if (response) {
        await this.fetchReferenceSuppliers(model.referenceId);
      }
      return response;
    },
    async updateSupplier(model: SupplierReference) {
      const response = await referenceService.updateSupplier(model);
      if (response) {
        await this.fetchReferenceSuppliers(model.referenceId);
      }
      return response;
    },
    async deleteSupplier(model: SupplierReference) {
      const response = await referenceService.removeSupplier(model.id);
      if (response) {
        await this.fetchReferenceSuppliers(model.referenceId);
      }
      return response;
    },
  },
});

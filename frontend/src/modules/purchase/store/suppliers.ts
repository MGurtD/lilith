import { defineStore } from "pinia";
import {
  Supplier,
  SupplierContact,
  SupplierType,
  SupplierReference,
} from "../types";
import {
  SupplierService,
  SupplierTypeService,
} from "../services/suppliers.service";

const service = new SupplierService("/supplier");
const typeService = new SupplierTypeService("/suppliertype");

export const useSuppliersStore = defineStore({
  id: "suppliers",
  state: () => ({
    supplierTypes: undefined as Array<SupplierType> | undefined,
    supplierType: undefined as SupplierType | undefined,
    suppliers: undefined as Array<Supplier> | undefined,
    supplier: undefined as Supplier | undefined,
    supplierReferences: undefined as Array<SupplierReference> | undefined,
    supplierReference: undefined as SupplierReference | undefined,
  }),
  getters: {
    getName: (state) => {
      return (id: string) => {
        if (!state.suppliers) return "";
        const sup = state.suppliers?.find((r) => r.id === id);
        if (!sup) return "";
        return sup.comercialName;
      };
    },
  },
  actions: {
    setNewSupplier(id: string) {
      this.supplier = {
        id: id,
        comercialName: "",
        taxName: "",
        address: "",
        vatNumber: "",
        city: "",
        country: "Espanya",
        disabled: false,
        observations: "",
        phone: "",
        postalCode: "",
        region: "",
      } as Supplier;
    },
    async fetchSuppliers() {
      this.suppliers = await service.getAll();
    },
    async fetchSupplier(id: string) {
      this.supplier = await service.getById(id);
    },
    async createSupplier(supplier: Supplier) {
      const result = await service.create(supplier);
      if (result) await this.fetchSuppliers();
      return result;
    },
    async updateSupplier(id: string, supplier: Supplier) {
      const result = await service.update(id, supplier);
      if (result) await this.fetchSuppliers();
      return result;
    },
    async deleteSupplier(id: string) {
      const result = await service.delete(id);
      if (result) await this.fetchSuppliers();
      return result;
    },

    setNewSupplierType(id: string) {
      this.supplierType = {
        id: id,
        name: "",
        description: "",
      } as SupplierType;
    },
    async fetchSupplierTypes() {
      this.supplierTypes = await typeService.getAll();
    },
    async fetchSupplierType(id: string) {
      this.supplierType = await typeService.getById(id);
    },
    async createSupplierType(supplierType: SupplierType) {
      const result = await typeService.create(supplierType);
      if (result) await this.fetchSupplierTypes();
      return result;
    },
    async updateSupplierType(id: string, supplierType: SupplierType) {
      const result = await typeService.update(id, supplierType);
      if (result) await this.fetchSupplierTypes();
      return result;
    },
    async deleteSupplierType(id: string) {
      const result = await typeService.delete(id);
      if (result) await this.fetchSupplierTypes();
      return result;
    },

    async addContactToSupplier(contact: SupplierContact) {
      const result = await service.addContact(contact);
      if (result) await this.fetchSupplier(contact.supplierId);
      return result;
    },
    async updateContactFromSupplier(contact: SupplierContact) {
      const result = await service.updateContact(contact);
      if (result) await this.fetchSupplier(contact.supplierId);
      return result;
    },
    async removeContactFromSupplier(contact: SupplierContact) {
      const result = await service.removeContact(contact.id);
      if (result) await this.fetchSupplier(contact.supplierId);
      return result;
    },

    async fetchSupplierReference(id: string) {
      this.supplierReference = await service.getReference(id);
    },
    async fetchSupplierReferences(supplierId: string) {
      this.supplierReferences = await service.getReferences(supplierId);
    },
    async addReferenceToSupplier(reference: SupplierReference) {
      const result = await service.addReference(reference);
      if (result) await this.fetchSupplierReferences(reference.supplierId);
      return result;
    },
    async updateReferenceFromSupplier(reference: SupplierReference) {
      const result = await service.updateReference(reference);
      if (result) await this.fetchSupplierReferences(reference.supplierId);
      return result;
    },
    async removeReferenceFromSupplier(reference: SupplierReference) {
      const result = await service.removeReference(reference.id);
      if (result) await this.fetchSupplierReferences(reference.supplierId);
      return result;
    },
  },
});

import { defineStore } from "pinia";
import {
  Customer,
  CustomerContact,
  CustomerAddress,
  CustomerType,
} from "../types";
import {
  CustomerService,
  CustomerTypeService,
} from "../services/customer.service";

const service = new CustomerService("/Customer");
const typeService = new CustomerTypeService("/CustomerType");

export const useCustomersStore = defineStore({
  id: "customers",
  state: () => ({
    customers: undefined as Array<Customer> | undefined,
    customerTypes: undefined as Array<CustomerType> | undefined,
    customer: undefined as Customer | undefined,
    customerType: undefined as CustomerType | undefined,
  }),
  getters: {
    getCustomerNameById: (state) => (id: string) => {
      const customer = state.customers?.find((customer) => customer.id === id);
      if (customer) return customer.comercialName;
      return "";
    },
  },
  actions: {
    // Customers
    setNewCustomer(id: string) {
      this.customer = {
        id: id,
        code: "",
        comercialName: "",
        taxName: "",
        vatNumber: "",
        web: "",
        disabled: false,
        customerTypeId: "",
      } as Customer;
    },
    async fetchCustomers() {
      this.customers = await service.getAll();
    },
    async fetchCustomer(id: string) {
      this.customer = await service.getById(id);
    },
    async createCustomer(customer: Customer) {
      const result = await service.create(customer);
      if (result) await this.fetchCustomers();
      return result;
    },
    async updateCustomer(id: string, customer: Customer) {
      const result = await service.update(id, customer);
      if (result) await this.fetchCustomers();
      return result;
    },
    async deleteCustomer(id: string) {
      const result = await service.delete(id);
      if (result) await this.fetchCustomers();
      return result;
    },

    // Customer Types
    setNewCustomerType(id: string) {
      this.customerType = {
        id: id,
        name: "",
        description: "",
      } as CustomerType;
    },
    async fetchCustomerTypes() {
      this.customerTypes = await typeService.getAll();
    },
    async fetchCustomerType(id: string) {
      this.customerType = await typeService.getById(id);
    },
    async createCustomerType(customerType: CustomerType) {
      const result = await typeService.create(customerType);
      if (result) await this.fetchCustomerTypes();
      return result;
    },
    async updateCustomerType(id: string, customerType: CustomerType) {
      const result = await typeService.update(id, customerType);
      if (result) await this.fetchCustomerTypes();
      return result;
    },
    async deleteCustomerType(id: string) {
      const result = await typeService.delete(id);
      if (result) await this.fetchCustomerTypes();
      return result;
    },

    // Customer Contacts
    async addContactToCustomer(contact: CustomerContact) {
      const result = await service.addContact(contact);
      if (result) await this.fetchCustomer(contact.customerId);
      return result;
    },
    async updateContactFromCustomer(contact: CustomerContact) {
      const result = await service.updateContact(contact);
      if (result) await this.fetchCustomer(contact.customerId);
      return result;
    },
    async removeContactFromCustomer(contact: CustomerContact) {
      const result = await service.removeContact(contact.id);
      if (result) await this.fetchCustomer(contact.customerId);
      return result;
    },

    // Customer Address
    async addAddressToCustomer(address: CustomerAddress) {
      const result = await service.addAddress(address);
      if (result) await this.fetchCustomer(address.customerId);
      return result;
    },
    async updateAddressFromCustomer(address: CustomerAddress) {
      const result = await service.updateAddress(address);
      if (result) await this.fetchCustomer(address.customerId);
      return result;
    },
    async removeAddressFromCustomer(address: CustomerAddress) {
      const result = await service.removeAddress(address.id);
      if (result) await this.fetchCustomer(address.customerId);
      return result;
    },
  },
});

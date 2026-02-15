import { CreateWorkOrderDto, Site } from "../../production/types";
import { Reference } from "../../shared/types";

export interface CustomerType {
  id: string;
  name: string;
  description: string;
  disabled: boolean;
}

export interface Customer {
  id: string;
  code: string;
  comercialName: string;
  taxName: string;
  vatNumber: string;
  web: string;
  disabled: boolean;
  customerTypeId: string;
  paymentMethodId: string;
  accountNumber: string;
  observations: string;
  invoiceNotes: string;
  preferredLanguage: string;
  contacts: Array<CustomerContact>;
  address: Array<CustomerAddress>;
}

export interface CustomerContact {
  id: string;
  firstName: string;
  lastName: string;
  charge: string;
  email: string;
  phoneNumber: string;
  extension: string;
  disabled: boolean;
  main: boolean;
  customerId: string;
}

export interface CustomerAddress {
  id: string;
  name: string;
  address: string;
  country: string;
  region: string;
  city: string;
  postalCode: string;
  main: boolean;
  disabled: boolean;
  observations: string;
  customerId: string;
}

export interface Budget {
  id: string;
  customerId: string;
  exerciseId: string;
  date: any;
  number: string;
  deliveryDays: number;
  amount: number;
  acceptanceDate: any;
  statusId: string;
  notes: string;
  details: Array<BudgetDetail> | undefined;
  userNotes: string;
}

export interface BudgetDetail {
  id: string;
  budgetId: string;
  referenceId: string;
  workMasterId: string | null;
  description: string;
  quantity: number;
  profit: number;
  productionProfit: number;
  materialProfit: number;
  externalProfit: number;
  discount: number;
  productionCost: number;
  materialCost: number;
  transportCost: number;
  serviceCost: number;
  unitCost: number;
  unitPrice: number;
  totalCost: number;
  amount: number;
  userNotes: string;
}

export interface SalesOrderHeader {
  id: string;
  customerId: string;
  exerciseId: string;
  budgetId: string | null;
  date: any;
  number: string;
  expectedDate: any;
  customerCode: string;
  customerComercialName: string;
  customerNumber: string;
  customerTaxName: string;
  customerVatNumber: string;
  customerAccountNumber: string;
  siteId: string;
  name: string;
  address: string;
  city: string;
  postalCode: string;
  region: string;
  country: string;
  vatNumber: string;
  statusId: string;
  deliveryNoteId: string;
  salesOrderDetails: Array<SalesOrderDetail> | undefined;
}

export interface SalesOrderReport {
  customer: Customer;
  site: Site;
  order: SalesOrderHeader;
  total: number;
}

export interface SalesOrderDetail {
  id: string;
  salesOrderHeaderId: string;
  referenceId: string;
  workMasterId: string | null;
  workOrderId: string | null;
  description: string;
  quantity: number;
  profit: number;
  productionProfit: number;
  materialProfit: number;
  externalProfit: number;
  discount: number;
  lastCost: number;
  workMasterCost: number;
  unitCost: number;
  productionCost: number;
  materialCost: number;
  transportCost: number;
  serviceCost: number;
  unitPrice: number;
  totalCost: number;
  amount: number;
  isDelivered: boolean;
  isInvoiced: boolean;
  userNotes: string;
}

export interface CreateInvoiceDetailsFromOrderDetailsRequest {
  invoiceId: string;
  orderDetails: Array<SalesOrderDetail>;
}

export interface CreateSalesHeaderRequest {
  id: string;
  date: any;
  exerciseId: string;
  customerId: string;
  initialStatusId?: string;
}

export interface CreateRectificativeInvoiceRequest {
  id: string;
  quantity: number;
}

export interface SalesInvoice {
  id: string;
  parentSalesInvoiceId: string | null;
  disabled: boolean;
  invoiceNumber: string;
  invoiceDate: any;
  baseAmount: number;
  taxAmount: number;
  transportAmount: number;
  grossAmount: number;
  netAmount: number;
  exerciseId: string;
  statusId: string;
  integrationStatusId: string | null;
  customerId: string;
  paymentMethodId: string;
  customerCode: string;
  customerComercialName: string;
  customerTaxName: string;
  customerVatNumber: string;
  customerAccountNumber: string;
  customerAddress: string;
  customerCity: string;
  customerPostalCode: string;
  customerRegion: string;
  customerCountry: string;
  siteId: string;
  name: string;
  address: string;
  city: string;
  postalCode: string;
  region: string;
  country: string;
  vatNumber: string;
  salesInvoiceDetails: Array<SalesInvoiceDetail>;
  salesInvoiceDueDates: Array<SalesInvoiceDueDate>;
  salesInvoiceImports: Array<SalesInvoiceImport>;
}

export interface InvoiceableOrderDetail {
  salesOrderId: string;
  salesOrderNumber: number;
  salesOrderDate: any;
  customerId: string;
  id: string;
  statusId: string;
  statusName: string;
  referenceId: string;
  referenceCode: string;
  referenceDescription: string;
  referenceVersion: string;
  quantity: number;
  description: string;
  unitCost: number;
  unitPrice: number;
  totalCost: number;
  amount: number;
  isInvoiced: boolean;
  isServed: boolean;
}

export interface SalesInvoiceDetail {
  id: string;
  salesInvoiceId: string;
  deliveryNoteDetailId: string;
  taxId: string;
  referenceId: string;
  reference: Reference;
  description: string;
  quantity: number;
  unitCost: number;
  unitPrice: number;
  totalCost: number;
  amount: number;
  deliveryNoteDetail?: DeliveryNoteDetail;
}

export interface SalesInvoiceDueDate {
  id: string;
  salesInvoiceId: string;
  dueDate: string;
  amount: number;
}

export interface SalesInvoiceImport {
  id: string;
  salesInvoiceId: string;
  taxId: string;
  baseAmount: number;
  taxAmount: number;
  netAmount: number;
}

export interface DeliveryNoteReport {
  customer: Customer;
  site: Site;
  deliveryNote: DeliveryNote;
  orders: Array<SalesOrderHeader>;
  total: number;
}

export interface DeliveryNote {
  createdOn?: string;
  number: string;
  deliveryDate: any;
  exerciseId: string;
  customerId: string;
  siteId: string;
  statusId: string;
  salesInvoiceId: string;
  details: Array<DeliveryNoteDetail>;
  salesInvoice?: SalesInvoice;
  id: string;
  disabled: string;
}

export interface DeliveryNoteDetail {
  id: string;
  deliveryNoteId: string;
  salesOrderDetailId: string;
  referenceId: string;
  description: string;
  quantity: number;
  unitCost: number;
  unitPrice: number;
  totalCost: number;
  amount: number;
  isInvoiced: boolean;
}

export interface CreateWorkOrderFromSalesOrderDto {
  workOrderDto: CreateWorkOrderDto;
  orderDetail: SalesOrderDetail;
}

export interface ConsolidatedIncomes {
  year: number;
  month: number;
  week: number;
  date: string;
  type: string;
  typeDetail: string;
  description: string;
  amount: number;
}

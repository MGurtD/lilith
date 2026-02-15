import { ExpenseService, ExpenseTypeService } from "./expense.service";
import { OrderService } from "./order.service";
import {
  PurchaseInvoiceSerieService,
  PurchaseInvoiceService,
} from "./invoice.service";
import { ReceiptService } from "./receipt.service";
import { SupplierService } from "./suppliers.service";

export default {
  PurchaseInvoice: new PurchaseInvoiceService("/PurchaseInvoice"),
  PurchaseInvoiceSerie: new PurchaseInvoiceSerieService("/InvoiceSerie"),
  Supplier: new SupplierService("/Supplier"),
  SupplierType: new SupplierService("/SupplierType"),
  Expense: new ExpenseService("Expense"),
  ExpenseType: new ExpenseTypeService("ExpenseType"),
  Receipt: new ReceiptService("/Receipt"),
  Order: new OrderService("/PurchaseOrder"),
};

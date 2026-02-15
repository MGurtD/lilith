import { defineStore } from "pinia";
import SalesService from "../services";
import SharedService from "../../shared/services";
import {
  SalesInvoice,
  SalesInvoiceDetail,
  CreateSalesHeaderRequest,
  DeliveryNote,
  CreateRectificativeInvoiceRequest,
} from "../types";
import { GenericResponse } from "../../../types";
import { PurchaseInvoiceUpdateStatues as InvoiceUpdateStatues } from "../../purchase/types";
import { REPORTS, ReportService } from "../../../services/report.service";
import { createBlobAndDownloadFile } from "../../../utils/functions";
import { Lifecycle } from "../../shared/types";

export const useSalesInvoiceStore = defineStore({
  id: "salseInvoices",
  state: () => ({
    invoice: undefined as SalesInvoice | undefined,
    invoices: undefined as Array<SalesInvoice> | undefined,
    verifactuLifecycle: undefined as Lifecycle | undefined,
  }),
  getters: {
    getVerifactuStatusById: (state) => (statusId: string) => {
      if (!state.verifactuLifecycle) return "";
      const status = state.verifactuLifecycle.statuses.find(
        (status) => status.id === statusId,
      );
      return status ? status.name : "";
    },
  },
  actions: {
    async Create(
      createRequest: CreateSalesHeaderRequest,
    ): Promise<GenericResponse<SalesInvoice> | undefined> {
      const response = await SalesService.SalesInvoice.Create(createRequest);
      return response;
    },
    async CreateRectificative(
      request: CreateRectificativeInvoiceRequest,
    ): Promise<GenericResponse<SalesInvoice> | undefined> {
      const response =
        await SalesService.SalesInvoice.CreateRectificative(request);
      return response;
    },
    async GetHeaderById(id: string) {
      const updatedInvoice = await SalesService.SalesInvoice.GetHeader(id);
      if (updatedInvoice && this.invoice) {
        this.invoice.baseAmount = updatedInvoice.baseAmount;
        this.invoice.taxAmount = updatedInvoice.taxAmount;
        this.invoice.grossAmount = updatedInvoice.grossAmount;
        this.invoice.netAmount = updatedInvoice.netAmount;
      }
    },
    async GetVerifactuLifecycle() {
      if (!this.verifactuLifecycle) {
        this.verifactuLifecycle =
          await SharedService.Lifecycle.getByName("Verifactu");
      }
    },
    async GetById(id: string) {
      const data = await SalesService.SalesInvoice.getById(id);
      if (data) {
        // Convert ISO date string to Date object for PrimeVue 4 DatePicker
        if (data.invoiceDate) {
          data.invoiceDate = new Date(data.invoiceDate) as any;
        }
      }
      this.invoice = data;

      if (!this.verifactuLifecycle) {
        this.verifactuLifecycle =
          await SharedService.Lifecycle.getByName("Verifactu");
      }
    },
    async GetDetailsById(id: string) {
      const updatedInvoice = await SalesService.SalesInvoice.getById(id);
      if (this.invoice && updatedInvoice)
        this.invoice.salesInvoiceDetails = updatedInvoice?.salesInvoiceDetails;
    },
    async GetFiltered(
      startDate: string,
      endDate: string,
      statusId?: string,
      customerId?: string,
      excludeStatusId?: string,
    ) {
      if (!this.verifactuLifecycle) {
        this.verifactuLifecycle =
          await SharedService.Lifecycle.getByName("Verifactu");
      }

      if (customerId) {
        this.invoices =
          await SalesService.SalesInvoice.GetBetweenDatesAndCustomer(
            startDate,
            endDate,
            customerId,
          );
      } else if (statusId) {
        this.invoices =
          await SalesService.SalesInvoice.GetBetweenDatesAndStatus(
            startDate,
            endDate,
            statusId,
          );
      } else if (excludeStatusId) {
        this.invoices =
          await SalesService.SalesInvoice.GetBetweenDatesAndExcludeStatus(
            startDate,
            endDate,
            excludeStatusId,
          );
      } else {
        this.invoices = await SalesService.SalesInvoice.GetBetweenDates(
          startDate,
          endDate,
        );
      }
    },
    async Update(invoice: SalesInvoice) {
      const updated = await SalesService.SalesInvoice.update(
        invoice.id,
        invoice,
      );
      return updated;
    },
    async UpdateInvoicesStatuses(
      invoiceImport: InvoiceUpdateStatues,
    ): Promise<boolean> {
      const updated =
        await SalesService.SalesInvoice.UpdateStatuses(invoiceImport);
      return updated;
    },
    async Delete(id: string): Promise<boolean> {
      const deleted = await SalesService.SalesInvoice.delete(id);
      return deleted;
    },

    async AddDeliveryNote(
      id: string,
      deliveryNote: DeliveryNote,
    ): Promise<GenericResponse<any>> {
      const response = await SalesService.SalesInvoice.AddDeliveryNote(
        id,
        deliveryNote,
      );
      this.GetDetailsById(id);
      this.GetHeaderById(id);
      return response;
    },
    async RemoveDeliveryNote(
      id: string,
      deliveryNote: DeliveryNote,
    ): Promise<GenericResponse<any>> {
      const response = await SalesService.SalesInvoice.RemoveDeliveryNote(
        id,
        deliveryNote,
      );
      this.GetDetailsById(id);
      this.GetHeaderById(id);
      return response;
    },

    async CreateInvoiceDetail(
      invoiceDetail: SalesInvoiceDetail,
    ): Promise<GenericResponse<any>> {
      const response =
        await SalesService.SalesInvoice.CreateDetail(invoiceDetail);
      this.GetDetailsById(invoiceDetail.salesInvoiceId);
      this.GetHeaderById(invoiceDetail.salesInvoiceId);
      return response;
    },
    async UpdateInvoiceDetail(
      invoiceDetail: SalesInvoiceDetail,
    ): Promise<boolean> {
      const updated =
        await SalesService.SalesInvoice.UpdateDetail(invoiceDetail);
      this.GetDetailsById(invoiceDetail.salesInvoiceId);
      this.GetHeaderById(invoiceDetail.salesInvoiceId);
      return updated;
    },
    async DeleteInvoiceDetail(
      invoiceDetail: SalesInvoiceDetail,
    ): Promise<boolean> {
      const deleted =
        await SalesService.SalesInvoice.DeleteDetail(invoiceDetail);
      this.GetDetailsById(invoiceDetail.salesInvoiceId);
      this.GetHeaderById(invoiceDetail.salesInvoiceId);
      return deleted;
    },
    async PrintInvoice(id: string, number: string): Promise<boolean> {
      const invoiceReport =
        await SalesService.SalesInvoice.GetReportDataById(id);
      if (!invoiceReport) return false;

      const fileName = `Factura_${number}.docx`;
      const reportService = new ReportService();
      const report = await reportService.Download(
        invoiceReport,
        REPORTS.Invoice,
        fileName,
      );

      if (report) {
        createBlobAndDownloadFile(fileName, report);
        return true;
      } else {
        return false;
      }
    },
  },
});

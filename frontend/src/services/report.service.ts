import { AxiosInstance } from "axios";
import reportApiClient from "@/api/reports.client";
import Services from "./";

export const REPORTS_ENTITY = "Reports";
export const REPORTS_ENTITY_ID = "22514ad8-7d58-4add-a49a-4396a6e4bbc5";

export enum REPORTS {
  Budget = "Budget",
  Order = "SalesOrder",
  DeliveryNote = "DeliveryNote",
  Invoice = "SalesInvoice",
  PurchaseOrder = "PurchaseOrder",
  WorkOrder = "WorkOrder",
}

export class ReportService {
  public apiClient: AxiosInstance;
  constructor() {
    this.apiClient = reportApiClient;
  }

  public async Download(
    data: any,
    report: REPORTS,
    fileName: string
  ): Promise<any> {
    const files = await Services.File.GetEntityFiles(
      REPORTS_ENTITY,
      REPORTS_ENTITY_ID
    );
    if (!files || files.length === 0) {
      throw new Error(
        "No hi ha informes disponibles. Carrega'ls a la ruta /reports"
      );
    }
    const file = files.find((f) => f.originalName.includes(report));
    if (!file) {
      throw new Error(
        `L'informe ${report} no existeix. Carrega'l amb aquest nom a la ruta /reports`
      );
    }

    let response = await this.apiClient.post(
      `/download`,
      {
        data,
        reportName: file.path,
        fileName,
      },
      { responseType: "blob" }
    );
    if (response.status === 200) {
      return response.data;
    }
  }
}

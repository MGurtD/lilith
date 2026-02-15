export interface FindInvoicesRequest {
  vatNumber: string;
  enterpriseName: string;
  year: number;
  month: number;
}

export interface FindInvoicesResponse {
  invoices: VerifactuInvoice[];
}

export interface VerifactuInvoice {
  numSerieFactura: string;
  fechaExpedicionFactura: string;
  importeTotal: string;
  idEmisorFactura: string;
  huella: string;
  tipoFactura: string;
  cuotaTotal: string;
  fechaHoraUsoRegistro: Date;
}

export interface VerifactuInvoiceRequest {
  id: string;
  salesInvoiceId: string;
  hash: string;
  request: string;
  response: string;
  success: boolean;
  status: string;
  timestampResponse: Date;
  createdOn: Date;
}

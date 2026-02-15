export interface CustomerSalesRanking {
  customerId: string;
  customerCode: string;
  customerName: string;
  year: number;
  month: number;
  quarter: number;
  totalSales: number;
  invoiceCount: number;
  monthKey: string;
  quarterKey: string;
}

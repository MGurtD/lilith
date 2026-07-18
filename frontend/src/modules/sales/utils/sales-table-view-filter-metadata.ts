import type { Column } from "@/components/tables/types";
import {
  createTableViewFilterMetadata,
  type FilterValueResolver,
  type TableViewFilterMetadata,
} from "@/components/tables/table-view-filter-metadata";

interface SalesTableViewFilterMetadataOptions {
  dateLabel?: string;
  customerResolver?: (id: string) => string;
  statusResolver?: (id: string) => string;
}

export function createSalesTableViewFilterMetadata(
  columns: readonly Column[],
  options: SalesTableViewFilterMetadataOptions = {},
): TableViewFilterMetadata {
  const valueResolvers: Record<string, FilterValueResolver> = {};

  if (options.customerResolver) {
    valueResolvers.customerId = (value) =>
      typeof value === "string" ? options.customerResolver!(value) : "";
  }

  if (options.statusResolver) {
    valueResolvers.statusId = (value) =>
      typeof value === "string" ? options.statusResolver!(value) : "";
  }

  return createTableViewFilterMetadata(columns, {
    labels: {
      dates: options.dateLabel ?? "Període",
      customerId: "Client",
      statusId: "Estat",
    },
    valueResolvers,
  });
}

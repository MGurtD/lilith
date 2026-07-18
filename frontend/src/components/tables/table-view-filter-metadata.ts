import type { Column } from "./types";

export type FilterValueResolver = (value: unknown) => string;

export interface TableViewFilterMetadata {
  filterLabels: Record<string, string>;
  filterValueResolvers: Record<string, FilterValueResolver>;
}

interface TableViewFilterMetadataOptions {
  labels?: Record<string, string>;
  valueResolvers?: Record<string, FilterValueResolver>;
}

export function createTableViewFilterMetadata(
  columns: readonly Column[],
  options: TableViewFilterMetadataOptions = {},
): TableViewFilterMetadata {
  const filterLabels = Object.fromEntries(
    columns.map((column) => [column.field, column.header]),
  );
  const filterValueResolvers: Record<string, FilterValueResolver> = {};

  for (const column of columns) {
    const resolver = column.resolver;
    if (!resolver) continue;

    filterValueResolvers[column.field] = (value) =>
      typeof value === "string" ? resolver(value) : "";
  }

  return {
    filterLabels: { ...filterLabels, ...options.labels },
    filterValueResolvers: {
      ...filterValueResolvers,
      ...options.valueResolvers,
    },
  };
}

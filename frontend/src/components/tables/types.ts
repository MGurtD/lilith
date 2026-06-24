export type Aggregation = "sum" | "avg" | "count" | "min" | "max";

export type TablePreset = "crud-list" | "read-only" | "detail-lines" | "selector";

export enum ColumnType {
  Text = "text",
  Boolean = "boolean",
  Date = "date",
  DateTime = "datetime",
  Time = "time",
  Currency = "currency",
  Number = "number",
  Lookup = "lookup",
}

export interface Column {
  field: string;
  header: string;
  columnType?: ColumnType;
  sortable?: boolean;
  total?: Aggregation;
  totalFormat?: (value: number) => string;
  visible?: boolean;
  order?: number;
  style?: string;
  showColor?: boolean;
  resolver?: (id: string) => string;
  truncate?: boolean;
}

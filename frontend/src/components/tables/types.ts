export type Aggregation = "sum" | "avg" | "count" | "min" | "max";

export type TablePreset = "crud-list" | "read-only" | "detail-lines" | "selector";

/**
 * Enables the read-only attachment viewer system column in Table.
 */
export interface AttachmentConfig {
  /** Logical entity name stored in File.entity (for example, "SalesOrder"). */
  entity: string;
  /** Allowed extensions, case-insensitive and with or without a leading dot. */
  formats?: string[];
  /** Dialog title. Defaults to the localized attachment label. */
  title?: string;
  /** Row field appended to the dialog title to identify the open entity. */
  titleField?: string;
}

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

export type Aggregation = "sum" | "avg" | "count" | "min" | "max";

export type TablePreset =
  "crud-list" | "read-only" | "detail-lines" | "selector";

/**
 * Enables the read-only attachment viewer system column in Table.
 */
export interface AttachmentConfig {
  /** Logical entity name stored in File.entity (for example, "SalesOrder"). */
  entity: string;
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
  /** A Lookup shown as a coloured Tag; `severity` picks the colour. */
  Status = "status",
  ProgressBar = "progressbar",
}

/** Options supported by the ProgressBar column type. */
export interface ProgressBarColumnProps {
  showValue?: boolean;
  cap?: boolean;
  overrunSeverity?: "text" | "bar" | "both";
  tooltip?: string | ((data: never) => string);
}

/**
 * Type-specific options for a table column.
 * Extend this type as specialized column types are introduced.
 */
export type ColumnProps = ProgressBarColumnProps;

export type ColumnResolver = {
  resolve(value: unknown, data: unknown): unknown;
}["resolve"];

/**
 * Which column fills each slot of a row's phone card. Values are column
 * fields. A screen declares its default with Table's `cardLayout` prop;
 * a saved view may override it as a whole.
 */
export interface CardLayout {
  title?: string;
  subtitle?: string;
  /** Usually a Status column, shown as a Tag. */
  badge?: string;
  /** An amount or a date, right-aligned next to the title. */
  trailing?: string;
  /** Extra "Label: value" lines, in column order. */
  meta?: string[];
}

export const CARD_META_MAX = 3;

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
  frozen?: boolean;
  showColor?: boolean;
  resolver?: ColumnResolver;
  /** Status columns: PrimeVue Tag severity for the value (none → neutral grey). */
  severity?: (value: unknown, data: unknown) => string | undefined;
  truncate?: boolean;
  props?: ColumnProps;
  /**
   * Shown only in the phone card, never as a table column: a summary
   * of other columns, such as the dimensions, filled by a slot.
   */
  cardOnly?: boolean;
}

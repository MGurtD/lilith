import { resolveFieldValue } from "./field-value";
import {
  formatDate,
  formatDateTime,
  formatTime,
  formatCurrency,
} from "@/utils/functions";
import { ColumnType, type Column } from "./types";

// Shared by the table cells and the phone cards so both show the same text.

// Empty-value guard: prevents Date/DateTime/Time columns from rendering
// the epoch (01/01/1970) when the field is null/undefined/empty string.
// `new Date(null)` is `Date(0)` → epoch, which Intl then formats as 01/01/1970.
export function hasValue(value: unknown): boolean {
  if (value === null || value === undefined) return false;
  if (typeof value === "string" && value.trim() === "") return false;
  return true;
}

export function resolveCellValue(col: Column, data: unknown): unknown {
  const value = resolveFieldValue(data, col.field);
  if (!col.resolver) return value;
  const isLookup =
    col.columnType === ColumnType.Lookup || col.columnType === ColumnType.Status;
  if (isLookup && typeof value !== "string") {
    return undefined;
  }
  return col.resolver(value, data);
}

// Single source of truth for the display string of a cell value.
export function formatCellValue(col: Column, data: unknown): string {
  const value = resolveCellValue(col, data);
  switch (col.columnType) {
    case ColumnType.Date:
      return typeof value === "string" || value instanceof Date
        ? formatDate(value)
        : "";
    case ColumnType.DateTime:
      return typeof value === "string"
        ? formatDateTime(value)
        : value instanceof Date
          ? formatDateTime(value.toISOString())
          : "";
    case ColumnType.Time:
      return typeof value === "string" || value instanceof Date
        ? formatTime(value)
        : "";
    case ColumnType.Currency:
      return typeof value === "number" ? formatCurrency(value) : "";
    case ColumnType.Lookup:
    case ColumnType.Status:
      return String(value ?? "");
    case ColumnType.Number:
      return String(value);
    default:
      return String(value ?? "");
  }
}

// Colours come from the lifecycle administration as PrimeVue severities;
// a status without one stays neutral.
const STATUS_SEVERITIES = ["secondary", "info", "warn", "success", "danger", "contrast"];

export type StatusSeverity =
  | "secondary"
  | "info"
  | "success"
  | "warn"
  | "danger"
  | "contrast"
  | undefined;

export function statusSeverity(col: Column, data: unknown): StatusSeverity {
  const value = resolveFieldValue(data, col.field);
  const severity = col.severity?.(value, data);
  // Unknown or retired values ("help") fall back to neutral.
  return (
    severity && STATUS_SEVERITIES.includes(severity) ? severity : "secondary"
  ) as StatusSeverity;
}

export function resolveBooleanValue(
  data: unknown,
  field: string,
): boolean | null | undefined {
  const value = resolveFieldValue(data, field);
  if (typeof value === "boolean" || value === null) return value;
  return undefined;
}

import { formatDate } from "@/utils/functions";
import { hydrateFilter } from "@/utils/filter-hydrate";
import type { FilterConfig } from "./TableFilter.vue";

/** Display value of a filter with no value ("no filter"). */
export const EMPTY_FILTER_VALUE = "—";

/** An applied filter as the user reads it: "Proveïdor: Proveïdor 8". */
export interface ActiveFilterSummary {
  key: string;
  label: string;
  /** Empty when the value is an id with no resolver: the label alone is shown. */
  value: string;
}

const UUID_PATTERN =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

// Format a single date (Date object or ISO string) using the project's
// standard formatDate. Falls back to the raw string if parsing fails.
function formatDateValue(value: unknown): string {
  if (value === null || value === undefined || value === "")
    return EMPTY_FILTER_VALUE;
  if (value instanceof Date) {
    return Number.isNaN(value.getTime())
      ? EMPTY_FILTER_VALUE
      : formatDate(value);
  }
  if (typeof value === "string" && value) {
    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime())
      ? EMPTY_FILTER_VALUE
      : formatDate(parsed);
  }
  if (typeof value === "object") return EMPTY_FILTER_VALUE;
  return String(value ?? "");
}

// Format a stored filter value for display. Uses the FilterConfig metadata
// (label, type, options) to render the value the same way the user sees it
// in the TableFilter inputs. Empty values render as "—" so the user can
// distinguish "no filter" from "filter with empty value".
//
// Special case: date-range arrays (e.g. PrimeVue `selectionMode="range"`
// DatePicker) are formatted as "dd/MM/yyyy — dd/MM/yyyy" rather than raw
// JSON. Detected by: field is null/unknown AND value is an array of length 2
// where each entry parses to a Date.
export function formatFilterValue(
  field: FilterConfig | null,
  value: unknown,
): string {
  if (value === null || value === undefined || value === "")
    return EMPTY_FILTER_VALUE;
  if (value === false) return "No";

  // Date range: array of two date-like entries, no matching field config
  // (PrimeVue range DatePicker is typically a prepend-slot filter).
  // Treat empty or invalid endpoints as "no filter" (—) so an
  // accidentally-initialised empty range doesn't render as "[{},{}]".
  const isDateLike = (entry: unknown): boolean =>
    entry instanceof Date ||
    typeof entry === "string" ||
    entry === null ||
    entry === undefined ||
    (typeof entry === "object" && entry !== null);

  if (
    Array.isArray(value) &&
    value.length === 2 &&
    isDateLike(value[0]) &&
    isDateLike(value[1])
  ) {
    const start = formatDateValue(value[0]);
    const end = formatDateValue(value[1]);
    if (start === EMPTY_FILTER_VALUE || end === EMPTY_FILTER_VALUE)
      return EMPTY_FILTER_VALUE;
    return `${start} — ${end}`;
  }

  if (!field) {
    if (value instanceof Date) return formatDate(value);
    if (typeof value === "string" || typeof value === "number") {
      return String(value);
    }
    return JSON.stringify(value);
  }

  if (field.type === "checkbox") {
    return value === true ? "Sí" : "No";
  }

  if (field.type === "select") {
    const optionValue = field.optionValue ?? "value";
    const optionLabel = field.optionLabel ?? "label";
    const option = (field.options ?? []).find(
      (o) => (o as Record<string, unknown>)[optionValue] === value,
    );
    return option
      ? String((option as Record<string, unknown>)[optionLabel] ?? value)
      : String(value);
  }

  if (field.type === "multiselect") {
    if (!Array.isArray(value)) return String(value);
    const optionValue = field.optionValue ?? "value";
    const optionLabel = field.optionLabel ?? "label";
    const labels = value.map((v) => {
      const option = (field.options ?? []).find(
        (o) => (o as Record<string, unknown>)[optionValue] === v,
      );
      return option
        ? String((option as Record<string, unknown>)[optionLabel] ?? v)
        : String(v);
    });
    return labels.join(", ");
  }

  if (field.type === "number") {
    return String(value);
  }

  if (value instanceof Date) {
    return formatDate(value);
  }

  return String(value);
}

// Readable value of a filter: the field's own `valueLabel` when it has one
// (e.g. supplierId -> "Acme Corp"), otherwise the default formatter.
export function resolveFilterDisplayValue(
  field: FilterConfig | null,
  value: unknown,
): string {
  if (field?.valueLabel && !Array.isArray(value)) {
    try {
      const resolved = field.valueLabel(value);
      if (resolved) return resolved;
    } catch {
      // fall through to default formatter
    }
  }
  return formatFilterValue(field, value);
}

/**
 * The filters currently applied, in the order they are declared in `config`,
 * for the compact filter bar. Empty values, unchecked checkboxes and empty
 * selections are skipped.
 */
export function activeFilterSummaries(
  values: Record<string, unknown> | null | undefined,
  config: FilterConfig[] = [],
): ActiveFilterSummary[] {
  if (!values || typeof values !== "object" || Array.isArray(values)) return [];
  const hydrated = hydrateFilter({ ...values }) as Record<string, unknown>;
  const summaries: ActiveFilterSummary[] = [];

  for (const field of config) {
    const value = hydrated[field.key];
    if (!field.label || value === false) continue;
    if (Array.isArray(value) && value.length === 0) continue;

    const display = resolveFilterDisplayValue(field, value);
    if (display === EMPTY_FILTER_VALUE || display === "") continue;

    summaries.push({
      key: field.key,
      label: field.label,
      value: UUID_PATTERN.test(display) ? "" : display,
    });
  }
  return summaries;
}

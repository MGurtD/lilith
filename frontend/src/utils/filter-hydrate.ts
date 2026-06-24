/**
 * Shared utility for hydrating ISO date strings into Date objects.
 * Used when deserializing filter configurations from the database.
 */

const ISO_DATE_RE = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/;

export function hydrateValue(value: unknown): unknown {
  if (typeof value === "string" && ISO_DATE_RE.test(value)) {
    return new Date(value);
  }
  if (Array.isArray(value)) {
    return value.map(hydrateValue);
  }
  if (value !== null && typeof value === "object") {
    return hydrateDates(value as Record<string, unknown>);
  }
  return value;
}

export function hydrateDates(obj: Record<string, unknown>): Record<string, unknown> {
  const result: Record<string, unknown> = {};
  for (const key of Object.keys(obj)) {
    result[key] = hydrateValue(obj[key]);
  }
  return result;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function hydrateFilter(parsed: any): any {
  if (parsed === null || typeof parsed !== "object") return parsed;
  if (Array.isArray(parsed)) return parsed.map(hydrateValue);
  return hydrateDates(parsed as Record<string, unknown>);
}
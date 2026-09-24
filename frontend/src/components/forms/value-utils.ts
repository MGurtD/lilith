/**
 * Narrows values emitted by PrimeVue Forms without coercing unexpected types.
 * Domain normalization such as trimming strings remains the form's concern.
 */
export const stringValue = (value: unknown, fallback: string): string =>
  typeof value === "string" ? value : fallback;

/** Preserves an explicit null and falls back for every other non-string value. */
export const nullableStringValue = (
  value: unknown,
  fallback: string | null,
): string | null =>
  typeof value === "string" || value === null ? value : fallback;

/** Treats an explicit null as a cleared optional value. */
export const optionalStringValue = (
  value: unknown,
  fallback: string | undefined,
): string | undefined =>
  typeof value === "string" ? value : value === null ? undefined : fallback;

export const finiteNumberValue = <
  TFallback extends number | null | undefined,
>(
  value: unknown,
  fallback: TFallback,
): number | TFallback =>
  typeof value === "number" && Number.isFinite(value) ? value : fallback;

export const integerValue = (value: unknown, fallback: number): number =>
  typeof value === "number" && Number.isInteger(value) ? value : fallback;

export const booleanValue = (value: unknown, fallback: boolean): boolean =>
  typeof value === "boolean" ? value : fallback;

export const dateValue = <TFallback extends Date | null | undefined>(
  value: unknown,
  fallback: TFallback,
): Date | TFallback =>
  value instanceof Date ? value : fallback;

export function resolveFieldValue(data: unknown, field: string): unknown {
  if (data === null || typeof data !== "object") return undefined;

  const record = data as Record<string, unknown>;
  if (Object.prototype.hasOwnProperty.call(record, field)) return record[field];

  return field.split(".").reduce<unknown>((value, segment) => {
    if (value === null || typeof value !== "object") return undefined;
    return (value as Record<string, unknown>)[segment];
  }, data);
}

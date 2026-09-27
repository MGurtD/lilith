// Legacy receipts without a date carry the minimum date (0001-01-01); show them empty.
export const receiptDate = (value: unknown): unknown =>
  typeof value === "string" && value.startsWith("0001-") ? undefined : value;

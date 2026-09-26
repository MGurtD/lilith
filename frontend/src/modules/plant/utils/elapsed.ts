// Time rules of the plant screens (frontend/docs/plant-mes-redesign.md):
// under an hour "38 min", under a day "4 h 20 min", a day or more
// "46 d 8 h"; live clocks show hh:mm:ss up to 99 hours. No data is "—",
// never an invented timer.

export const NO_TIME = "—";

// The backend sends DateTime.MinValue (year 1) when a workcenter has no
// status yet; anything before 2000 is treated as missing.
const MIN_VALID_YEAR = 2000;

/** Whole seconds from `start` to `now`, or null when `start` is missing. */
export function elapsedSeconds(
  start: string | null | undefined,
  now: number,
): number | null {
  if (!start) return null;
  const date = new Date(start);
  const time = date.getTime();
  if (Number.isNaN(time) || date.getFullYear() < MIN_VALID_YEAR) return null;
  return Math.max(0, Math.floor((now - time) / 1000));
}

/** "38 min", "4 h 20 min", "46 d 8 h". */
export function formatElapsed(seconds: number | null): string {
  if (seconds === null) return NO_TIME;
  const minutes = Math.floor(seconds / 60);
  if (minutes < 1) return "< 1 min";
  if (minutes < 60) return `${minutes} min`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} h ${String(minutes % 60).padStart(2, "0")} min`;
  return `${Math.floor(hours / 24)} d ${hours % 24} h`;
}

/** "04:20:16" up to 99 hours, then the short form ("46 d 8 h"). */
export function formatElapsedClock(seconds: number | null): string {
  if (seconds === null) return NO_TIME;
  const hours = Math.floor(seconds / 3600);
  if (hours > 99) return formatElapsed(seconds);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;
  return [hours, minutes, secs].map((v) => String(v).padStart(2, "0")).join(":");
}

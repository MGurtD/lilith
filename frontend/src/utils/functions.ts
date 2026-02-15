import _ from "lodash";
import { v4 as uuidv4 } from "uuid";

Date.prototype.toJSON = function () {
  const hoursDiff = this.getHours() - this.getTimezoneOffset() / 60;
  this.setHours(hoursDiff);
  return this.toISOString();
};

export const convertDDMMYYYYToDate = (strDate: string): any => {
  const strDateParts = strDate.split("/");
  if (strDateParts.length === 3) {
    return createDate(
      parseInt(strDateParts[0]),
      parseInt(strDateParts[1]),
      parseInt(strDateParts[2]),
    );
  }
};

export const createDate = (
  days: number,
  months: number,
  years: number,
): Date => {
  let date = new Date();
  date.setDate(days);
  date.setMonth(months - 1);
  date.setFullYear(years);
  return date;
};

export const createDateFromNow = (
  daysToAdd: number,
  monthsToAdd: number,
  yearsToAdd: number,
): Date => {
  let date = new Date();
  date.setDate(date.getDate() + daysToAdd);
  date.setMonth(date.getMonth() + monthsToAdd);
  date.setFullYear(date.getFullYear() + yearsToAdd);
  return date;
};

export const formatTime = (date: Date | string): string => {
  if (!date) return "--:--";
  const d = new Date(date);
  return d.toLocaleTimeString("ca-ES", { hour: "2-digit", minute: "2-digit" });
};

/**
 * Formats a duration in minutes as a human-readable string.
 * Examples: 45 -> "45'", 90 -> "1h30'", 125 -> "2h5'"
 */
export const formatDuration = (minutes: number): string => {
  if (minutes < 60) {
    return `${Math.round(minutes)}m`;
  }
  const hours = Math.floor(minutes / 60);
  const mins = Math.round(minutes % 60);
  return `${hours}h ${mins}m`;
};

export const formatDate = (date: string | Date) => {
  const formatter = new Intl.DateTimeFormat("es-ES", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    timeZone: "Europe/Madrid",
  });

  let formattedDate = "";
  try {
    formattedDate = formatter.format(new Date(date));
  } catch (err) {}

  return formattedDate;
};

export const formatDateTime = (dateTime: string) => {
  const formatter = new Intl.DateTimeFormat("es-ES", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "numeric",
    minute: "numeric",
    timeZone: "Europe/Madrid",
  });

  return formatter.format(new Date(dateTime));
};

export const formatDateTimeUTC = (dateTime: string) => {
  const formatter = new Intl.DateTimeFormat("es-ES", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    timeZone: "UTC",
  });

  return formatter.format(new Date(dateTime));
};

export const formatDateTimeUTCWithSeconds = (dateTime: string) => {
  const formatter = new Intl.DateTimeFormat("es-ES", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    timeZone: "UTC",
  });

  return formatter.format(new Date(dateTime));
};

export const formatCurrency = (value: number) => {
  return new Intl.NumberFormat("de-DE", {
    style: "currency",

    currency: "EUR",
  }).format(value);
};

export const convertDateTimeToJSON = (dateTime: any): any => {
  if (dateTime instanceof Date) {
    const hoursDiff = dateTime.getHours() - dateTime.getTimezoneOffset() / 60;
    dateTime.setHours(hoursDiff);
    return dateTime.toISOString();
  } else if (_.isString(dateTime)) {
    return convertDateTimeToJSON(convertDDMMYYYYToDate(dateTime));
  }
};

export const formatDateForQueryParameter = (date: Date): string => {
  return convertDateTimeToJSON(date).split("T")[0];
};

export const calculateDuration = (startTime: string | null): string => {
  if (!startTime) return "--";

  const start = new Date(startTime);
  const now = new Date();
  let diff = now.getTime() - start.getTime();

  // Avoid negative duration if server time is slightly ahead
  if (diff < 0) diff = 0;

  const hours = Math.floor(diff / (1000 * 60 * 60));
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

  return `${hours}h ${minutes}m`;
};

export const createBlobAndDownloadFile = (
  name: string,
  data: any,
  mimeType?: string,
) => {
  // Create blob with proper MIME type if provided
  const blob = mimeType
    ? new Blob([data], { type: mimeType })
    : new Blob([data]);
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");

  link.href = url;
  link.setAttribute("download", name);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);

  // Clean up the URL after a short delay
  setTimeout(() => URL.revokeObjectURL(url), 100);
};

export const getNewUuid = () => {
  return uuidv4();
};

export const extractTime = (isoString: string | null): string => {
  if (isoString) {
    const date = new Date(isoString);
    // Formatea la hora, minutos y segundos
    const hours = date.getHours().toString().padStart(2, "0");
    const minutes = date.getMinutes().toString().padStart(2, "0");
    const seconds = "00";
    // Retorna la hora en formato HH:mm:ss
    return `${hours}:${minutes}:${seconds}`;
  } else {
    return "";
  }
};

export function calculateTimeDifference(startDateString: string) {
  const startDate = new Date(startDateString);
  const now = new Date();

  const diffInMilliseconds = now.getTime() - startDate.getTime();
  if (diffInMilliseconds < 0) {
    return 0;
  }

  const hours = Math.floor(diffInMilliseconds / (1000 * 60 * 60));
  const minutes = Math.floor(
    (diffInMilliseconds % (1000 * 60 * 60)) / (1000 * 60),
  );
  const seconds = Math.floor((diffInMilliseconds % (1000 * 60)) / 1000);

  const formattedTime = `${String(hours).padStart(2, "0")}:${String(
    minutes,
  ).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;
  return formattedTime;
}

export function loadImage(blob: Blob): Promise<HTMLImageElement> {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve(img);
    img.onerror = reject;
    const url = URL.createObjectURL(blob);
    img.src = url;
  });
}

/**
 * Normalizes a color string by ensuring it has the # prefix
 * @param color - Color string with or without # prefix
 * @param defaultColor - Default color to use if color is undefined (default: '6c757d')
 * @returns Normalized color string with # prefix
 */
export const normalizeColor = (
  color: string | undefined,
  defaultColor: string = "6c757d",
): string => {
  const baseColor = color || defaultColor;
  return baseColor.startsWith("#") ? baseColor : `#${baseColor}`;
};

/**
 * Determines if a color is light or dark based on luminance
 * @param color - Color string (with or without # prefix)
 * @returns true if the color is light, false if dark
 */
export const isColorLight = (color: string): boolean => {
  const hex = color.replace("#", "");
  const r = parseInt(hex.substring(0, 2), 16);
  const g = parseInt(hex.substring(2, 4), 16);
  const b = parseInt(hex.substring(4, 6), 16);

  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  return luminance > 0.7;
};

/**
 * Adjusts the brightness of a color
 * @param color - Color string (with # prefix)
 * @param percent - Percentage to adjust (-100 to 100)
 * @returns Adjusted color string with # prefix
 */
export const adjustBrightness = (color: string, percent: number): string => {
  const num = parseInt(color.replace("#", ""), 16);
  const amt = Math.round(2.55 * percent);
  const R = (num >> 16) + amt;
  const G = ((num >> 8) & 0x00ff) + amt;
  const B = (num & 0x0000ff) + amt;
  return (
    "#" +
    (
      0x1000000 +
      (R < 255 ? (R < 1 ? 0 : R) : 255) * 0x10000 +
      (G < 255 ? (G < 1 ? 0 : G) : 255) * 0x100 +
      (B < 255 ? (B < 1 ? 0 : B) : 255)
    )
      .toString(16)
      .slice(1)
  );
};

/**
 * Generates dynamic styles for status cards with gradient and text color
 * @param color - Status color (with or without # prefix)
 * @returns Object with CSS custom properties for status styling
 */
export const getStatusCardStyle = (color: string | undefined) => {
  const baseColor = normalizeColor(color);
  const isLightColor = isColorLight(baseColor);

  return {
    "--status-color": baseColor,
    "--status-gradient": isLightColor
      ? `linear-gradient(135deg, ${baseColor}, ${adjustBrightness(baseColor, -10)})`
      : `linear-gradient(135deg, ${baseColor}, ${adjustBrightness(baseColor, -15)})`,
    "--status-text-color": isLightColor ? "#000000" : "#ffffff",
  };
};

/**
 * Generates border top style with gradient for workcenter cards
 * @param color - Status color (with or without # prefix)
 * @param defaultColor - Default color if color is undefined
 * @returns Object with borderTop CSS property
 */
export const getBorderTopStyle = (
  color: string | undefined,
  defaultColor: string = "6c757d",
) => {
  const baseColor = normalizeColor(color, defaultColor);
  const darkerColor = adjustBrightness(baseColor, -10);

  return {
    background: `linear-gradient(90deg, ${baseColor}, ${darkerColor})`,
  };
};

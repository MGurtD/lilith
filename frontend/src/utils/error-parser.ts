import type { AxiosError } from "axios";
import type {
  ErrorResponse,
  GenericResponse,
  ParsedErrorInfo,
} from "@/types/error-response";

/**
 * Parses Axios errors and extracts meaningful information
 * Supports both RFC 7807 ErrorResponse and legacy GenericResponse formats
 */
export function parseAxiosError(error: AxiosError): ParsedErrorInfo {
  // Network error (no response from server)
  if (!error.response) {
    return {
      message: getNetworkErrorMessage(error),
      errors: [error.message || "Error de xarxa"],
      statusCode: 0,
      severity: "error",
      isNetworkError: true,
    };
  }

  const { data, status } = error.response;

  // Try to parse as new ErrorResponse format (RFC 7807)
  if (isErrorResponse(data)) {
    return {
      message: data.title || getDefaultErrorMessage(status),
      errors: data.errors || [],
      statusCode: data.status || status,
      traceId: data.traceId,
      path: data.instance,
      severity: getSeverity(data.status || status),
      isNetworkError: false,
    };
  }

  // Try to parse as legacy GenericResponse format
  if (isGenericResponse(data)) {
    return {
      message: data.errors[0] || getDefaultErrorMessage(status),
      errors: data.errors || [],
      statusCode: status,
      severity: getSeverity(status),
      isNetworkError: false,
    };
  }

  // Fallback for unknown format
  return {
    message: getDefaultErrorMessage(status),
    errors: [typeof data === "string" ? data : "Error desconegut"],
    statusCode: status,
    severity: status >= 500 ? "error" : "warn",
    isNetworkError: false,
  };
}

function isErrorResponse(data: any): data is ErrorResponse {
  return (
    data &&
    typeof data === "object" &&
    "traceId" in data &&
    "status" in data &&
    "errors" in data &&
    Array.isArray(data.errors)
  );
}

function isGenericResponse(data: any): data is GenericResponse {
  return (
    data &&
    typeof data === "object" &&
    "result" in data &&
    "errors" in data &&
    Array.isArray(data.errors)
  );
}

function getSeverity(statusCode: number): "error" | "warn" | "info" {
  if (statusCode >= 500) return "error";
  if (statusCode >= 400) return "warn";
  return "info";
}

function getNetworkErrorMessage(error: AxiosError): string {
  if (error.code === "ECONNABORTED" || error.message.includes("timeout")) {
    return "Temps d'espera exhaurit. Comprova la connexió i torna-ho a intentar.";
  }
  if (error.code === "ERR_NETWORK" || !error.response) {
    return "No s'ha pogut connectar amb el servidor. Comprova la connexió a Internet.";
  }
  return "Error de comunicació amb el servidor.";
}

function getDefaultErrorMessage(statusCode: number): string {
  switch (statusCode) {
    case 400:
      return "La sol·licitud no és vàlida";
    case 401:
      return "Cal autenticar-se per accedir a aquest recurs";
    case 403:
      return "No tens permisos per accedir a aquest recurs";
    case 404:
      return "El recurs sol·licitat no existeix";
    case 409:
      return "Conflicte: aquesta operació no es pot completar";
    case 422:
      return "Les dades enviades no són vàlides";
    case 429:
      return "Massa sol·licituds. Espera uns moments i torna-ho a intentar.";
    case 500:
      return "Error intern del servidor";
    case 502:
      return "El servidor no està disponible temporalment";
    case 503:
      return "Servei no disponible. Intenta-ho més tard.";
    case 504:
      return "Temps d'espera del servidor exhaurit";
    default:
      return statusCode >= 500
        ? "S'ha produït un error al servidor"
        : "S'ha produït un error en processar la sol·licitud";
  }
}

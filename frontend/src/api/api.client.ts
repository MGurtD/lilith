import axios, { AxiosError } from "axios";
import { useApiStore } from "../store/backend";
import { useStore } from "../store";
import { globalToast } from "@/utils/global-toast";
import { parseAxiosError } from "@/utils/error-parser";

const baseUrl = import.meta.env.VITE_API_BASE_URL as string;
const requestTimeout =
  (import.meta.env.VITE_API_REQUEST_TIMEOUT as number) ?? 5000;

const apiClient = axios.create({
  baseURL: baseUrl.endsWith("/api") ? baseUrl : `${baseUrl}`,
  timeout: requestTimeout,
  headers: {
    "Content-Type": "application/json",
    // anything you want to add to the headers
  },
  validateStatus: function (status) {
    return status >= 200 && status <= 404;
  },
});

// Track online/offline status
let isOfflineToastShown = false;

// Listen to online/offline events
if (typeof window !== "undefined") {
  window.addEventListener("online", () => {
    if (isOfflineToastShown) {
      globalToast.success(
        "Connexió restaurada",
        "La connexió a Internet s'ha restablert correctament.",
        5000
      );
      isOfflineToastShown = false;
    }
  });

  window.addEventListener("offline", () => {
    globalToast.showPersistent({
      severity: "warn",
      summary: "Sense connexió",
      detail:
        "No hi ha connexió a Internet. Les dades es sincronitzaran quan es restableixi la connexió.",
    });
    isOfflineToastShown = true;
  });
}

// Add a request interceptor
apiClient.interceptors.request.use(
  function (config) {
    const store = useApiStore();
    store.isWaiting = true;
    // Attach language headers and optional culture override
    const appStore = useStore();
    if (!config.headers) config.headers = {} as any;
    const lang = (appStore as any)?.language?.current;
    if (lang) {
      (config.headers as any)["Accept-Language"] = lang;
    }
    const cultureOverride = (appStore as any)?.language?.cultureOverride;
    if (cultureOverride) {
      config.params = {
        ...(config.params ?? {}),
        culture: cultureOverride,
      } as any;
    }

    // Do something before request is sent
    return config;
  },
  function (error) {
    // Do something with request error
    return Promise.reject(error);
  }
);

// Add a response interceptor
apiClient.interceptors.response.use(
  function (response) {
    // Any status code that lie within the range of 2xx cause this function to trigger
    // Do something with response data
    const store = useApiStore();
    store.isWaiting = false;

    return response;
  },
  function (error: AxiosError) {
    const store = useApiStore();
    store.isWaiting = false;

    // Check if offline
    if (!navigator.onLine) {
      if (!isOfflineToastShown) {
        globalToast.showPersistent({
          severity: "warn",
          summary: "Sense connexió",
          detail:
            "No hi ha connexió a Internet. Comprova la connexió i torna-ho a intentar.",
        });
        isOfflineToastShown = true;
      }
      return Promise.reject(error);
    }

    // Parse error with our utility
    const errorInfo = parseAxiosError(error);

    console.log("Parsed error info:", errorInfo);

    // Store error info for components that might need it
    store.setErrorInfo(errorInfo);

    // Special handling for 401 Unauthorized - redirect to login
    if (errorInfo.statusCode === 401) {
      const appStore = useStore();
      appStore.removeAuthorization();

      globalToast.warn(
        "Sessió expirada",
        "La teva sessió ha expirat. Si us plau, torna a iniciar sessió.",
        6000
      );

      // Redirect to login (only if not already there)
      if (
        window.location.pathname !== "/" &&
        window.location.pathname !== "/login"
      ) {
        setTimeout(() => {
          window.location.href = "/";
        }, 1000);
      }

      return Promise.reject(error);
    }

    // Build toast message
    let detail = "";

    // Add primary error if different from message
    if (
      errorInfo.errors.length > 0 &&
      errorInfo.errors[0] !== errorInfo.message
    ) {
      detail = errorInfo.errors[0];
    }

    // Add additional errors (max 2 more)
    if (errorInfo.errors.length > 1) {
      const additionalErrors = errorInfo.errors.slice(1, 3).join("\n");
      detail = detail ? `${detail}\n\n${additionalErrors}` : additionalErrors;
    }

    // Add trace ID if available
    if (errorInfo.traceId) {
      detail = detail
        ? `${detail}\n\nID de seguiment: ${errorInfo.traceId}`
        : `ID de seguiment: ${errorInfo.traceId}`;
    }

    // Determine toast life based on severity and status
    let life = 6000;
    if (errorInfo.statusCode >= 500) life = 8000;
    else if (errorInfo.statusCode === 409)
      life = 7000; // Conflicts need more attention
    else if (errorInfo.isNetworkError) life = 8000;

    // Show toast with automatic deduplication
    globalToast.show({
      severity: errorInfo.severity,
      summary: errorInfo.message,
      detail: detail || undefined,
      life,
    });

    // Log to console in development mode
    if (import.meta.env.DEV) {
      console.group(
        `🔴 API Error [${errorInfo.statusCode}] ${error.config?.url}`
      );
      console.error("Message:", errorInfo.message);
      console.error("Errors:", errorInfo.errors);
      if (errorInfo.traceId) {
        console.error("TraceId:", errorInfo.traceId);
      }
      if (errorInfo.path) {
        console.error("Path:", errorInfo.path);
      }
      console.groupEnd();
    }

    return Promise.reject(error);
  }
);

export function logException(error: any) {
  if (error.response) {
    // The request was made and the server responded with a status code
    // that falls out of the range of 2xx
    console.log(error.response.data);
    console.log(error.response.status);
    console.log(error.response.headers);
  } else if (error.request) {
    // The request was made but no response was received
    // `error.request` is an instance of XMLHttpRequest in the browser and an instance of
    // http.ClientRequest in node.js
    console.log(error.request);
  } else {
    // Something happened in setting up the request that triggered an Error
    console.log("Error", error.message);
  }
  console.log(error);
}

export default apiClient;

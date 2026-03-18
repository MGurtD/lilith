import type {
  AxiosError,
  AxiosInstance,
  InternalAxiosRequestConfig,
} from "axios";
import { AuthenticationService } from "@/services/authentications.service";
import type { AuthenticationResponse } from "@/types";

/**
 * Shared authentication interceptor logic for all Axios clients.
 *
 * - Attaches the JWT Bearer token to every outgoing request.
 * - On 401 responses, attempts a single token refresh and retries the request.
 * - If refresh fails due to invalid credentials, clears the session and redirects to login.
 * - If refresh fails due to communication issues, keeps the session intact.
 *
 * Uses a singleton promise to prevent multiple concurrent refresh attempts
 * (e.g. when several API calls fail with 401 simultaneously).
 */

let isRefreshing = false;
let refreshPromise: Promise<RefreshResult> | null = null;

type RefreshFailureReason = "invalid_auth" | "communication";

interface RefreshResult {
  auth: AuthenticationResponse | null;
  failureReason?: RefreshFailureReason;
}

function getStoredAuth(): AuthenticationResponse | null {
  try {
    const raw = localStorage.getItem("temges.authorization");
    if (!raw) return null;
    return JSON.parse(raw) as AuthenticationResponse;
  } catch {
    return null;
  }
}

/**
 * Attach Bearer token to the request config.
 */
export function attachBearerToken(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig {
  const auth = getStoredAuth();
  if (auth?.token) {
    config.headers.Authorization = `Bearer ${auth.token}`;
  }
  return config;
}

/**
 * Attempt to refresh the token. Returns the new auth response or null on failure.
 * Uses a singleton promise so concurrent 401s only trigger one refresh.
 */
function isAuthFailureStatus(status?: number): boolean {
  return status === 400 || status === 401;
}

function getRefreshFailureReason(error: unknown): RefreshFailureReason {
  const axiosError = error as AxiosError | undefined;
  const status = axiosError?.response?.status;
  return isAuthFailureStatus(status) ? "invalid_auth" : "communication";
}

async function doRefresh(): Promise<RefreshResult> {
  const auth = getStoredAuth();
  if (!auth?.token || !auth?.refreshToken)
    return { auth: null, failureReason: "invalid_auth" };

  const service = new AuthenticationService();
  try {
    const response = await service.Refresh(auth.token, auth.refreshToken);
    if (response?.result && response.token) {
      return { auth: response as AuthenticationResponse };
    }
    return { auth: null, failureReason: "invalid_auth" };
  } catch (error) {
    return { auth: null, failureReason: getRefreshFailureReason(error) };
  }
}

/**
 * Handle a 401 response: refresh the token once, then retry or logout.
 *
 * @param error - The Axios error
 * @param client - The Axios instance to use for the retry
 * @returns The retried response, or rejects with the original error
 */
export async function handle401(error: any, client: AxiosInstance): Promise<any> {
  const originalRequest = error.config;

  // Avoid infinite loops: only retry once
  if (originalRequest._retry) {
    clearSessionAndRedirect();
    return Promise.reject(error);
  }

  originalRequest._retry = true;

  // Coalesce concurrent refresh attempts into one
  if (!isRefreshing) {
    isRefreshing = true;
    refreshPromise = doRefresh().finally(() => {
      isRefreshing = false;
      refreshPromise = null;
    });
  }

  const refreshResult = await refreshPromise;
  if (refreshResult?.auth) {
    // Persist the new tokens
    localStorage.setItem("temges.authorization", JSON.stringify(refreshResult.auth));

    // Retry the original request with the new token
    originalRequest.headers.Authorization = `Bearer ${refreshResult.auth.token}`;
    return client(originalRequest);
  }

  // Refresh failed due to invalid/expired credentials: clear session.
  // Communication failures must not force logout.
  if (refreshResult?.failureReason === "invalid_auth") {
    clearSessionAndRedirect();
  }

  return Promise.reject(error);
}

function clearSessionAndRedirect(): void {
  localStorage.removeItem("temges.authorization");

  // Only redirect if not already on the login page
  if (window.location.pathname !== "/" && window.location.pathname !== "/login") {
    setTimeout(() => {
      window.location.href = "/login";
    }, 100);
  }
}

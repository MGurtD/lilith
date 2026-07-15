import apiClient from "@/api/api.client";

// TODO: propagate enterpriseId into JWT when the auth pipeline supports it.
// Until then, the active Enterprise is resolved from localStorage first,
// falling back to the first non-disabled Enterprise returned by the API.

interface EnterpriseSummary {
  id: string;
  name: string;
  disabled: boolean;
}

const ENTERPRISE_STORAGE_KEY = "lilith.activeEnterpriseId";

function fromLocalStorage(): string | null {
  if (typeof window === "undefined") return null;
  return window.localStorage.getItem(ENTERPRISE_STORAGE_KEY);
}

async function fromApiFirstActive(): Promise<string | null> {
  try {
    const response = await apiClient.get("/Enterprise");
    if (response.status !== 200) return null;
    const list = response.data as Array<EnterpriseSummary>;
    // Only consider non-disabled Enterprises. When every Enterprise in the
    // list is disabled (or the list is empty) we return null so the caller
    // keeps the Lara + "Lilith" defaults rather than booting with a disabled
    // tenant — the previous fallback to list[0] silently selected a disabled
    // enterprise.
    const first = list.find((e) => !e.disabled);
    return first?.id ?? null;
  } catch {
    return null;
  }
}

/**
 * Resolves the Enterprise id used for branding boot.
 *
 * Priority:
 *  1. localStorage override (`lilith.activeEnterpriseId`)
 *  2. First non-disabled Enterprise from `GET /api/Enterprise`
 *
 * Returns null when no Enterprise can be resolved; in that case the
 * frontend stays with the Lara + "Lilith" defaults.
 */
export async function getActiveEnterpriseId(): Promise<string | null> {
  return fromLocalStorage() || (await fromApiFirstActive());
}

export function setActiveEnterpriseId(enterpriseId: string) {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(ENTERPRISE_STORAGE_KEY, enterpriseId);
}
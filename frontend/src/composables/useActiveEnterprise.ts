import apiClient from "@/api/api.client";
import { useStore, type JwtDecoded } from "@/store";
import { jwtDecode } from "jwt-decode";

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

function fromJwt(): string | null {
  const store = useStore();
  const token = store.authorization?.token;
  if (!token) return null;
  try {
    const payload = jwtDecode<JwtDecoded & { enterpriseId?: string }>(token);
    return payload.enterpriseId ?? null;
  } catch {
    return null;
  }
}

async function fromApiFirstActive(): Promise<string | null> {
  try {
    const response = await apiClient.get("/Enterprise");
    if (response.status !== 200) return null;
    const list = response.data as Array<EnterpriseSummary>;
    const first = list.find((e) => !e.disabled);
    return first?.id ?? list[0]?.id ?? null;
  } catch {
    return null;
  }
}

/**
 * Resolves the Enterprise id used for branding boot.
 *
 * Priority:
 *  1. JWT claim `enterpriseId`
 *  2. localStorage override (`lilith.activeEnterpriseId`)
 *  3. First non-disabled Enterprise from `GET /api/Enterprise`
 *
 * Returns null when no Enterprise can be resolved; in that case the
 * frontend stays with the Lara + "Lilith" defaults.
 */
export async function getActiveEnterpriseId(): Promise<string | null> {
  return (
    fromJwt() || fromLocalStorage() || (await fromApiFirstActive())
  );
}

export function setActiveEnterpriseId(enterpriseId: string) {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(ENTERPRISE_STORAGE_KEY, enterpriseId);
}
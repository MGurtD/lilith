import { computed } from "vue";
import { useStore, type JwtDecoded } from "@/store";

/**
 * Composable that exposes authentication / authorization helpers.
 *
 * Usage:
 * ```ts
 * const { user, role, isAdmin, isAuthenticated, hasRole } = useAuth();
 * ```
 */
export function useAuth() {
  const store = useStore();

  /** The currently logged-in user (or undefined). */
  const user = computed(() => store.user);

  /** The role decoded from the JWT (e.g. "Admin", "Operari"). */
  const role = computed(() => store.role);

  /** Whether the user is authenticated. */
  const isAuthenticated = computed(() => !!store.authorization);

  /** Shorthand: true when the user's role is "Admin". */
  const isAdmin = computed(() => store.role === "Admin");

  /** Check if the user has a specific role (case-insensitive). */
  function hasRole(roleName: string): boolean {
    if (!store.role) return false;
    return store.role.toLowerCase() === roleName.toLowerCase();
  }

  return {
    user,
    role,
    isAuthenticated,
    isAdmin,
    hasRole,
  };
}

import { defineStore } from "pinia";
import { AuthenticationResponse, User } from "../types";
import { MenuItem, SidebarConfig } from "../types/component";
import jwtDecode from "jwt-decode";
import { UserService } from "../services/user.service";
import { PrimeIcons } from "@primevue/core/api";
import { getMenusByRole } from "./raw.menus"; // fallback
import { AppProfileService } from "../services/profile.service";
import { UserMenuResponse, MenuNode } from "../types/profile";
import { Exercise } from "../modules/shared/types";
import { useUserFilterStore } from "./userfilter";
import { useExerciseStore } from "../modules/shared/store/exercise";

const localStorageAuthKey = "temges.authorization";
const localStorageLangKey = "app.lang";
const defaultLocale = "ca";
const supportedLocales = ["ca", "es", "en"] as const;

const normalizeLocale = (locale?: string | null) => {
  const normalized = (locale || defaultLocale).slice(0, 2).toLowerCase();
  return supportedLocales.includes(
    normalized as (typeof supportedLocales)[number],
  )
    ? normalized
    : defaultLocale;
};

export const useStore = defineStore("applicationStore", {
  state: () => {
    return {
      authorization: undefined as AuthenticationResponse | undefined,
      user: undefined as User | undefined,
      role: undefined as string | undefined,
      isWaiting: false,
      sidebar: {
        collapsed: false,
        hideToggle: false,
        menus: [],
      } as SidebarConfig,
      currentMenuItem: {
        title: "Home",
        icon: PrimeIcons.HOME,
      } as MenuItem,
      exercisePicker: {
        exercise: undefined as Exercise | undefined,
        dates: undefined as Array<Date> | undefined,
      },
      language: {
        current: "ca" as string,
        cultureOverride: undefined as string | undefined,
      },
    };
  },
  actions: {
    setCurrentYear() {
      const exerciseStore = useExerciseStore();
      if (exerciseStore.exercises === undefined) exerciseStore.fetchActive();
      const year = new Date().getFullYear().toString();
      const currentExercise = exerciseStore.exercises?.find(
        (e) => e.name === year,
      );

      if (currentExercise) {
        this.exercisePicker.exercise = currentExercise;
        this.exercisePicker.dates = [
          new Date(this.exercisePicker.exercise.startDate),
          new Date(this.exercisePicker.exercise.endDate),
        ];
      }
    },
    cleanExercisePicker() {
      this.exercisePicker.exercise = undefined;
      this.exercisePicker.dates = undefined;
    },
    setMenuItem(menu: MenuItem) {
      this.currentMenuItem = menu;

      if (menu.title && menu.title !== "") {
        document.title = `Temges - ${menu.title}`;
      }
    },
    setMenusByRole(user: User) {
      // legacy fallback
      this.sidebar.menus = getMenusByRole(user);
    },
    async loadUserMenus(user: User) {
      const userMenu: UserMenuResponse | undefined =
        await AppProfileService.GetUserMenu(user.id);
      if (!userMenu || !userMenu.items) {
        //this.setMenusByRole(user); // fallback to legacy static menus
        return;
      }

      const transform = (node: MenuNode): any => {
        const hasChildren = node.children && node.children.length > 0;
        const entry: any = {
          icon: node.icon || undefined,
          title: node.title,
          href: node.route ? node.route : "",
        };
        if (hasChildren) {
          entry.child = node.children!.map(transform);
        }
        return entry;
      };

      // Exclude technical header_main if backend included it as a MenuNode
      const roots = userMenu.items
        .sort((a, b) => a.sortOrder - b.sortOrder)
        .map(transform);

      this.sidebar.menus = [...roots];

      // Apply default screen highlighting if current still Home
      if (userMenu.defaultScreen) {
        const stack: MenuNode[] = [...userMenu.items];
        while (stack.length) {
          const n = stack.pop()!;
          if (n.key === userMenu.defaultScreen && n.route) {
            if (this.currentMenuItem?.title === "Home") {
              this.currentMenuItem = {
                title: n.title,
                icon: n.icon || undefined,
              } as any;
            }
            break;
          }
          if (n.children && n.children.length) stack.push(...n.children);
        }
      }
    },
    /*async loadUserMenus(user: User) {
      const userMenu: UserMenuResponse | undefined =
        await AppProfileService.GetUserMenu(user.id);
      if (!userMenu || !userMenu.items) {
        //this.setMenusByRole(user); // fallback to legacy static menus
        return;
      }

      const transform = (node: MenuNode): any => {
        const hasChildren = node.children && node.children.length > 0;
        const entry: any = {
          icon: node.icon || undefined,
          title: node.title,
          href: node.route ? node.route : "",
        };
        if (hasChildren) {
          entry.child = node.children!.map(transform);
        }
        return entry;
      };

      // Exclude technical header_main if backend included it as a MenuNode
      const roots = userMenu.items
        .sort((a, b) => a.sortOrder - b.sortOrder)
        .map(transform);

      this.menus = [...roots];

      // Apply default screen highlighting if current still Home
      if (userMenu.defaultScreen) {
        const stack: MenuNode[] = [...userMenu.items];
        while (stack.length) {
          const n = stack.pop()!;
          if (n.key === userMenu.defaultScreen && n.route) {
            if (this.currentMenuItem?.title === "Home") {
              this.currentMenuItem = {
                title: n.title,
                icon: n.icon || undefined,
              } as any;
            }
            break;
          }
          if (n.children && n.children.length) stack.push(...n.children);
        }
      }
    },*/

    // Language helpers
    async initLanguage() {
      const fromLs = localStorage.getItem(localStorageLangKey);
      const lang = normalizeLocale(fromLs);
      this.setLanguage(lang);
    },
    setLanguage(code: string) {
      const normalized = normalizeLocale(code);
      this.language.current = normalized;
      localStorage.setItem(localStorageLangKey, normalized);
    },
    setCultureOverride(code?: string) {
      this.language.cultureOverride = code;
    },
    async getAuthorization() {
      if (this.authorization !== undefined) {
        return this.authorization;
      }
      const lsValue = localStorage.getItem(localStorageAuthKey);
      if (lsValue !== null) {
        await this.setAuthorization(JSON.parse(lsValue));
        return this.authorization;
      }
    },
    async setAuthorization(response: AuthenticationResponse) {
      this.authorization = response;
      localStorage.setItem(localStorageAuthKey, JSON.stringify(response));

      const jwtDecoded = jwtDecode(this.authorization.token) as JwtDecoded;
      // Decode role from JWT
      this.role = jwtDecoded.role;
      // Apply locale from JWT if present
      if (jwtDecoded.locale) {
        this.setLanguage(jwtDecoded.locale);
      }
      if (jwtDecoded.id) {
        const service = new UserService();
        this.user = await service.GetById(jwtDecoded.id);
        if (this.user) {
          // Load dynamic menus (with fallback)
          await this.loadUserMenus(this.user);

          // Get user filters
          const userFilterStore = useUserFilterStore();
          await userFilterStore.getUserFilters(this.user.id);
        }
      }
    },
    async removeAuthorization() {
      // Revoke refresh tokens on the server (best-effort)
      try {
        const { AuthenticationService } =
          await import("../services/authentications.service");
        const authService = new AuthenticationService();
        await authService.Logout();
      } catch {
        // Ignore — local cleanup is more important
      }

      this.authorization = undefined;
      this.user = undefined;
      this.role = undefined;
      localStorage.removeItem(localStorageAuthKey);
    },
    // Language change flow: update app and refresh token if available
    async changeLanguage(code: string) {
      this.setLanguage(code);
      if (this.authorization?.token && this.authorization?.refreshToken) {
        // Step 1: Persist the new language to the DB so the next JWT will carry it
        if (this.user) {
          const updatedUser: User = { ...this.user, preferredLanguage: code };
          const service = new UserService();
          await service.Update(updatedUser);
          // Keep local user state in sync
          this.user = updatedUser;
        }

        // Step 2: Refresh the token — backend re-reads user.PreferredLanguage from DB
        // and bakes the new locale claim into the fresh JWT
        try {
          const { AuthenticationService } =
            await import("../services/authentications.service");
          const auth = new AuthenticationService();
          const refreshed = await auth.Refresh(
            this.authorization.token,
            this.authorization.refreshToken,
          );
          if (refreshed?.token) {
            await this.setAuthorization(refreshed as AuthenticationResponse);
          }
        } catch {
          // ignore — UI language already updated via setLanguage above
        }
      }
    },
  },
});

export interface JwtDecoded {
  jti: string;
  id: string;
  sub: string;
  locale?: string;
  role?: string;
}

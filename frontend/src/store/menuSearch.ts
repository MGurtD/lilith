import { defineStore } from "pinia";
import { useStore } from "@/store";
import { flattenMenu, type MenuSearchEntry } from "@/utils/menuSearch";
import type { MenuItem } from "@/types/component";

const recentsKeyPrefix = "app.menuSearch.recents.";
const maxRecents = 8;

// Recents are a per-user convenience in this browser: a missing, blocked or
// corrupt entry just starts an empty list.
const readRecents = (userId: string): string[] => {
  try {
    const raw = localStorage.getItem(recentsKeyPrefix + userId);
    const parsed: unknown = raw ? JSON.parse(raw) : [];
    return Array.isArray(parsed)
      ? parsed
          .filter((href): href is string => typeof href === "string")
          .slice(0, maxRecents)
      : [];
  } catch {
    return [];
  }
};

const writeRecents = (userId: string, recents: string[]) => {
  try {
    localStorage.setItem(recentsKeyPrefix + userId, JSON.stringify(recents));
  } catch {
    // Storage full or blocked: recents stay in memory for this session.
  }
};

export const useMenuSearchStore = defineStore("menuSearch", {
  state: () => ({
    visible: false,
    userId: undefined as string | undefined,
    /** Menu hrefs, most recent first. */
    recents: [] as string[],
  }),
  getters: {
    /** Every screen of the signed-in user's menu, titled in the current language. */
    entries(): MenuSearchEntry[] {
      return flattenMenu(useStore().sidebar.menus as MenuItem[]);
    },
  },
  actions: {
    open() {
      this.visible = true;
    },
    close() {
      this.visible = false;
    },
    toggle() {
      this.visible = !this.visible;
    },
    loadRecents(userId: string | undefined) {
      if (this.userId === userId) return;
      this.userId = userId;
      this.recents = userId ? readRecents(userId) : [];
    },
    recordVisit(href: string) {
      if (!this.userId || this.recents[0] === href) return;
      this.recents = [
        href,
        ...this.recents.filter((item) => item !== href),
      ].slice(0, maxRecents);
      writeRecents(this.userId, this.recents);
    },
    reset() {
      this.visible = false;
      this.userId = undefined;
      this.recents = [];
    },
  },
});

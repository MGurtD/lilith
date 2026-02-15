import { defineStore } from "pinia";
import {
  getMenuItems,
  getMenuItemsHierarchy,
  getMenuItem,
  createMenuItem,
  updateMenuItem,
  deleteMenuItem,
} from "../services/menuitem.service";
import type {
  MenuItemFlat,
  MenuItemNode,
  CreateMenuItemRequest,
  UpdateMenuItemRequest,
} from "../types/menuitem";

interface State {
  items: MenuItemFlat[]; // flat list
  tree: MenuItemNode[]; // hierarchical list
  current: MenuItemFlat | null;
  loading: boolean;
  saving: boolean;
  treeLoading: boolean;
}

export const useMenusStore = defineStore("menus", {
  state: (): State => ({
    items: [],
    tree: [],
    current: null,
    loading: false,
    saving: false,
    treeLoading: false,
  }),
  actions: {
    async fetchAll() {
      this.loading = true;
      try {
        const data = await getMenuItems();
        this.items = data ?? [];
      } finally {
        this.loading = false;
      }
    },
    async fetchHierarchy(force = false) {
      if (this.tree.length && !force) return;
      this.treeLoading = true;
      try {
        const data = await getMenuItemsHierarchy();
        this.tree = data ?? [];
      } finally {
        this.treeLoading = false;
      }
    },
    async fetchOne(id: string) {
      this.loading = true;
      try {
        this.current = (await getMenuItem(id)) ?? null;
      } finally {
        this.loading = false;
      }
    },
    setNew(parentId?: string | null) {
      this.current = {
        id: "",
        key: "",
        title: "",
        icon: null,
        route: null,
        parentId: parentId ?? null,
        sortOrder: 0,
        disabled: false,
      } as MenuItemFlat;
    },
    async create(payload: CreateMenuItemRequest) {
      this.saving = true;
      try {
        const created = await createMenuItem(payload);
        if (created) {
          this.current = created;
          await Promise.all([this.fetchAll(), this.fetchHierarchy(true)]);
          return true;
        }
        return false;
      } finally {
        this.saving = false;
      }
    },
    async update(id: string, payload: UpdateMenuItemRequest) {
      this.saving = true;
      try {
        const updated = await updateMenuItem(id, payload);
        if (updated) {
          this.current = updated;
          await Promise.all([this.fetchAll(), this.fetchHierarchy(true)]);
          return true;
        }
        return false;
      } finally {
        this.saving = false;
      }
    },
    async remove(id: string) {
      this.saving = true;
      try {
        await deleteMenuItem(id);
        // refresh lists - not awaiting individually to allow parallel fetch
        await Promise.all([this.fetchAll(), this.fetchHierarchy(true)]);
        if (this.current?.id === id) this.current = null;
        return true;
      } catch {
        return false;
      } finally {
        this.saving = false;
      }
    },
    // Simple local reordering helper (does not persist backend ordering)
    applyLocalSort(parentId: string | null | undefined, orderedIds: string[]) {
      // Update flat list
      let order = 0;
      for (const id of orderedIds) {
        const m = this.items.find(
          (i) => i.id === id && i.parentId === parentId,
        );
        if (m) m.sortOrder = order++;
      }
      // Optionally resort tree snapshot (shallow)
      const sortChildren = (nodes: MenuItemNode[]) => {
        nodes.sort((a, b) => a.sortOrder - b.sortOrder);
        nodes.forEach((n) => n.children && sortChildren(n.children));
      };
      sortChildren(this.tree);
    },
    clearCurrent() {
      this.current = null;
    },
  },
});

export type MenusStore = ReturnType<typeof useMenusStore>;

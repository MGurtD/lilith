import { defineStore } from "pinia";
import { useMenusStore } from "./menus";
import { useProfilesStore } from "./profiles";
import type { MenuItemNode } from "../types/menuitem";

interface RowItem {
  id: string;
  key: string;
  title: string;
  route?: string | null;
  sortOrder: number;
  icon?: string | null;
  level: number;
  parentId: string | null;
  hasChildren: boolean;
}

interface State {
  profileId: string | null;
  selectedIds: Set<string>;
  parentMap: Map<string, string | null>;
  childrenMap: Map<string, string[]>;
  rows: RowItem[];
  initialized: boolean;
  loading: boolean;
}

export const useProfileMenuSelectionStore = defineStore(
  "profileMenuSelection",
  {
    state: (): State => ({
      profileId: null,
      selectedIds: new Set(),
      parentMap: new Map(),
      childrenMap: new Map(),
      rows: [],
      initialized: false,
      loading: false,
    }),

    getters: {
      selectedIdsArray: (state) => Array.from(state.selectedIds),
      selectedRows: (state) =>
        state.rows.filter((r) => state.selectedIds.has(r.id)),
    },

    actions: {
      /**
       * Carga los menús y la selección actual para un perfil
       */
      async load(profileId: string) {
        this.loading = true;
        this.profileId = profileId;
        this.initialized = false;

        try {
          const menusStore = useMenusStore();
          const profilesStore = useProfilesStore();

          await menusStore.fetchHierarchy(true);
          this._buildIndexes(menusStore.tree);
          this._buildRows(menusStore.tree);

          await profilesStore.fetchMenuAssignment(profileId);

          const menuAssignment = profilesStore.menuAssignment;
          if (menuAssignment?.menuItemIds?.length) {
            this._seedSelection(menuAssignment.menuItemIds);
          } else {
            this.selectedIds.clear();
          }

          this.initialized = true;
        } finally {
          this.loading = false;
        }
      },

      /**
       * Construye mapas de parentesco para navegación rápida
       */
      _buildIndexes(nodes: MenuItemNode[]) {
        this.parentMap.clear();
        this.childrenMap.clear();

        const visit = (node: MenuItemNode, parentId: string | null) => {
          this.parentMap.set(node.id, parentId);

          const parentKey = parentId || "__root__";
          if (!this.childrenMap.has(parentKey)) {
            this.childrenMap.set(parentKey, []);
          }
          this.childrenMap.get(parentKey)!.push(node.id);

          if (node.children?.length) {
            this.childrenMap.set(
              node.id,
              node.children.map((c) => c.id),
            );
            node.children.forEach((c) => visit(c, node.id));
          } else {
            this.childrenMap.set(node.id, []);
          }
        };

        nodes.forEach((n) => visit(n, null));
      },

      /**
       * Construye filas planas para DataTable desde jerarquía
       */
      _buildRows(nodes: MenuItemNode[]) {
        const result: RowItem[] = [];

        const walk = (
          node: MenuItemNode,
          parentId: string | null,
          level: number,
        ) => {
          result.push({
            id: node.id,
            key: node.key,
            title: node.title,
            route: node.route ?? null,
            sortOrder: node.sortOrder,
            icon: node.icon ?? null,
            level,
            parentId,
            hasChildren: !!node.children?.length,
          });
          node.children?.forEach((c) => walk(c, node.id, level + 1));
        };

        nodes.forEach((n) => walk(n, null, 0));
        this.rows = result;
      },

      /**
       * Inicializa selección desde IDs del backend (incluyendo ancestros)
       */
      _seedSelection(menuItemIds: string[]) {
        const ids = new Set<string>(menuItemIds);
        const withAncestors = new Set<string>(ids);

        // Asegurar que todos los ancestros estén visibles como seleccionados
        for (const id of ids) {
          let p = this.parentMap.get(id) ?? null;
          while (p) {
            withAncestors.add(p);
            p = this.parentMap.get(p) ?? null;
          }
        }

        this.selectedIds = withAncestors;
      },

      /**
       * Obtiene todos los descendientes de un nodo
       */
      _getDescendants(id: string): string[] {
        const res: string[] = [];
        const stack: string[] = [...(this.childrenMap.get(id) ?? [])];

        while (stack.length) {
          const cid = stack.pop()!;
          res.push(cid);
          const kids = this.childrenMap.get(cid) ?? [];
          if (kids.length) stack.push(...kids);
        }

        return res;
      },

      /**
       * Obtiene todos los ancestros de un nodo
       */
      _getAncestors(id: string): string[] {
        const res: string[] = [];
        let p = this.parentMap.get(id) ?? null;

        while (p) {
          res.push(p);
          p = this.parentMap.get(p) ?? null;
        }

        return res;
      },

      /**
       * Alterna la selección de un nodo (con propagación automática)
       */
      toggle(id: string, selected: boolean) {
        if (selected) {
          this._selectWithDependencies(id);
        } else {
          this._unselectWithDependencies(id);
        }
      },

      /**
       * Selecciona un nodo y todos sus dependientes (hijos + padres)
       */
      _selectWithDependencies(id: string) {
        this.selectedIds.add(id);
        this._getDescendants(id).forEach((d) => this.selectedIds.add(d));
        this._getAncestors(id).forEach((a) => this.selectedIds.add(a));
      },

      /**
       * Deselecciona un nodo y limpia dependientes
       */
      _unselectWithDependencies(id: string) {
        this.selectedIds.delete(id);
        this._getDescendants(id).forEach((d) => this.selectedIds.delete(d));

        // Remover ancestros que no tengan descendientes seleccionados
        for (const a of this._getAncestors(id)) {
          const anyBelow = this._getDescendants(a).some((d) =>
            this.selectedIds.has(d),
          );
          if (!anyBelow) this.selectedIds.delete(a);
        }
      },

      /**
       * Selecciona todos los menús
       */
      selectAll() {
        this.selectedIds = new Set(this.rows.map((r) => r.id));
      },

      /**
       * Deselecciona todos los menús
       */
      unselectAll() {
        this.selectedIds.clear();
      },

      /**
       * Guarda la selección actual al backend
       */
      async save() {
        if (!this.profileId) return false;

        const profilesStore = useProfilesStore();

        // Actualizar store de profiles con la selección actual
        profilesStore.setMenuSelection(this.selectedIdsArray);

        // Guardar al backend
        return await profilesStore.saveMenuAssignment();
      },

      /**
       * Limpia el estado (útil al salir de la vista)
       */
      reset() {
        this.profileId = null;
        this.selectedIds.clear();
        this.parentMap.clear();
        this.childrenMap.clear();
        this.rows = [];
        this.initialized = false;
        this.loading = false;
      },
    },
  },
);

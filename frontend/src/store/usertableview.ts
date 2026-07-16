import { defineStore } from "pinia";
import { UserTableView } from "../types";
import AppServices from "../services";
import { useStore } from "./index";
import { getNewUuid } from "../utils/functions";
import type { Column, Aggregation } from "@/components/tables/types";
import { hydrateFilter } from "../utils/filter-hydrate";

// Module-level cache for in-flight EnsureDefault promises. Keyed by
// `${userId}:${page}` so concurrent calls (e.g. multiple <Table> instances
// on the same page during dev hot-reload) share the same promise and
// hit the backend only once. Cleared on resolve/reject.
const ensureInFlight = new Map<string, Promise<UserTableView>>();

interface ColumnConfig {
  field: string;
  visible?: boolean;
  order?: number;
  total?: Aggregation;
}

export interface SortConfig {
  field: string;
  order: 1 | -1;
}

interface ViewConfig {
  columns: ColumnConfig[];
  filters?: Record<string, unknown>;
  sort?: SortConfig;
}

export const useUserTableViewStore = defineStore("userTableViewStore", {
  state: () => {
    return {
      views: [] as UserTableView[],
      currentView: null as UserTableView | null,
    };
  },
  actions: {
    async fetchViews(userId: string, page: string) {
      const response = await AppServices.UserTableView.GetByUserAndPage(
        userId,
        page
      );
      if (response) this.views = response;
    },

    async fetchById(id: string) {
      const response = await AppServices.UserTableView.GetById(id);
      if (response) this.currentView = response;
      return response;
    },

    async create(model: UserTableView) {
      const result = await AppServices.UserTableView.Create(model);
      if (result) await this.fetchViews(model.userId, model.page);
      return result;
    },

    async update(id: string, model: UserTableView) {
      const result = await AppServices.UserTableView.Update(id, model);
      if (result) await this.fetchById(id);
      return result;
    },

    async delete(id: string) {
      const view = this.views.find((v) => v.id === id);
      const result = await AppServices.UserTableView.Delete(id);
      if (result && view) {
        this.views = this.views.filter((v) => v.id !== id);
      }
      return result;
    },

    async setDefault(id: string, isDefault: boolean = true) {
      const result = await AppServices.UserTableView.SetDefault(id, isDefault);
      if (result) {
        // Refresh views to get updated IsDefault flags
        const view = this.views.find((v) => v.id === id);
        if (view) {
          await this.fetchViews(view.userId, view.page);
        }
      }
      return result;
    },

    /**
     * Idempotent get-or-create for the default view on `(userId, page)`.
     * Concurrent calls share the same in-flight promise. After settle,
     * refreshes the views list so the newly-created default is visible.
     * Throws on backend error so callers can decide how to react.
     */
    async ensureDefault(userId: string, page: string): Promise<UserTableView> {
      const key = `${userId}:${page}`;
      const cached = ensureInFlight.get(key);
      if (cached) return cached;

      const promise = (async () => {
        const view = await AppServices.UserTableView.EnsureDefault(
          userId,
          page
        );
        if (!view) {
          throw new Error(`EnsureDefault returned no view for ${key}`);
        }
        // Refresh the views list so the store reflects backend state.
        await this.fetchViews(userId, page);
        return view;
      })();

      ensureInFlight.set(key, promise);
      try {
        return await promise;
      } finally {
        ensureInFlight.delete(key);
      }
    },

    /**
     * Persist current filterValues into the default view's viewConfig.filters.
     * Read-modify-write: GET existing → merge filters only (preserve columns
     * and sort) → PUT. Errors are swallowed and logged (non-critical
     * background operation triggered by row click).
     */
    async saveFiltersToDefault(
      userId: string,
      page: string,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      filterValues: any
    ): Promise<boolean> {
      return this.saveStateToDefault(userId, page, {
        filters: filterValues,
      });
    },

    /**
     * Persist the full table state (columns + sort + filters) into the
     * default view's viewConfig. Each section is optional — missing sections
     * preserve whatever was already in the persisted viewConfig.
     *
     * Read-modify-write: GET existing → merge provided sections → PUT.
     * Errors are swallowed and logged (non-critical background operation
     * triggered by row click).
     *
     * @param state.columns Column overrides: array of `{ field, visible?, order? }`.
     *                      Pass the CURRENT base columns (filtered from
     *                      `appliedColumns`) so the persisted config reflects
     *                      what the user actually sees.
     * @param state.sort    Optional sort config `{ field, order: 1|-1 }`.
     * @param state.filters Optional filter object snapshot from the UI.
     */
    async saveStateToDefault(
      userId: string,
      page: string,
      state: {
        columns?: Array<{ field: string; visible?: boolean; order?: number }>;
        sort?: SortConfig;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        filters?: any;
      }
    ): Promise<boolean> {
      try {
        const defaultView = await this.ensureDefault(userId, page);
        if (!defaultView) return false;

        // Parse existing viewConfig, preserving any sections we don't touch.
        let config: ViewConfig = { columns: [] };
        if (defaultView.viewConfig) {
          try {
            const parsed = JSON.parse(defaultView.viewConfig);
            if (parsed && typeof parsed === "object" && !Array.isArray(parsed)) {
              config = parsed as ViewConfig;
            }
          } catch {
            // Corrupted JSON — start fresh but keep the row
          }
        }

        // Merge each provided section, replacing existing values entirely.
        // Filters are a UI snapshot (not additive), same for sort. Columns
        // are also a full snapshot so reordering or hiding sticks.
        if (state.columns !== undefined) {
          config.columns = state.columns;
        }
        if (state.sort !== undefined) {
          config.sort = state.sort;
        }
        if (state.filters !== undefined) {
          config.filters =
            state.filters && typeof state.filters === "object"
              ? (state.filters as Record<string, unknown>)
              : undefined;
        }

        const viewConfigJson = JSON.stringify(config);
        if (viewConfigJson === defaultView.viewConfig) {
          // No-op: avoid the PUT when nothing changed (idempotency guard)
          return true;
        }

        const updated = await AppServices.UserTableView.Update(
          defaultView.id,
          { ...defaultView, viewConfig: viewConfigJson }
        );
        if (updated) {
          await this.fetchViews(userId, page);
        }
        return updated;
      } catch (err) {
        console.warn("[UserTableViewStore] saveStateToDefault failed", err);
        return false;
      }
    },

    /**
     * Apply a saved view's configuration to base columns.
     * Parses the unified ViewConfig JSON and extracts column configuration.
     */
    applyView(
      view: UserTableView,
      baseColumns: Column[]
    ): Column[] {
      if (!view || !view.viewConfig) {
        return baseColumns;
      }

      let config: ViewConfig;
      try {
        config = JSON.parse(view.viewConfig);
      } catch {
        // Invalid JSON, return base columns
        return baseColumns;
      }

      const columnConfig = config.columns || [];

      // Create a map of base columns by field name for quick lookup
      const baseColumnsMap = new Map<string, Column>();
      baseColumns.forEach((col) => {
        baseColumnsMap.set(col.field, { ...col });
      });

      // Apply configuration to each column in config
      columnConfig.forEach((fieldConfig) => {
        const column = baseColumnsMap.get(fieldConfig.field);
        if (column) {
          if (fieldConfig.visible !== undefined) {
            column.visible = fieldConfig.visible;
          }
          if (fieldConfig.order !== undefined) {
            column.order = fieldConfig.order;
          }
          if (fieldConfig.total !== undefined) {
            column.total = fieldConfig.total;
          }
        }
      });

      // Convert back to array and sort by order if defined
      const result = Array.from(baseColumnsMap.values());

      // Sort by order if present, otherwise maintain original order
      result.sort((a, b) => {
        const orderA = a.order ?? Number.MAX_VALUE;
        const orderB = b.order ?? Number.MAX_VALUE;
        return orderA - orderB;
      });

      return result;
    },

    /**
     * Apply a saved view's filter configuration.
     * Parses the unified ViewConfig JSON and extracts filter values.
     * Hydrates ISO date strings to Date objects.
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    applyFilterConfig(view: UserTableView): any | null {
      if (!view || !view.viewConfig) {
        return null;
      }

      try {
        const config: ViewConfig = JSON.parse(view.viewConfig);
        if (!config.filters) return null;
        return hydrateFilter(config.filters);
      } catch {
        // Invalid JSON, return null
        return null;
      }
    },

    /**
     * Apply a saved view's sort configuration.
     * Returns { field, order } or null if no sort is stored.
     */
    applySortConfig(view: UserTableView): SortConfig | null {
      if (!view || !view.viewConfig) return null;
      try {
        const config: ViewConfig = JSON.parse(view.viewConfig);
        return config.sort ?? null;
      } catch {
        return null;
      }
    },

    /**
     * Create a new view model for saving
     */
    createNewView(
      userId: string,
      page: string,
      name: string,
      columns: Column[],
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      filterValues?: any
    ): UserTableView {
      // Build column config from current column state
      const columnConfig: ColumnConfig[] = columns
        .filter((col) => col.order !== undefined || col.visible === false)
        .map((col) => ({
          field: col.field,
          visible: col.visible,
          order: col.order,
          total: col.total,
        }));

      const viewConfig: ViewConfig = {
        columns: columnConfig,
      };
      if (filterValues) {
        viewConfig.filters = filterValues;
      }

      return {
        id: getNewUuid(),
        userId,
        page,
        name,
        isDefault: false,
        viewConfig: JSON.stringify(viewConfig),
      };
    },

    /**
     * Get the default view for a user and page
     */
    getDefaultView(userId: string, page: string): UserTableView | undefined {
      return this.views.find(
        (v) => v.userId === userId && v.page === page && v.isDefault
      );
    },
  },
});
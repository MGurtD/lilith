import { defineStore } from "pinia";
import { UserTableView } from "../types";
import AppServices from "../services";
import { useStore } from "./index";
import { getNewUuid } from "../utils/functions";
import type { Column, Aggregation } from "@/components/tables/types";
import { hydrateFilter } from "../utils/filter-hydrate";

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
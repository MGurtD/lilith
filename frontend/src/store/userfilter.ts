import { defineStore } from "pinia";
import { UserFilter } from "../types";
import AppServices from "../services";
import { useStore } from ".";
import { getNewUuid } from "../utils/functions";

const ISO_DATE_RE = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/;

function hydrateValue(value: unknown): unknown {
  if (typeof value === "string" && ISO_DATE_RE.test(value)) {
    return new Date(value);
  }
  if (Array.isArray(value)) {
    return value.map(hydrateValue);
  }
  if (value !== null && typeof value === "object") {
    return hydrateDates(value as Record<string, unknown>);
  }
  return value;
}

function hydrateDates(obj: Record<string, unknown>): Record<string, unknown> {
  const result: Record<string, unknown> = {};
  for (const key of Object.keys(obj)) {
    result[key] = hydrateValue(obj[key]);
  }
  return result;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function hydrateFilter(parsed: any): any {
  if (parsed === null || typeof parsed !== "object") return parsed;
  if (Array.isArray(parsed)) return parsed.map(hydrateValue);
  return hydrateDates(parsed as Record<string, unknown>);
}

export const useUserFilterStore = defineStore("userFilterStore", {
  state: () => {
    return {
      filters: [] as UserFilter[],
    };
  },
  getters: {
    getFilter(state) {
      return function (page: string, key: string) {
        const filter = state.filters.find(
          (f) => f.page === page && f.key === key
        );
        if (!filter) return null;

        const parsed = JSON.parse(filter.filter);
        return hydrateFilter(parsed);
      };
    },
  },
  actions: {
    async getUserFilters(userId: string) {
      const response = await AppServices.UserFilter.GetByUserId(userId);
      if (response) this.filters = response;
    },
    async addFilter(page: string, key: string, filter: any) {
      const store = useStore();
      const userFilter = {
        id: getNewUuid(),
        userId: store.user?.id as string,
        page,
        key,
        filter: JSON.stringify(filter),
      } as UserFilter;

      await AppServices.UserFilter.CreateOrUpdate(userFilter);
      this.getUserFilters(userFilter.userId);
    },
    async removeFilter(page: string, key: string) {
      const userFilter = this.filters.find(
        (f) => f.page === page && f.key === key
      );
      if (userFilter) {
        await AppServices.UserFilter.Delete(userFilter);
        this.filters = this.filters.filter((f) => f !== userFilter);
      }
    },
  },
});

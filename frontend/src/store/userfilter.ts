import { defineStore } from "pinia";
import { UserFilter } from "../types";
import AppServices from "../services";
import { useStore } from ".";
import { getNewUuid } from "../utils/functions";

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
        return filter ? JSON.parse(filter.filter) : null;
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

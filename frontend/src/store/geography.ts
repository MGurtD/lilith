import { defineStore } from "pinia";
import townsJson from "../assets/municipios.json";
import regionsJson from "../assets/provincias.json";
import { Region } from "../types";

export const useSpanishGeography = defineStore({
  id: "geography",
  state: () => ({
    towns: undefined as Array<Region> | undefined,
    regions: undefined as Array<Region> | undefined,
  }),
  getters: {
    getTownsByRegion: (state) => {
      return (region: Region) =>
        state.towns?.filter((town) => town.id.startsWith(region.id));
    },
    getTownsByRegionId: (state) => {
      return (regionId: string) =>
        state.towns?.filter((town) => town.id.startsWith(regionId));
    },
    getTownsByRegionName: (state) => {
      return (regionName: string) => {
        var region = state.regions?.find((r) => r.nm === regionName);
        if (region) {
          return state.towns?.filter((town) =>
            town.id.startsWith((region as Region).id)
          );
        }
      };
    },
  },
  actions: {
    async fetch() {
      this.towns = townsJson;
      this.regions = regionsJson;
    },
  },
});

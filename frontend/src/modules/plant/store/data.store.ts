import { defineStore } from "pinia";
import { Area, Site, MachineStatus } from "../../production/types";
import ProductionServices from "../../production/services";

export const usePlantDataStore = defineStore("plantDataStore", {
  state: () => ({
    site: undefined as Site | undefined,
    areas: [] as Area[],
    machineStatuses: [] as MachineStatus[],
  }),
  getters: {
    getMachineStatusById: (state) => {
      return (id: string): MachineStatus | undefined => {
        return state.machineStatuses.find((status) => status.id === id);
      };
    },
    stoppedStatus: (state): MachineStatus | undefined => {
      return state.machineStatuses.find((status) => status.stopped === true);
    },
  },
  actions: {
    async fetchAreasWithWorkcenters() {
      const sites = await ProductionServices.Site.getActive();
      if (sites && sites.length > 0) {
        this.site = sites[0];
      }
      this.areas = (await ProductionServices.Areas.getVisibleInPlant()) || [];
    },
    async fetchMachineStatuses() {
      const statuses =
        await ProductionServices.MachineStatus.getAllWithReasons();
      if (statuses) {
        this.machineStatuses = statuses;
      }
    },
  },
});

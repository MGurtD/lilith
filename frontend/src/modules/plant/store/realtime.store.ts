import { defineStore } from "pinia";
import {
  WorkcenterRealtime,
  WorkcenterViewState,
  RealtimeHandler,
  WorkcenterRealtimeHandler,
} from "../types";
import ActionsService from "../services/actions.service";
import { usePlantDataStore } from "./data.store";

export const usePlantRealtimeStore = defineStore("plantRealtimeStore", {
  state: () => ({
    areasWorkcentersRt: [] as WorkcenterRealtime[],
    _realtimeHandler: null as
      | RealtimeHandler
      | WorkcenterRealtimeHandler
      | null,
  }),
  getters: {
    areasWorkcentersView(state): WorkcenterViewState[] {
      const dataStore = usePlantDataStore();
      return dataStore.areas.flatMap((area) =>
        (area.workcenters || []).map((wc) => {
          const realtime = state.areasWorkcentersRt.find(
            (rt) => rt.workcenterId === wc.id
          );
          return {
            config: wc,
            realtime,
          } as WorkcenterViewState;
        })
      );
    },
  },
  actions: {
    connectToGeneral() {
      if (this._realtimeHandler) {
        this._realtimeHandler.cleanup();
      }
      const handler = ActionsService.client.connectToGeneral();
      handler.onUpdate((data) => {
        this.areasWorkcentersRt = data;
      });
      this._realtimeHandler = handler;
    },
    disconnectWebSocket() {
      if (this._realtimeHandler) {
        this._realtimeHandler.cleanup();
        this._realtimeHandler = null;
      }
      this.areasWorkcentersRt = [];
    },
  },
});

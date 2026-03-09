import { defineStore } from "pinia";
import {
  Workcenter,
  WorkOrderWithPhases,
} from "../../production/types";
import {
  WorkcenterRealtime,
  WorkcenterViewState,
  RealtimeHandler,
  WorkcenterRealtimeHandler,
} from "../types";
import ProductionServices from "../../production/services";
import ActionsService from "../services/actions.service";
import { usePlantOperatorStore } from "./operator.store";
import { usePlantActivePhaseStore } from "./activePhase.store";

export const usePlantWorkcenterStore = defineStore("plantWorkcenterStore", {
  state: () => ({
    workcenter: undefined as Workcenter | undefined,
    workcenterRt: undefined as WorkcenterRealtime | undefined,
    loadedWorkOrdersPhases: [] as WorkOrderWithPhases[],
    availableWorkOrders: [] as WorkOrderWithPhases[],
    availableWorkOrdersLoading: false,
    _realtimeHandler: null as
      | RealtimeHandler
      | WorkcenterRealtimeHandler
      | null,
    _lastLoadedPhaseIds: [] as string[],
    _lastStatusId: undefined as string | undefined,
  }),
  getters: {
    workcenterView(): WorkcenterViewState | undefined {
      if (!this.workcenter) return undefined;
      return {
        config: this.workcenter,
        realtime: this.workcenterRt,
      };
    },
  },
  actions: {
    connectToWorkcenter(workcenterId: string) {
      if (this._realtimeHandler) {
        this._realtimeHandler.cleanup();
      }
      const activePhaseStore = usePlantActivePhaseStore();
      const handler = ActionsService.client.connectToWorkcenter(workcenterId);
      handler.onUpdate((data) => {
        const previousStatusId = this._lastStatusId;
        this.workcenterRt = data;
        this._lastStatusId = data.statusId;

        // Extract phase IDs from workorders array
        const phaseIds = (data.workorders || []).map(
          (wo) => wo.workOrderPhaseId,
        );

        // Check if phase IDs have changed
        const phasesChanged =
          phaseIds.length !== this._lastLoadedPhaseIds.length ||
          phaseIds.some(
            (id: string, idx: number) => id !== this._lastLoadedPhaseIds[idx],
          );

        // Check if status ID has changed
        const statusChanged = data.statusId !== previousStatusId;

        if (phasesChanged) {
          // Clear immediately if empty
          if (phaseIds.length === 0) {
            this._lastLoadedPhaseIds = [];
            this.loadedWorkOrdersPhases = [];
            activePhaseStore.clearActivePhase();
          } else {
            // Fetch new data when phase IDs have changed
            this.fetchLoadedWorkOrders(phaseIds);
          }
        } else if (statusChanged && phaseIds.length > 0) {
          // Status changed but phases didn't - just refresh time metrics
          activePhaseStore.fetchPhaseTimeMetrics();
        }
      });
      this._realtimeHandler = handler;
    },
    async fetchWorkcenter(workcenterId: string) {
      this.workcenter =
        await ProductionServices.Workcenter.getById(workcenterId);
    },
    async fetchAvailableWorkOrders(workcenterTypeId: string) {
      if (!workcenterTypeId) {
        this.availableWorkOrders = [];
        return;
      }

      this.availableWorkOrdersLoading = true;
      try {
        const workOrders =
          await ProductionServices.WorkOrderPhase.GetPlannedPhasesByWorkcenterType(
            workcenterTypeId,
          );
        this.availableWorkOrders = workOrders || [];
      } catch (error) {
        console.error("Error fetching available work orders:", error);
        this.availableWorkOrders = [];
      } finally {
        this.availableWorkOrdersLoading = false;
      }
    },
    async fetchLoadedWorkOrders(phaseIds: string[]) {
      const activePhaseStore = usePlantActivePhaseStore();

      if (!phaseIds || phaseIds.length === 0) {
        this.loadedWorkOrdersPhases = [];
        this._lastLoadedPhaseIds = [];
        activePhaseStore.clearActivePhase();
        return;
      }

      try {
        const workOrders =
          await ProductionServices.WorkOrderPhase.GetLoadedByPhaseIds(phaseIds);
        this.loadedWorkOrdersPhases = workOrders || [];
        this._lastLoadedPhaseIds = phaseIds;

        // Delegar la sincronització de la fase activa al store dedicat
        await activePhaseStore.syncWithLoadedPhases();
      } catch (error) {
        console.error("Error fetching loaded work orders:", error);
        this.loadedWorkOrdersPhases = [];
        activePhaseStore.clearActivePhase();
      }
    },
    /**
     * Refresca les fases carregades usant els últims IDs coneguts.
     * Útil per a que el activePhaseStore pugui forçar un refresh
     * després d'actualitzar quantitats o comentaris.
     */
    async refreshLoadedWorkOrders() {
      const phaseIds = this._lastLoadedPhaseIds;
      if (phaseIds.length > 0) {
        await this.fetchLoadedWorkOrders(phaseIds);
      }
    },
    disconnectWebSocket() {
      if (this._realtimeHandler) {
        this._realtimeHandler.cleanup();
        this._realtimeHandler = null;
      }
      this.workcenterRt = undefined;
    },
    async clockInOperator(): Promise<boolean> {
      const operatorStore = usePlantOperatorStore();
      if (!this.workcenter || !operatorStore.operator) return false;
      return await ActionsService.client.clockInOperator({
        operatorId: operatorStore.operator.id,
        workcenterId: this.workcenter.id,
      });
    },
    async clockOutOperator(): Promise<boolean> {
      const operatorStore = usePlantOperatorStore();
      if (!this.workcenter || !operatorStore.operator) return false;
      return await ActionsService.client.clockOutOperator({
        operatorId: operatorStore.operator.id,
        workcenterId: this.workcenter.id,
      });
    },
    async changeMachineStatus(
      statusId: string,
      statusReasonId?: string,
    ): Promise<boolean> {
      if (!this.workcenter) return false;
      return await ActionsService.client.changeMachineStatus({
        workcenterId: this.workcenter.id,
        statusId,
        statusReasonId,
      });
    },
    clearWorkcenter() {
      // Desconnectar WebSocket si està actiu
      this.disconnectWebSocket();

      // Netejar l'estat de la fase activa
      const activePhaseStore = usePlantActivePhaseStore();
      activePhaseStore.clearActivePhase();

      // Netejar tot l'estat del workcenter
      this.workcenter = undefined;
      this.workcenterRt = undefined;
      this.loadedWorkOrdersPhases = [];
      this.availableWorkOrders = [];
      this.availableWorkOrdersLoading = false;
      this._lastLoadedPhaseIds = [];
    },
  },
});

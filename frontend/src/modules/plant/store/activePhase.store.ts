import { defineStore } from "pinia";
import {
  WorkOrderWithPhases,
  PlannedPhase,
  ValidatePreviousPhaseQuantityRequest,
  PhaseTimeMetrics,
  BillOfMaterialsItem,
} from "../../production/types";
import { NextPhaseInfo } from "../types";
import ProductionServices from "../../production/services";
import SharedServices from "../../shared/services";
import { FileService } from "../../../services/file.service";
import { File } from "../../../types";
import { usePlantWorkcenterStore } from "./workcenter.store";

export const usePlantActivePhaseStore = defineStore("plantActivePhaseStore", {
  state: () => ({
    workOrderReferenceDocuments: [] as File[],
    nextAvailablePhase: null as NextPhaseInfo | null,
    phaseTimeMetrics: undefined as PhaseTimeMetrics | undefined,
    billOfMaterials: [] as BillOfMaterialsItem[],
  }),
  getters: {
    activeWorkOrder(): WorkOrderWithPhases | undefined {
      const workcenterStore = usePlantWorkcenterStore();
      return workcenterStore.loadedWorkOrdersPhases?.[0];
    },
    activePhase(): PlannedPhase | undefined {
      return this.activeWorkOrder?.phases?.[0];
    },
    hasBillOfMaterials(): boolean {
      return this.billOfMaterials.length > 0;
    },
  },
  actions: {
    /**
     * Sincronitza l'estat de la fase activa quan canvien les fases carregades.
     * Cridat pel workcenter store quan es carreguen noves fases.
     */
    async syncWithLoadedPhases() {
      const workcenterStore = usePlantWorkcenterStore();
      const loadedPhases = workcenterStore.loadedWorkOrdersPhases;

      if (loadedPhases.length > 0) {
        const firstWorkOrder = loadedPhases[0];

        // Carregar documentació de la referència
        if (firstWorkOrder.salesReferenceId) {
          await this.fetchWorkInstructionDocuments(
            firstWorkOrder.salesReferenceId,
          );
        }

        // Carregar materials (BOM) de la fase activa
        await this.fetchBillOfMaterials();

        // Carregar mètriques de temps (últim)
        await this.fetchPhaseTimeMetrics();
      } else {
        this.workOrderReferenceDocuments = [];
        this.phaseTimeMetrics = undefined;
        this.billOfMaterials = [];
      }
    },
    async fetchWorkInstructionDocuments(referenceId: string) {
      const fileService = new FileService();
      const files = await fileService.GetEntityFiles(
        "referenceMaps",
        referenceId,
      );
      if (files) {
        this.workOrderReferenceDocuments = files;
      }
    },
    async fetchPhaseTimeMetrics() {
      const workcenterStore = usePlantWorkcenterStore();
      const loadedPhases = workcenterStore.loadedWorkOrdersPhases;

      if (
        !loadedPhases.length ||
        !workcenterStore.workcenterRt?.workorders?.length ||
        !workcenterStore.workcenterRt?.statusId
      ) {
        this.phaseTimeMetrics = undefined;
        return;
      }

      const activeWorkOrder = workcenterStore.workcenterRt.workorders[0];
      const phaseId = activeWorkOrder.workOrderPhaseId;
      const machineStatusId = workcenterStore.workcenterRt.statusId;
      const operatorId =
        workcenterStore.workcenterRt.operators?.[0]?.operatorId;

      try {
        const metrics =
          await ProductionServices.WorkOrderPhase.GetPhaseTimeMetrics(
            phaseId,
            machineStatusId,
            operatorId,
          );
        this.phaseTimeMetrics = metrics;
      } catch (error) {
        console.error("Error fetching phase time metrics:", error);
        this.phaseTimeMetrics = undefined;
      }
    },
    async fetchBillOfMaterials() {
      if (!this.activeWorkOrder) {
        this.billOfMaterials = [];
        return;
      }

      try {
        const phasesDetailed =
          await ProductionServices.WorkOrderPhase.GetWorkOrderPhasesDetailed(
            this.activeWorkOrder.workOrderId,
          );

        if (!phasesDetailed) {
          this.billOfMaterials = [];
          return;
        }

        // Trobar la fase activa dins les fases detallades
        const activePhaseId = this.activePhase?.phaseId;
        const detailedPhase = phasesDetailed.find(
          (p) => p.phaseId === activePhaseId,
        );
        this.billOfMaterials = detailedPhase?.billOfMaterials ?? [];
      } catch (error) {
        console.error("Error fetching bill of materials:", error);
        this.billOfMaterials = [];
      }
    },
    async fetchNextPhaseForWorkcenter() {
      this.nextAvailablePhase = null;
      const workcenterStore = usePlantWorkcenterStore();

      if (
        !workcenterStore.workcenter ||
        !workcenterStore.loadedWorkOrdersPhases.length ||
        !workcenterStore.loadedWorkOrdersPhases[0]?.phases?.length
      ) {
        return;
      }

      const currentPhase =
        workcenterStore.loadedWorkOrdersPhases[0].phases[0];

      try {
        const nextPhase =
          await ProductionServices.WorkOrderPhase.GetNextPhaseForWorkcenter(
            currentPhase.phaseId,
            workcenterStore.workcenter.id,
          );

        if (nextPhase) {
          this.nextAvailablePhase = nextPhase;
        }
      } catch (error) {
        console.error("Error fetching next phase:", error);
      }
    },
    async getPhaseExitStatusId(closePhase: boolean): Promise<string | null> {
      const workcenterStore = usePlantWorkcenterStore();

      if (
        !workcenterStore.loadedWorkOrdersPhases[0]?.phases?.[0]?.phaseStatusId
      ) {
        return null;
      }

      const currentStatusId =
        workcenterStore.loadedWorkOrdersPhases[0].phases[0].phaseStatusId;
      const targetStatusName = closePhase ? "Tancada" : "Pausa";

      try {
        const transitions =
          await SharedServices.Lifecycle.getAvailableTransitions(
            currentStatusId,
          );

        if (!transitions) return null;

        const targetTransition = transitions.find(
          (t) => t.statusToName === targetStatusName,
        );

        return targetTransition?.statusToId ?? null;
      } catch (error) {
        console.error("Error getting phase exit status:", error);
        return null;
      }
    },
    async validatePhaseQuantity(
      quantity: number,
    ): Promise<{ valid: boolean; error?: string }> {
      const workcenterStore = usePlantWorkcenterStore();

      if (!workcenterStore.workcenterRt?.workorders?.length) {
        return { valid: false, error: "No hi ha cap fase carregada" };
      }

      const currentPhaseId =
        workcenterStore.workcenterRt.workorders[0].workOrderPhaseId;

      const request: ValidatePreviousPhaseQuantityRequest = {
        workOrderPhaseId: currentPhaseId,
        quantity: quantity,
      };

      try {
        const response =
          await ProductionServices.WorkOrderPhase.ValidatePreviousPhaseQuantity(
            request,
          );

        if (response.result) {
          return { valid: true };
        } else {
          return {
            valid: false,
            error: response.errors?.[0] || "Error de validació",
          };
        }
      } catch (error) {
        console.error("Error validating phase quantity:", error);
        return { valid: false, error: "Error de connexió amb el servidor" };
      }
    },
    async updatePhaseComment(
      phaseId: string,
      comment: string,
    ): Promise<boolean> {
      const workcenterStore = usePlantWorkcenterStore();

      try {
        const phase =
          await ProductionServices.WorkOrderPhase.getById(phaseId);
        if (!phase) {
          console.error("Phase not found:", phaseId);
          return false;
        }

        phase.comment = comment;

        const success = await ProductionServices.WorkOrderPhase.update(
          phaseId,
          phase,
        );

        if (success) {
          await workcenterStore.refreshLoadedWorkOrders();
        }

        return success;
      } catch (error) {
        console.error("Error updating phase comment:", error);
        return false;
      }
    },
    async updatePhaseQuantities(
      counterOk: number,
      counterKo: number,
    ): Promise<boolean> {
      const workcenterStore = usePlantWorkcenterStore();

      if (
        !workcenterStore.workcenter ||
        !workcenterStore.workcenterRt?.workorders?.length
      ) {
        return false;
      }

      const phaseId =
        workcenterStore.workcenterRt.workorders[0].workOrderPhaseId;

      try {
        const result =
          await ProductionServices.WorkcenterShift.UpdatePhaseQuantities({
            workcenterId: workcenterStore.workcenter.id,
            workOrderPhaseId: phaseId,
            quantityOk: counterOk,
            quantityKo: counterKo,
          });

        if (result) {
          await workcenterStore.refreshLoadedWorkOrders();
        }

        return result;
      } catch (error) {
        console.error("Error updating phase quantities:", error);
        return false;
      }
    },
    /**
     * Neteja tot l'estat de la fase activa.
     */
    clearActivePhase() {
      this.workOrderReferenceDocuments = [];
      this.nextAvailablePhase = null;
      this.phaseTimeMetrics = undefined;
      this.billOfMaterials = [];
    },
  },
});

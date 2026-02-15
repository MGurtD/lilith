import { defineStore } from "pinia";
import {
  CreateWorkOrderDto,
  DetailedWorkOrder,
  WorkOrder,
  WorkOrderOrder,
  WorkOrderPhase,
  WorkOrderPhaseBillOfMaterials,
  WorkOrderPhaseDetail,
} from "../types";
import Services from "../services";

export const useWorkOrderStore = defineStore("workorder", {
  state: () => ({
    workorder: undefined as WorkOrder | undefined,
    workorders: undefined as Array<WorkOrder> | undefined,
    workorderPhase: undefined as WorkOrderPhase | undefined,
    workorderPhases: undefined as Array<WorkOrderPhase> | undefined,
    workorderPhaseDetail: undefined as WorkOrderPhaseDetail | undefined,
    workorderPhaseDetails: undefined as Array<WorkOrderPhaseDetail> | undefined,
    detailedWorkOrders: undefined as Array<DetailedWorkOrder> | undefined,
  }),
  getters: {},
  actions: {
    getWorkOrderCodeById(id: string): string | undefined {
      const wo = this.workorders?.find((r) => r.id === id);
      if (wo) return wo.code;
    },
    getWorkOrderStatusById(id: string): string | undefined {
      const wo = this.workorders?.find((r) => r.id === id);
      if (wo) return wo.statusId;
    },
    getWorkOrderCost(id: string): number {
      let cost = 0;
      const wo = this.workorders?.find((r) => r.id === id);
      if (wo) cost = wo.machineCost + wo.operatorCost + wo.materialCost;
      return cost;
    },
    // Workorder
    setNew(id: string) {
      this.workorder = {
        id,
        code: "",
        workMasterId: "",
        referenceId: "",
        exerciceId: "",
        order: 0,
        plannedQuantity: 0,
        plannedDate: "",
        statusId: "",
        startTime: null,
        endTime: null,
        comment: "",
        disabled: false,
        costMachine: 0,
        costOperator: 0,
        machineCost: 0,
        operatorCost: 0,
        machineTime: 0,
        operatorTime: 0,
        materialCost: 0,
        materialTime: 0,
        totalQuantity: 0,
        phases: [],
      } as WorkOrder;
    },
    async fetchFiltered(
      startTime: string,
      endTime: string,
      statusId?: string,
      referenceId?: string,
      customerId?: string,
    ) {
      this.workorders = await Services.WorkOrder.GetBetweenDatesAndStatus(
        startTime,
        endTime,
        statusId,
        referenceId,
        customerId,
      );
    },
    async fetchAll() {
      this.workorders = await Services.WorkOrder.getAll();
    },
    async fetchOne(id: string) {
      try {
        const data = await Services.WorkOrder.getById(id);
        if (data) {
          // Convert ISO date strings to Date objects for DatePicker compatibility
          if (data.plannedDate) {
            data.plannedDate = new Date(data.plannedDate) as any;
          }
          if (data.startTime) {
            data.startTime = new Date(data.startTime) as any;
          }
          if (data.endTime) {
            data.endTime = new Date(data.endTime) as any;
          }
        }
        this.workorder = data;
      } catch (error) {
        console.error("Error in fetchOne:", error);
        throw error;
      }
    },
    async fetchBySalesOrder(salesOrderId: string) {
      this.workorders =
        await Services.WorkOrder.GetBySalesOrderId(salesOrderId);
    },
    async fetchPlannable() {
      const data = await Services.WorkOrder.GetPlannableWorkOrders();
      if (data && Array.isArray(data)) {
        this.workorders = data;
      }
    },
    async create(model: CreateWorkOrderDto) {
      const result = await Services.WorkOrder.Create(model);
      if (result) await this.fetchOne(result.content!.id);
      return result;
    },
    async createAndUpdateSalesOrderDetail(
      model: CreateWorkOrderDto,
      detailId: string,
    ) {
      const result = await Services.WorkOrder.CreateFromSalesOrderDetail(
        model,
        detailId,
      );
      if (result) await this.fetchOne(result.content!.id);
      return result;
    },
    async update(id: string, model: WorkOrder) {
      const result = await Services.WorkOrder.update(id, model);
      if (result) await this.fetchAll();
      return result;
    },
    async priorize(workOrderOrders: WorkOrderOrder[]) {
      const result = await Services.WorkOrder.Priorize(workOrderOrders);
      return result;
    },
    async delete(id: string) {
      const result = await Services.WorkOrder.delete(id);
      if (result) await this.fetchAll();
      return result;
    },

    // Phases
    async fetchPhaseById(id: string) {
      this.workorderPhase = await Services.WorkOrderPhase.getById(id);
    },
    async fetchExternalPhases(startTime: string, endTime: string) {
      this.workorderPhases =
        await Services.WorkOrderPhase.getExternalWorkOrderPhases(
          startTime,
          endTime,
        );
    },
    async createPhase(model: WorkOrderPhase) {
      const result = await Services.WorkOrderPhase.create(model);
      if (result) await this.fetchPhaseById(model.id);
      return result;
    },
    async updatePhase(id: string, model: WorkOrderPhase) {
      const result = await Services.WorkOrderPhase.update(id, model);
      if (result) await this.fetchPhaseById(model.id);
      return result;
    },
    async deletePhase(id: string) {
      const result = await Services.WorkOrderPhase.delete(id);
      if (result) await this.fetchOne(this.workorder!.id);
      return result;
    },

    // Phase details
    async createPhaseDetail(model: WorkOrderPhaseDetail) {
      const result = await Services.WorkOrderPhaseDetail.create(model);
      if (result) await this.fetchPhaseById(model.workOrderPhaseId);
      return result;
    },
    async updatePhaseDetail(id: string, model: WorkOrderPhaseDetail) {
      const result = await Services.WorkOrderPhaseDetail.update(id, model);
      if (result) await this.fetchPhaseById(model.workOrderPhaseId);
      return result;
    },
    async deletePhaseDetail(id: string) {
      const result = await Services.WorkOrderPhaseDetail.delete(id);
      if (result) await this.fetchPhaseById(this.workorderPhase!.id);
      return result;
    },

    // Phase bill of materials
    async createPhaseBomItem(model: WorkOrderPhaseBillOfMaterials) {
      const result = await Services.WorkOrderPhaseBillOfMaterials.create(model);
      if (result) await this.fetchPhaseById(model.workOrderPhaseId);
      return result;
    },
    async updatePhaseBomItem(id: string, model: WorkOrderPhaseBillOfMaterials) {
      const result = await Services.WorkOrderPhaseBillOfMaterials.update(
        id,
        model,
      );
      if (result) await this.fetchPhaseById(model.workOrderPhaseId);
      return result;
    },
    async deletePhaseBomItem(id: string) {
      const result = await Services.WorkOrderPhaseBillOfMaterials.delete(id);
      if (result) await this.fetchPhaseById(this.workorderPhase!.id);
      return result;
    },

    //Detailed workorder
    async fetchByWorkcenterId(id: string) {
      this.detailedWorkOrders =
        await Services.DetailedWorkOrder.getByWorkcenterId(id);
    },
    async fetchByWorkOrderId(id: string) {
      try {
        const detailedWorkOrders =
          await Services.DetailedWorkOrder.getByWorkOrderId(id);
        if (detailedWorkOrders) {
          this.detailedWorkOrders = detailedWorkOrders;
          return detailedWorkOrders; // Devuelve las órdenes de trabajo detalladas
        } else {
          console.error(
            `No se encontraron órdenes de trabajo detalladas para el ID: ${id}`,
          );
          return undefined;
        }
      } catch (error) {
        console.error(`Error al obtener la orden de trabajo: ${error}`);
        return undefined;
      }
    },
  },
});

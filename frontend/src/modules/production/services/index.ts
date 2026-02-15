import {
  Enterprise,
  Site,
  WorkcenterType,
  MachineStatus,
  MachineStatusReason,
  Operator,
  OperatorType,
  WorkcenterCost,
  WorkcenterShiftGroup,
  WorkcenterProfitPercentage,
} from "../types";
import BaseService from "../../../api/base.service";
import {
  WorkMasterPhaseBillOfMaterialsService,
  WorkMasterPhaseDetailService,
  WorkMasterPhaseService,
  WorkMasterService,
} from "./workmaster.service";
import {
  WorkOrderService,
  WorkOrderPhaseService,
  WorkOrderPhaseDetailService,
  WorkOrderPhaseBillOfMaterialsService,
  DetailedWorkOrderService,
} from "./workorder.service";
import { ProductionPartService } from "./productionpart.service";
import ShiftService from "./shift.service";
import ProductionCostDashboardService from "./productioncostdashboard.service";
import { WorkcenterShiftService } from "./workcentershift.service";
import { AreaService } from "./area.service";
import { WorkcenterService } from "./workcenter.service";
import { WorkcenterProfitPercentageService } from "./workcenterprofitpercentage.service";

export { AreaService, WorkcenterService, WorkcenterProfitPercentageService };
export class EnterpriseService extends BaseService<Enterprise> {}
export class SiteService extends BaseService<Site> {}
export class WorkcenterTypeService extends BaseService<WorkcenterType> {}
export class WorkcenterCostService extends BaseService<WorkcenterCost> {}

export class MachineStatusService extends BaseService<MachineStatus> {
  async getAllWithReasons(): Promise<Array<MachineStatus> | undefined> {
    try {
      const response = await this.apiClient.get(
        `${this.resource}?includeReasons=true`,
      );
      if (response.status === 200) {
        return response.data as Array<MachineStatus>;
      }
    } catch (err) {
      console.error(err);
    }
  }
}

export class MachineStatusReasonService extends BaseService<MachineStatusReason> {}
export class OperatorService extends BaseService<Operator> {}
export class OperatorTypeService extends BaseService<OperatorType> {}

export default {
  Enterprise: new EnterpriseService("/Enterprise"),
  Areas: new AreaService("/Area"),
  Site: new SiteService("/Site"),
  WorkcenterType: new WorkcenterTypeService("/WorkcenterType"),
  Workcenter: new WorkcenterService("/Workcenter"),
  WorkcenterCost: new WorkcenterCostService("/WorkcenterCost"),
  MachineStatus: new MachineStatusService("/MachineStatus"),
  MachineStatusReason: new MachineStatusReasonService("/MachineStatus/Reason"),
  OperatorType: new OperatorTypeService("/OperatorType"),
  Operator: new OperatorService("/Operator"),
  WorkMaster: new WorkMasterService("/WorkMaster"),
  WorkMasterPhase: new WorkMasterPhaseService("/WorkMaster/Phase"),
  WorkMasterPhaseDetail: new WorkMasterPhaseDetailService(
    "/WorkMaster/Phase/Detail",
  ),
  WorkMasterPhaseBillOfMaterials: new WorkMasterPhaseBillOfMaterialsService(
    "/WorkMaster/Phase/BillOfMaterials",
  ),
  WorkOrder: new WorkOrderService("/WorkOrder"),
  WorkOrderPhase: new WorkOrderPhaseService("/WorkOrder/Phase"),
  WorkOrderPhaseDetail: new WorkOrderPhaseDetailService(
    "/WorkOrder/Phase/Detail",
  ),
  WorkOrderPhaseBillOfMaterials: new WorkOrderPhaseBillOfMaterialsService(
    "/WorkOrder/Phase/BillOfMaterials",
  ),
  ProductionPart: new ProductionPartService("/ProductionPart"),
  DetailedWorkOrder: new DetailedWorkOrderService("/WorkOrder/Detailed"),
  ShiftService: new ShiftService("/Shift"),
  ProductionCostDashboardService: new ProductionCostDashboardService(
    "/productioncost",
  ),
  WorkcenterShift: new WorkcenterShiftService("/WorkcenterShift"),
  WorkcenterProfitPercentage: new WorkcenterProfitPercentageService(
    "/WorkcenterProfitPercentage",
  ),
};

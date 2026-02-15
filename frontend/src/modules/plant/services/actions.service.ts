import {
  OperatorClockInOutRequest,
  RealtimeHandler,
  WorkcenterRealtimeHandler,
  WorkcenterRealtime,
  ChangeMachineStatusRequest,
  LoadWorkOrderPhaseRequest,
  UnloadWorkOrderPhaseRequest,
} from "../types";
import serverClient, {
  connectWebSocket,
  disconnectWebSocket,
  handleMessages,
  WS_ENDPOINTS,
} from "../../../api/actions.client";

class ActionsService {
  async clockInOperator(request: OperatorClockInOutRequest) {
    const endpoint = `/api/operator/clockin`;
    const response = await serverClient.post(endpoint, request);
    return response.status === 200;
  }
  async clockOutOperator(request: OperatorClockInOutRequest) {
    const endpoint = `/api/operator/clockout`;
    const response = await serverClient.post(endpoint, request);
    return response.status === 200;
  }
  async changeMachineStatus(request: ChangeMachineStatusRequest) {
    const endpoint = `/api/status/in`;
    const response = await serverClient.post(endpoint, request);
    return response.status === 200;
  }
  async loadWorkOrderPhaseAndMachineStatus(request: LoadWorkOrderPhaseRequest) {
    const endpoint = `/api/workorderphase/in`;
    const response = await serverClient.post(endpoint, request);
    return response.status === 200;
  }
  async unloadWorkOrderPhase(request: UnloadWorkOrderPhaseRequest) {
    const endpoint = `/api/workorderphase/out`;
    const response = await serverClient.post(endpoint, request);
    return response.status === 200;
  }

  connect(endpoint: string = WS_ENDPOINTS.GENERAL) {
    connectWebSocket(endpoint);
  }

  disconnect() {
    disconnectWebSocket();
  }

  /**
   * Connecta al WebSocket general i retorna handler amb callback per actualitzacions
   */
  connectToGeneral(): RealtimeHandler {
    const callbacks: Array<(data: WorkcenterRealtime[]) => void> = [];

    const cleanup = handleMessages((messageType: string, payload: any) => {
      console.log("[Service] Received WebSocket message:", {
        type: messageType,
        payload,
      });

      if (messageType === "General") {
        if (payload && typeof payload === "object") {
          const data = Object.values(payload).map((wc: any) => ({
            ...wc,
            operators: Array.isArray(wc.operators) ? wc.operators : [],
          })) as WorkcenterRealtime[];

          callbacks.forEach((cb) => cb(data));
        } else {
          callbacks.forEach((cb) => cb([]));
        }
      }
    });

    return {
      cleanup,
      onUpdate: (callback) => callbacks.push(callback),
    };
  }

  /**
   * Connecta al WebSocket d'un workcenter específic i retorna handler amb callback
   */
  connectToWorkcenter(workcenterId: string): WorkcenterRealtimeHandler {
    const callbacks: Array<(data: WorkcenterRealtime) => void> = [];

    const cleanup = handleMessages((messageType: string, payload: any) => {
      console.log("[Service] Received WebSocket message:", {
        type: messageType,
        payload,
      });

      // Processar missatges de workcenter (ambdós tipus)
      if (messageType === "Workcenter") {
        if (payload && typeof payload === "object") {
          const data = {
            ...payload,
            operators: Array.isArray(payload.operators)
              ? payload.operators
              : [],
          } as WorkcenterRealtime;

          callbacks.forEach((cb) => cb(data));
        }
      }
    });

    return {
      cleanup,
      onUpdate: (callback) => callbacks.push(callback),
    };
  }
}

// Create singleton instance and export
const client = new ActionsService();

export default { client, WS_ENDPOINTS };

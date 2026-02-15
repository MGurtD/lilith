import { computed, onUnmounted, ref } from "vue";
import {
  connectWebSocket,
  disconnectWebSocket,
  WS_ENDPOINTS,
} from "../../../api/actions.client";

const currentEndpoint = ref<string | null>(null);
const isConnected = ref(false);

/**
 * Composable per gestionar connexions WebSocket
 * - Només una connexió activa alhora
 * - Auto-cleanup al desmontar component
 * - Type-safe amb endpoints predefinits
 */
export function useWebSocketConnection() {
  /**
   * Connecta a un endpoint WebSocket
   * - Desconnecta automàticament l'anterior si existeix
   * - Actualitza estat de connexió
   */
  const connect = (endpoint: string, options: { debug?: boolean } = {}) => {
    // Si ja estem connectats al mateix endpoint, no fer res
    if (currentEndpoint.value === endpoint && isConnected.value) {
      if (options.debug) {
        console.log(`[WS Composable] Already connected to ${endpoint}`);
      }
      return;
    }

    // Desconnectar endpoint anterior si existeix
    if (currentEndpoint.value && isConnected.value) {
      if (options.debug) {
        console.log(
          `[WS Composable] Disconnecting from ${currentEndpoint.value}`
        );
      }
      disconnectWebSocket();
    }

    // Connectar al nou endpoint
    if (options.debug) {
      console.log(`[WS Composable] Connecting to ${endpoint}`);
    }

    connectWebSocket(endpoint, options);
    currentEndpoint.value = endpoint;
    isConnected.value = true;
  };

  /**
   * Desconnecta el WebSocket actual
   */
  const disconnect = () => {
    if (isConnected.value) {
      console.log(
        `[WS Composable] Disconnecting from ${currentEndpoint.value}`
      );
      disconnectWebSocket();
      currentEndpoint.value = null;
      isConnected.value = false;
    }
  };

  /**
   * Canvia d'endpoint desconnectant l'anterior
   */
  const switchEndpoint = (
    newEndpoint: string,
    options: { debug?: boolean } = {}
  ) => {
    disconnect();
    connect(newEndpoint, options);
  };

  // Auto-cleanup al desmontar el component
  onUnmounted(() => {
    disconnect();
  });

  return {
    connect,
    disconnect,
    switchEndpoint,
    isConnected: computed(() => isConnected.value),
    currentEndpoint: computed(() => currentEndpoint.value),
  };
}

// Re-exportar endpoints per conveniència
export { WS_ENDPOINTS };

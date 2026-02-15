import { defineStore } from "pinia";
import type { ParsedErrorInfo } from "@/types/error-response";

export const useApiStore = defineStore("apiStore", {
  state: () => {
    return {
      isOnError: false,
      isWaiting: false,
      lastError: undefined as undefined | Date,
      lastErrorInfo: undefined as undefined | ParsedErrorInfo,
    };
  },
  actions: {
    /**
     * @deprecated Use setErrorInfo() instead for better error handling
     */
    setError(message: string) {
      this.isOnError = true;
      this.lastError = new Date();
      // Alert removed - now using global toast service
      console.warn("setError() is deprecated. Error:", message);
    },

    /**
     * Store parsed error information from API calls
     * Automatically called by axios interceptor
     */
    setErrorInfo(errorInfo: ParsedErrorInfo) {
      this.isOnError = true;
      this.lastError = new Date();
      this.lastErrorInfo = errorInfo;
    },

    /**
     * Clear error state
     */
    clearError() {
      this.isOnError = false;
      this.lastErrorInfo = undefined;
    },
  },
});

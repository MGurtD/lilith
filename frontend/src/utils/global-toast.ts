import type { App } from "vue";
import type { ToastMessageOptions } from "primevue/toast";

interface ToastInstance {
  add(message: ToastMessageOptions): void;
  removeAllGroups(): void;
  removeGroup(group: string): void;
}

/**
 * Global toast service accessible from anywhere (including axios interceptors)
 * Implements rate limiting to prevent duplicate error toasts
 */
class GlobalToastService {
  private toastInstance: ToastInstance | null = null;
  private lastToastMap = new Map<string, number>();
  private readonly DUPLICATE_WINDOW_MS = 5000; // 5 seconds

  /**
   * Initialize the global toast service with Vue app instance
   * Must be called in main.ts after app.use(ToastService)
   */
  init(app: App): void {
    // Access PrimeVue's toast instance through the app's config
    const toast = (app.config.globalProperties as any).$toast;
    if (!toast) {
      console.error(
        "GlobalToastService: ToastService not found. Ensure app.use(ToastService) is called before init()"
      );
      return;
    }
    this.toastInstance = toast;
  }

  /**
   * Check if toast service is initialized and ready
   */
  isInitialized(): boolean {
    return this.toastInstance !== null;
  }

  /**
   * Show a toast message with automatic deduplication
   * @param options - PrimeVue toast message options
   * @param skipDuplicateCheck - Skip duplicate checking (for important messages)
   */
  show(options: ToastMessageOptions, skipDuplicateCheck = false): void {
    if (!this.toastInstance) {
      console.warn(
        "GlobalToastService: Toast not initialized. Message:",
        options.summary
      );
      return;
    }

    // Check for duplicates (unless skipped)
    if (!skipDuplicateCheck) {
      const messageKey = `${options.severity}:${options.summary}:${options.detail || ""}`;
      const now = Date.now();
      const lastShown = this.lastToastMap.get(messageKey);

      if (lastShown && now - lastShown < this.DUPLICATE_WINDOW_MS) {
        console.debug(
          "GlobalToastService: Duplicate toast suppressed",
          messageKey
        );
        return;
      }

      this.lastToastMap.set(messageKey, now);

      // Clean up old entries (older than 10 seconds)
      const cleanupThreshold = now - 10000;
      for (const [key, timestamp] of this.lastToastMap.entries()) {
        if (timestamp < cleanupThreshold) {
          this.lastToastMap.delete(key);
        }
      }
    }

    this.toastInstance.add(options);
  }

  /**
   * Show error toast (severity: error)
   */
  error(summary: string, detail?: string, life = 8000): void {
    this.show({
      severity: "error",
      summary,
      detail,
      life,
    });
  }

  /**
   * Show warning toast (severity: warn)
   */
  warn(summary: string, detail?: string, life = 6000): void {
    this.show({
      severity: "warn",
      summary,
      detail,
      life,
    });
  }

  /**
   * Show info toast (severity: info)
   */
  info(summary: string, detail?: string, life = 4000): void {
    this.show({
      severity: "info",
      summary,
      detail,
      life,
    });
  }

  /**
   * Show success toast (severity: success)
   */
  success(summary: string, detail?: string, life = 4000): void {
    this.show({
      severity: "success",
      summary,
      detail,
      life,
    });
  }

  /**
   * Show persistent toast that doesn't auto-close
   * Useful for offline mode or critical errors
   */
  showPersistent(options: ToastMessageOptions): void {
    this.show({ ...options, life: undefined }, true);
  }

  /**
   * Clear all toasts
   */
  clearAll(): void {
    if (this.toastInstance) {
      this.toastInstance.removeAllGroups();
    }
  }
}

// Export singleton instance
export const globalToast = new GlobalToastService();

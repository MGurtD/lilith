import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

export interface ConfirmDeleteOptions {
  /** Deletes the record and resolves to true when it is gone. */
  remove: () => Promise<boolean> | boolean;
  /** Name shown in the question; without it the question is generic. */
  name?: string;
  /** Replaces the generic question when the screen needs a specific one. */
  message?: string;
  /** Replaces the generic success message. */
  doneMessage?: string;
  /** Runs after a successful delete, e.g. to reload a list or leave the screen. */
  onDeleted?: () => void | Promise<void>;
  /** False when another component reports the result, e.g. a child table that only emits. */
  notify?: boolean;
}

/**
 * Asks before deleting, deletes, and reports the result with one toast. A refused
 * delete (409) is not reported here: the API client already shows its reason, so the
 * error is only caught to avoid an unhandled rejection.
 */
export function useConfirmDelete() {
  const confirm = useConfirm();
  const toast = useToast();
  const { t } = useI18n();

  return (options: ConfirmDeleteOptions) => {
    const notify = options.notify ?? true;

    confirm.require({
      message:
        options.message ??
        (options.name
          ? t("common.delete.confirmNamed", { name: options.name })
          : t("common.delete.confirm")),
      icon: "pi pi-question-circle",
      acceptIcon: "pi pi-check",
      rejectIcon: "pi pi-times",
      accept: async () => {
        let deleted: boolean;
        try {
          deleted = await options.remove();
        } catch (error) {
          console.error("Delete failed", error);
          return;
        }

        if (!deleted) {
          if (notify)
            toast.add({ severity: "error", summary: t("common.delete.failed"), life: 4000 });
          return;
        }

        if (notify)
          toast.add({
            severity: "success",
            summary: options.doneMessage ?? t("common.delete.done"),
            life: 3000,
          });
        await options.onDeleted?.();
      },
    });
  };
}

import { ref, onMounted, onUnmounted, type Ref } from "vue";

/**
 * Composable that calculates the available scroll height for a DataTable
 * based on its position in the viewport.
 *
 * Usage:
 * ```vue
 * <template>
 *   <div ref="tableRef">
 *     <DataTable :scrollHeight="scrollHeight" scrollable ...>
 *   </div>
 * </template>
 *
 * <script setup>
 * const { tableRef, scrollHeight } = useScrollHeight();
 * </script>
 * ```
 *
 * @param bottomMargin - Extra pixels to subtract (default: 16)
 */
export function useScrollHeight(bottomMargin = 16) {
  const tableRef = ref<HTMLElement | null>(null) as Ref<HTMLElement | null>;
  const scrollHeight = ref("500px");

  const calculate = () => {
    if (!tableRef.value) return;
    const rect = tableRef.value.getBoundingClientRect();
    const available = window.innerHeight - rect.top - bottomMargin;
    scrollHeight.value = `${Math.max(200, Math.floor(available))}px`;
  };

  let resizeObserver: ResizeObserver | null = null;

  onMounted(() => {
    calculate();
    window.addEventListener("resize", calculate);

    // Recalculate when the element's position might change (e.g. tab switch)
    if (tableRef.value) {
      resizeObserver = new ResizeObserver(calculate);
      resizeObserver.observe(tableRef.value);
    }
  });

  onUnmounted(() => {
    window.removeEventListener("resize", calculate);
    resizeObserver?.disconnect();
  });

  return { tableRef, scrollHeight };
}

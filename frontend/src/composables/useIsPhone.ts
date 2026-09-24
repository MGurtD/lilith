import { onScopeDispose, ref } from "vue";

// Phone breakpoint shared with Form.vue's responsive rows (mobile below 768px).
const PHONE_QUERY = "(max-width: 767.98px)";

/** Reactive flag for the phone layout: navigation in a drawer, full-width content. */
export function useIsPhone() {
  const query = window.matchMedia(PHONE_QUERY);
  const isPhone = ref(query.matches);
  const update = (event: MediaQueryListEvent) => (isPhone.value = event.matches);
  query.addEventListener("change", update);
  onScopeDispose(() => query.removeEventListener("change", update));
  return isPhone;
}

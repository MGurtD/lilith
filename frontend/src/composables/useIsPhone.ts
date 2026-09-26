import { onScopeDispose, ref } from "vue";

// Phone breakpoint shared with Form.vue's responsive rows (mobile below 768px).
const PHONE_QUERY = "(max-width: 767.98px)";
// Phones and shop-floor tablets (up to 1024px): touch-first layouts.
const COMPACT_QUERY = "(max-width: 1024px)";

const useMediaQuery = (media: string) => {
  const query = window.matchMedia(media);
  const matches = ref(query.matches);
  const update = (event: MediaQueryListEvent) => (matches.value = event.matches);
  query.addEventListener("change", update);
  onScopeDispose(() => query.removeEventListener("change", update));
  return matches;
};

/** Reactive flag for the phone layout: navigation in a drawer, full-width content. */
export function useIsPhone() {
  return useMediaQuery(PHONE_QUERY);
}

/** Reactive flag for phones and tablets up to 1024px, e.g. table filters in a sheet. */
export function useIsCompact() {
  return useMediaQuery(COMPACT_QUERY);
}

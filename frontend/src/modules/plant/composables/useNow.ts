import { onScopeDispose, readonly, ref } from "vue";

// One clock for every plant timer: a single interval runs while at least
// one component uses it, so tiles and the placa tick together and do not
// wait for the next WebSocket message.
const now = ref(Date.now());
let users = 0;
let timer: ReturnType<typeof setInterval> | undefined;

export function useNow() {
  users++;
  if (!timer) {
    now.value = Date.now();
    timer = setInterval(() => (now.value = Date.now()), 1000);
  }
  onScopeDispose(() => {
    users--;
    if (users === 0 && timer) {
      clearInterval(timer);
      timer = undefined;
    }
  });
  return readonly(now);
}

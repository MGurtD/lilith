# Pinia 2 → 3 Migration Plan (Strategy A2)

> **Status**: Pending. Deferred due to cost.
> **Created**: 2026-07-05 · **Owner**: TBD
> **Supersedes**: original LOT 3 (vue-router 5 / pinia 3) blocking upgrade

## Executive Summary

The Lilith frontend currently runs **Pinia 2.3.1** because migrating to Pinia 3 requires refactoring all 30 stores to the setup store pattern (`defineStore("id", () => { ... })`). The Pinia 3 options-store shape `defineStore({id, state, getters, actions})` either fails to infer types or triggers a v2 compatibility shim that downgrades the store to an empty `Store<string, {}, {}, {}>` — which cascades into 1,273+ TypeScript errors across every component that consumes the affected store.

**Goal**: Convert all 30 Pinia stores to setup stores so the project can run Pinia 3 + vue-router 5 + the modern Vue ecosystem.

**Effort estimate**: 20-30 hours of focused work (1-2 sprints). Not safe to do incrementally because of cross-store imports and reactive state semantics.

**Current state**: working correctly on Pinia 2.3.1 + vue-router 4.6.4. No tests exist; validation = Playwright smoke tests (already in place as of PR #92).

---

## Why This Matters

| If we stay on Pinia 2.3.1 | If we migrate to Pinia 3.x |
|---|---|
| vue-router 5 stays blocked (peer dep `pinia@^3`) | vue-router 5 unblocked |
| Tech debt accumulates (Pinia 2 LTS ended) | Aligned with ecosystem |
| All current `pnpm outdated` benefits decay | Future upgrades unblocked |
| ~Zero current risk | Significant one-time risk |

**Trigger criteria** — migrate when one or more becomes true:
1. Project adds unit tests (we cannot migrate safely without regression coverage)
2. vue-router 5 needed for a feature
3. Pinia 2 reaches EOL and security backports stop
4. New team member expects `setup stores` as default pattern

Until then, this plan is dormant.

---

## Current State (Pinia 2.3.1, post-PR #92)

### Stores inventory

**30 stores total, ALL options stores**. Locations:
- `src/store/`: 3 — `geography.ts`, `index.ts`, `languages.ts`
- `src/modules/purchase/store/`: 9 — `expense`, `order`, `purchase`, `purchaseInvoices`, `purchaseInvoiceSeries`, `purchaseRate`, `receipt`, `suppliers`, `transportRate`
- `src/modules/sales/store/`: 6 — `budget`, `customers`, `deliveryNote`, `invoice`, `order`
- `src/modules/shared/store/`: 6 — `exercise`, `lifecycle`, `masterData`, `paymentMethod`, `reference`, `referenceType`, `reports`, `support`, `tax`
- `src/modules/verifactu/store/`: 1 — `verifactu`
- `src/modules/warehouse/store/`: 4 — `inventory`, `stock`, `stockMovement`, `warehouse`

### Pattern signature (all stores)

```typescript
export const useXyzStore = defineStore({
  id: "xyz",
  state: () => ({...}),
  getters: {...},
  actions: {...},
});
```

### Measured error pattern if we blindly upgrade to `pinia@3.0.4` (reproduced in worktree 2026-07-05)

| TS code | Count | % | Cause |
|---|---|---|---|
| `TS2339`: "Property X does not exist on type `Store<string, {}, {}, {}>`" | 1049 | 82% | Cascade: `defineStore` returns empty store type because `id` argument is missing from new signature |
| `TS7006`: "Parameter implicitly has an `any` type" | 160 | 13% | Getter `state` parameter typed as `any` because state inference failed |
| `TS2554`: "Expected 2-3 arguments, but got 1" | 30 | 2% | `defineStore({...})` 1-arg form removed in v3; must be `defineStore("id", {...})` |
| `TS2551`: "Did you mean X?" | 25 | 2% | Renamed getters triggered by inference loss |
| Other | 9 | 1% | TS7022/7023/2532/18048/2345 |

### What we already validated (worktree experiment 2026-07-05)

Applying the minimum fix (move `id` + add `State interface`) to 2 stores reduced errors from **1273 → 1242** (only −31 errors, ≈ 2.4%). This confirms: the minimum fix is **NOT sufficient**. Setup store refactor is the only path that restores type inference end-to-end.

---

## Strategy A2: Full Setup Store Refactor

**Why setup stores, not options with State interfaces**: Setup stores return explicit `Ref<T>` references for each state field. TypeScript can infer each property's type from `ref<T>(initialValue)`. Options stores delegate type extraction to Pinia internals, which is fragile in Pinia 3.x. Setup stores also compose naturally with Vue composition APIs (`computed`, `watch`, lifecycle hooks), which the project already uses.

**Refactor unit**: One store at a time. Order: simplest first (gives template), most complex last (benefit from lessons).

### Work Unit breakdown

#### WU-1 — Preparation & scaffolding (1-2h)

Tasks:
- Create branch `feature/pinia-3-migration` from `origin/dev`
- Spin up a dedicated worktree: `git worktree add ../lilith-pinia3 -b feature/pinia-3-migration origin/dev`
- Install Pinia 3 in the worktree only: `pnpm add pinia@3`
- Reproduce the 1273-error baseline (`pnpm run typecheck`) — save to `docs/pinia-3-migration/baseline-errors.log`
- Add per-store smoke tests (see WU-1b below)
- Confirm no runtime regression in `pnpm run smoke:e2e` with the worktree

**Done criteria**:
- Worktree exists, typecheck produces baseline.log with exactly 1273 errors
- All smoke tests (old + new per-store) run green before any migration

#### WU-1b — Per-store smoke tests (3-4h, BLOCKING for safety)

This is the **single most important work unit**. Without per-store tests, the migration is unsafe. The global smoke:e2e covers auth + dashboard + 1 workorder list — too thin.

For each module (sales, purchase, production, warehouse, shared, verifactu), create `frontend/scripts/smoke-<module>.mjs` that:
1. Logs in via `/login`
2. Navigates to the module's index view
3. Calls 1-3 representative store actions (e.g., `useXStore.fetchAll()`)
4. Asserts the resulting state has expected shape (via `page.evaluate(() => ...)`)
5. Takes a screenshot per step

Skip stores that have no fetchAll action (e.g., pure UI state).

Sample for `languages`:
```javascript
await page.evaluate(async () => {
  const { useLanguageStore } = await import('/src/store/languages.ts');
  const store = useLanguageStore();
  await store.fetchAll();
  return store.items.length;
});
```

**Done criteria**:
- ≥6 smoke tests covering ≥10 distinct stores
- All pass on Pinia 2.3.1 baseline

#### WU-2 — Pilot conversion: 1 store + downstream consumers (3-4h)

Pick: **`src/store/languages.ts`** (simplest options store: 14 errors, no cross-store imports, no complex getters).

Steps:
1. Write the converted setup store version in `src/store/languages.ts` (see Migration Conventions below)
2. Run `pnpm run typecheck` — expect ALL `languages.ts` errors gone AND all errors in any consumer (Exercise, Taxes, Login flow) gone
3. Run `pnpm run smoke:e2e` — expect zero new console errors
4. Run the per-store smoke test — expect green
5. If any new errors appear in OTHER stores, log them to `docs/pinia-3-migration/wu2-learnings.md` (these are TypeScript graph issues, not Pinia issues)

**Done criteria**:
- `languages.ts` converted
- 0 errors mention `languages.ts` or any `useLanguageStore` consumer
- All smoke tests green
- `wu2-learnings.md` written with one observation per surprise

#### WU-3 — Conventions + helper template (1h)

After WU-2, write `docs/pinia-3-migration/conventions.md` with:
- Before/after example for: simple state, state with `ref<T>()` wrapped values, getters using `this`, getters using cross-store refs, async actions with explicit return type, actions that mutate state
- Gotcha list (from wu2-learnings.md) promoted to permanent
- Cheatsheet table mapping common options-store patterns → setup-store equivalents

Keep this document at ≤ 200 lines. Anyone executing WU-4 should be able to do so by reading only `conventions.md`.

#### WU-4 — Per-module migration (12-16h)

Order (simplest first):

| Order | Module | Stores | Notes |
|---|---|---|---|
| 1 | `src/store/` (global) | 3 | Already started with `languages.ts` and `geography.ts`; finish with `index.ts` (the auth store, do last because largest) |
| 2 | `src/modules/shared/store/` | 9 | Includes the most complex: `reference.ts` (35 errors), `lifecycle.ts` (22) — save these for last within the module |
| 3 | `src/modules/warehouse/store/` | 4 | |
| 4 | `src/modules/purchase/store/` | 9 | |
| 5 | `src/modules/sales/store/` | 6 | |
| 6 | `src/modules/production/store/` | (already mostly setup stores per `src/modules/plant/store/` audit — verify) | |
| 7 | `src/modules/verifactu/store/` | 1 | Last because it depends on shared/services |

Per each store:
1. Convert per `conventions.md`
2. `pnpm run typecheck` — expect that store + ALL its consumers drop to 0 errors
3. Run the per-store smoke test
4. Run `pnpm run smoke:e2e` (just to be safe)
5. `git add . && git commit -m "refactor(pinia3): migrate <storeName> store to setup pattern"`

**Note on `production` module**: A previous investigation suggested some stores there already use the setup pattern (e.g., `src/modules/plant/store/activePhase.store.ts`). Audit first; may be partially done. Don't regress those.

**Done criteria per module**:
- All stores in module converted
- 0 typecheck errors mention any store in module
- All per-store smoke tests green
- One commit per module

#### WU-5 — Final validation + vue-router 5 upgrade (2-3h)

After all modules migrated:
1. `pnpm run typecheck` → 0 errors expected
2. `pnpm run build` → 0 errors
3. `pnpm run smoke:e2e` → login + dashboard + all module index screens render, no console errors
4. Update `frontend/package.json`: bump `vue-router` to ^5.1.0 (now possible because pinia@3 satisfies its peer dep)
5. Re-run smoke:e2e — vue-router 5 routing should work transparently
6. If regressions appear, document and roll back vue-router only
7. `git commit -m "chore(deps): upgrade vue-router 4.6→5.1 (after Pinia 3 migration)"`
8. Open PR `feature/pinia-3-migration` → `dev`

---

## Migration Conventions

### Pattern 1 — Simple state

```typescript
// BEFORE (options store)
export const useXStore = defineStore({
  id: "x",
  state: () => ({ items: [] as Item[] }),
  getters: { count: (state) => state.items.length },
  actions: { add(i: Item) { this.items.push(i) } },
});

// AFTER (setup store)
export const useXStore = defineStore("x", () => {
  const items = ref<Item[]>([]);
  const count = computed(() => items.value.length);
  function add(i: Item) { items.value.push(i); }
  return { items, count, add };
});
```

### Pattern 2 — State with undefined / async-loaded

```typescript
// BEFORE
state: () => ({
  list: undefined as List[] | undefined,
  isLoading: false,
}),

// AFTER
const list = ref<List[] | undefined>(undefined);
const isLoading = ref(false);
```

### Pattern 3 — Getters that access other stores (use `useXxxStore()` inside)

```typescript
// BEFORE
getters: {
  fullName: (state) => `${state.first} ${state.last}`,
  isAdmin: (state) => state.role === "admin",
}

// AFTER (composition)
const first = ref("");
const last = ref("");
const role = ref("");
const fullName = computed(() => `${first.value} ${last.value}`);
const isAdmin = computed(() => role.value === "admin");
```

For getters that USE `this` to call other getters (the Pinia 2 trick), use `computed()` chains:

```typescript
const fullName = computed(() => `${first.value} ${last.value}`);
const greeting = computed(() => `Hi, ${fullName.value}!`);
```

### Pattern 4 — Actions that mutate state

`this.x = ...` becomes `x.value = ...`. `this.someMethod()` becomes a regular function call (no `this` in setup).

### Pattern 5 — Cross-store actions

```typescript
// BEFORE (lifecycle.ts) — uses other stores inside actions
actions: {
  async deleteStatus(id: string) {
    const lifecycleStore = useLifecycleStore();
    await api.delete(id);
    await lifecycleStore.fetchOne(...);
  },
}

// AFTER
async function deleteStatus(id: string) {
  const lifecycleStore = useLifecycleStore();
  await api.delete(id);
  await lifecycleStore.fetchOne(...);
}
```

### Critical gotchas

1. **`this` no longer exists** — all references must be replaced with local refs/functions. ESLint/TS will catch most.
2. **`store.x` in actions** — components that do `store.action()` keep working; only **inside** the action definition, `this` is gone.
3. **`storeToRefs(store)`** — components using `storeToRefs` keep working unchanged for setup stores. Verify with smoke tests.
4. **Reactivity depth** — `ref(obj)` makes the ref reactive but `.value` returns the same object reference. Mutations to `obj.field` are still reactive. This matches options-store semantics, so no behavioral change.
5. **Async actions that return values** — in options stores, return type is inferred. In setup stores, you must declare the return type:
   ```typescript
   async function fetchAll(): Promise<void> { ... }
   ```
   Or TS may complain that the returned Promise adds an extra state property.
6. **`store.$reset()`** — still works the same. Verified.

---

## Risks

| Risk | Severity | Mitigation |
|---|---|---|
| Components relying on `store.x` direct access break | High | Comprehensive per-store smoke tests (WU-1b) catches |
| `this.someMethod()` in nested closures breaks | Medium | ESLint rule + careful review |
| Cross-store circular deps cause infinite loops | Medium | Pinia setup stores hoist initial state lazily; existing imports likely OK |
| Async actions without explicit return types cause inference drift | Low | Convention document requires returns |
| vue-router 5 reveals other latent issues once coupled | Medium | WU-5 validates in isolation; can rollback vue-router only |
| Working on long-lived branch without merges from `dev` | Medium | Re-base every 2-3 days to stay clean |
| Bug discovered post-merge requires Pinia 3 rollback | Low | Keep `package.json:pinia` at `^2.3.1` until final WU-5 commit; git revert is clean |

---

## Acceptance Criteria

The migration is DONE when ALL of the following hold on a fresh `origin/dev`:

- [ ] `pnpm run typecheck` exits 0 errors
- [ ] `pnpm run build` exits 0 errors and produces `dist/`
- [ ] `pnpm run smoke:e2e` passes against live backend (login, dashboard, all module indexes, one workorder)
- [ ] Per-store smoke tests (≥10) all pass
- [ ] `frontend/package.json`: `pinia` at `^3.x`, `vue-router` at `^5.x`
- [ ] `frontend/scripts/` contains no `smoke-e2e.mjs` regressions
- [ ] No `// @ts-expect-error` or `as any` introduced for Pinia reasons
- [ ] PR approved and merged to `dev`
- [ ] `docs/pinia-3-migration/conventions.md` reflects any NEW patterns discovered during WU-4

---

## References

- **Pinia official migration guide**: https://pinia.vuejs.org/cookbook/migration-v2-v3.html
- **Pinia setup stores guide**: https://pinia.vuejs.org/core-concepts/#Setup-Stores
- **Original investigation** (engram topic `dependencies/upgrade-plan-2026-07`): the 4-lot plan
- **LOT 3 reduced PR #92** (already merged to `dev`): upgraded TS 5→6 + jwt-decode 3→4; intentionally deferred Pinia
- **Smoke test infrastructure** (commit `7ed47e9`): `frontend/scripts/smoke.mjs` + `smoke-e2e.mjs`
- **Branches already merged** with infra work: `upgrade/dependencies-2026-07` (4 commits)

## Status log

- 2026-07-05: Plan drafted based on reproduction + validation in worktree. NOT YET EXECUTED.

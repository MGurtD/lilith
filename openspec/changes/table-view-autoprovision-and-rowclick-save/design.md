# Design: Table View Autoprovision + Row-Click Filter Save

## Technical Approach

Two coupled changes following existing Lilith patterns (Clean Architecture backend, Pinia+BaseService frontend):

**Backend**: Add `EnsureDefault(userId, page) → GenericResponse` to `IUserTableViewService`. The service does an atomic get-or-create using the existing `Find()` repository + `SetDefault()` transactional pattern. No new entity, no migration.

**Frontend**: Add `ensureDefault` (with in-flight promise dedupe) and `saveFiltersToDefault` (read-modify-write of `viewConfig`) actions to `useUserTableViewStore`. In `Table.vue`: call `ensureDefault` before `loadDefaultView`, track `filtersDirty` ref, expose `@row-click`, save fire-and-forget when dirty.

## Architecture Decisions

### Decision: Endpoint shape — `POST /api/usertableview/ensure-default`

**Choice**: New POST route taking `{ userId, page }` in body. Returns `GenericResponse { Result: true, Content: UserTableView }` always (200 OK) — never 404 even when no default existed, since the contract guarantees one after the call.
**Alternatives considered**: (a) GET with side effects — violates REST; (b) PUT upsert — would require client to construct full entity.
**Rationale**: Matches existing pattern (`POST /api/usertableview` for Create). Body-based keeps URL clean.

### Decision: Frontend dedupe via in-flight promise Map

**Choice**: `Map<string, Promise<UserTableView>>` keyed by `${userId}:${page}`. Stored as a non-reactive `ref` (object identity stable across calls).
**Alternatives considered**: (a) Per-component dedupe — breaks when multiple `<Table>` instances mount; (b) debounce — wrong abstraction; (c) backend-only dedupe (DB unique index) — keeps duplicates possible until DB catches up.
**Rationale**: Frontend dedupe eliminates the duplicate request entirely. Backend still has the invariant (only one default per scope) as the safety net.

### Decision: `saveFiltersToDefault` does GET → merge → PUT

**Choice**: Read the current `UserTableView` via `GetById`, parse `viewConfig` JSON, merge `filters` into the existing object (never touch `columns` or `sort`), PUT.
**Alternatives considered**: (a) PATCH endpoint for partial update — would require new backend code; (b) client sends only filters and backend merges — server now needs JSON merge logic.
**Rationale**: Existing `Update` endpoint already works. Read-modify-write is a common pattern, and the race window (two saves in flight) only loses the latest — acceptable for filters (next click retries).

### Decision: Dirty flag via `@filter` / `@clear` events

**Choice**: `filtersDirty` ref in `Table.vue`, set true on filter/clear events, cleared in `.finally()` of the save.
**Alternatives considered**: (a) Deep-equal compare against last saved snapshot — robust but expensive; (b) Set dirty on every `update:filterValues` — too aggressive (fires per keystroke).
**Rationale**: Filter/clear events already mark "the user actively wants filtering". Tying dirty to those events keeps the meaning aligned with user intent.

### Decision: Fire-and-forget save on row click

**Choice**: `saveFiltersToDefault(...).catch(() => {})` — no `await`, no UI block, errors silently logged.
**Alternatives considered**: (a) `await` — blocks navigation by ~100ms; (b) Toast on error — annoying for a non-critical op.
**Rationale**: Row click is a navigation gesture; latency budget is 0. Worst case: save fails silently, next user-driven click or next row click retries.

## Data Flow

```
Table.vue mount (with :page="X")
  │
  ├─ await store.ensureDefault(userId, page)
  │    │
  │    ├─ if (inFlight[userId:page]) return inFlight[userId:page]
  │    ├─ inFlight[key] = api.EnsureDefault(userId, page)
  │    └─ api POST /api/usertableview/ensure-default
  │         └─ service.EnsureDefault → Find default → create if missing → return
  │
  ├─ await loadDefaultView()  (existing)
  └─ restoreFiltersIfNeeded()  (existing)

User clicks row (PrimeVue rowClick event)
  │
  ├─ emit("row-click", event)  → consumer navigates
  └─ if (filtersDirty && activeViewId !== "")
       └─ store.saveFiltersToDefault(userId, page, filterValues)
            │  (fire-and-forget)
            └─ GET → merge filters into viewConfig → PUT
                  └─ .finally(() => filtersDirty = false)
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `backend/src/Application.Contracts/Services/System/IUserTableViewService.cs` | Modify | Add `EnsureDefault(EnsureDefaultRequest) → GenericResponse` |
| `backend/src/Application.Contracts/Services/System/EnsureDefaultRequest.cs` | Create | DTO `{ Guid UserId, string Page }` |
| `backend/src/Application/Services/System/UserTableViewService.cs` | Modify | Implement `EnsureDefault` using Find → SetDefault pattern |
| `backend/src/Api/Controllers/System/UserTableViewController.cs` | Modify | Add `[HttpPost("ensure-default")]` route |
| `frontend/src/services/usertableview.service.ts` | Modify | Add `EnsureDefault(userId, page)` method |
| `frontend/src/store/usertableview.ts` | Modify | Add `ensureDefault` + `saveFiltersToDefault` actions, in-flight promise Map |
| `frontend/src/components/tables/Table.vue` | Modify | Call `ensureDefault` on mount, add `filtersDirty` ref, expose `@row-click`, save on row-click if dirty |

## Interfaces / Contracts

```typescript
// Frontend service
class UserTableViewService {
  public async EnsureDefault(
    userId: string,
    page: string
  ): Promise<UserTableView | undefined>;
}

// Frontend store
class UserTableViewStore {
  // dedupes concurrent calls via in-flight promise Map
  async ensureDefault(userId: string, page: string): Promise<UserTableView>;
  // read-modify-write of viewConfig.filters
  async saveFiltersToDefault(
    userId: string,
    page: string,
    filterValues: Record<string, unknown> | undefined
  ): Promise<boolean>;
}

// Table.vue new emit
const emit = defineEmits<{
  // ...existing
  (e: "row-click", event: DataTableRowClickEvent): void;
}>();
```

```csharp
// Backend contract
public record EnsureDefaultRequest(Guid UserId, string Page);
public interface IUserTableViewService {
    Task<GenericResponse> EnsureDefault(EnsureDefaultRequest request);
}
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Manual | First visit creates default (1 row in DB) | DB inspect after fresh user navigates to `/budgets` |
| Manual | Concurrent `ensureDefault` → 1 row | Open `/budgets` in 2 tabs simultaneously |
| Manual | Row click saves filters | Apply filter → click row → inspect `viewConfig.filters` in DB |
| Manual | Clean row click does NOT call PUT | Network tab: no PUT on row click when no filter applied |
| Typecheck | `pnpm run typecheck` clean | CI gate |
| Build | `dotnet build` clean | CI gate |
| Regression | `Budgets.vue` works unchanged | Smoke test on `/budgets` |

## Migration / Rollout

No migration required. Pure additive:
- New backend endpoint, existing endpoints untouched
- New frontend service method + store actions, existing actions untouched
- New `@row-click` emit fires only when `page` is set; consumers without `page` use `v-bind="$attrs"` as before

Rollback: drop the new endpoint + remove the new store actions + revert `Table.vue` mount call and row-click handler. Three small PRs (backend, frontend store, frontend component) can be reverted independently.

## Open Questions

- **None blocking.** All design decisions have rationale; user has resolved the two behavioral forks (save target + save condition) up front.
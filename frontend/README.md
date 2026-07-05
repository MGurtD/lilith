# Lilith Frontend

> Part of the [Lilith monorepo](../README.md). Frontend-only docs — assume you already cloned and have prerequisites installed (see root README).

Vue 3 + TypeScript SPA. Domain-centric modules (sales, production, purchase, warehouse, …) on top of a shared component and service layer, centralized state via Pinia, deployed as a static bundle served by Nginx.

## Stack

| Layer | Choice |
|-------|--------|
| Framework | Vue 3 (Composition API, `<script setup>` only — never Options API) |
| Build | Vite |
| Language | TypeScript 5 (strict mode, no implicit `any`) |
| State | Pinia (global `src/store`, per-module `src/modules/*/store`) |
| Routing | Vue Router 4 — per-domain `routes.ts` aggregated at the root |
| UI | PrimeVue 4, PrimeFlex, PrimeIcons + base components under `src/components` |
| Validation | Yup + `FormValidation` helper, toast feedback |
| HTTP | Axios wrapped by `BaseService<T>` in `src/api` |
| Reporting | Blob-download helper for server-generated PDFs |
| Package manager | **pnpm v10+** (not npm/yarn) |

## Project layout

```
frontend/
├─ src/
│  ├─ api/                # Base service, Axios clients
│  ├─ assets/             # Styles, images, geography JSON
│  ├─ components/         # Base & shared components (inputs, tables, dialogs)
│  ├─ i18n/               # Translation sources
│  ├─ modules/
│  │  └─ <domain>/
│  │      ├─ routes.ts        # RouteRecordRaw[], lazy-loaded views
│  │      ├─ components/      # Domain-scoped components
│  │      ├─ services/        # API services extending BaseService<T>
│  │      ├─ store/           # Pinia stores
│  │      ├─ types/           # TypeScript interfaces
│  │      └─ views/           # Page components
│  ├─ services/           # Global services (auth, reports, user)
│  ├─ store/              # Global stores (auth, menu, filters, geography)
│  ├─ types/              # Shared interfaces
│  ├─ utils/              # getNewUuid, convertDateTimeToJSON, formatCurrency, …
│  └─ views/              # Top-level pages (Login, Home)
├─ .nginx/                # Nginx SPA fallback config
├─ Dockerfile             # Static Nginx image (consumes pre-built dist)
├─ docker-compose.yml     # Single-service orchestration
├─ vite.config.ts
└─ README.md
```

## Environment variables

Vite only exposes variables prefixed with `VITE_`. Create `.env`, `.env.development`, or `.env.preprod` per environment.

| Var | Purpose |
|-----|---------|
| `VITE_API_BASE_URL` | Main backend URL |
| `VITE_REPORTS_BASE_URL` | Reports microservice URL |
| `VITE_API_APP_NAME` | App name (cosmetic) |

Access in code: `import.meta.env.VITE_API_BASE_URL`. Each mode loads its own `.env.<mode>` automatically.

## Daily commands

```bash
pnpm install              # Install dependencies
pnpm run dev              # Dev server at http://localhost:8100
pnpm run typecheck        # vue-tsc --noEmit (no build artifacts)
pnpm run build            # Production build → dist/
pnpm run build-development # Dev-mode build → dist-test/
pnpm run build-preprod    # Preprod build → dist-preprod/
pnpm run preview          # Serve dist/ locally at http://localhost:4173
```

| Build command | Mode | Output |
|---------------|------|--------|
| `pnpm run build` | production | `dist/` |
| `pnpm run build-development` | development | `dist-test/` |
| `pnpm run build-preprod` | preprod | `dist-preprod/` |

## Conventions

### Components
- Always Composition API + `<script setup>`. Never Options API.
- PascalCase file names: `WorkOrderDetail.vue`.
- Keep components lean (≤ ~200 lines) — extract subcomponents when larger.
- Type emitted events explicitly:
  ```ts
  const emit = defineEmits<{
    (e: "saved", entity: MyEntity): void;
    (e: "cancelled"): void;
  }>();
  ```

### Naming
| Type | Convention | Example |
|------|------------|---------|
| Components | PascalCase | `WorkOrderDetail.vue` |
| Utilities | kebab-case | `form-validator.ts` |
| Pinia stores | `use<Entity>Store` | `useWorkOrderStore` |
| Services | `<Entity>Service` | `WorkOrderService` |
| Interfaces | PascalCase | `WorkOrder`, `PhaseDetail` |

### Imports
- Path alias `@/` → `./src/`. Always.
- Lazy-load every route view: `component: () => import('./views/MyView.vue')`.
- Group imports: Vue/external libs first, then internal modules.

### State (Pinia)
- Stores own state + orchestrate services.
- **Mutate state only inside actions.**
- After create/update/delete, re-fetch to sync UI.
- Use `setNew(id?)` to initialize blank entities with `getNewUuid()`.

### Dates
`Date.prototype.toJSON` is globally overridden — `JSON.stringify` converts `Date` objects to ISO automatically. This only works for native `Date`, **not strings**.

- **Store `GetById`**: convert API strings → `Date` objects immediately.
- **DatePicker model**: pass `Date` objects, never formatted strings.
- **Submit**: pass the entity directly. No manual conversion needed.
- **`formatDate()` is display-only** (columns, labels). Never set it on a DatePicker.
- **Don't mutate date fields on a reactive ref before `router.back()`** — causes `Cannot read properties of null (reading 'parentNode')` DOM crashes.

### UI
- PrimeVue components are globally registered.
- All user-facing text in **Catalan**.
- Toast severity: `info`, `success`, `warn`, `error` — match the situation.
- Clone objects before editing in dialogs: `const editModel = { ...original }`.
- Numeric defaults are `0`, not `undefined`.

## Adding a new domain module — checklist

1. Create `src/modules/<domain>/` with `components/ services/ store/ types/ views/` and `routes.ts`.
2. Define entity interfaces in `types/`.
3. Implement a service extending `BaseService<T>` (constructor takes the API resource name).
4. Build the Pinia store with CRUD actions + re-fetch after each write.
5. Add `routes.ts` with lazy-loaded views; ensure the root router aggregates it.
6. Build list and detail views using the existing form/dialog patterns.
7. Use `getNewUuid()` for client-only IDs.
8. Provide Catalan toast messages and labels.
9. If a report is needed, extend the report enum and follow the blob-download pattern.
10. Update `frontend/AGENTS.md` if conventions change.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| 404 on refresh in production | Missing SPA fallback in Nginx | Repo Nginx config uses `try_files` — ensure deployed config matches |
| Docker build fails (missing `dist`) | Built app before running `docker build` | Run `pnpm run build` first, or switch to a multi-stage Dockerfile |
| Env var not applied | Missing `VITE_` prefix or wrong `.env.<mode>` file | Prefix with `VITE_`; check `.env.<mode>` name |
| Type errors during build | Out-of-sync types or implicit `any` | `pnpm exec vue-tsc --noEmit`, fix definitions |
| `formatDate` breaks DatePicker | `formatDate()` returns a string, not a `Date` | Convert strings → `Date` in the store's `GetById` instead |
| Crash on `router.back()` after edit | Mutating reactive date field before navigation | Don't mutate date fields on a reactive ref — store owns the entity |

## Docker

The provided `Dockerfile` is a single-stage Nginx image — it expects a pre-built `dist` folder.

```bash
pnpm run build
docker build -t lilith-frontend .
docker run -p 9000:80 --name lilith-frontend lilith-frontend
# App served at http://localhost:9000
```

For multi-stage builds (build inside Docker) see the optional snippet in the previous version of this README, or switch `Dockerfile` to a two-stage `node:18-alpine` build + `nginx:alpine` runtime.

## Security notes

- JWT lives in state. If you ever persist it (localStorage, cookies), mark it `httpOnly` + `secure` and add refresh-token rotation.
- Always validate date filters before sending queries tied to the active exercise context.

## Contributing

1. Branch from `dev`.
2. `pnpm run build` must pass (typecheck + build).
3. Update `frontend/AGENTS.md` and any sibling help docs if you change conventions or add a new module.
4. Open PR → review → merge to `dev` → promote to `main`.

For deeper patterns (dialog CRUD, report downloads, exercise picker, auth flow) see [frontend/AGENTS.md](AGENTS.md).

## License

Internal project — all rights reserved.
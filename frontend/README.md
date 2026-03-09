# Lilith Frontend

> **Note**: This frontend is part of the [Lilith monorepo](https://github.com/MGurtD/lilith). For full system documentation, see the root README.

A modular Vue 3 + TypeScript SPA for the Lilith platform. It provides domain-centric modules (sales, production, purchase, warehouse, etc.), a shared component and service layer, centralized state via Pinia, and deployment through a pre-built static bundle served by Nginx.

---

## Key Concepts

- **Framework / Tooling**: Vue 3 (Composition API, `<script setup>`), Vite, TypeScript.
- **State**: Pinia stores (`src/store` for global, `src/modules/*/store` for domain stores).
- **Routing**: Vue Router 4; each domain exposes its own `routes.ts` aggregated by the root router.
- **UI Stack**: PrimeVue, PrimeFlex, PrimeIcons, plus custom base components under `src/components`.
- **Validation**: Yup + custom helper or manual checks with toast feedback (Catalan messages).
- **HTTP Layer**: Axios wrapped in reusable base + domain services in `src/api` and `src/modules/*/services`.
- **Reporting**: Report service downloads server-generated documents (blob → local download).
- **Internationalization**: Structure present (`src/i18n`), but many strings are still inline Catalan; maintain Catalan for consistency.
- **Build & Deploy**: `vite build` outputs `/dist` which the Docker image copies into an Nginx Alpine container.

---

## Directory Overview

```
├─ src/
│  ├─ api/                # Base service, API clients
│  ├─ assets/             # Static assets (styles, images, JSON geography)
│  ├─ components/         # Base & shared reusable components
│  ├─ i18n/               # Translation sources
│  ├─ modules/            # Domain modules (production, sales, etc.)
│  │   └─ <domain>/
│  │       ├─ routes.ts
│  │       ├─ components/
│  │       ├─ services/
│  │       ├─ store/
│  │       ├─ types/
│  │       └─ views/
│  ├─ store/              # Global (auth, menus, filters, geography)
│  ├─ types/              # Shared types/interfaces
│  ├─ utils/              # Helpers (uuid, date, currency, file download)
│  └─ views/              # Top-level simple pages (Login, Home, etc.)
├─ .nginx/                # Nginx config (SPA fallback)
├─ docker-compose.yml     # Container orchestration (expects pre-built dist)
├─ Dockerfile             # Static Nginx image (copies dist only)
├─ package.json
├─ vite.config.ts
└─ README.md
```

---

## Environment Variables

Vite exposes variables prefixed with `VITE_`. Configure them via a local `.env` (or `.env.development`, `.env.preprod`). Example:

```
VITE_API_APP_NAME=Lilith
```

Access in code with `import.meta.env.VITE_API_BASE_URL`.

---

## Prerequisites

- **Node.js**: Recommend LTS (≥ 18) for best compatibility with Vite 4.
- **Package Manager**: pnpm (recommended) - Install with `npm install -g pnpm`

---

## Install Dependencies

```powershell
pnpm install
```

---

## Run in Development

```powershell
pnpm run dev
```

Default dev server: http://localhost:8100

If you need different env values for dev, create `.env.development`.

---

## Type Check (Without Building)

```powershell
pnpm exec vue-tsc --noEmit
```

---

## Build Variants

| Command                      | Purpose                   | Output Directory | Mode                   |
| ---------------------------- | ------------------------- | ---------------- | ---------------------- |
| `pnpm run build`             | Production build          | `dist/`          | default (`production`) |
| `pnpm run build-development` | Dev-like build artifact   | `dist-test/`     | `development`          |
| `pnpm run build-preprod`     | Pre-production test build | `dist-preprod/`  | `preprod`              |

Each mode loads corresponding `.env.<mode>` if present.

---

## Preview a Production Build Locally

```powershell
pnpm run build
pnpm run preview
```

Preview server (default): http://localhost:4173

---

## Docker Usage

The provided `Dockerfile` expects a pre-built `dist` folder.

### Build & Run Manually

```powershell
pnpm run build
docker build -t lilith-frontend .
docker run -p 9000:80 --name lilith-frontend lilith-frontend
```

App served at http://localhost:9000

### Using docker-compose

`docker-compose.yml` defines a single service with environment variables. You must build the app first:

```powershell
npm run build
docker compose up --build
```

If you want the image to build the app itself, you would need a multi-stage Dockerfile (not currently implemented).

---

## Suggested Multi-Stage Dockerfile (Optional Improvement)

(Only if you choose to change deployment approach.)

```dockerfile
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY ./.nginx/nginx.conf /etc/nginx/nginx.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]
```

---

## Architecture Highlights

- **Separation by Domain**: Each module encapsulates its own UI, store, services, and types to reduce cross-domain coupling.
- **Service Abstraction**: Components call Pinia store actions; stores call service layer; services isolate HTTP details.
- **Reusability**: Base input, dropdown, table column components unify styling & patterns.
- **State Flow**: Global stores provide auth context, navigation menu state, and date/exercise filters consumed by views.
- **Validation & UX**: Consistent toast-based feedback; Catalan language throughout.

---

## Adding a New Domain Module (Checklist)

1. Create folder `src/modules/<domain>/` with subfolders: `components/ services/ store/ types/ views/` and `routes.ts`.
2. Define entity interfaces in `types/`.
3. Implement service class extending the base service.
4. Create Pinia store (CRUD actions, re-fetch patterns).
5. Add routes in `routes.ts` (lazy-loaded views).
6. Build list & detail views using existing form/dialog patterns.
7. Use `getNewUuid()` for client-only IDs; convert dates with `convertDateTimeToJSON` before submit.
8. Provide Catalan messages & toasts.
9. If report needed, extend report enum/constants & follow existing download pattern.
10. Register routes with the root (if not auto-imported).

---

## Common Scripts

| Script                      | Description                    |
| --------------------------- | ------------------------------ |
| `npm run dev`               | Start Vite dev server.         |
| `npm run build`             | Type check + production build. |
| `npm run build-development` | Build with development mode.   |
| `npm run build-preprod`     | Build with preprod mode.       |
| `npm run preview`           | Preview production output.     |

---

## Coding Guidelines (Summary)

- Use Composition API with `<script setup>`.
- Keep business logic in Pinia stores or services; keep components lean.
- Reuse utilities; do not duplicate date/currency formatting.
- Catalan UI text; unify toast messages.
- Round monetary values only at display.
- Null/undefined guard nested access; prefer explicit types.

---

## Troubleshooting

| Issue                             | Cause                                    | Fix                                                                         |
| --------------------------------- | ---------------------------------------- | --------------------------------------------------------------------------- |
| 404 on refresh in production      | Missing SPA fallback                     | Nginx config already uses `try_files`; ensure deployed config matches repo. |
| Docker build fails (missing dist) | `dist` not built before `docker build`   | Run `npm run build` first or switch to multi-stage Dockerfile.              |
| Env var not applied               | Missing VITE\_ prefix or wrong mode file | Prefix with `VITE_`; ensure correct `.env.<mode>` name.                     |
| Type errors during build          | Out-of-sync types or implicit any        | Run `npx vue-tsc --noEmit` locally; fix definitions.                        |

---

## Recommended Node / Dependency Updates

Periodically run:

```powershell
npx npm-check-updates -u
npm install
```

(Review breaking changes, especially PrimeVue & Vite major versions.)

---

## Security / Auth Notes

- JWT stored in state; ensure any future persistence (localStorage) is done securely.
- Always validate date filters before sending queries reliant on exercise context.

---

## Contributing

1. Create feature branch from `dev`.
2. Ensure `npm run build` passes and app launches locally.
3. Update documentation if adding new domain module or utilities.
4. Open PR → code review → merge into `dev` → eventual promotion to `main`.

---

## License

(Define project license here if applicable. Add a `LICENSE` file if missing.)

---

## Quick Start (TL;DR)

```powershell
# Clone & enter
git clone <repo-url>
cd lilith-frontend

# Install & run
pnpm install
pnpm run dev
# Visit http://localhost:8100

# Build production
pnpm run build

# Docker (after build)
docker compose up --build
```

---

For deeper architectural guidance, see `.github/copilot_instruction.md`.

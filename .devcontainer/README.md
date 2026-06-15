# Dev Containers

## Quick Start

### VS Code
1. Open the project in VS Code.
2. When prompted, click "Reopen in Container".
3. Wait for the container to build (first time ~2-3 min).
4. Backend: http://localhost:5000
5. Frontend: http://localhost:8100

### JetBrains Rider
1. Open the project in Rider.
2. **One-click run:** Select `Lilith Docker Compose` from the run configuration dropdown (top-right) and click Run ▶️
3. Or: Go to Services tool window → Docker → right-click `docker-compose.yml` → Run.
4. Or run from terminal: `docker compose up`

### Visual Studio
1. Open Developer Command Prompt.
2. Run: `docker compose up`
3. Or use Container Tools: right-click `docker-compose.yml` → Run with Docker Compose.

## Commands

```bash
# Start everything
docker compose up

# Start in background
docker compose up -d

# View logs
docker compose logs -f

# Stop everything
docker compose down

# Rebuild after changes to Dockerfiles
docker compose build --no-cache
```

## Services

| Service   | Port | Description                    |
|-----------|------|--------------------------------|
| backend   | 5000 | .NET 10 API with hot reload    |
| frontend  | 8100 | Vue 3 + Vite dev server        |

## Database

The development database runs on an external VPS.
Connection string is configured via the `.env` file (see `DATABASE_CONNECTION_STRING`).

## Environment Variables

### Backend
- `ASPNETCORE_ENVIRONMENT=Development`
- `ConnectionStrings__Default` - External PostgreSQL VPS (configured in `.env`)
- `DOTNET_USE_POLLING_FILE_WATCHER=1` (required for file watching in Docker bind mounts)

### Frontend
- `NODE_ENV=development`
- `VITE_API_BASE_URL=http://localhost:5000/api` (overrides .env.local)
- `CHOKIDAR_USEPOLLING=1` (required for file watching in Docker bind mounts on Windows)

## Volumes

- `backend-nuget` - NuGet package cache
- `pnpm-store` - pnpm store cache
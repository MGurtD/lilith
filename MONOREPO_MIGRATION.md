# Monorepo Migration Status

## ✅ Completed Steps

1. **Analyzed current structure** - Verified both repos are independent with separate remotes
2. **Created backup branches** - Both repos have `monorepo-backup-before-merge` branches
3. **Removed nested .git directories** - Eliminated `lilith-backend/.git/` and `lilith-frontend/.git/`
4. **Created fresh root repository** - Initialized clean git repo at root
5. **Initial commit** - Committed initial monorepo state with all code
6. **Updated .gitignore** - Refined for monorepo structure
7. **Updated VS Code workspace** - Modified to reference `frontend/` and `backend/`
8. **Created CI/CD workflows** - Both workflows support old and new directory names
9. **Created docker-compose.yml** - Orchestrates backend, frontend, and PostgreSQL
10. **Created .env.example** - Template for environment variables
11. **Created README.md** - Comprehensive monorepo documentation
12. **Updated AGENTS.md** - References both old and new directory structures

## 🚧 Pending: Manual Directory Rename

The directories `lilith-backend/` and `lilith-frontend/` are currently **locked** (likely by VS Code, dev servers, or other processes) and cannot be renamed programmatically.

### Steps to Complete the Rename:

1. **Close all applications accessing the project:**
   - Close VS Code completely
   - Stop all dev servers (`dotnet watch`, `pnpm dev`, etc.)
   - Stop any Docker containers if running
   - Close any file explorers browsing these directories

2. **Rename the directories manually:**

   **Option A: Using File Explorer (Windows)**
   - Navigate to `C:\Users\mgurt\source\personal\lilith`
   - Rename `lilith-backend` to `backend`
   - Rename `lilith-frontend` to `frontend`

   **Option B: Using PowerShell (if files are no longer locked)**
   ```powershell
   cd C:\Users\mgurt\source\personal\lilith
   Rename-Item -Path "lilith-backend" -NewName "backend"
   Rename-Item -Path "lilith-frontend" -NewName "frontend"
   ```

3. **Commit the rename:**
   ```bash
   git add -A
   git commit -m "refactor: rename directories to backend/ and frontend/"
   ```

4. **Reopen VS Code:**
   ```bash
   code lilith.code-workspace
   ```

## 📋 Next Steps After Rename

### 1. Test Local Builds

**Backend:**
```bash
cd backend
dotnet restore
dotnet build
dotnet ef database update --project src/Infrastructure/
dotnet run --project src/Api/
```

**Frontend:**
```bash
cd frontend
pnpm install
pnpm run typecheck
pnpm run build
pnpm run dev
```

### 2. Test Docker Compose

```bash
cp .env.example .env
# Edit .env with real values:
# - POSTGRES_USER
# - POSTGRES_PASSWORD
# - POSTGRES_DB
# - VITE_API_BASE_URL
# - VITE_REPORTS_BASE_URL
# - etc.

docker-compose up -d
```

Verify services:
- Backend API: http://localhost:5000
- Frontend: http://localhost:8080
- PostgreSQL: localhost:5432

### 3. Create GitHub Repository

The monorepo needs a new unified GitHub repository.

**Option A: Create new repo (recommended)**
```bash
# Create new repo on GitHub (e.g., MGurtD/lilith)
git remote add origin https://github.com/MGurtD/lilith.git
git push -u origin master
```

**Option B: Repurpose existing backend repo**
```bash
# Update existing remote
git remote add origin https://github.com/MGurtD/lilith-backend.git
git push -u origin master --force  # ⚠️ This will overwrite history
```

### 4. Configure GitHub Secrets

For CI/CD workflows to work, configure these secrets in GitHub repository settings:

**Docker Hub:**
- `DOCKER_USERNAME`
- `DOCKER_PASSWORD`

**Deployment:**
- `DEV_SERVER_HOST`
- `DEV_SERVER_USERNAME`
- `DEV_SERVER_SSH_KEY`

**Frontend environment variables:**
- `VITE_API_APP_NAME`
- `VITE_API_BASE_URL`
- `VITE_REPORTS_BASE_URL`
- `VITE_API_ACTIONS_URL`

### 5. Update Original Repositories (Optional)

Add deprecation notices to the original repos:

**lilith-backend README:**
```markdown
# ⚠️ REPOSITORY ARCHIVED

This repository has been merged into the unified monorepo: https://github.com/MGurtD/lilith

Please use the monorepo for all future development.
```

**lilith-frontend README:**
```markdown
# ⚠️ REPOSITORY ARCHIVED

This repository has been merged into the unified monorepo: https://github.com/MGurtD/lilith

Please use the monorepo for all future development.
```

### 6. Update Remote Server docker-compose (If Applicable)

If you have a remote server running docker-compose, update its directory structure:

```bash
ssh user@server
cd ~/lilith
# Backup current setup
cp docker-compose.yml docker-compose.yml.backup

# Pull new monorepo structure
git pull

# If directories were renamed, update docker-compose.yml service paths
# Then restart services
docker-compose down
docker-compose up -d
```

## 🔍 Verification Checklist

After completing the rename and setup:

- [ ] Directories are named `backend/` and `frontend/`
- [ ] Backend builds successfully with `dotnet build`
- [ ] Frontend builds successfully with `pnpm run build`
- [ ] Database migrations apply correctly
- [ ] Backend Swagger UI loads at https://localhost:5001/swagger
- [ ] Frontend dev server runs at http://localhost:8100
- [ ] Docker Compose starts all services successfully
- [ ] CI/CD workflows trigger correctly on GitHub
- [ ] VS Code workspace loads both projects correctly

## 📊 Repository Structure (After Rename)

```
lilith/
├── .git/                          # Unified Git repository
├── .github/
│   └── workflows/
│       ├── backend-ci.yml         # Backend CI/CD
│       └── frontend-ci.yml        # Frontend CI/CD
├── .opencode/                     # OpenCode configuration
│   └── skills/                    # Task-specific guides
├── backend/                       # .NET 10 backend (formerly lilith-backend)
│   ├── src/
│   │   ├── Api/
│   │   ├── Application/
│   │   ├── Application.Contracts/
│   │   ├── Domain/
│   │   └── Infrastructure/
│   ├── docs/                      # Architecture documentation
│   └── Dockerfile
├── frontend/                      # Vue 3 frontend (formerly lilith-frontend)
│   ├── src/
│   │   ├── modules/               # Domain modules
│   │   ├── router/
│   │   ├── services/
│   │   ├── store/
│   │   └── types/
│   ├── public/
│   └── Dockerfile
├── .env.example                   # Environment variables template
├── .gitignore                     # Monorepo gitignore
├── docker-compose.yml             # Full stack orchestration
├── lilith.code-workspace          # VS Code multi-root workspace
├── AGENTS.md                      # AI coding guidelines
├── README.md                      # Monorepo documentation
└── MONOREPO_MIGRATION.md         # This file
```

## 🐛 Troubleshooting

### "Cannot rename directory: Device or resource busy"

**Cause**: Files are locked by another process.

**Solutions:**
1. Close VS Code completely (not just files)
2. Stop all terminal processes (Ctrl+C on any running servers)
3. Check Task Manager for any `node`, `dotnet`, or `Code.exe` processes
4. Restart your computer if issue persists
5. Use File Explorer to rename manually

### Git shows massive changes after rename

This is expected. Git will show the rename as:
- Delete all files in `lilith-backend/`
- Add all files in `backend/`

Just commit with: `git add -A && git commit -m "refactor: rename directories"`

### CI/CD workflows not triggering

The workflows are configured to trigger on changes to **either** `backend/**` OR `lilith-backend/**` (same for frontend). This ensures they work during the transition period.

After the rename is committed, the workflows will automatically detect the new paths.

### Docker Compose fails after rename

Update the `docker-compose.yml` file to reference the new paths:
```yaml
services:
  lilith-backend:
    build:
      context: ./backend  # Changed from ./lilith-backend
```

## 📞 Support

If you encounter issues during the migration:

1. Check git status: `git status`
2. Check for locked files: `ls -la` (look for unexpected file handles)
3. Verify directory structure matches expected layout
4. Ensure all original code is present and unchanged
5. Test builds locally before pushing to remote

---

**Migration initiated**: 2026-02-15  
**Current status**: Ready for manual directory rename  
**Commits made**: 5 commits creating unified monorepo structure

# Plan de Implementación: Automatización OpenCode para Lilith ERP

**Actualizado**: 19 Feb 2026 (revisión de la arquitectura original del 15 Feb 2026)  
**Objetivo**: Sistema de agentes especializados con lazy-loading de skills para reducir tiempo en tareas repetitivas

---

## Arquitectura implementada

```
Build (Primary) — orquestador por defecto
  ├── @backend (subagent) → skills: adding-backend-entity, backend-localization, git-commits
  └── @frontend (subagent) → skills: adding-frontend-entity, frontend-patterns, git-commits
```

**Decisiones tomadas:**
- 2 subagentes en lugar de 4 (se eliminaron `fullstack` y `reviewer` para simplificar)
- Sin commands (el equipo prefirió agentes directos sobre atajos `/comando`)
- Lazy-loading de skills: los agentes cargan instrucciones detalladas solo cuando las necesitan
- Modelo: `github-copilot/claude-sonnet-4.6` para ambos subagentes
- `Build` actúa de orquestador sin configuración adicional (ya tiene acceso a `task` tool)

---

## Estado de implementación

### ✅ Completado

#### Agentes (`.opencode/agents/`)
- [x] `backend.md` — Especialista .NET 10 + Clean Architecture (~120 líneas)
- [x] `frontend.md` — Especialista Vue 3 + TypeScript (~160 líneas)

#### Skills (`.opencode/skills/`)
- [x] `adding-backend-entity/SKILL.md` — Refactorizado de 1044 → ~500 líneas
- [x] `adding-backend-entity/references/SCENARIOS.md` — 4 escenarios detallados extraídos del skill principal
- [x] `adding-frontend-entity/SKILL.md` — Nuevo skill, workflow de 10 pasos
- [x] `adding-frontend-entity/references/COMPONENT_TEMPLATES.md` — Templates copy-paste
- [x] `frontend-patterns/SKILL.md` — Nuevo skill, 7 patrones UI
- [x] `frontend-patterns/references/PRIMEVUE_PATTERNS.md` — Componentes PrimeVue 4
- [x] `frontend-patterns/references/SERVICE_PATTERNS.md` — Patrones de service layer
- [x] `backend-localization/SKILL.md` — Existente (sin cambios)
- [x] `git-commits/SKILL.md` — Expandido con scopes del proyecto Lilith

#### Configuración raíz
- [x] `opencode.json` — `permission.task` configurado: Build puede invocar `@backend` y `@frontend`

---

## Fase 2: Pendiente (si se decide implementar)

### Commands (`.opencode/commands/`)
Referencia: ver sección "Fase 1" del plan original para las especificaciones detalladas de cada command.

- [ ] `/add-entity <EntityName>` — Invoca @backend con skill adding-backend-entity
- [ ] `/new-migration <MigrationName>` — Crea migración EF Core
- [ ] `/build-backend` — Compila backend y reporta errores
- [ ] `/add-frontend-crud <EntityName>` — Invoca @frontend con skill adding-frontend-entity
- [ ] `/sync-types <EntityName>` — Sincroniza interfaces TS con entidades C#
- [ ] `/build-frontend` — Type check + build frontend
- [ ] `/commit` — Genera commit convencional con skill git-commits
- [ ] `/deploy-dev` — Push a rama dev

### Agentes adicionales (si se necesitan)
- [ ] `reviewer.md` — Revisión de código read-only (spec completa en plan original)

---

## Guía de uso actual

### Añadir una entidad nueva al backend:
```
@backend añade la entidad Warehouse con CRUD completo
```
El agente cargará el skill `adding-backend-entity` automáticamente.

### Implementar CRUD en frontend:
```
@frontend implementa la vista y store para la entidad Warehouse
```
El agente cargará `adding-frontend-entity` o `frontend-patterns` según el caso.

### Feature completa (backend + frontend):
```
Necesito añadir la entidad Warehouse con backend y frontend completos
```
Build delegará a @backend y luego a @frontend en secuencia.

### Commit convencional:
```
@backend (o @frontend) prepara el commit de estos cambios
```
El agente cargará `git-commits` para el mensaje correcto.

---

## Referencias

- Agents docs: `C:\Users\mgurt\source\personal\lilith\.opencode\agents\`
- Skills docs: `C:\Users\mgurt\source\personal\lilith\.opencode\skills\`
- Config: `C:\Users\mgurt\source\personal\lilith\opencode.json`
- AGENTS.md del proyecto: `C:\Users\mgurt\source\personal\lilith\AGENTS.md`

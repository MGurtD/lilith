# Plan de Implementación: Automatización OpenCode para Lilith ERP

**Fecha**: 15 Feb 2026  
**Objetivo**: Automatizar workflows repetitivos con Commands y Agents especializados

---

## 📋 Resumen Ejecutivo

Este plan implementa automatización mediante:
- **8 Commands**: Atajos `/comando` para tareas frecuentes
- **4 Agents**: Asistentes especializados por dominio (backend, frontend, fullstack, reviewer)

**Beneficio esperado**: Reducir tiempo en tareas repetitivas ~60% mediante workflows predefinidos.

---

## 🎯 Fase 1: Commands (Alta prioridad)

### 1.1 Backend Commands

#### `/add-entity <EntityName>`
**Archivo**: `.opencode/commands/add-entity.md`

```markdown
---
description: Añadir nueva entidad al backend con CRUD completo
agent: backend
model: anthropic/claude-sonnet-4-20250514
---
Crea la entidad "$ARGUMENTS" en el backend siguiendo el skill "adding-backend-entity".

Workflow:
1. Carga el skill "adding-backend-entity"
2. Determina si usar Pattern A (generic repo) o Pattern B (specific repo)
3. Ejecuta TODOS los pasos: Domain -> Contracts -> Application -> Infrastructure -> Api
4. Al finalizar, ejecuta el script de validación: `python .opencode/skills/adding-backend-entity/scripts/validate_entity.py $ARGUMENTS`
5. Si hay errores, corrígelos y vuelve a validar
6. Pregunta: "¿Quieres que genere también el frontend (service, store, types, vista)?"
```

#### `/new-migration <MigrationName>`
**Archivo**: `.opencode/commands/new-migration.md`

```markdown
---
description: Crear nueva migración EF Core
agent: backend
---
Crea una nueva migración EF Core llamada "$ARGUMENTS".

Pasos:
1. Revisa los cambios en las entidades desde la última migración
2. Ejecuta: `dotnet ef migrations add $ARGUMENTS --project src/Infrastructure/ --startup-project src/Api/`
3. Muestra el resumen de la migración generada
4. Ejecuta: `dotnet ef database update --project src/Infrastructure/ --startup-project src/Api/`
5. Verifica que la migración se aplicó correctamente
```

#### `/build-backend`
**Archivo**: `.opencode/commands/build-backend.md`

```markdown
---
description: Build del backend con análisis de errores
agent: backend
subtask: false
---
Compila el backend y muestra errores si los hay.

Pasos:
1. Ejecuta: `dotnet build` desde `lilith-backend/`
2. Si hay errores de compilación, analízalos y sugiere correcciones
3. Si hay warnings, lista los más importantes
4. Reporta el resultado final
```

### 1.2 Frontend Commands

#### `/add-frontend-crud <EntityName>`
**Archivo**: `.opencode/commands/add-frontend-crud.md`

```markdown
---
description: Genera CRUD completo en frontend para una entidad
agent: frontend
model: anthropic/claude-sonnet-4-20250514
---
Crea el CRUD completo en el frontend para la entidad "$ARGUMENTS".

Workflow:
1. Determina el módulo correcto (sales, purchase, production, warehouse, shared)
2. Crea/actualiza la interface TypeScript en `modules/<modulo>/types/`
3. Crea el service extendiendo BaseService en `modules/<modulo>/services/`
4. Crea el store Pinia en `modules/<modulo>/stores/`
5. Crea la vista con PrimeVue DataTable en `modules/<modulo>/views/`
6. Añade la ruta en `modules/<modulo>/router.ts`
7. Verifica que todos los archivos usan:
   - Composition API con `<script setup>`
   - PascalCase en nombres de componentes
   - Imports con alias `@/`
   - Catalan en todos los textos UI
```

#### `/sync-types <EntityName>`
**Archivo**: `.opencode/commands/sync-types.md`

```markdown
---
description: Sincroniza tipos TypeScript con entidad C# del backend
agent: fullstack
---
Compara y sincroniza la entidad C# "$ARGUMENTS" con su interface TypeScript.

Pasos:
1. Lee la entidad C# en `lilith-backend/src/Domain/Entities/**/$ARGUMENTS.cs`
2. Lee la interface TS en `lilith-frontend/src/modules/**/types/index.ts`
3. Identifica diferencias:
   - Propiedades añadidas/eliminadas
   - Cambios de tipo
   - Cambios de nullable
4. Si hay diferencias, pregunta si actualizar el frontend
5. Actualiza la interface TypeScript aplicando la convención:
   - `Guid` -> `string`
   - `DateTime` -> `any` (con comentario para convertir con `convertDateTimeToJSON`)
   - `decimal`/`int` -> `number`
   - PascalCase -> camelCase en nombres de propiedades
```

#### `/build-frontend`
**Archivo**: `.opencode/commands/build-frontend.md`

```markdown
---
description: Build del frontend con type checking
agent: frontend
subtask: false
---
Compila el frontend y ejecuta type checking.

Pasos:
1. Ejecuta desde `lilith-frontend/`: `pnpm run typecheck`
2. Si hay errores de tipos, analízalos y sugiere correcciones
3. Ejecuta: `pnpm run build`
4. Reporta el resultado final
```

### 1.3 Fullstack Commands

#### `/commit`
**Archivo**: `.opencode/commands/commit.md`

```markdown
---
description: Genera commit convencional analizando cambios
agent: build
---
Analiza los cambios y genera un commit message convencional.

Pasos:
1. Ejecuta: `git status` para ver archivos modificados
2. Ejecuta: `git diff` para ver los cambios
3. Determina el tipo de commit según el skill "git-commits":
   - feat: nueva funcionalidad
   - fix: corrección de bug
   - refactor: refactorización sin cambio funcional
   - docs: cambios en documentación
   - chore: cambios en build, CI, o herramientas
4. Genera un mensaje conciso en formato: `tipo(scope): descripción`
5. Ejecuta: `git add .`
6. Ejecuta: `git commit -m "mensaje-generado"`
7. Muestra el resultado del commit
```

#### `/deploy-dev`
**Archivo**: `.opencode/commands/deploy-dev.md`

```markdown
---
description: Deploy a entorno dev (backend y/o frontend)
agent: build
subtask: false
---
Despliega los cambios al entorno de desarrollo.

Pasos:
1. Pregunta qué desplegar: backend, frontend, o ambos
2. Para backend:
   - Verifica branch `dev`: `git branch --show-current`
   - Push: `git push origin dev`
   - Indica que el CI se encargará del deploy automático
3. Para frontend:
   - Verifica branch `dev`: `git branch --show-current`
   - Push: `git push origin dev`
   - Indica que el CI se encargará del deploy automático
4. Muestra los URLs de los workflows de GitHub Actions
```

---

## 🤖 Fase 2: Agents Especializados

### 2.1 Backend Agent

**Archivo**: `.opencode/agents/backend.md`

```markdown
---
description: Especialista en backend .NET 10 Clean Architecture de Lilith ERP
mode: subagent
model: anthropic/claude-sonnet-4-20250514
temperature: 0.2
tools:
  write: true
  edit: true
  bash: true
permission:
  skill:
    "adding-backend-entity": allow
    "backend-localization": allow
  bash:
    "*": ask
    "dotnet *": allow
    "git status": allow
    "git diff*": allow
---
Eres un experto en el backend de Lilith ERP, un sistema ERP de manufactura construido con .NET 10 y Clean Architecture.

## Conocimiento del dominio

**Arquitectura**: 6 capas (Domain, Application.Contracts, Application, Infrastructure, Api, Verifactu)
**Base de datos**: PostgreSQL 16+ con EF Core
**Localización**: Catalan (primario), Spanish, English vía ILocalizationService

## Convenciones OBLIGATORIAS

### Código C# 12
- **Primary constructors**: `public class BudgetService(IUnitOfWork unitOfWork, ILocalizationService localization)`
- **SIEMPRE async/await** para operaciones I/O
- **SIEMPRE inyecta ILocalizationService** en servicios
- **NUNCA hardcodees strings de estado/lifecycle** - usa `StatusConstants`
- **SIEMPRE devuelve GenericResponse** en operaciones de escritura
- **Nullable reference types**: `string Name { get; set; } = string.Empty;` o `string? Description { get; set; }`

### Responsabilidades por capa
- **Controllers**: Solo validación HTTP, NUNCA lógica de negocio
- **Services**: TODA la lógica de negocio + localización + orquestación
- **Repositories**: Solo acceso a datos, queries EF Core con `Include()` para relaciones

### Naming
- Services: `IBudgetService`, `BudgetService`
- Repositories: `IBudgetRepository`, `BudgetRepository`
- Controllers: `BudgetController`
- Entities: PascalCase (`SalesOrderHeader`, `WorkOrder`)

## Flujo de trabajo típico

1. Verifica que entiendes el dominio (Sales, Purchase, Production, Warehouse)
2. Sigue el patrón Controller -> Service -> Repository
3. Usa los skills "adding-backend-entity" y "backend-localization" cuando aplique
4. Valida con el script `.opencode/skills/adding-backend-entity/scripts/validate_entity.py`
5. SIEMPRE ejecuta `dotnet build` antes de finalizar

## Anti-patterns a evitar

❌ Lógica de negocio en controllers
❌ Strings hardcodeados para errores (usa ILocalizationService)
❌ Magic strings para lifecycle/status (usa StatusConstants)
❌ IUnitOfWork directamente en controllers (usa interfaces de servicio)
❌ Mezclar idiomas en mensajes
❌ Devolver `bool` en lugar de `GenericResponse`
```

### 2.2 Frontend Agent

**Archivo**: `.opencode/agents/frontend.md`

```markdown
---
description: Especialista en frontend Vue 3 + TypeScript de Lilith ERP
mode: subagent
model: anthropic/claude-sonnet-4-20250514
temperature: 0.2
tools:
  write: true
  edit: true
  bash: true
permission:
  bash:
    "*": ask
    "pnpm *": allow
    "git status": allow
    "git diff*": allow
---
Eres un experto en el frontend de Lilith ERP, un sistema ERP de manufactura construido con Vue 3, TypeScript, Pinia, PrimeVue y Vite.

## Conocimiento del dominio

**Stack**: Vue 3 + Composition API, TypeScript, Pinia, PrimeVue 4.5, Axios, Vite 6
**Estructura**: Módulos por dominio (sales, purchase, production, warehouse, shared, analytics, plant, verifactu)
**Idioma**: Catalan (todos los textos UI)

## Convenciones OBLIGATORIAS

### TypeScript + Vue 3
- **SIEMPRE Composition API** con `<script setup>` (NUNCA Options API)
- **SIEMPRE PascalCase** en archivos de componentes: `WorkOrderDetail.vue`
- **SIEMPRE path alias** `@/` para imports: `import { useStore } from "@/store"`
- **SIEMPRE lazy-load routes**: `component: () => import('./views/WorkOrder.vue')`
- **SIEMPRE normaliza fechas** antes de API calls: `convertDateTimeToJSON(date)`
- **SIEMPRE Catalan** en todos los mensajes y labels UI

### Patrones arquitecturales
- Services extienden `BaseService<T>` con CRUD heredado
- Pinia stores gestionan estado + orquestan llamadas a servicios
- Clonar objetos antes de editar en dialogs: `const editModel = { ...original }`
- Defaults numéricos son `0`, no `undefined`

### Naming
- Componentes: PascalCase (archivos e imports)
- Stores: `useWorkOrderStore`, `useBudgetStore`
- Services: `WorkOrderService`, `BudgetService`
- Funciones: camelCase

## Flujo de datos

```
Component → Store → Service → API
    ↓        ↓        ↓
   UI     State    HTTP
Rendering  Mgmt   (Axios)
```

## Estructura típica de feature

```
modules/sales/
  types/index.ts        # Interfaces TypeScript
  services/            # Servicios que extienden BaseService
  stores/              # Pinia stores
  views/               # Páginas con PrimeVue components
  router.ts            # Rutas del módulo
```

## Flujo de trabajo típico

1. Identifica el módulo correcto según el dominio
2. Crea/actualiza interfaces TypeScript en `types/`
3. Crea service extendiendo `BaseService<T>`
4. Crea Pinia store que orquesta el service
5. Crea vista con componentes PrimeVue
6. Añade ruta en el router del módulo
7. SIEMPRE ejecuta `pnpm run typecheck` antes de finalizar

## Anti-patterns a evitar

❌ Options API (solo Composition API con `<script setup>`)
❌ Mutar store state fuera de actions
❌ Hard-coding API URLs (usa service layer)
❌ Usar tipo `any`
❌ Olvidar normalizar fechas antes de API calls
❌ Mezclar idiomas (mantener Catalan)
```

### 2.3 Fullstack Agent

**Archivo**: `.opencode/agents/fullstack.md`

```markdown
---
description: Especialista fullstack para features que tocan backend y frontend
mode: subagent
model: anthropic/claude-sonnet-4-20250514
temperature: 0.3
tools:
  write: true
  edit: true
  bash: true
permission:
  skill:
    "*": allow
  bash:
    "*": ask
    "dotnet *": allow
    "pnpm *": allow
    "git *": allow
---
Eres un experto fullstack en Lilith ERP que puede trabajar tanto en backend (.NET 10) como frontend (Vue 3).

## Tu misión

Implementar features completos end-to-end que requieren cambios coordinados en backend y frontend.

## Workflow típico

### Para añadir una feature completa:

1. **Backend primero**:
   - Invoca el agent `@backend` para cambios en .NET
   - O usa el command `/add-entity` si es una entidad nueva
   - Verifica: `dotnet build`

2. **Frontend después**:
   - Invoca el agent `@frontend` para cambios en Vue
   - O usa el command `/add-frontend-crud` si es CRUD
   - Sincroniza tipos con `/sync-types`
   - Verifica: `pnpm run typecheck`

3. **Validación**:
   - Comprueba que los tipos están sincronizados
   - Verifica que las rutas de API coinciden
   - Revisa que la localización es consistente

## Sincronización de tipos crítica

Cuando cambies una entidad en backend, SIEMPRE actualiza el frontend:

**Backend** (`Domain/Entities/Sales/Budget.cs`):
```csharp
public class Budget : Entity {
    public string Number { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}
```

**Frontend** (`modules/sales/types/index.ts`):
```typescript
export interface Budget {
    id: string;           // Guid -> string
    number: string;
    date: any;            // DateTime -> any (usa convertDateTimeToJSON)
    amount: number;       // decimal -> number
}
```

## Reglas de conversión C# → TypeScript

- `Guid` → `string`
- `DateTime` / `DateTime?` → `any`
- `decimal` / `int` / `double` → `number`
- `bool` → `boolean`
- `ICollection<T>` → `Array<T>`
- PascalCase → camelCase (en nombres de propiedades)

## Delegates work effectively

Cuando una tarea es puramente backend o puramente frontend, delega al agent especializado apropiado en lugar de intentar hacerlo todo tú mismo.
```

### 2.4 Reviewer Agent

**Archivo**: `.opencode/agents/reviewer.md`

```markdown
---
description: Revisa código sin modificar, enfocado en calidad y mejores prácticas
mode: subagent
model: anthropic/claude-sonnet-4-20250514
temperature: 0.1
tools:
  write: false
  edit: false
  bash: true
permission:
  bash:
    "*": deny
    "git diff*": allow
    "git log*": allow
    "git status": allow
---
Eres un revisor de código experto en Lilith ERP. Tu rol es analizar código y sugerir mejoras SIN hacer cambios directos.

## Tu enfoque

1. **Lee y analiza** el código usando herramientas de solo lectura
2. **Identifica problemas** de:
   - Seguridad
   - Performance
   - Mantenibilidad
   - Adherencia a convenciones del proyecto
   - Posibles bugs
3. **Sugiere mejoras** con explicaciones claras
4. **NO hagas cambios** - deja que el usuario o otro agent implemente

## Checklist de revisión

### Backend (.NET)
- ✅ Usa primary constructors
- ✅ Inyecta ILocalizationService
- ✅ Usa StatusConstants (no magic strings)
- ✅ Devuelve GenericResponse
- ✅ Separa responsabilidades por capa
- ✅ Todas las operaciones I/O son async
- ✅ Nullable reference types correctos

### Frontend (Vue 3)
- ✅ Usa Composition API con `<script setup>`
- ✅ PascalCase en componentes
- ✅ Path alias `@/` en imports
- ✅ Normaliza fechas antes de API calls
- ✅ Textos en Catalan
- ✅ No hay tipos `any` innecesarios
- ✅ Stores no se mutan fuera de actions

### General
- ✅ Sin código duplicado
- ✅ Nombres descriptivos
- ✅ Sin dead code
- ✅ Manejo de errores apropiado
- ✅ Sin console.log olvidados

## Formato de feedback

Estructura tus reviews así:

**🔴 Crítico** (debe arreglarse):
- Descripción del problema
- Por qué es crítico
- Sugerencia de solución

**🟡 Recomendado** (mejora la calidad):
- Descripción del issue
- Beneficio de arreglarlo
- Sugerencia de solución

**🟢 Opcional** (nice to have):
- Mejora menor
- Beneficio potencial
```

---

## 📝 Checklist de Implementación

### Fase 1: Commands (2-3 horas)
- [ ] Crear directorio `.opencode/commands/`
- [ ] `/add-entity`
- [ ] `/new-migration`
- [ ] `/build-backend`
- [ ] `/add-frontend-crud`
- [ ] `/sync-types`
- [ ] `/build-frontend`
- [ ] `/commit`
- [ ] `/deploy-dev`

### Fase 2: Agents (1-2 horas)
- [ ] Crear directorio `.opencode/agents/`
- [ ] Agent `backend`
- [ ] Agent `frontend`
- [ ] Agent `fullstack`
- [ ] Agent `reviewer`

### Fase 3: Testing (1 hora)
- [ ] Probar cada command con casos reales
- [ ] Probar cada agent con @mention
- [ ] Ajustar prompts según resultados
- [ ] Documentar en AGENTS.md el uso de commands/agents

---

## 🎓 Guía de Uso Post-Implementación

### Para añadir una entidad nueva completa:
```
/add-entity Product
# Cuando termine, si pregunta por frontend:
Sí, genera el frontend
```

### Para sincronizar tipos después de cambio en backend:
```
/sync-types Budget
```

### Para revisar código antes de commit:
```
@reviewer revisa los cambios en el módulo sales
```

### Para deploy rápido a dev:
```
/deploy-dev
```

### Para commit convencional automático:
```
/commit
```

---

## 📚 Referencias

- Documentación OpenCode Commands: https://opencode.ai/docs/commands
- Documentación OpenCode Agents: https://opencode.ai/docs/agents
- Skills existentes: `.opencode/skills/`
- AGENTS.md del proyecto: `C:\Users\mgurt\source\personal\lilith\AGENTS.md`

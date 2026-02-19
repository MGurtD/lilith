---
name: git-commits
description: Generate commit messages following conventional commit format. Use when the user asks for help with git commits.
---

# Conventional Commits — Lilith ERP

Format: `type(scope): description`

---

## Tipos

| Tipo | Cuándo usarlo |
|------|--------------|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de bug |
| `docs` | Solo documentación |
| `refactor` | Reestructuración sin cambio funcional |
| `test` | Añadir o corregir tests |
| `chore` | Build, CI, herramientas, dependencias |
| `perf` | Mejora de rendimiento |
| `style` | Formato, espacios, comas (sin lógica) |

---

## Scopes del proyecto

### Backend
| Scope | Qué cubre |
|-------|-----------|
| `sales` | Presupuestos, pedidos de venta, facturas |
| `purchase` | Pedidos de compra, proveedores |
| `production` | Órdenes de fabricación, work orders |
| `warehouse` | Almacenes, inventario, movimientos de stock |
| `shared` | Entidades compartidas entre módulos (clientes, materiales) |
| `infra` | EF Core, migraciones, configuración de BD |
| `auth` | Autenticación, autorización, JWT |
| `localization` | Archivos de traducción (ca/es/en), ILocalizationService |
| `api` | Configuración del proyecto API, middleware, DI |

### Frontend
| Scope | Qué cubre |
|-------|-----------|
| `frontend` | Cambios generales de frontend |
| `sales` | Módulo sales (vistas, stores, services) |
| `purchase` | Módulo purchase |
| `production` | Módulo production |
| `warehouse` | Módulo warehouse |
| `shared` | Módulo shared (componentes, tipos compartidos) |
| `analytics` | Módulo analytics / dashboards |
| `plant` | Módulo plant |
| `router` | Configuración de rutas |
| `store` | Stores Pinia transversales |

### Transversal
| Scope | Qué cubre |
|-------|-----------|
| `config` | opencode.json, AGENTS.md, .opencode/ |
| `ci` | GitHub Actions, pipelines |
| `deps` | Actualización de dependencias |

---

## Reglas

1. **Descripción en infinitivo**, en el idioma del proyecto (español para el equipo, inglés también aceptado)
2. **Máximo 72 caracteres** en la línea del subject
3. **Sin punto final** en el subject
4. Si el cambio afecta backend Y frontend a la vez, usa el scope del área de mayor impacto o `shared`
5. Los breaking changes se marcan con `!` antes de los dos puntos: `feat(sales)!: cambiar API de presupuestos`

---

## Ejemplos

```
feat(sales): añadir endpoint de duplicación de presupuesto
fix(production): corregir cálculo de cantidad en work orders
refactor(shared): extraer lógica de validación a servicio base
feat(frontend/sales): implementar vista de detalle de pedido
fix(warehouse): resolver error de stock negativo en movimientos
chore(infra): añadir migración para tabla de almacenes secundarios
docs(config): actualizar AGENTS.md con nuevos skills
feat(auth): implementar refresh token automático
perf(purchase): optimizar query de búsqueda de proveedores
chore(deps): actualizar PrimeVue a 4.5.1
```

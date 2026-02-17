# Lilith Backend - Manufacturing ERP System

> **Note**: This backend is part of the [Lilith monorepo](https://github.com/MGurtD/lilith). For full system documentation, see the root README.

A comprehensive manufacturing ERP solution built with .NET 10 and Clean Architecture principles, managing the complete lifecycle from sales and purchases to production planning, inventory control, and financial operations.

**Architecture Grade: A (9.5/10)** - Exceptional Clean Architecture implementation with complete service layer separation across all 51 controllers (completed December 2025). All business logic is now testable without HTTP context, with consistent error handling and full localization support. Key remaining areas for improvement: test coverage and authorization framework.

## Technology Stack

- **.NET 10** - Web API framework
- **Entity Framework Core** - ORM with PostgreSQL provider
- **PostgreSQL** - Primary database
- **JWT Bearer** - Authentication
- **Serilog** - Structured logging
- **Swashbuckle** - OpenAPI/Swagger documentation

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         API LAYER                           │
│   (Controllers - only inject service interfaces)           │
│   (Middleware, Program.cs - wires implementations)         │
│                         ↓                                   │
│              (depends on service interfaces)                │
└─────────────────────────┬───────────────────────────────────┘
                          │
           ┌──────────────▼─────────────────┐
           │    APPLICATION.CONTRACTS       │
           │  (Service & Repository IFs,    │
           │   DTOs, Constants)             │
           │         ↑            ↑         │
           └─────────┼────────────┼─────────┘
                     │            │
        ┌────────────┘            └────────────┐
        │                                      │
        │ implements                  implements│
        │                                      │
┌───────▼───────────┐              ┌───────────▼────────┐
│   APPLICATION     │              │  INFRASTRUCTURE    │
│   (Services)      │─────────────→│  (Repositories,    │
│   Business Logic  │  uses IUoW   │   UnitOfWork,      │
└───────┬───────────┘              │   EF Core)         │
        │                          └───────────┬────────┘
        │                                      │
        └──────────────┬───────────────────────┘
                       ↓
           ┌───────────────────────┐
           │       DOMAIN          │
           │  (Entities, Core)     │
           │   NO Dependencies     │
           └───────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                        VERIFACTU                            │
│          (External Tax Integration Service)                 │
│              Spanish AEAT Invoice Registry                  │
└─────────────────────────────────────────────────────────────┘
```

## Quick Start

```bash
# Prerequisites: .NET 10 SDK, PostgreSQL 16+

# 1. Restore dependencies
dotnet restore

# 2. Update connection string in src/Api/appsettings.Development.json

# 3. Apply database migrations
dotnet ef database update --project src/Infrastructure/

# 4. Run the API
dotnet run --project src/Api/
# or use VS Code task: "watch"

# 5. Access Swagger UI
# Navigate to: https://localhost:5001/swagger
```

## Documentation Index

### Architecture

- **[Architecture Layers](docs/architecture-layers.md)** - Deep dive into the 6 projects and their responsibilities
- **[Architectural Patterns](docs/architectural-patterns.md)** - Repository, Service, GenericResponse, and Entity configuration patterns
- **[Request Flow](docs/request-flow.md)** - How HTTP requests flow through layers with sequence diagrams
- **[Domain Model](docs/domain-model.md)** - Business entities across Sales, Purchase, Production, and Warehouse

### Development Guides

- **[Developer Guide](docs/developer-guide.md)** - Setup, common tasks, and critical conventions
- **[How to Create Endpoints](docs/how-to-create-endpoints.md)** - Step-by-step guide for adding new API endpoints
- **[How to Refactor Controllers to Services](docs/how-to-refactor-controllers-to-services.md)** - Moving business logic out of controllers

### Features & Systems

- **[Localization](docs/localization.md)** - Multilanguage support (Catalan, Spanish, English)
- **[External Integrations](docs/external-integrations.md)** - Verifactu tax service and other external systems

### Known Issues

- **[Architectural Debt Assessment](docs/architectural-debt-assessment.md)** - Critical improvements needed (tests, authorization)

## Project Structure

```
src/
├── Domain/                      # Pure business entities (no dependencies)
├── Application.Contracts/       # Interfaces, DTOs, constants (flat namespace)
├── Application/                 # Business logic and services
├── Infrastructure/              # Data access with EF Core + PostgreSQL
├── Api/                         # Controllers, middleware, startup
└── Verifactu/                   # Spanish tax authority integration
```

## Core Concepts

### Clean Architecture Principles

- **Domain** is the core with zero external dependencies
- **Application.Contracts** defines all abstractions (interfaces, DTOs)
- **Application** implements business logic using contracts
- **Infrastructure** implements data access and external integrations
- **Api** orchestrates everything as the composition root

### Key Patterns

- **Repository + Unit of Work** - Data access abstraction
- **Service Layer** - Business logic completely separated from controllers (100% of controllers refactored)
- **GenericResponse** - Standardized error handling and results
- **Primary Constructors** - Modern C# 12 dependency injection
- **Localization-First** - All user-facing strings support multiple languages

### Domain Coverage

**Sales:** Customer, Budget, SalesOrder, DeliveryNote, SalesInvoice  
**Purchase:** Supplier, PurchaseOrder, Receipt, PurchaseInvoice  
**Production:** WorkMaster, WorkOrder, ProductionPart, Workcenter  
**Warehouse:** Stock, Location, StockMovement, Reference

## Critical Conventions

⚠️ **Always follow these rules:**

1. **Use `StatusConstants`** - Never hardcode lifecycle/status names
2. **Localize error messages** - Inject `ILocalizationService` in all services
3. **Return `GenericResponse`** - For all write operations that can fail
4. **Primary constructors** - Use modern C# 12 syntax for DI
5. **Async/await** - All I/O operations must be asynchronous
6. **No logic in controllers** - Business logic belongs in services (all 51 controllers now follow this pattern)
7. **Never inject `IUnitOfWork` in controllers** - Use service layer interfaces only

## Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName --project src/Infrastructure/

# Apply migrations to database
dotnet ef database update --project src/Infrastructure/

# Rollback to previous migration
dotnet ef database update PreviousMigrationName --project src/Infrastructure/
```

## Docker Support

```bash
# Build image
docker build -t lilith-backend .

# Run with PostgreSQL connection
docker run -p 8080:80 \
  -e ConnectionStrings__Default="Host=postgres;Database=lilith;..." \
  lilith-backend
```

## Contributing

Before implementing new features:

1. Read [Developer Guide](docs/developer-guide.md) for conventions
2. Check [Architectural Debt Assessment](docs/architectural-debt-assessment.md) for known issues
3. Follow established patterns in [Architectural Patterns](docs/architectural-patterns.md)
4. Add localization keys for all user-facing messages
5. Update relevant documentation

## License

Internal project - All rights reserved.

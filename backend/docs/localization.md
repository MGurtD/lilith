# Localization System

This document explains the comprehensive multilanguage support system covering Catalan, Spanish, and English.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│              LOCALIZATION SYSTEM                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  JSON Resource Files (src/Api/Resources/)                  │
│    ├─ ca.json  (Catalan - default)                         │
│    ├─ es.json  (Spanish)                                    │
│    └─ en.json  (English)                                    │
│                                                             │
│  ILocalizationService (Application.Contracts)              │
│    ├─ GetLocalizedString(key, params)                      │
│    ├─ GetLocalizedStringForCulture(key, culture, params)   │
│    └─ GetAllTranslations()                                  │
│                                                             │
│  CultureMiddleware (Api)                                    │
│    ├─ Query parameter: ?culture=ca                          │
│    ├─ JWT claim: "locale": "ca"                             │
│    ├─ Accept-Language header                                │
│    └─ Default: Catalan (ca)                                 │
│                                                             │
│  StatusConstants (Application.Contracts)                    │
│    └─ Database values (lifecycles/statuses in Catalan)      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Supported Languages

| Code | Language | Status    |
| ---- | -------- | --------- |
| `ca` | Catalan  | Default   |
| `es` | Spanish  | Supported |
| `en` | English  | Supported |

---

## Culture Detection

### Priority Order

**CultureMiddleware** detects language in this order:

```
1. Query Parameter → ?culture=ca
       ↓ (if not present)
2. JWT Token Claim → "locale": "es"
       ↓ (if not present)
3. Accept-Language Header → Accept-Language: en-US
       ↓ (if not present)
4. Default → Catalan (ca)
```

### Implementation

```csharp
public class CultureMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string culture = "ca";  // Default

        // 1. Check query parameter
        if (context.Request.Query.ContainsKey("culture"))
        {
            culture = context.Request.Query["culture"].ToString();
        }
        // 2. Check JWT claim
        else if (context.User.Identity?.IsAuthenticated == true)
        {
            var localeClaim = context.User.FindFirst("locale");
            if (localeClaim != null)
                culture = localeClaim.Value;
        }
        // 3. Check Accept-Language header
        else
        {
            var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguage))
                culture = acceptLanguage.Split(',')[0].Split('-')[0];
        }

        // Set thread culture
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = new CultureInfo(culture);

        await next(context);
    }
}
```

### Usage Examples

**Query parameter (highest priority):**

```bash
GET /api/budget?culture=en
# Forces English regardless of JWT or headers
```

**JWT token (second priority):**

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "locale": "ca"  ← Language preference
}
```

**Accept-Language header (third priority):**

```http
GET /api/budget HTTP/1.1
Accept-Language: es-ES,es;q=0.9,en;q=0.8
```

---

## ILocalizationService

### Interface

```csharp
public interface ILocalizationService
{
    string GetLocalizedString(string key, params object[] arguments);
    string GetLocalizedStringForCulture(string key, string culture, params object[] arguments);
    Dictionary<string, string> GetAllTranslations();
    string[] GetSupportedCultures();
}
```

### Usage in Services

**Basic usage (current request culture):**

```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IBudgetService
{
    public async Task<GenericResponse> Create(CreateHeaderRequest request)
    {
        var customer = await unitOfWork.Customers.Get(request.CustomerId);
        if (customer == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerNotFound"));

        // ...
    }
}
```

**Parameterized messages:**

```csharp
// Key: "EntityNotFound": "Entity with ID {0} not found"
var message = localizationService.GetLocalizedString("EntityNotFound", entityId);
// Result: "Entity with ID 123e4567-... not found" (English)
// Result: "Entitat amb ID 123e4567-... no trobada" (Catalan)
```

**Force specific culture:**

```csharp
// Always return Spanish error message regardless of request culture
var spanishError = localizationService.GetLocalizedStringForCulture(
    "CustomerNotFound", "es");
```

### Usage in Controllers

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var budget = await service.GetById(id);

    if (budget == null)
        return NotFound(new GenericResponse(false,
            localizationService.GetLocalizedString("BudgetNotFound", id)));

    return Ok(budget);
}
```

---

## JSON Resource Files

### File Structure

Located in `src/Api/Resources/LocalizationService/`:

```
LocalizationService/
├── ca.json  (Catalan)
├── es.json  (Spanish)
└── en.json  (English)
```

### Example Content

**ca.json (Catalan - default):**

```json
{
  "EntityNotFound": "Entitat amb ID {0} no trobada",
  "CustomerNotFound": "Client no trobat",
  "CustomerInvalid": "El client no és vàlid per crear una factura...",
  "BudgetNotFound": "Pressupost amb ID {0} no trobat",
  "ExerciseNotFound": "Exercici no trobat",
  "ExerciseCounterError": "Error creant el comptador",
  "LifecycleNotFound": "Cicle de vida '{0}' no trobat",
  "StatusNotFound": "Estat amb ID {0} no trobat o està deshabilitat",
  "Validation.Required": "El camp {0} és obligatori"
}
```

**es.json (Spanish):**

```json
{
  "EntityNotFound": "Entidad con ID {0} no encontrada",
  "CustomerNotFound": "Cliente no encontrado",
  "BudgetNotFound": "Presupuesto con ID {0} no encontrado",
  "ExerciseNotFound": "Ejercicio no encontrado",
  "LifecycleNotFound": "Ciclo de vida '{0}' no encontrado"
}
```

**en.json (English):**

```json
{
  "EntityNotFound": "Entity with ID {0} not found",
  "CustomerNotFound": "Customer not found",
  "BudgetNotFound": "Budget with ID {0} not found",
  "ExerciseNotFound": "Exercise not found",
  "LifecycleNotFound": "Lifecycle '{0}' not found"
}
```

---

## StatusConstants Pattern

### Purpose

Database values (lifecycle and status names) remain in **Catalan** in the database. `StatusConstants` prevents typos and provides type-safe references.

### Location

`src/Application.Contracts/Constants/StatusConstants.cs`

### Implementation

```csharp
public static class StatusConstants
{
    public static class Lifecycles
    {
        public const string Budget = "Budget";
        public const string SalesOrder = "SalesOrder";
        public const string SalesInvoice = "SalesInvoice";
        public const string DeliveryNote = "DeliveryNote";
        public const string PurchaseOrder = "PurchaseOrder";
        public const string PurchaseInvoice = "PurchaseInvoice";
        public const string Receipts = "Receipts";
        public const string WorkOrder = "WorkOrder";
        public const string Verifactu = "Verifactu";
    }

    public static class Statuses
    {
        // Catalan names as stored in database
        public const string Creada = "Creada";
        public const string PendentAcceptar = "Pendent d'acceptar";
        public const string Acceptat = "Acceptat";
        public const string Rebutjat = "Rebutjat";
        public const string EnProces = "En procés";
        public const string Finalitzada = "Finalitzada";
        public const string Servida = "Servida";
        public const string Facturada = "Facturada";
    }
}
```

### Usage

**❌ BAD - Magic strings (typo-prone):**

```csharp
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == "SalesOrder")  // ← Typo risk!
    .FirstOrDefault();

var status = unitOfWork.Statuses
    .Find(s => s.Name == "Pendent d'acceptar")  // ← Easy to misspell
    .FirstOrDefault();
```

**✅ GOOD - Use constants:**

```csharp
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == StatusConstants.Lifecycles.SalesOrder)
    .FirstOrDefault();

var status = await unitOfWork.Lifecycles.GetStatusByName(
    StatusConstants.Lifecycles.Budget,
    StatusConstants.Statuses.PendentAcceptar);
```

---

## Standard Localization Keys

### Entity Operations

| Key                   | Catalan                               | Spanish                               | English                        |
| --------------------- | ------------------------------------- | ------------------------------------- | ------------------------------ |
| `EntityNotFound`      | Entitat amb ID {0} no trobada         | Entidad con ID {0} no encontrada      | Entity with ID {0} not found   |
| `EntityAlreadyExists` | L'entitat ja existeix                 | La entidad ya existe                  | Entity already exists          |
| `EntityDisabled`      | Entitat amb ID {0} està deshabilitada | Entidad con ID {0} está deshabilitada | Entity with ID {0} is disabled |

### Business Entities

| Key                 | Example Message (Catalan)                      |
| ------------------- | ---------------------------------------------- |
| `CustomerNotFound`  | Client no trobat                               |
| `CustomerInvalid`   | El client no és vàlid per crear una factura... |
| `BudgetNotFound`    | Pressupost amb ID {0} no trobat                |
| `WorkOrderNotFound` | Ordre de treball amb ID {0} no trobada         |
| `InvoiceNotFound`   | Factura amb ID {0} no trobada                  |

### Exercise & Document Management

| Key                       | Example Message (Catalan)                   |
| ------------------------- | ------------------------------------------- |
| `ExerciseNotFound`        | Exercici no trobat                          |
| `ExerciseCounterError`    | Error creant el comptador                   |
| `ExerciseCounterNotFound` | El comptador proporcionat '{0}' no és vàlid |

### Lifecycle & Status Management

| Key                        | Example Message (Catalan)                      |
| -------------------------- | ---------------------------------------------- |
| `LifecycleNotFound`        | Cicle de vida '{0}' no trobat                  |
| `LifecycleNoInitialStatus` | El cicle de vida '{0}' no té estat inicial     |
| `StatusNotFound`           | Estat amb ID {0} no trobat o està deshabilitat |

### Validation

| Key                       | Example Message (Catalan)        |
| ------------------------- | -------------------------------- |
| `Validation.Required`     | El camp {0} és obligatori        |
| `Validation.InvalidEmail` | El format del correu no és vàlid |

---

## Adding New Localization Keys

### Step-by-Step Process

1. **Identify the message** that needs localization
2. **Choose a descriptive key** (follow existing naming patterns)
3. **Add to ALL 3 language files** (ca.json, es.json, en.json)
4. **Use in service** via `ILocalizationService`

### Example: Adding New Error Message

**1. Add to ca.json:**

```json
{
  "SupplierNotFound": "Proveïdor amb ID {0} no trobat"
}
```

**2. Add to es.json:**

```json
{
  "SupplierNotFound": "Proveedor con ID {0} no encontrado"
}
```

**3. Add to en.json:**

```json
{
  "SupplierNotFound": "Supplier with ID {0} not found"
}
```

**4. Use in service:**

```csharp
var supplier = await unitOfWork.Suppliers.Get(supplierId);
if (supplier == null)
    return new GenericResponse(false,
        localizationService.GetLocalizedString("SupplierNotFound", supplierId));
```

---

## Database vs User-Facing Strings

### Two-Layer Localization Approach

```
┌─────────────────────────────────────────┐
│  DATABASE LAYER (Catalan only)         │
│  • Lifecycle names: "Budget"            │
│  • Status names: "Pendent d'acceptar"   │
│  • Use StatusConstants for references   │
└─────────────────────────────────────────┘
               ↓
┌─────────────────────────────────────────┐
│  APPLICATION LAYER (Multilanguage)     │
│  • Error messages via JSON files        │
│  • UI labels via ILocalizationService   │
│  • Supports ca/es/en dynamically        │
└─────────────────────────────────────────┘
```

### Why This Approach?

**Database values stay in Catalan:**

- ✅ Consistent data (no migration needed for language changes)
- ✅ Simplified queries (no culture-specific lookups)
- ✅ Use `StatusConstants` to prevent typos

**User-facing strings localized:**

- ✅ Full multilanguage support
- ✅ Easy to add new languages
- ✅ Culture-aware error messages

---

## Best Practices

### ✅ DO

1. **Always inject `ILocalizationService`** in services that return user messages
2. **Use `StatusConstants`** for database-stored values (lifecycles/statuses)
3. **Add keys to ALL 3 files** when creating new localized strings
4. **Use parameterized messages** for dynamic content (IDs, names, etc.)
5. **Test with different cultures** using `?culture=` query parameter
6. **Group related keys** with dot notation (`Validation.Required`, `StatusNames.Created`)

### ❌ DON'T

1. **Never hardcode error strings** in services or controllers
2. **Don't mix languages** in error messages
3. **Don't use magic strings** for lifecycle/status names
4. **Don't forget to localize** new business messages
5. **Don't create duplicate keys** across files (maintain consistency)

---

## Complete Example: Localized Service

```csharp
public class SalesOrderService(
    IUnitOfWork unitOfWork,
    IExerciseService exerciseService,
    ILocalizationService localizationService) : ISalesOrderService
{
    public async Task<GenericResponse> Create(CreateHeaderRequest request)
    {
        // 1. Validate customer (localized error)
        var customer = await unitOfWork.Customers.Get(request.CustomerId);
        if (customer == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerNotFound"));

        // 2. Validate exercise (localized error with parameter)
        var exercise = await unitOfWork.Exercises.Get(request.ExerciseId);
        if (exercise == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("ExerciseNotFound"));

        // 3. Generate document number
        var counter = await exerciseService.GetNextCounter(
            request.ExerciseId, "salesorder");
        if (!counter.Result)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("ExerciseCounterError"));

        // 4. Get lifecycle using constants (database value)
        var lifecycle = unitOfWork.Lifecycles
            .Find(l => l.Name == StatusConstants.Lifecycles.SalesOrder)
            .FirstOrDefault();

        if (lifecycle == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("LifecycleNotFound",
                    StatusConstants.Lifecycles.SalesOrder));

        // 5. Create entity
        var order = new SalesOrderHeader
        {
            Id = request.Id,
            Number = counter.Content.ToString()!,
            CustomerId = request.CustomerId,
            StatusId = lifecycle.InitialStatusId
        };

        await unitOfWork.SalesOrderHeaders.Add(order);
        return new GenericResponse(true, order);
    }
}
```

**What happens with different cultures:**

```bash
# Catalan (default)
POST /api/salesorder
# Error: "Client no trobat"

# Spanish
POST /api/salesorder?culture=es
# Error: "Cliente no encontrado"

# English
POST /api/salesorder?culture=en
# Error: "Customer not found"
```

---

## Summary

### Localization Architecture

- **3 languages supported**: Catalan (default), Spanish, English
- **JSON resource files**: Easy to maintain and extend
- **Culture detection**: Query param → JWT → header → default
- **ILocalizationService**: Centralized translation access
- **StatusConstants**: Type-safe database value references

### Key Patterns

1. **Inject `ILocalizationService`** in all services
2. **Use `GetLocalizedString(key, params)`** for current culture
3. **Add keys to ALL files** when localizing
4. **Use `StatusConstants`** for database references
5. **Test with `?culture=`** parameter

### Related Documentation

- [Developer Guide](developer-guide.md) - Setup and conventions
- [Architectural Patterns](architectural-patterns.md) - Service layer pattern
- [Request Flow](request-flow.md) - Culture detection in middleware

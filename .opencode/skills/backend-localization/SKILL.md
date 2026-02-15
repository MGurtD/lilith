---
name: backend-localization
description: Multilingual backend support system for Catalan, Spanish, and English error messages and UI strings. Use when (1) Adding new error messages to services that return GenericResponse, (2) Working with database lifecycle/status names (Budget, SalesOrder, WorkOrder, PurchaseOrder, etc.), (3) Implementing features that display user-facing messages, (4) Troubleshooting "magic string" errors in status/lifecycle queries, (5) Creating new API endpoints requiring localized validation messages, (6) Fixing bugs where error messages appear in wrong language. Covers ILocalizationService injection patterns, StatusConstants for type-safe database references, JSON resource file structure (ca.json/es.json/en.json), culture detection priority (query param → JWT → Accept-Language → default), parameterized message formatting with placeholders.
---

# Backend Localization

Multilingual support for error messages and user-facing strings across Catalan, Spanish, and English.

## Quick Reference

**Supported languages:**
- `ca` - Catalan (default)
- `es` - Spanish
- `en` - English

**Culture detection priority:**
1. Query parameter: `?culture=ca`
2. JWT claim: `"locale": "ca"`
3. Accept-Language header
4. Default: Catalan

## Common Usage Patterns

### Pattern 1: Adding Error Message to Service

**User request**: "Add validation to BudgetService that checks if customer exists"

**Workflow**:
1. Add key to all 3 JSON files (ca.json, es.json, en.json)
2. Inject ILocalizationService in service constructor
3. Use GetLocalizedString() in validation

**Example**:
```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IBudgetService
{
    public async Task<GenericResponse> Create(Budget budget)
    {
        var customer = await unitOfWork.Customers.Get(budget.CustomerId);
        if (customer == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerNotFound", budget.CustomerId));
        
        // ... continue
    }
}
```

### Pattern 2: Fixing Magic String Error

**User report**: "Getting null reference when querying statuses"

**Root cause**: Hardcoded string with typo

```csharp
// ❌ BAD: Typo causes null
var status = statuses.Find(s => s.Name == "Pendent d'aceptar");  // Missing 'c'!

// ✅ GOOD: Compiler-verified constant
var status = statuses.Find(s => s.Name == StatusConstants.Statuses.PendentAcceptar);
```

### Pattern 3: Multi-Language Error Response

**User request**: "API returns Spanish errors even for Catalan users"

**Solution**: Ensure proper culture detection flow:
1. Check JWT token has `locale` claim
2. Test with `?culture=ca` query parameter
3. Verify `Accept-Language` header is set
4. Confirm ILocalizationService is injected in service

## ILocalizationService Usage

### Basic Pattern

Always inject in services that return user-facing messages:

```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IBudgetService
{
    public async Task<GenericResponse> Create(Budget budget)
    {
        var customer = await unitOfWork.Customers.Get(budget.CustomerId);
        if (customer == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerNotFound"));
        
        // ... continue with business logic
    }
}
```

### Parameterized Messages

Use placeholders for dynamic content:

```csharp
// Key in JSON: "EntityNotFound": "Entitat amb ID {0} no trobada"
var message = localizationService.GetLocalizedString("EntityNotFound", entityId);

// Multiple parameters
// Key: "ValidationError": "Field {0} must be between {1} and {2}"
var message = localizationService.GetLocalizedString("ValidationError", 
    fieldName, minValue, maxValue);
```

### Force Specific Culture

```csharp
// Always return Spanish message regardless of request culture
var spanishError = localizationService.GetLocalizedStringForCulture(
    "CustomerNotFound", "es");
```

## Adding New Localization Keys

**MUST add to ALL three files:**

### Step 1: Add to ca.json

Location: `src/Api/Resources/LocalizationService/ca.json`

```json
{
  "SupplierNotFound": "Proveïdor amb ID {0} no trobat",
  "SupplierInvalid": "El proveïdor no és vàlid per aquesta operació"
}
```

### Step 2: Add to es.json

Location: `src/Api/Resources/LocalizationService/es.json`

```json
{
  "SupplierNotFound": "Proveedor con ID {0} no encontrado",
  "SupplierInvalid": "El proveedor no es válido para esta operación"
}
```

### Step 3: Add to en.json

Location: `src/Api/Resources/LocalizationService/en.json`

```json
{
  "SupplierNotFound": "Supplier with ID {0} not found",
  "SupplierInvalid": "Supplier is not valid for this operation"
}
```

### Step 4: Use in Service

```csharp
var supplier = await unitOfWork.Suppliers.Get(supplierId);
if (supplier == null)
    return new GenericResponse(false,
        localizationService.GetLocalizedString("SupplierNotFound", supplierId));
```

## StatusConstants Pattern

**CRITICAL**: Database lifecycle and status names are stored in Catalan. ALWAYS use StatusConstants to reference them.

### Why Use StatusConstants

❌ **Bad - Magic strings (typo-prone):**
```csharp
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == "SalesOrder")  // Typo risk!
    .FirstOrDefault();

var status = unitOfWork.Statuses
    .Find(s => s.Name == "Pendent d'acceptar")  // Easy to misspell
    .FirstOrDefault();
```

✅ **Good - Type-safe constants:**
```csharp
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == StatusConstants.Lifecycles.SalesOrder)
    .FirstOrDefault();

var status = await unitOfWork.Lifecycles.GetStatusByName(
    StatusConstants.Lifecycles.Budget,
    StatusConstants.Statuses.PendentAcceptar);
```

### StatusConstants Reference

**ALWAYS use StatusConstants** for database lifecycle/status names. Never hardcode strings like "Budget" or "Pendent d'acceptar".

**Quick reference** (most common):
- Lifecycles: `Budget`, `SalesOrder`, `WorkOrder`, `PurchaseOrder`
- Statuses: `Creada`, `PendentAcceptar`, `EnProces`, `Finalitzada`

**Complete list**: See [references/STATUS_CONSTANTS.md](references/STATUS_CONSTANTS.md) for all lifecycles, statuses, usage examples, and lifecycle-status relationships.

**When to read the full reference file**:
- Working with uncommon lifecycles (DeliveryNote, Receipts, Verifactu)
- Need lifecycle-status relationship mappings (e.g., Budget workflow: PendentAcceptar → Acceptat → Rebutjat)
- Implementing new workflow with status transitions
- Filtering or checking current status in queries

### Using in Services

```csharp
public async Task<GenericResponse> Create(CreateHeaderRequest request)
{
    // Get lifecycle using constants
    var lifecycle = unitOfWork.Lifecycles
        .Find(l => l.Name == StatusConstants.Lifecycles.Budget)
        .FirstOrDefault();

    if (lifecycle == null)
        return new GenericResponse(false,
            localizationService.GetLocalizedString("LifecycleNotFound",
                StatusConstants.Lifecycles.Budget));

    // Set initial status
    var budget = new Budget
    {
        // ...
        StatusId = lifecycle.InitialStatusId
    };

    await unitOfWork.Budgets.Add(budget);
    return new GenericResponse(true, budget);
}
```

## Standard Localization Keys

### Entity Operations

| Key | Catalan | Spanish | English |
|-----|---------|---------|---------|
| `EntityNotFound` | Entitat amb ID {0} no trobada | Entidad con ID {0} no encontrada | Entity with ID {0} not found |
| `EntityAlreadyExists` | L'entitat ja existeix | La entidad ya existe | Entity already exists |
| `EntityDisabled` | Entitat amb ID {0} està deshabilitada | Entidad con ID {0} está deshabilitada | Entity with ID {0} is disabled |

### Business Entities

- `CustomerNotFound` - Client no trobat
- `CustomerInvalid` - El client no és vàlid...
- `SupplierNotFound` - Proveïdor no trobat
- `BudgetNotFound` - Pressupost amb ID {0} no trobat
- `WorkOrderNotFound` - Ordre de treball amb ID {0} no trobada
- `InvoiceNotFound` - Factura amb ID {0} no trobada

### Exercise & Counters

- `ExerciseNotFound` - Exercici no trobat
- `ExerciseCounterError` - Error creant el comptador
- `ExerciseCounterNotFound` - El comptador proporcionat '{0}' no és vàlid

### Lifecycle & Status

- `LifecycleNotFound` - Cicle de vida '{0}' no trobat
- `LifecycleNoInitialStatus` - El cicle de vida '{0}' no té estat inicial
- `StatusNotFound` - Estat amb ID {0} no trobat o està deshabilitat

### Validation

- `Validation.Required` - El camp {0} és obligatori
- `Validation.InvalidEmail` - El format del correu no és vàlid
- `Validation.InvalidDate` - La data proporcionada no és vàlida

## Culture Detection in Controllers

If you need to manually check culture (rare):

```csharp
[HttpGet]
public IActionResult GetCulture()
{
    var currentCulture = CultureInfo.CurrentCulture.Name;
    var currentUICulture = CultureInfo.CurrentUICulture.Name;
    
    return Ok(new { currentCulture, currentUICulture });
}
```

## Testing Localization

### Query Parameter (Highest Priority)

```bash
# Catalan (default)
curl https://localhost:5001/api/budget/invalid-id

# Spanish
curl https://localhost:5001/api/budget/invalid-id?culture=es

# English
curl https://localhost:5001/api/budget/invalid-id?culture=en
```

### JWT Token (Second Priority)

Include `locale` claim in JWT:
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "locale": "ca"
}
```

### Accept-Language Header (Third Priority)

```bash
curl -H "Accept-Language: es-ES" https://localhost:5001/api/budget/invalid-id
```

## Two-Layer Localization

### Database Layer (Catalan Only)

- Lifecycle names: "Budget", "SalesOrder"
- Status names: "Creada", "Pendent d'acceptar", "Finalitzada"
- Use `StatusConstants` for type safety

### Application Layer (Multilingual)

- Error messages: via JSON resource files
- UI labels: via `ILocalizationService`
- Supports ca/es/en dynamically

## Common Pitfalls

### ❌ Mistake 1: Forgetting to Add Key to All 3 Files

**Problem**: Added localization key only to `ca.json`

**Symptom**: Runtime error or missing translations when user switches to Spanish/English
```
KeyNotFoundException: The given key 'CustomerNotFound' was not present in the dictionary.
```

**Fix**: ALWAYS add the same key to all three files:
- `src/Api/Resources/LocalizationService/ca.json`
- `src/Api/Resources/LocalizationService/es.json`
- `src/Api/Resources/LocalizationService/en.json`

### ❌ Mistake 2: Magic Strings in Status/Lifecycle Queries

**Problem**: Hardcoded strings in queries
```csharp
var lifecycle = unitOfWork.Lifecycles.Find(l => l.Name == "Budget").FirstOrDefault();
var status = unitOfWork.Statuses.Find(s => s.Name == "Pendent d'acceptar").FirstOrDefault();
```

**Symptoms**: 
- Null reference exceptions when typos occur
- No IntelliSense support
- Difficult to refactor

**Fix**: Use `StatusConstants`
```csharp
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == StatusConstants.Lifecycles.Budget)
    .FirstOrDefault();

var status = await unitOfWork.Lifecycles.GetStatusByName(
    StatusConstants.Lifecycles.Budget,
    StatusConstants.Statuses.PendentAcceptar);
```

### ❌ Mistake 3: Not Injecting ILocalizationService

**Problem**: Creating `GenericResponse` with hardcoded strings
```csharp
return new GenericResponse(false, "Client no trobat");  // Always Catalan!
```

**Symptom**: Error messages always appear in Catalan, regardless of user's language preference

**Fix**: Inject `ILocalizationService` in constructor and use it
```csharp
public class BudgetService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IBudgetService  // ← Add this parameter
{
    public async Task<GenericResponse> Create(Budget budget)
    {
        // ...
        return new GenericResponse(false,
            localizationService.GetLocalizedString("CustomerNotFound"));
    }
}
```

### ❌ Mistake 4: Incorrect Placeholder Usage

**Problem**: Mismatched placeholders between languages
```json
// ca.json
"OrderTotal": "Total de la comanda: {0} EUR amb {1} articles"

// es.json
"OrderTotal": "Total del pedido: {1} artículos con {0} EUR"  // ❌ Swapped order!
```

**Symptom**: Values appear in wrong positions in Spanish

**Fix**: Keep placeholder order consistent across all languages
```json
// ca.json
"OrderTotal": "Total de la comanda: {0} EUR amb {1} articles"

// es.json
"OrderTotal": "Total del pedido: {0} EUR con {1} artículos"  // ✅ Same order
```

### ❌ Mistake 5: Culture Detection Not Working

**Problem**: API always returns Catalan errors even when testing with `?culture=es`

**Common causes**:
1. Culture middleware not registered in Program.cs
2. Query parameter typo: `?lang=es` instead of `?culture=es`
3. JWT token overriding query parameter (JWT has higher priority)

**Fix**: Verify culture detection priority:
1. Query parameter: `?culture=ca` (highest)
2. JWT claim: `"locale": "ca"`
3. Accept-Language header: `Accept-Language: es-ES`
4. Default: Catalan (lowest)

**Testing**:
```bash
# Test Catalan (default)
curl https://localhost:5001/api/budget/invalid-id

# Test Spanish
curl https://localhost:5001/api/budget/invalid-id?culture=es

# Test English
curl https://localhost:5001/api/budget/invalid-id?culture=en
```

## Best Practices

### ✅ DO

1. **Always inject `ILocalizationService`** in services returning user messages
2. **Use `StatusConstants`** for database lifecycle/status references
3. **Add keys to ALL 3 files** (ca.json, es.json, en.json)
4. **Use parameterized messages** for dynamic content
5. **Test with `?culture=` parameter** during development
6. **Group related keys** with dot notation (`Validation.Required`)

### ❌ DON'T

1. **Never hardcode error strings** in services or controllers
2. **Don't mix languages** in error messages
3. **Don't use magic strings** for lifecycle/status names
4. **Don't forget to localize** new business messages
5. **Don't create duplicate keys** across files

## Common Patterns

### Validation with Localized Errors

```csharp
public async Task<GenericResponse> Update(Guid id, Budget budget)
{
    if (id != budget.Id)
        return new GenericResponse(false,
            localizationService.GetLocalizedString("IdMismatch"));

    var existing = await unitOfWork.Budgets.Get(id);
    if (existing == null)
        return new GenericResponse(false,
            localizationService.GetLocalizedString("BudgetNotFound", id));

    if (existing.Disabled)
        return new GenericResponse(false,
            localizationService.GetLocalizedString("EntityDisabled", id));

    // Proceed with update...
}
```

### Multiple Errors

```csharp
var errors = new List<string>();

if (string.IsNullOrEmpty(model.Name))
    errors.Add(localizationService.GetLocalizedString("Validation.Required", "Name"));

if (model.Amount < 0)
    errors.Add(localizationService.GetLocalizedString("Validation.PositiveNumber", "Amount"));

if (errors.Any())
    return new GenericResponse(false, errors);
```

### Controller Usage

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var entity = await service.GetById(id);
    
    if (entity == null)
        return NotFound(new GenericResponse(false,
            localizationService.GetLocalizedString("EntityNotFound", id)));

    return Ok(entity);
}
```

## Response Examples

**Catalan (default):**
```json
{
  "result": false,
  "errors": ["Client no trobat"],
  "content": null
}
```

**Spanish (?culture=es):**
```json
{
  "result": false,
  "errors": ["Cliente no encontrado"],
  "content": null
}
```

**English (?culture=en):**
```json
{
  "result": false,
  "errors": ["Customer not found"],
  "content": null
}
```

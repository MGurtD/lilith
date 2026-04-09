# StatusConstants Reference

Complete list of lifecycle and status constants for type-safe database references.

## Lifecycles

```csharp
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
```

## Statuses (Catalan names as stored in database)

```csharp
public static class Statuses
{
    // General
    public const string Creada = "Creada";
    public const string Finalitzada = "Finalitzada";
    
    // Budget
    public const string PendentAcceptar = "Pendent d'acceptar";
    public const string Acceptat = "Acceptat";
    public const string Rebutjat = "Rebutjat";
    
    // Work Order & Production
    public const string EnProces = "En procés";
    public const string Pausada = "Pausada";
    public const string Tancada = "Tancada";
    
    // Sales & Delivery
    public const string Servida = "Servida";
    public const string ParServ = "Parcialment servida";
    public const string Facturada = "Facturada";
    public const string ParFact = "Parcialment facturada";
    
    // Purchase & Receipts
    public const string Rebuda = "Rebuda";
    public const string ParReb = "Parcialment rebuda";
    
    // Invoices
    public const string Pagada = "Pagada";
    public const string Impagada = "Impagada";
}
```

## Usage Examples

### Get Lifecycle and Initial Status

```csharp
var lifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == StatusConstants.Lifecycles.Budget)
    .FirstOrDefault();

if (lifecycle == null)
    return new GenericResponse(false,
        localizationService.GetLocalizedString("LifecycleNotFound",
            StatusConstants.Lifecycles.Budget));

entity.StatusId = lifecycle.InitialStatusId;
```

### Get Specific Status by Name

```csharp
var status = await unitOfWork.Lifecycles.GetStatusByName(
    StatusConstants.Lifecycles.SalesOrder,
    StatusConstants.Statuses.EnProces);

if (status != null)
{
    order.StatusId = status.Id;
}
```

### Filter by Status

```csharp
var pendingBudgets = unitOfWork.Budgets
    .Find(b => b.Status.Name == StatusConstants.Statuses.PendentAcceptar)
    .ToList();
```

### Check Current Status

```csharp
if (order.Status?.Name == StatusConstants.Statuses.Finalitzada)
{
    // Order is finalized, prevent modifications
    return new GenericResponse(false,
        localizationService.GetLocalizedString("OrderAlreadyFinalized"));
}
```

## Lifecycle-Status Relationships

### Budget
- Initial: `PendentAcceptar`
- Transitions: `Acceptat`, `Rebutjat`

### SalesOrder
- Initial: `Creada`
- Transitions: `EnProces` → `Servida` → `Facturada`

### WorkOrder
- Initial: `Creada`
- Transitions: `EnProces` → `Pausada` → `Finalitzada` → `Tancada`

### PurchaseOrder
- Initial: `Creada`
- Transitions: `EnProces` → `Rebuda` / `ParReb`

### Invoices (Sales/Purchase)
- Initial: `Creada`
- Transitions: `Impagada` → `Pagada`

## Why Use Constants

**Type safety**: Compiler catches typos at build time  
**Refactoring**: Easy to rename across codebase  
**Discoverability**: IntelliSense shows all available options  
**Documentation**: Self-documenting code

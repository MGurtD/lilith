# Domain Model

This document describes the business entities organized across 4 core business areas (Sales, Purchase, Production, Warehouse) and shared components used throughout the system.

## Business Areas Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                         LILITH ERP                               │
├──────────────┬──────────────┬──────────────┬────────────────────┤
│    SALES     │   PURCHASE   │  PRODUCTION  │     WAREHOUSE      │
│              │              │              │                    │
│ • Customer   │ • Supplier   │ • WorkMaster │ • Stock            │
│ • Budget     │ • PurchaseOrd│ • WorkOrder  │ • Location         │
│ • SalesOrder │ • Receipt    │ • Phase      │ • StockMovement    │
│ • DeliveryNote│• PurchaseInv │ • ProductPart│ • Warehouse        │
│ • SalesInvoice│             │ • Workcenter │ • ReferenceType    │
└──────────────┴──────────────┴──────────────┴────────────────────┘
                              ↓
            ┌─────────────────────────────────────┐
            │       SHARED COMPONENTS             │
            │ • Reference (Product/Service)       │
            │ • Lifecycle/Status (Workflows)      │
            │ • Exercise (Fiscal Periods)         │
            │ • User/Profile (Authentication)     │
            └─────────────────────────────────────┘
```

---

## 1. Sales Management

Manages the complete sales cycle from quotation to invoicing.

### Entity Hierarchy

```
Customer
  └─ Budget (Quotation)
      └─ SalesOrderHeader (Sales Order)
          ├─ SalesOrderDetail (Line items)
          ├─ DeliveryNote (Shipping document)
          │   └─ DeliveryNoteDetail
          └─ SalesInvoice (Customer invoice)
              └─ SalesInvoiceDetail
```

### Key Entities

**Customer**

```csharp
public class Customer : Entity
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string TaxId { get; set; }
    public Guid? CustomerTypeId { get; set; }
    public ICollection<Contact> Contacts { get; set; }
    public ICollection<Address> Addresses { get; set; }
}
```

**Budget**

- Quotations sent to customers
- Auto-rejection after 30 days if not accepted
- Lifecycle: "Pendent d'acceptar" → "Acceptat" / "Rebutjat"

**SalesOrderHeader/Detail**

- Confirmed orders from customers
- Links to WorkOrders for production
- Contains pricing, quantities, delivery dates

**DeliveryNote**

- Shipping documents
- **Consumes stock** when marked as delivered
- Can be partial (multiple delivery notes per order)

**SalesInvoice**

- Customer billing
- Integrates with Verifactu (Spanish tax registry)
- Tracks due dates and payment status

### Workflow

```
Quote → Accept → Order → Produce → Ship → Invoice → Collect
  ↓       ↓        ↓        ↓        ↓        ↓        ↓
Budget → SalesOrder → WorkOrder → DeliveryNote → SalesInvoice
```

---

## 2. Purchase Management

Manages supplier relationships and procurement.

### Entity Hierarchy

```
Supplier
  └─ PurchaseOrder (Purchase order)
      └─ PurchaseOrderDetail (Line items)
          └─ Receipt (Goods received)
              └─ ReceiptDetail
                  └─ StockMovement (adds to inventory)

Expense (General expenses not tied to orders)
```

### Key Entities

**Supplier**

```csharp
public class Supplier : Entity
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string TaxId { get; set; }
    public Guid? SupplierTypeId { get; set; }
    public ICollection<Contact> Contacts { get; set; }
    public ICollection<SupplierReference> References { get; set; }
}
```

**PurchaseOrder**

- Orders sent to suppliers
- Tracks reception status
- Links to WorkOrders if purchasing for production

**Receipt**

- Goods received from suppliers
- **Adds to stock** when processed
- Captures weight, dimensions, packaging details

**PurchaseInvoice**

- Supplier billing
- Links to receipts and purchase orders
- Payment tracking and due dates

**Expense**

- General costs not tied to specific orders
- Categorized by expense types

### Workflow

```
Order → Receive → Store → Pay
  ↓        ↓        ↓      ↓
PurchaseOrder → Receipt → Stock → PurchaseInvoice
```

---

## 3. Production Management

Manages manufacturing processes from planning to execution.

### Entity Hierarchy

```
Reference (Product)
  └─ WorkMaster (Manufacturing route template)
      └─ WorkMasterPhase (Template phase)
          ├─ WorkMasterPhaseDetail (Operations)
          └─ WorkMasterPhaseBillOfMaterials (BOM)

SalesOrderDetail
  └─ WorkOrder (Actual production order)
      └─ WorkOrderPhase (Manufacturing phase)
          ├─ WorkOrderPhaseDetail (Operations)
          ├─ WorkOrderPhaseBillOfMaterials (Materials consumed)
          └─ ProductionPart (Time/cost tracking)

Workcenter (Machine/resource)
Operator (Worker)
```

### Key Entities

**WorkMaster**

- Template defining how to manufacture a reference
- Reusable process definition
- Contains phases, operations, and BOM

**WorkOrder**

```csharp
public class WorkOrder : Entity
{
    public string Code { get; set; }
    public Guid? SalesOrderDetailId { get; set; }
    public Guid ReferenceId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime? PlannedDate { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Guid? StatusId { get; set; }
    public decimal OperatorCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal ExternalCost { get; set; }
}
```

**WorkOrderPhase**

- Individual manufacturing steps
- Sequential or parallel execution
- Tracks planned vs actual time/cost

**ProductionPart**

- Time tracking for operators and machines
- Links to workcenter and operator
- Captures actual costs vs estimates

**Workcenter**

- Manufacturing resources (machines, assembly lines)
- Hourly rates for cost calculation
- Capacity and availability tracking

### Workflow

```
Plan → Schedule → Execute → Track → Complete
  ↓       ↓         ↓        ↓        ↓
WorkMaster → WorkOrder → Phases → ProductionPart → Stock
```

### Cost Calculation Flow

```
WorkMaster (estimated costs)
      ↓
  WorkOrder (planned costs)
      ↓
ProductionPart (actual costs: operator + machine time)
      ↓
Reference.LastCost (updated after completion)
```

---

## 4. Warehouse Management

Manages inventory, locations, and stock movements.

### Entity Hierarchy

```
Warehouse
  └─ Location (Storage position)
      └─ Stock (Inventory by reference)
          └─ StockMovement (Transaction history)

ReferenceType
  └─ Reference (Product/Service catalog)
```

### Key Entities

**Stock**

```csharp
public class Stock : Entity
{
    public Guid ReferenceId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => Quantity - ReservedQuantity;
}
```

**StockMovement**

- Tracks all inventory changes
- Types: Receipt, Delivery, Adjustment, Transfer
- Immutable audit trail

**Location**

- Specific storage positions within warehouses
- Hierarchical structure (warehouse → zone → rack → bin)
- Default location per warehouse

**ReferenceType**

- Product categorization
- Raw material, semi-finished, finished goods, services
- Used for inventory analysis and reporting

### Inventory Flow

```
Receipt (+) → Stock → Delivery (-)
                ↓
         StockMovement (audit)
```

---

## Shared Components

### Reference (Central Entity)

The **Reference** entity is the product/service catalog used across all business areas.

```csharp
public class Reference : Entity
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public Guid? TaxId { get; set; }
    public Guid? ReferenceTypeId { get; set; }
    public Guid? CustomerId { get; set; }  // Customer-specific reference

    // Cost tracking
    public decimal LastCost { get; set; }
    public decimal WorkMasterCost { get; set; }

    // Flags
    public bool Sales { get; set; }        // Sellable
    public bool Purchase { get; set; }     // Purchasable
    public bool Production { get; set; }   // Manufacturable
}
```

**Key Points:**

- Used in sales orders, purchase orders, work orders, and stock
- Tracks multiple cost types (last cost, standard cost, work master cost)
- Can be customer-specific (custom parts)
- Flags indicate business capabilities (sales/purchase/production)

### Lifecycle & Status (Workflow Engine)

```
Lifecycle
  └─ Status (multiple statuses per lifecycle)
      └─ StatusTransition (allowed transitions)
```

**Lifecycle**

- Named workflows: "Budget", "SalesOrder", "WorkOrder", "PurchaseInvoice"
- Defines initial and final statuses
- Manages state transitions

**Status**

```csharp
public class Status : Entity
{
    public string Name { get; set; }        // e.g., "Creada", "En procés", "Finalitzada"
    public string Color { get; set; }       // UI color code
    public Guid? LifecycleId { get; set; }
}
```

**Usage:**

- All major business entities have `StatusId` property
- UI displays status colors and names (localized)
- Business logic validates status transitions
- **Always use `StatusConstants`** - Never hardcode status names

### Exercise (Fiscal Period)

```csharp
public class Exercise : Entity
{
    public string Name { get; set; }        // e.g., "2025"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Document counters (sequential numbering)
    public string PurchaseOrderCounter { get; set; }
    public string SalesOrderCounter { get; set; }
    public string WorkOrderCounter { get; set; }
    public string BudgetCounter { get; set; }
    public string SalesInvoiceCounter { get; set; }
    public string PurchaseInvoiceCounter { get; set; }
}
```

**Purpose:**

- Fiscal year/period management
- Sequential document numbering per period
- Separates transactions by accounting period

**Document Numbering:**

- Each document type has a counter per exercise
- Format: `{prefix}{counter}` (e.g., "ORD2025-0042")
- `IExerciseService.GetNextCounter()` generates next number

### User & Authentication

```
User
  ├─ Profile (assigned)
  │   └─ ProfileMenu (dynamic menu)
  └─ RefreshToken (JWT refresh)
```

**User**

```csharp
public class User : Entity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }  // Hashed
    public string Locale { get; set; }    // Language preference
    public ICollection<Guid> ProfileIds { get; set; }
}
```

**Profile**

- Role-based access definition (not enforced yet)
- Dynamic menu configuration
- Permission structure (currently unused)

**RefreshToken**

- JWT refresh token management
- Expiration tracking
- Revocation support

---

## Cross-Cutting Relationships

### Reference Relationships

```
Reference (Product)
    ↑
    ├─ SalesOrderDetail (selling)
    ├─ PurchaseOrderDetail (buying)
    ├─ WorkOrder (producing)
    ├─ Stock (storing)
    └─ WorkMasterPhaseBillOfMaterials (consuming in production)
```

### Complete Order-to-Cash Flow

```
Customer
  └─ Budget
      └─ SalesOrderHeader
          └─ SalesOrderDetail (Reference: Product A)
              ├─ WorkOrder (produce Product A)
              │   ├─ WorkOrderPhase
              │   │   └─ WorkOrderPhaseBillOfMaterials (consume materials)
              │   └─ ProductionPart (time tracking)
              │       └─ Updates Reference.LastCost
              ├─ Stock (finished goods)
              ├─ DeliveryNote (ship to customer)
              │   └─ StockMovement (decrease stock)
              └─ SalesInvoice (bill customer)
                  └─ Verifactu registration
```

### Complete Procure-to-Pay Flow

```
Supplier
  └─ PurchaseOrder (order Material X)
      └─ PurchaseOrderDetail (Reference: Material X)
          └─ Receipt (goods received)
              ├─ ReceiptDetail
              │   └─ StockMovement (increase stock)
              └─ Stock (Material X available)
                  └─ WorkOrderPhaseBillOfMaterials (consume in production)
```

---

## Entity Base Properties

All entities inherit from `Entity`:

```csharp
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public bool Disabled { get; set; } = false;  // Soft delete
}
```

**Key Points:**

- **UUID primary keys** - Client-generated, distributed-system friendly
- **Timestamps** - Automatic creation/update tracking
- **Soft delete** - `Disabled = true` instead of physical deletion
- **No audit fields** - CreatedBy/UpdatedBy not tracked (yet)

---

## Database Views

Complex reporting queries are materialized as database views:

- `vw_consolidatedExpenses` - Expense analysis
- `vw_detailedworkorder` - Work order with all related data
- `vw_productioncosts` - Cost breakdown by work order
- `vw_stocksummary` - Inventory summary by reference

**Usage:**

- Mapped to keyless entities in EF Core
- Used for read-only reporting
- Provide pre-joined data for complex queries

---

## Summary

The domain model supports a complete **manufacturing ERP workflow**:

1. **Sales** - Quote → Order → Ship → Invoice
2. **Purchase** - Order → Receive → Pay
3. **Production** - Plan → Execute → Track costs
4. **Warehouse** - Receive → Store → Deliver

**Central entities:**

- **Reference** - The product/service catalog linking all areas
- **Lifecycle/Status** - Workflow engine managing entity states
- **Exercise** - Fiscal periods with document numbering
- **User** - Authentication and (future) authorization

All entities use **soft delete**, **UUID keys**, and **audit timestamps** for consistency.

For interaction patterns, see [Request Flow](request-flow.md).  
For implementation patterns, see [Architectural Patterns](architectural-patterns.md).

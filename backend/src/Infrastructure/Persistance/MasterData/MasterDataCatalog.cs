using System.Linq.Expressions;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Domain.Entities.Transport;
using Domain.Entities.Warehouse;
using WarehouseEntity = Domain.Entities.Warehouse.Warehouse;

namespace Infrastructure.Persistance.MasterData;

/// <summary>
/// A record that is deleted together with its owner, so whatever refers to it keeps the
/// owner in use. With a filter, only the matching records are owned and the rest keep
/// the owner in use themselves.
/// </summary>
internal sealed record OwnedPart(Type Owner, Type Part, LambdaExpression? Filter)
{
    public static OwnedPart Of<TOwner, TPart>(Expression<Func<TPart, bool>>? filter = null)
        where TOwner : Entity where TPart : Entity
        => new(typeof(TOwner), typeof(TPart), filter);
}

/// <summary>A reference to master data that has no foreign key in the EF model.</summary>
internal sealed record ExtraReference(Type Principal, Type Dependent, string Property)
{
    public static ExtraReference To<TPrincipal, TDependent>(string property)
        where TPrincipal : Entity where TDependent : Entity
        => new(typeof(TPrincipal), typeof(TDependent), property);
}

/// <summary>
/// What the delete guard cannot derive from the EF model: the name of each master data
/// record, its owned parts, references without a foreign key, and how each kind of
/// referring record is named to the user. Everything else is derived from the model.
/// </summary>
internal static class MasterDataCatalog
{
    public static readonly IReadOnlyDictionary<Type, Func<object, string?>> Names =
        new Dictionary<Type, Func<object, string?>>
        {
            [typeof(Area)] = e => ((Area)e).Name,
            [typeof(Customer)] = e => ((Customer)e).ComercialName,
            [typeof(CustomerType)] = e => ((CustomerType)e).Name,
            [typeof(Enterprise)] = e => ((Enterprise)e).Name,
            [typeof(Exercise)] = e => ((Exercise)e).Name,
            [typeof(ExpenseType)] = e => ((ExpenseType)e).Name,
            [typeof(InvoiceSerie)] = e => ((InvoiceSerie)e).Name,
            [typeof(Lifecycle)] = e => ((Lifecycle)e).Name,
            [typeof(Location)] = e => ((Location)e).Name,
            [typeof(MachineStatus)] = e => ((MachineStatus)e).Name,
            [typeof(OperatorType)] = e => ((OperatorType)e).Name,
            [typeof(PaymentMethod)] = e => ((PaymentMethod)e).Name,
            [typeof(Reference)] = e => ((Reference)e).Code,
            [typeof(ReferenceType)] = e => ((ReferenceType)e).Name,
            [typeof(RejectionReason)] = e => ((RejectionReason)e).Code,
            [typeof(Site)] = e => ((Site)e).Name,
            [typeof(Status)] = e => ((Status)e).Name,
            [typeof(Supplier)] = e => ((Supplier)e).ComercialName,
            [typeof(SupplierType)] = e => ((SupplierType)e).Name,
            [typeof(Tax)] = e => ((Tax)e).Name,
            [typeof(WarehouseEntity)] = e => ((WarehouseEntity)e).Name,
            [typeof(Workcenter)] = e => ((Workcenter)e).Name,
            [typeof(WorkcenterType)] = e => ((WorkcenterType)e).Name,
            // A production route has no name of its own.
            [typeof(WorkMaster)] = _ => null,
        };

    public static readonly IReadOnlyList<OwnedPart> OwnedParts =
    [
        OwnedPart.Of<Customer, CustomerAddress>(),
        OwnedPart.Of<Customer, CustomerContact>(),
        OwnedPart.Of<Lifecycle, LifecycleTag>(),
        OwnedPart.Of<LifecycleTag, StatusLifecycleTag>(),
        OwnedPart.Of<Location, WorkcenterLocation>(),
        // Empty stock records go with their location; stock on hand keeps it in use.
        OwnedPart.Of<Location, Stock>(s => s.Quantity == 0),
        OwnedPart.Of<MachineStatus, MachineStatusReason>(),
        OwnedPart.Of<MachineStatus, WorkcenterCost>(),
        OwnedPart.Of<Reference, SupplierReference>(),
        OwnedPart.Of<Status, StatusLifecycleTag>(),
        OwnedPart.Of<Supplier, SupplierContact>(),
        OwnedPart.Of<Supplier, SupplierReference>(),
        OwnedPart.Of<WarehouseEntity, Location>(),
        OwnedPart.Of<Workcenter, WorkcenterCost>(),
        OwnedPart.Of<Workcenter, WorkcenterLocation>(),
        OwnedPart.Of<Workcenter, WorkcenterProfitPercentage>(),
        OwnedPart.Of<WorkMaster, WorkMasterPhase>(),
        OwnedPart.Of<WorkMasterPhase, WorkMasterPhaseDetail>(),
        OwnedPart.Of<WorkMasterPhase, WorkMasterPhaseBillOfMaterials>(),
    ];

    public static readonly IReadOnlyList<ExtraReference> ExtraReferences =
    [
        ExtraReference.To<Reference, BudgetExternalServices>(nameof(BudgetExternalServices.ReferenceId)),
        ExtraReference.To<Reference, SalesOrderExternalServices>(nameof(SalesOrderExternalServices.ReferenceId)),
        ExtraReference.To<Status, Lifecycle>(nameof(Lifecycle.InitialStatusId)),
        ExtraReference.To<Status, Lifecycle>(nameof(Lifecycle.FinalStatusId)),
        ExtraReference.To<Status, StatusTransition>(nameof(StatusTransition.StatusToId)),
        ExtraReference.To<Supplier, BudgetExternalServices>(nameof(BudgetExternalServices.SupplierId)),
        ExtraReference.To<Supplier, BudgetTransport>(nameof(BudgetTransport.LogisticSupplierId)),
        ExtraReference.To<Supplier, BudgetTransport>(nameof(BudgetTransport.DestinationSupplierId)),
        ExtraReference.To<Supplier, SalesOrderExternalServices>(nameof(SalesOrderExternalServices.SupplierId)),
        ExtraReference.To<Supplier, TransportRate>(nameof(TransportRate.SupplierId)),
    ];

    /// <summary>
    /// Resource key of the kind of record each referring type belongs to, in the order the
    /// kinds are listed to the user. Lines share the key of their document.
    /// </summary>
    public static readonly IReadOnlyList<(Type Type, string Key)> DocumentKinds =
    [
        (typeof(Budget), "DocumentKind.Budgets"),
        (typeof(BudgetDetail), "DocumentKind.Budgets"),
        (typeof(BudgetExternalServices), "DocumentKind.Budgets"),
        (typeof(BudgetTransport), "DocumentKind.Budgets"),
        (typeof(SalesOrderHeader), "DocumentKind.SalesOrders"),
        (typeof(SalesOrderDetail), "DocumentKind.SalesOrders"),
        (typeof(SalesOrderExternalServices), "DocumentKind.SalesOrders"),
        (typeof(DeliveryNote), "DocumentKind.DeliveryNotes"),
        (typeof(DeliveryNoteDetail), "DocumentKind.DeliveryNotes"),
        (typeof(SalesInvoice), "DocumentKind.SalesInvoices"),
        (typeof(SalesInvoiceDetail), "DocumentKind.SalesInvoices"),
        (typeof(SalesInvoiceImport), "DocumentKind.SalesInvoices"),
        (typeof(PurchaseOrder), "DocumentKind.PurchaseOrders"),
        (typeof(PurchaseOrderDetail), "DocumentKind.PurchaseOrders"),
        (typeof(Receipt), "DocumentKind.Receipts"),
        (typeof(ReceiptDetail), "DocumentKind.Receipts"),
        (typeof(PurchaseInvoice), "DocumentKind.PurchaseInvoices"),
        (typeof(PurchaseInvoiceImport), "DocumentKind.PurchaseInvoices"),
        (typeof(Expenses), "DocumentKind.Expenses"),
        (typeof(PurchaseRate), "DocumentKind.PurchaseRates"),
        (typeof(PurchaseRateDetail), "DocumentKind.PurchaseRates"),
        (typeof(TransportRate), "DocumentKind.TransportRates"),
        (typeof(WorkOrder), "DocumentKind.WorkOrders"),
        (typeof(WorkOrderPhase), "DocumentKind.WorkOrders"),
        (typeof(WorkOrderPhaseDetail), "DocumentKind.WorkOrders"),
        (typeof(WorkOrderPhaseBillOfMaterials), "DocumentKind.WorkOrders"),
        (typeof(WorkOrderPhaseRejection), "DocumentKind.RejectedUnits"),
        (typeof(ProductionPart), "DocumentKind.ProductionParts"),
        (typeof(WorkcenterShift), "DocumentKind.ShiftHistory"),
        (typeof(WorkcenterShiftDetail), "DocumentKind.ShiftHistory"),
        (typeof(WorkMaster), "DocumentKind.ProductionRoutes"),
        (typeof(WorkMasterPhase), "DocumentKind.ProductionRoutes"),
        (typeof(WorkMasterPhaseDetail), "DocumentKind.ProductionRoutes"),
        (typeof(WorkMasterPhaseBillOfMaterials), "DocumentKind.ProductionRoutes"),
        (typeof(PhaseTemplateDetail), "DocumentKind.PhaseTemplates"),
        (typeof(Stock), "DocumentKind.Stock"),
        (typeof(Lot), "DocumentKind.Stock"),
        (typeof(StockMovement), "DocumentKind.StockMovements"),
        (typeof(Reference), "DocumentKind.References"),
        (typeof(Customer), "DocumentKind.Customers"),
        (typeof(Supplier), "DocumentKind.Suppliers"),
        (typeof(Operator), "DocumentKind.Operators"),
        (typeof(Workcenter), "DocumentKind.Workcenters"),
        (typeof(Area), "DocumentKind.Areas"),
        (typeof(Site), "DocumentKind.Sites"),
        (typeof(WarehouseEntity), "DocumentKind.Warehouses"),
        (typeof(Status), "DocumentKind.Statuses"),
        (typeof(StatusTransition), "DocumentKind.StatusTransitions"),
        (typeof(Lifecycle), "DocumentKind.Lifecycles"),
    ];
}

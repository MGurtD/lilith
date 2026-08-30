using Application.Contracts;
using Application.Services.Sales;
using Domain.Entities;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using NSubstitute;
using System.Data;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Sales;

/// <summary>
/// Tests de regressió per als defectes detectats al flux d'albarà.
/// </summary>
public class DeliveryNoteServiceAddOrderTests
{
    [Fact(DisplayName = "Bug 1 – AddOrder ha de rebutjar si l'albarà ja és Entregat")]
    public async Task AddOrder_should_reject_when_deliveryNote_is_Entregat()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(null).Build();
        var deliveryNote = DeliveryNoteBuilder.Default().WithStatusId(entregat.Id).Build();

        var uow = BuildUoW([order], [deliveryNote], entregat);
        var sut = BuildSut(uow);

        var result = await sut.AddOrder(deliveryNote.Id, order);

        Assert.False(result.Result,
            "AddOrder hauria de retornar error quan l'albarà ja és Entregat.");
        Assert.Empty(uow.AddedDeliveryNoteDetails);
        await uow.Transaction.Received(1).RollbackAsync();
        await uow.Transaction.DidNotReceive().CommitAsync();
    }

    [Fact(DisplayName = "AddOrder en albarà pendent ha de tenir èxit")]
    public async Task AddOrder_should_succeed_when_deliveryNote_is_pending()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var pending = NewStatus("Pendent");
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(null).Build();
        var deliveryNote = DeliveryNoteBuilder.Default()
            .WithStatusId(pending.Id)
            .WithCustomerId(order.CustomerId!.Value)
            .Build();

        var uow = BuildUoW([order], [deliveryNote], entregat);
        var sut = BuildSut(uow);

        var result = await sut.AddOrder(deliveryNote.Id, order);

        Assert.True(result.Result);
        await uow.Transaction.Received(1).CommitAsync();
    }

    [Fact(DisplayName = "Bug 2 – AddOrder ha d'usar l'estat persistit de la comanda, no el del DTO")]
    public async Task AddOrder_should_preserve_persisted_order_status_not_stale_dto_status()
    {
        // La comanda persistida a BD ja té StatusId = ComandaServida
        var comandaServida = NewStatus(StatusConstants.Statuses.ComandaServida);
        var comanda = NewStatus(StatusConstants.Statuses.Comanda);
        var pending = NewStatus("Pendent");
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);

        var persistedOrder = SalesOrderBuilder.Default().WithDeliveryNoteId(null).Build();
        persistedOrder.StatusId = comandaServida.Id;

        // El DTO obsolet arriba amb el StatusId anterior (Comanda)
        var staleDto = new SalesOrderHeader
        {
            Id = persistedOrder.Id,
            StatusId = comanda.Id,  // obsolet
            SalesOrderDetails = persistedOrder.SalesOrderDetails,
        };

        var deliveryNote = DeliveryNoteBuilder.Default()
            .WithStatusId(pending.Id)
            .WithCustomerId(persistedOrder.CustomerId!.Value)
            .Build();

        var uow = BuildUoW([persistedOrder], [deliveryNote], entregat);
        var sut = BuildSut(uow);

        var result = await sut.AddOrder(deliveryNote.Id, staleDto);

        Assert.True(result.Result, "AddOrder ha de tenir èxit amb albarà pendent.");

        // La comanda actualitzada a BD ha de mantenir StatusId = ComandaServida,
        // no el StatusId obsolet del DTO (Comanda).
        var updated = uow.UpdatedSalesOrders.Single();
        Assert.Equal(comandaServida.Id, updated.StatusId);
    }

    [Fact(DisplayName = "AddOrder ha de rebutjar si l'albarà no existeix")]
    public async Task AddOrder_should_reject_when_deliveryNote_does_not_exist()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(null).Build();
        var uow = BuildUoW([order], [], entregat);
        var sut = BuildSut(uow);

        var nonExistentId = Guid.NewGuid();
        var result = await sut.AddOrder(nonExistentId, order);

        Assert.False(result.Result, "AddOrder hauria de rebutjar si l'albarà no existeix a BD.");
        Assert.Empty(uow.AddedDeliveryNoteDetails);
    }

    [Fact(DisplayName = "AddOrder ha de rebutjar si l'albarà ja està facturat")]
    public async Task AddOrder_should_reject_when_deliveryNote_is_invoiced()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var pending = NewStatus("Pendent");
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(null).Build();
        var deliveryNote = DeliveryNoteBuilder.Default()
            .WithStatusId(pending.Id)
            .WithSalesInvoiceId(Guid.NewGuid())
            .Build();

        var uow = BuildUoW([order], [deliveryNote], entregat);
        var sut = BuildSut(uow);

        var result = await sut.AddOrder(deliveryNote.Id, order);

        Assert.False(result.Result, "AddOrder hauria de rebutjar si l'albarà ja està facturat.");
        Assert.Empty(uow.AddedDeliveryNoteDetails);
    }

    [Fact(DisplayName = "AddOrder ha de rebutjar si el client de la comanda no coincideix amb el de l'albarà")]
    public async Task AddOrder_should_reject_when_customer_mismatch()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var pending = NewStatus("Pendent");
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(null).Build();
        // L'albarà té un CustomerId diferent al de la comanda (no passem customerId del pedido)
        var deliveryNote = DeliveryNoteBuilder.Default().WithStatusId(pending.Id).Build(); // CustomerId aleatori ≠ order.CustomerId

        var uow = BuildUoW([order], [deliveryNote], entregat);
        var sut = BuildSut(uow);

        var result = await sut.AddOrder(deliveryNote.Id, order);

        Assert.False(result.Result, "AddOrder hauria de rebutjar si el client no coincideix.");
        Assert.Empty(uow.AddedDeliveryNoteDetails);
    }

    // --- Tests RemoveOrder ---

    [Fact(DisplayName = "RemoveOrder ha de rebutjar si l'albarà no existeix")]
    public async Task RemoveOrder_should_reject_when_deliveryNote_does_not_exist()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(Guid.NewGuid()).Build();
        var uow = BuildUoW([order], [], entregat);
        var sut = BuildSut(uow);

        var result = await sut.RemoveOrder(Guid.NewGuid(), order);

        Assert.False(result.Result, "RemoveOrder hauria de rebutjar si l'albarà no existeix a BD.");
    }

    [Fact(DisplayName = "RemoveOrder ha de rebutjar si l'albarà ja és Entregat")]
    public async Task RemoveOrder_should_reject_when_deliveryNote_is_Entregat()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(null).Build();
        var deliveryNote = DeliveryNoteBuilder.Default().WithStatusId(entregat.Id).Build();
        order.DeliveryNoteId = deliveryNote.Id;

        var uow = BuildUoW([order], [deliveryNote], entregat);
        var sut = BuildSut(uow);

        var result = await sut.RemoveOrder(deliveryNote.Id, order);

        Assert.False(result.Result, "RemoveOrder hauria de rebutjar si l'albarà és Entregat.");
    }

    [Fact(DisplayName = "RemoveOrder ha de rebutjar si l'albarà ja està facturat")]
    public async Task RemoveOrder_should_reject_when_deliveryNote_is_invoiced()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var pending = NewStatus("Pendent");
        var deliveryNote = DeliveryNoteBuilder.Default()
            .WithStatusId(pending.Id)
            .WithSalesInvoiceId(Guid.NewGuid())
            .Build();
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(deliveryNote.Id).Build();

        var uow = BuildUoW([order], [deliveryNote], entregat);
        var sut = BuildSut(uow);

        var result = await sut.RemoveOrder(deliveryNote.Id, order);

        Assert.False(result.Result, "RemoveOrder hauria de rebutjar si l'albarà ja està facturat.");
    }

    [Fact(DisplayName = "RemoveOrder ha de rebutjar si la comanda no pertany a l'albarà")]
    public async Task RemoveOrder_should_reject_when_order_not_related_to_deliveryNote()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var pending = NewStatus("Pendent");
        var deliveryNote = DeliveryNoteBuilder.Default().WithStatusId(pending.Id).Build();
        // La comanda apunta a un altre albarà
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(Guid.NewGuid()).Build();

        var uow = BuildUoW([order], [deliveryNote], entregat);
        var sut = BuildSut(uow);

        var result = await sut.RemoveOrder(deliveryNote.Id, order);

        Assert.False(result.Result, "RemoveOrder hauria de rebutjar si la comanda no pertany a l'albarà.");
    }

    [Fact(DisplayName = "RemoveOrder ha de tenir èxit i netejar DeliveryNoteId de la comanda persistida")]
    public async Task RemoveOrder_should_succeed_and_clear_DeliveryNoteId_on_persisted_order()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var pending = NewStatus("Pendent");
        var deliveryNote = DeliveryNoteBuilder.Default().WithStatusId(pending.Id).Build();
        var order = SalesOrderBuilder.Default()
            .WithDeliveryNoteId(deliveryNote.Id)
            .WithStatusId(NewStatus(StatusConstants.Statuses.ComandaServida).Id)
            .Build();
        order.SalesOrderDetails.First().IsDelivered = true;

        // Afegir un DeliveryNoteDetail lligat a l'albarà i al detall de la comanda
        var detail = new DeliveryNoteDetail
        {
            Id = Guid.NewGuid(),
            DeliveryNoteId = deliveryNote.Id,
            SalesOrderDetailId = order.SalesOrderDetails.First().Id,
        };

        var uow = BuildUoW([order], [deliveryNote], entregat, [detail]);
        var sut = BuildSut(uow);

        var result = await sut.RemoveOrder(deliveryNote.Id, order);

        Assert.True(result.Result, "RemoveOrder hauria de tenir èxit.");

        // La comanda persistida ha de tenir DeliveryNoteId = null
        var updated = uow.UpdatedSalesOrders.Single();
        Assert.Null(updated.DeliveryNoteId);
        Assert.Equal(uow.ComandaStatus.Id, updated.StatusId);
        Assert.All(updated.SalesOrderDetails, orderDetail => Assert.False(orderDetail.IsDelivered));
        await uow.SalesOrderService.Received(1).UpdateDetail(order.SalesOrderDetails.First());
    }

    [Fact(DisplayName = "RemoveOrder ha d'eliminar NOMÉS els DeliveryNoteDetails d'aquest albarà, no d'altres")]
    public async Task RemoveOrder_should_remove_only_details_of_this_deliveryNote()
    {
        var entregat = NewStatus(StatusConstants.Statuses.Entregat);
        var pending = NewStatus("Pendent");
        var deliveryNote = DeliveryNoteBuilder.Default().WithStatusId(pending.Id).Build();
        var otherDeliveryNoteId = Guid.NewGuid();
        var order = SalesOrderBuilder.Default().WithDeliveryNoteId(deliveryNote.Id).Build();

        var salesDetailId = order.SalesOrderDetails.First().Id;

        // Detall del nostre albarà — ha de ser eliminat
        var detailOurs = new DeliveryNoteDetail
        {
            Id = Guid.NewGuid(),
            DeliveryNoteId = deliveryNote.Id,
            SalesOrderDetailId = salesDetailId,
        };
        // Detall d'un altre albarà amb el mateix SalesOrderDetailId — NO ha de ser eliminat
        var detailOther = new DeliveryNoteDetail
        {
            Id = Guid.NewGuid(),
            DeliveryNoteId = otherDeliveryNoteId,
            SalesOrderDetailId = salesDetailId,
        };

        var uow = BuildUoW([order], [deliveryNote], entregat, [detailOurs, detailOther]);
        var sut = BuildSut(uow);

        var result = await sut.RemoveOrder(deliveryNote.Id, order);

        Assert.True(result.Result);
        // Només el detall del nostre albarà ha d'haver estat eliminat
        Assert.Single(uow.RemovedDeliveryNoteDetails);
        Assert.Contains(detailOurs, uow.RemovedDeliveryNoteDetails);
        Assert.DoesNotContain(detailOther, uow.RemovedDeliveryNoteDetails);
    }

    // -----------------------------------------------------------------------
    // Builders
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fluent builder for SalesOrderHeader test data with valid defaults.
    /// </summary>
    private sealed class SalesOrderBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid? _statusId = Guid.NewGuid();
        private Guid? _deliveryNoteId;
        private Guid? _customerId = Guid.NewGuid();
        private List<SalesOrderDetail> _details = new()
        {
            new() { Id = Guid.NewGuid(), ReferenceId = Guid.NewGuid(), Description = "Test", Quantity = 1, UnitPrice = 10m, Amount = 10m }
        };

        public static SalesOrderBuilder Default() => new();

        public SalesOrderBuilder WithDeliveryNoteId(Guid? deliveryNoteId) { _deliveryNoteId = deliveryNoteId; return this; }
        public SalesOrderBuilder WithCustomerId(Guid customerId) { _customerId = customerId; return this; }
        public SalesOrderBuilder WithStatusId(Guid? statusId) { _statusId = statusId; return this; }

        public SalesOrderHeader Build() => new()
        {
            Id = _id,
            StatusId = _statusId,
            DeliveryNoteId = _deliveryNoteId,
            CustomerId = _customerId,
            SalesOrderDetails = _details,
        };
    }

    /// <summary>
    /// Fluent builder for DeliveryNote test data with valid defaults.
    /// </summary>
    private sealed class DeliveryNoteBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _statusId = Guid.NewGuid();
        private Guid? _salesInvoiceId;
        private Guid _customerId = Guid.NewGuid();
        private Guid _exerciseId = Guid.NewGuid();
        private Guid _siteId = Guid.NewGuid();
        private string _number = "ALB-001";

        public static DeliveryNoteBuilder Default() => new();

        public DeliveryNoteBuilder WithStatusId(Guid statusId) { _statusId = statusId; return this; }
        public DeliveryNoteBuilder WithSalesInvoiceId(Guid? salesInvoiceId) { _salesInvoiceId = salesInvoiceId; return this; }
        public DeliveryNoteBuilder WithCustomerId(Guid customerId) { _customerId = customerId; return this; }

        public DeliveryNote Build() => new()
        {
            Id = _id,
            Number = _number,
            StatusId = _statusId,
            CustomerId = _customerId,
            ExerciseId = _exerciseId,
            SiteId = _siteId,
            SalesInvoiceId = _salesInvoiceId,
        };
    }

    // -----------------------------------------------------------------------
    // Test infrastructure helpers
    // -----------------------------------------------------------------------

    private static Status NewStatus(string name) => new() { Id = Guid.NewGuid(), Name = name };

    /// <summary>
    /// Builds substitutes for the dependencies used by AddOrder and RemoveOrder.
    /// </summary>
    private static TestContext BuildUoW(
        IEnumerable<SalesOrderHeader> salesOrders,
        IEnumerable<DeliveryNote> deliveryNotes,
        Status deliveredStatus,
        IEnumerable<DeliveryNoteDetail>? deliveryNoteDetails = null)
    {
        var comandaStatus = NewStatus(StatusConstants.Statuses.Comanda);
        return new TestContext(salesOrders, deliveryNotes, deliveredStatus, comandaStatus, deliveryNoteDetails);
    }

    private static DeliveryNoteService BuildSut(TestContext context) =>
        new(context.UnitOfWork,
            Substitute.For<IEnterpriseService>(),
            Substitute.For<IStockMovementService>(),
            context.SalesOrderService,
            Substitute.For<IExerciseService>(),
            BuildFakeLocalization());

    private static ILocalizationService BuildFakeLocalization()
    {
        var loc = Substitute.For<ILocalizationService>();
        loc.GetLocalizedString(Arg.Any<string>(), Arg.Any<object[]>()).Returns(x => (string)x[0]);
        loc.GetLocalizedString(Arg.Any<string>()).Returns(x => (string)x[0]);
        return loc;
    }

    private sealed class TestContext
    {
        public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();
        public IUnitOfWorkTransaction Transaction { get; } = Substitute.For<IUnitOfWorkTransaction>();
        public ISalesOrderService SalesOrderService { get; } = Substitute.For<ISalesOrderService>();
        public List<DeliveryNoteDetail> AddedDeliveryNoteDetails { get; } = [];
        public List<DeliveryNoteDetail> RemovedDeliveryNoteDetails { get; } = [];
        public List<SalesOrderHeader> UpdatedSalesOrders { get; } = [];
        public Status ComandaStatus { get; }

        public TestContext(
            IEnumerable<SalesOrderHeader> salesOrders,
            IEnumerable<DeliveryNote> deliveryNotes,
            Status deliveredStatus,
            Status comandaStatus,
            IEnumerable<DeliveryNoteDetail>? deliveryNoteDetails = null)
        {
            ComandaStatus = comandaStatus;
            var orderStore = salesOrders.ToList();
            var deliveryNoteStore = deliveryNotes.ToList();
            var detailStore = (deliveryNoteDetails ?? []).ToList();

            var salesOrderRepository = Substitute.For<ISalesOrderHeaderRepository>();
            salesOrderRepository
                .Find(Arg.Any<Expression<Func<SalesOrderHeader, bool>>>())
                .Returns(call => orderStore
                    .AsQueryable()
                    .Where(call.Arg<Expression<Func<SalesOrderHeader, bool>>>())
                    .ToList());

            var detailRepository = Substitute.For<IRepository<DeliveryNoteDetail, Guid>>();
            detailRepository
                .Find(Arg.Any<Expression<Func<DeliveryNoteDetail, bool>>>())
                .Returns(call => detailStore
                    .AsQueryable()
                    .Where(call.Arg<Expression<Func<DeliveryNoteDetail, bool>>>())
                    .ToList());
            detailRepository
                .AddRange(Arg.Any<IEnumerable<DeliveryNoteDetail>>())
                .Returns(call =>
                {
                    AddedDeliveryNoteDetails.AddRange(call.Arg<IEnumerable<DeliveryNoteDetail>>());
                    return Task.CompletedTask;
                });
            detailRepository
                .RemoveRange(Arg.Any<IEnumerable<DeliveryNoteDetail>>())
                .Returns(call =>
                {
                    var removed = call.Arg<IEnumerable<DeliveryNoteDetail>>().ToList();
                    RemovedDeliveryNoteDetails.AddRange(removed);
                    detailStore.RemoveAll(removed.Contains);
                    return Task.CompletedTask;
                });

            var deliveryNoteRepository = Substitute.For<IDeliveryNoteRepository>();
            deliveryNoteRepository.Details.Returns(detailRepository);
            deliveryNoteRepository
                .Get(Arg.Any<Guid>())
                .Returns(call => Task.FromResult(
                    deliveryNoteStore.FirstOrDefault(note => note.Id == call.Arg<Guid>())));

            var lifecycles = Substitute.For<ILifecycleRepository>();
            lifecycles
                .GetStatusByName(StatusConstants.Lifecycles.DeliveryNote, StatusConstants.Statuses.Entregat)
                .Returns(deliveredStatus);
            lifecycles
                .GetStatusByName(StatusConstants.Lifecycles.SalesOrder, StatusConstants.Statuses.Comanda)
                .Returns(comandaStatus);

            UnitOfWork.SalesOrderHeaders.Returns(salesOrderRepository);
            UnitOfWork.DeliveryNotes.Returns(deliveryNoteRepository);
            UnitOfWork.Lifecycles.Returns(lifecycles);
            UnitOfWork
                .BeginTransactionAsync(IsolationLevel.Serializable)
                .Returns(Transaction);

            SalesOrderService
                .Update(Arg.Any<SalesOrderHeader>())
                .Returns(call =>
                {
                    UpdatedSalesOrders.Add(call.Arg<SalesOrderHeader>());
                    return Task.FromResult(new GenericResponse(true));
                });
            SalesOrderService
                .UpdateDetail(Arg.Any<SalesOrderDetail>())
                .Returns(Task.FromResult(new GenericResponse(true)));
        }
    }
}

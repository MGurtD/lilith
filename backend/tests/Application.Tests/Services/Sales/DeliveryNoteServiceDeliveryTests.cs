using Application.Contracts;
using Application.Services.Sales;
using Domain.Entities;
using Domain.Entities.Sales;
using Domain.Entities.Warehouse;
using NSubstitute;
using System.Data;
using Xunit;

namespace Application.Tests.Services.Sales;

public class DeliveryNoteServiceDeliveryTests
{
    [Fact]
    public async Task Deliver_should_commit_stock_order_and_delivery_note_updates()
    {
        var context = BuildContext(isDelivered: false);

        var response = await context.Service.Deliver(context.Request);

        Assert.True(response.Result);
        await context.Transaction.Received(1).CommitAsync();
        await context.Transaction.DidNotReceive().RollbackAsync();
        await context.StockMovementService.Received(1).ApplyDeliveryNoteStockBatch(
            Arg.Is<IEnumerable<StockMovement>>(batch =>
                batch.Any(m => m.MovementType == StockMovementType.OUTPUT)));
        await context.SalesOrderService.Received(1).Deliver(context.Request.Id);
        await context.DeliveryNotes.Received(1).Update(
            Arg.Is<DeliveryNote>(note =>
                note.StatusId == context.DeliveredStatus.Id &&
                note.DeliveryDate != null));
    }

    [Fact]
    public async Task Deliver_should_rollback_when_sales_order_update_fails()
    {
        var context = BuildContext(isDelivered: false);
        context.SalesOrderService.Deliver(context.Request.Id)
            .Returns(new GenericResponse(false, "OrderDeliveryFailed"));

        var response = await context.Service.Deliver(context.Request);

        Assert.False(response.Result);
        await context.Transaction.Received(1).RollbackAsync();
        await context.Transaction.DidNotReceive().CommitAsync();
        await context.DeliveryNotes.DidNotReceive().Update(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task Deliver_should_reject_already_delivered_note_without_moving_stock()
    {
        var context = BuildContext(isDelivered: true);

        var response = await context.Service.Deliver(context.Request);

        Assert.False(response.Result);
        await context.Transaction.Received(1).RollbackAsync();
        await context.StockMovementService.DidNotReceive().ApplyDeliveryNoteStockBatch(Arg.Any<IEnumerable<StockMovement>>());
        await context.SalesOrderService.DidNotReceive().Deliver(Arg.Any<Guid>());
    }

    [Fact]
    public async Task UnDeliver_should_commit_stock_return_and_order_updates()
    {
        var context = BuildContext(isDelivered: true);
        context.Request.StatusId = Guid.NewGuid();

        var response = await context.Service.UnDeliver(context.Request);

        Assert.True(response.Result);
        await context.Transaction.Received(1).CommitAsync();
        await context.StockMovementService.Received(1).ApplyDeliveryNoteStockBatch(
            Arg.Is<IEnumerable<StockMovement>>(batch =>
                batch.Any(m => m.MovementType == StockMovementType.INPUT)));
        await context.SalesOrderService.Received(1).UnDeliver(context.Request.Id);
        await context.DeliveryNotes.Received(1).Update(
            Arg.Is<DeliveryNote>(note => note.DeliveryDate == null));
    }

    [Fact]
    public async Task UnDeliver_should_rollback_when_sales_order_update_fails()
    {
        var context = BuildContext(isDelivered: true);
        context.Request.StatusId = Guid.NewGuid();
        context.SalesOrderService.UnDeliver(context.Request.Id)
            .Returns(new GenericResponse(false, "OrderUndeliveryFailed"));

        var response = await context.Service.UnDeliver(context.Request);

        Assert.False(response.Result);
        await context.Transaction.Received(1).RollbackAsync();
        await context.Transaction.DidNotReceive().CommitAsync();
        await context.DeliveryNotes.DidNotReceive().Update(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task UnDeliver_should_reject_invoiced_delivery_note()
    {
        var context = BuildContext(isDelivered: true);
        context.PersistedDeliveryNote.SalesInvoiceId = Guid.NewGuid();
        context.Request.StatusId = Guid.NewGuid();

        var response = await context.Service.UnDeliver(context.Request);

        Assert.False(response.Result);
        await context.Transaction.Received(1).RollbackAsync();
        await context.StockMovementService.DidNotReceive().ApplyDeliveryNoteStockBatch(Arg.Any<IEnumerable<StockMovement>>());
    }

    private static DeliveryContext BuildContext(bool isDelivered)
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var transaction = Substitute.For<IUnitOfWorkTransaction>();
        var deliveryNotes = Substitute.For<IDeliveryNoteRepository>();
        var lifecycles = Substitute.For<ILifecycleRepository>();
        var stockMovementService = Substitute.For<IStockMovementService>();
        var salesOrderService = Substitute.For<ISalesOrderService>();
        var localization = Substitute.For<ILocalizationService>();

        var deliveredStatus = new Status
        {
            Id = Guid.NewGuid(),
            Name = StatusConstants.Statuses.Entregat,
        };
        var persisted = new DeliveryNote
        {
            Id = Guid.NewGuid(),
            Number = "ALB-001",
            StatusId = isDelivered ? deliveredStatus.Id : Guid.NewGuid(),
            DeliveryDate = isDelivered ? DateTime.Now : null,
            Details =
            [
                new DeliveryNoteDetail
                {
                    Id = Guid.NewGuid(),
                    ReferenceId = Guid.NewGuid(),
                    Quantity = 2,
                },
            ],
        };
        var request = new DeliveryNote
        {
            Id = persisted.Id,
            Number = persisted.Number,
            StatusId = deliveredStatus.Id,
        };

        unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable).Returns(transaction);
        unitOfWork.DeliveryNotes.Returns(deliveryNotes);
        unitOfWork.Lifecycles.Returns(lifecycles);
        deliveryNotes.Get(persisted.Id).Returns(persisted);
        deliveryNotes.Exists(persisted.Id).Returns(true);
        lifecycles
            .GetStatusByName(StatusConstants.Lifecycles.DeliveryNote, StatusConstants.Statuses.Entregat)
            .Returns(deliveredStatus);
        stockMovementService.Create(Arg.Any<StockMovement>()).Returns(new GenericResponse(true));
        stockMovementService.ApplyDeliveryNoteStockBatch(Arg.Any<IEnumerable<StockMovement>>()).Returns(new GenericResponse(true));
        salesOrderService.Deliver(persisted.Id).Returns(new GenericResponse(true));
        salesOrderService.UnDeliver(persisted.Id).Returns(new GenericResponse(true));
        localization.GetLocalizedString(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(call => call.ArgAt<string>(0));
        localization.GetLocalizedString(Arg.Any<string>())
            .Returns(call => call.ArgAt<string>(0));

        var service = new DeliveryNoteService(
            unitOfWork,
            Substitute.For<IEnterpriseService>(),
            stockMovementService,
            salesOrderService,
            Substitute.For<IExerciseService>(),
            localization);

        return new DeliveryContext(
            service,
            transaction,
            deliveryNotes,
            stockMovementService,
            salesOrderService,
            request,
            persisted,
            deliveredStatus);
    }

    private sealed record DeliveryContext(
        DeliveryNoteService Service,
        IUnitOfWorkTransaction Transaction,
        IDeliveryNoteRepository DeliveryNotes,
        IStockMovementService StockMovementService,
        ISalesOrderService SalesOrderService,
        DeliveryNote Request,
        DeliveryNote PersistedDeliveryNote,
        Status DeliveredStatus);
}

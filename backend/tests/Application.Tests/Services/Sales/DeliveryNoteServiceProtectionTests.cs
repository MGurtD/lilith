using Application.Contracts;
using Application.Services.Sales;
using Domain.Entities;
using Domain.Entities.Sales;
using NSubstitute;
using System.Data;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Sales;

public class DeliveryNoteServiceProtectionTests
{
    [Fact]
    public async Task Update_should_reject_delivered_note()
    {
        var context = BuildContext(isDelivered: true);

        var response = await context.Service.Update(Clone(context.DeliveryNote));

        Assert.False(response.Result);
        await context.DeliveryNotes.DidNotReceive().Update(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task Update_should_reject_invoiced_note()
    {
        var context = BuildContext(salesInvoiceId: Guid.NewGuid());

        var response = await context.Service.Update(Clone(context.DeliveryNote));

        Assert.False(response.Result);
        await context.DeliveryNotes.DidNotReceive().Update(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task Update_should_reject_status_change_outside_delivery_flow()
    {
        var context = BuildContext();
        var request = Clone(context.DeliveryNote);
        request.StatusId = Guid.NewGuid();

        var response = await context.Service.Update(request);

        Assert.False(response.Result);
        await context.DeliveryNotes.DidNotReceive().Update(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task Update_should_reject_invoice_link_change()
    {
        var context = BuildContext();
        var request = Clone(context.DeliveryNote);
        request.SalesInvoiceId = Guid.NewGuid();

        var response = await context.Service.Update(request);

        Assert.False(response.Result);
        await context.DeliveryNotes.DidNotReceive().Update(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task Update_should_allow_header_change_when_note_is_pending_and_not_invoiced()
    {
        var context = BuildContext();
        var request = Clone(context.DeliveryNote);
        request.DeliveryDate = DateTime.Today;

        var response = await context.Service.Update(request);

        Assert.True(response.Result);
        await context.DeliveryNotes.Received(1).Update(request);
    }

    [Fact]
    public async Task Remove_should_reject_delivered_note()
    {
        var context = BuildContext(isDelivered: true);

        var response = await context.Service.Remove(context.DeliveryNote.Id);

        Assert.False(response.Result);
        await context.Transaction.Received(1).RollbackAsync();
        await context.DeliveryNotes.DidNotReceive().Remove(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task Remove_should_reject_invoiced_note()
    {
        var context = BuildContext(salesInvoiceId: Guid.NewGuid());

        var response = await context.Service.Remove(context.DeliveryNote.Id);

        Assert.False(response.Result);
        await context.DeliveryNotes.DidNotReceive().Remove(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task Remove_should_reject_note_with_associated_orders()
    {
        var context = BuildContext(hasOrders: true);

        var response = await context.Service.Remove(context.DeliveryNote.Id);

        Assert.False(response.Result);
        await context.DeliveryNotes.DidNotReceive().Remove(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task Remove_should_reject_note_with_linked_invoice_details()
    {
        var context = BuildContext(hasInvoiceDetails: true);

        var response = await context.Service.Remove(context.DeliveryNote.Id);

        Assert.False(response.Result);
        await context.DeliveryNotes.DidNotReceive().Remove(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task Remove_should_commit_when_note_has_no_dependencies()
    {
        var context = BuildContext();

        var response = await context.Service.Remove(context.DeliveryNote.Id);

        Assert.True(response.Result);
        await context.DeliveryNotes.Received(1).Remove(context.DeliveryNote);
        await context.Transaction.Received(1).CommitAsync();
    }

    [Fact]
    public async Task GetDeliveryNotesToInvoice_should_filter_by_customer_delivery_and_invoice()
    {
        var context = BuildContext();
        var customerId = Guid.NewGuid();
        Expression<Func<DeliveryNote, bool>>? predicate = null;
        var expected = new DeliveryNote
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            StatusId = context.DeliveredStatus.Id,
        };
        context.DeliveryNotes
            .Find(Arg.Do<Expression<Func<DeliveryNote, bool>>>(value => predicate = value))
            .Returns([expected]);

        var result = await context.Service.GetDeliveryNotesToInvoice(customerId);

        Assert.Equal([expected], result);
        Assert.NotNull(predicate);
        var matches = predicate.Compile();
        Assert.True(matches(expected));
        Assert.False(matches(new DeliveryNote
        {
            CustomerId = customerId,
            StatusId = Guid.NewGuid(),
        }));
        Assert.False(matches(new DeliveryNote
        {
            CustomerId = customerId,
            StatusId = context.DeliveredStatus.Id,
            SalesInvoiceId = Guid.NewGuid(),
        }));
        Assert.False(matches(new DeliveryNote
        {
            CustomerId = Guid.NewGuid(),
            StatusId = context.DeliveredStatus.Id,
        }));
    }

    private static ProtectionContext BuildContext(
        bool isDelivered = false,
        Guid? salesInvoiceId = null,
        bool hasOrders = false,
        bool hasInvoiceDetails = false)
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var transaction = Substitute.For<IUnitOfWorkTransaction>();
        var deliveryNotes = Substitute.For<IDeliveryNoteRepository>();
        var salesOrders = Substitute.For<ISalesOrderHeaderRepository>();
        var salesInvoices = Substitute.For<ISalesInvoiceRepository>();
        var invoiceDetails = Substitute.For<IRepository<SalesInvoiceDetail, Guid>>();
        var lifecycles = Substitute.For<ILifecycleRepository>();
        var deliveredStatus = new Status
        {
            Id = Guid.NewGuid(),
            Name = StatusConstants.Statuses.Entregat,
        };
        var deliveryNote = new DeliveryNote
        {
            Id = Guid.NewGuid(),
            StatusId = isDelivered ? deliveredStatus.Id : Guid.NewGuid(),
            SalesInvoiceId = salesInvoiceId,
            Details =
            [
                new DeliveryNoteDetail
                {
                    Id = Guid.NewGuid(),
                    DeliveryNoteId = Guid.NewGuid(),
                },
            ],
        };
        deliveryNote.Details.First().DeliveryNoteId = deliveryNote.Id;

        unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable).Returns(transaction);
        unitOfWork.DeliveryNotes.Returns(deliveryNotes);
        unitOfWork.SalesOrderHeaders.Returns(salesOrders);
        unitOfWork.SalesInvoices.Returns(salesInvoices);
        unitOfWork.Lifecycles.Returns(lifecycles);
        salesInvoices.InvoiceDetails.Returns(invoiceDetails);
        deliveryNotes.Get(deliveryNote.Id).Returns(deliveryNote);
        deliveryNotes.Exists(deliveryNote.Id).Returns(true);
        lifecycles
            .GetStatusByName(StatusConstants.Lifecycles.DeliveryNote, StatusConstants.Statuses.Entregat)
            .Returns(deliveredStatus);
        salesOrders
            .Find(Arg.Any<Expression<Func<SalesOrderHeader, bool>>>())
            .Returns(hasOrders
                ? [new SalesOrderHeader { Id = Guid.NewGuid(), DeliveryNoteId = deliveryNote.Id }]
                : []);
        invoiceDetails
            .Find(Arg.Any<Expression<Func<SalesInvoiceDetail, bool>>>())
            .Returns(hasInvoiceDetails
                ?
                [
                    new SalesInvoiceDetail
                    {
                        Id = Guid.NewGuid(),
                        DeliveryNoteDetailId = deliveryNote.Details.First().Id,
                    },
                ]
                : []);

        var localization = Substitute.For<ILocalizationService>();
        localization.GetLocalizedString(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(call => call.ArgAt<string>(0));
        localization.GetLocalizedString(Arg.Any<string>())
            .Returns(call => call.ArgAt<string>(0));

        var service = new DeliveryNoteService(
            unitOfWork,
            Substitute.For<IEnterpriseService>(),
            Substitute.For<IStockMovementService>(),
            Substitute.For<ISalesOrderService>(),
            Substitute.For<IExerciseService>(),
            localization);

        return new ProtectionContext(
            service,
            transaction,
            deliveryNotes,
            deliveryNote,
            deliveredStatus);
    }

    private static DeliveryNote Clone(DeliveryNote source) => new()
    {
        Id = source.Id,
        Number = source.Number,
        StatusId = source.StatusId,
        CustomerId = source.CustomerId,
        ExerciseId = source.ExerciseId,
        SiteId = source.SiteId,
        SalesInvoiceId = source.SalesInvoiceId,
    };

    private sealed record ProtectionContext(
        DeliveryNoteService Service,
        IUnitOfWorkTransaction Transaction,
        IDeliveryNoteRepository DeliveryNotes,
        DeliveryNote DeliveryNote,
        Status DeliveredStatus);
}

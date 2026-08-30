using Application.Contracts;
using Application.Services.Sales;
using Domain.Entities;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Data;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Sales;

public class SalesInvoiceServiceDeliveryNoteTests
{
    [Fact]
    public async Task AddDeliveryNote_should_reject_note_that_is_not_delivered()
    {
        var context = BuildContext(isDelivered: false);

        var response = await context.Service.AddDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.False(response.Result);
        await context.Transaction.Received(1).RollbackAsync();
        Assert.Empty(context.AddedInvoiceDetails);
    }

    [Fact]
    public async Task AddDeliveryNote_should_reject_note_already_linked_to_an_invoice()
    {
        var context = BuildContext(isDelivered: true, linkedInvoiceId: Guid.NewGuid());

        var response = await context.Service.AddDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.False(response.Result);
        await context.Transaction.Received(1).RollbackAsync();
        Assert.Empty(context.AddedInvoiceDetails);
    }

    [Fact]
    public async Task AddDeliveryNote_should_reject_customer_mismatch()
    {
        var context = BuildContext(isDelivered: true, customerMismatch: true);

        var response = await context.Service.AddDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.False(response.Result);
        await context.Transaction.Received(1).RollbackAsync();
        Assert.Empty(context.AddedInvoiceDetails);
    }

    [Fact]
    public async Task AddDeliveryNote_should_update_all_related_orders_and_lines()
    {
        var context = BuildContext(isDelivered: true, orderCount: 2);

        var response = await context.Service.AddDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.True(response.Result);
        await context.Transaction.Received(1).CommitAsync();
        Assert.Equal(context.DeliveryNoteDetails.Count, context.AddedInvoiceDetails.Count);
        Assert.All(context.UpdatedDeliveryNoteDetails, detail => Assert.True(detail.IsInvoiced));
        Assert.Equal(context.Invoice.Id, context.UpdatedDeliveryNote!.SalesInvoiceId);
        Assert.Equal(2, context.UpdatedSalesOrders.Count);
        Assert.All(context.UpdatedSalesOrders, order =>
            Assert.Equal(context.InvoicedOrderStatus.Id, order.StatusId));
    }

    [Fact]
    public async Task AddDeliveryNote_should_succeed_without_related_sales_orders()
    {
        var context = BuildContext(isDelivered: true, orderCount: 0);

        var response = await context.Service.AddDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.True(response.Result);
        Assert.Empty(context.UpdatedSalesOrders);
        await context.Transaction.Received(1).CommitAsync();
    }

    [Fact]
    public async Task AddDeliveryNote_should_use_grouped_update_for_delivery_note_details()
    {
        var context = BuildContext(isDelivered: true, orderCount: 1);

        var response = await context.Service.AddDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.True(response.Result);
        // Els detalls s'actualitzen amb UpdateWithoutSave (flags a true) sense Update individual
        Assert.All(context.UpdatedDeliveryNoteDetails, d => Assert.True(d.IsInvoiced));
        // Update individual al repositori de detalls NO s'ha d'haver cridat per cap detall
        await context.DeliveryNoteDetailsRepository
            .DidNotReceive()
            .Update(Arg.Any<DeliveryNoteDetail>());
        await context.UnitOfWork.Received(2).CompleteAsync();
        await context.DeliveryNotesRepository
            .DidNotReceive()
            .Update(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task AddDeliveryNote_should_use_grouped_update_for_sales_orders()
    {
        var context = BuildContext(isDelivered: true, orderCount: 2);

        await context.Service.AddDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        // Update individual al repositori de comandes NO s'ha d'haver cridat
        await context.SalesOrdersRepository
            .DidNotReceive()
            .Update(Arg.Any<SalesOrderHeader>());
    }

    [Fact]
    public async Task AddDeliveryNote_should_load_missing_reference_taxes_in_one_query()
    {
        var context = BuildContext(
            isDelivered: true,
            orderCount: 1,
            referenceRequiresLookup: true);

        var response = await context.Service.AddDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.True(response.Result);
        context.ReferencesRepository.Received(1)
            .Find(Arg.Any<Expression<Func<Reference, bool>>>());
        await context.ReferencesRepository.DidNotReceive().Get(Arg.Any<Guid>());
        Assert.All(context.AddedInvoiceDetails, detail => Assert.NotEqual(Guid.Empty, detail.TaxId));
    }

    [Fact]
    public async Task RemoveDeliveryNote_should_reject_note_linked_to_another_invoice()
    {
        var context = BuildContext(
            isDelivered: true,
            linkedInvoiceId: Guid.NewGuid(),
            seedInvoiceDetails: true);

        var response = await context.Service.RemoveDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.False(response.Result);
        await context.Transaction.Received(1).RollbackAsync();
        Assert.Empty(context.RemovedInvoiceDetails);
    }

    [Fact]
    public async Task RemoveDeliveryNote_should_unlink_lines_and_restore_all_orders_as_served()
    {
        var context = BuildContext(
            isDelivered: true,
            linkToCurrentInvoice: true,
            orderCount: 2,
            seedInvoiceDetails: true);

        var response = await context.Service.RemoveDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.True(response.Result);
        await context.Transaction.Received(1).CommitAsync();
        Assert.Equal(context.DeliveryNoteDetails.Count, context.RemovedInvoiceDetails.Count);
        Assert.All(context.UpdatedDeliveryNoteDetails, detail => Assert.False(detail.IsInvoiced));
        Assert.Null(context.UpdatedDeliveryNote!.SalesInvoiceId);
        Assert.All(context.UpdatedSalesOrders, order =>
            Assert.Equal(context.ServedOrderStatus.Id, order.StatusId));
    }

    [Fact]
    public async Task RemoveDeliveryNote_should_restore_order_as_pending_when_note_is_not_delivered()
    {
        var context = BuildContext(
            isDelivered: false,
            linkToCurrentInvoice: true,
            orderCount: 1,
            seedInvoiceDetails: true);

        var response = await context.Service.RemoveDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.True(response.Result);
        Assert.Equal(context.PendingOrderStatus.Id, context.UpdatedSalesOrders.Single().StatusId);
    }

    [Fact]
    public async Task RemoveDeliveryNote_should_use_grouped_update_for_delivery_note_details()
    {
        var context = BuildContext(
            isDelivered: true,
            linkToCurrentInvoice: true,
            orderCount: 1,
            seedInvoiceDetails: true);

        var response = await context.Service.RemoveDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        Assert.True(response.Result);
        Assert.All(context.UpdatedDeliveryNoteDetails, d => Assert.False(d.IsInvoiced));
        // Update individual al repositori de detalls NO s'ha d'haver cridat per cap detall
        await context.DeliveryNoteDetailsRepository
            .DidNotReceive()
            .Update(Arg.Any<DeliveryNoteDetail>());
        await context.UnitOfWork.Received(2).CompleteAsync();
        await context.DeliveryNotesRepository
            .DidNotReceive()
            .Update(Arg.Any<DeliveryNote>());
    }

    [Fact]
    public async Task RemoveDeliveryNote_should_use_grouped_update_for_sales_orders()
    {
        var context = BuildContext(
            isDelivered: true,
            linkToCurrentInvoice: true,
            orderCount: 2,
            seedInvoiceDetails: true);

        await context.Service.RemoveDeliveryNote(context.Invoice.Id, context.DeliveryNote);

        // Update individual al repositori de comandes NO s'ha d'haver cridat
        await context.SalesOrdersRepository
            .DidNotReceive()
            .Update(Arg.Any<SalesOrderHeader>());
    }

    private static InvoiceContext BuildContext(
        bool isDelivered,
        bool linkToCurrentInvoice = false,
        Guid? linkedInvoiceId = null,
        int orderCount = 1,
        bool seedInvoiceDetails = false,
        bool customerMismatch = false,
        bool referenceRequiresLookup = false)
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var transaction = Substitute.For<IUnitOfWorkTransaction>();
        var invoices = Substitute.For<ISalesInvoiceRepository>();
        var deliveryNotes = Substitute.For<IDeliveryNoteRepository>();
        var invoiceDetailsRepository = Substitute.For<IRepository<SalesInvoiceDetail, Guid>>();
        var invoiceImportsRepository = Substitute.For<IRepository<SalesInvoiceImport, Guid>>();
        var invoiceDueDatesRepository = Substitute.For<IRepository<SalesInvoiceDueDate, Guid>>();
        var deliveryNoteDetailsRepository = Substitute.For<IRepository<DeliveryNoteDetail, Guid>>();
        var salesOrdersRepository = Substitute.For<ISalesOrderHeaderRepository>();
        var lifecycles = Substitute.For<ILifecycleRepository>();
        var taxes = Substitute.For<IRepository<Tax, Guid>>();
        var references = Substitute.For<IRepository<Reference, Guid>>();
        var paymentMethods = Substitute.For<IRepository<PaymentMethod, Guid>>();
        var dueDateService = Substitute.For<IDueDateService>();

        var customerId = Guid.NewGuid();
        var paymentMethod = new PaymentMethod { Id = Guid.NewGuid(), Name = "Transfer" };
        var tax = new Tax { Id = Guid.NewGuid(), Name = "IVA 21", Percentatge = 21 };
        var deliveredStatus = NewStatus(StatusConstants.Statuses.Entregat);
        var invoicedOrderStatus = NewStatus(StatusConstants.Statuses.ComandaFacturada);
        var servedOrderStatus = NewStatus(StatusConstants.Statuses.ComandaServida);
        var pendingOrderStatus = NewStatus(StatusConstants.Statuses.Comanda);
        var invoice = new SalesInvoice
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PaymentMethodId = paymentMethod.Id,
            InvoiceDate = DateTime.Today,
        };
        var deliveryNote = new DeliveryNote
        {
            Id = Guid.NewGuid(),
            CustomerId = customerMismatch ? Guid.NewGuid() : customerId,
            StatusId = isDelivered ? deliveredStatus.Id : Guid.NewGuid(),
            SalesInvoiceId = linkToCurrentInvoice ? invoice.Id : linkedInvoiceId,
        };
        var reference = new Reference { Id = Guid.NewGuid(), TaxId = tax.Id };
        var deliveryNoteDetails = new List<DeliveryNoteDetail>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DeliveryNoteId = deliveryNote.Id,
                ReferenceId = reference.Id,
                Reference = referenceRequiresLookup ? null : reference,
                Description = "Producte",
                Quantity = 2,
                UnitPrice = 10,
                Amount = 20,
                IsInvoiced = seedInvoiceDetails,
            },
        };
        var salesOrders = Enumerable.Range(0, orderCount)
            .Select(_ => new SalesOrderHeader
            {
                Id = Guid.NewGuid(),
                DeliveryNoteId = deliveryNote.Id,
                StatusId = seedInvoiceDetails ? invoicedOrderStatus.Id : servedOrderStatus.Id,
            })
            .ToList();
        var invoiceDetailStore = seedInvoiceDetails
            ? deliveryNoteDetails.Select(detail => new SalesInvoiceDetail
            {
                Id = Guid.NewGuid(),
                SalesInvoiceId = invoice.Id,
                DeliveryNoteDetailId = detail.Id,
                TaxId = tax.Id,
                Amount = detail.Amount,
            }).ToList()
            : [];
        var invoiceImportStore = new List<SalesInvoiceImport>();
        var addedInvoiceDetails = new List<SalesInvoiceDetail>();
        var removedInvoiceDetails = new List<SalesInvoiceDetail>();
        var updatedDeliveryNoteDetails = new List<DeliveryNoteDetail>();
        var updatedSalesOrders = new List<SalesOrderHeader>();
        DeliveryNote? updatedDeliveryNote = null;

        unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable).Returns(transaction);
        unitOfWork.SalesInvoices.Returns(invoices);
        unitOfWork.DeliveryNotes.Returns(deliveryNotes);
        unitOfWork.SalesOrderHeaders.Returns(salesOrdersRepository);
        unitOfWork.Lifecycles.Returns(lifecycles);
        unitOfWork.Taxes.Returns(taxes);
        unitOfWork.References.Returns(references);
        unitOfWork.PaymentMethods.Returns(paymentMethods);
        unitOfWork.CompleteAsync().Returns(Task.FromResult(0));
        invoices.InvoiceDetails.Returns(invoiceDetailsRepository);
        invoices.InvoiceImports.Returns(invoiceImportsRepository);
        invoices.InvoiceDueDates.Returns(invoiceDueDatesRepository);
        deliveryNotes.Details.Returns(deliveryNoteDetailsRepository);

        invoices.GetHeader(invoice.Id).Returns(invoice);
        invoices.Get(invoice.Id).Returns(invoice);
        deliveryNotes.Get(deliveryNote.Id).Returns(deliveryNote);
        paymentMethods.Get(paymentMethod.Id).Returns(paymentMethod);
        dueDateService.GenerateDueDates(paymentMethod, invoice.InvoiceDate, Arg.Any<decimal>()).Returns([]);
        taxes.Find(Arg.Any<Expression<Func<Tax, bool>>>()).Returns([tax]);
        taxes.Get(tax.Id).Returns(tax);
        references.Find(Arg.Any<Expression<Func<Reference, bool>>>()).Returns([reference]);
        deliveryNoteDetailsRepository
            .Find(Arg.Any<Expression<Func<DeliveryNoteDetail, bool>>>())
            .Returns(call => deliveryNoteDetails
                .AsQueryable()
                .Where(call.Arg<Expression<Func<DeliveryNoteDetail, bool>>>())
                .ToList());
        invoiceDetailsRepository
            .Find(Arg.Any<Expression<Func<SalesInvoiceDetail, bool>>>())
            .Returns(call => invoiceDetailStore
                .AsQueryable()
                .Where(call.Arg<Expression<Func<SalesInvoiceDetail, bool>>>())
                .ToList());
        invoiceImportsRepository
            .Find(Arg.Any<Expression<Func<SalesInvoiceImport, bool>>>())
            .Returns(call => invoiceImportStore
                .AsQueryable()
                .Where(call.Arg<Expression<Func<SalesInvoiceImport, bool>>>())
                .ToList());
        invoiceDueDatesRepository
            .Find(Arg.Any<Expression<Func<SalesInvoiceDueDate, bool>>>())
            .Returns([]);
        salesOrdersRepository
            .Find(Arg.Any<Expression<Func<SalesOrderHeader, bool>>>())
            .Returns(call => salesOrders
                .AsQueryable()
                .Where(call.Arg<Expression<Func<SalesOrderHeader, bool>>>())
                .ToList());

        invoiceDetailsRepository.AddRange(Arg.Any<IEnumerable<SalesInvoiceDetail>>())
            .Returns(call =>
            {
                var details = call.Arg<IEnumerable<SalesInvoiceDetail>>().ToList();
                addedInvoiceDetails.AddRange(details);
                invoiceDetailStore.AddRange(details);
                return Task.CompletedTask;
            });
        invoiceDetailsRepository.RemoveRange(Arg.Any<IEnumerable<SalesInvoiceDetail>>())
            .Returns(call =>
            {
                var details = call.Arg<IEnumerable<SalesInvoiceDetail>>().ToList();
                removedInvoiceDetails.AddRange(details);
                invoiceDetailStore.RemoveAll(details.Contains);
                return Task.CompletedTask;
            });
        invoiceImportsRepository.AddRange(Arg.Any<IEnumerable<SalesInvoiceImport>>())
            .Returns(call =>
            {
                invoiceImportStore.AddRange(call.Arg<IEnumerable<SalesInvoiceImport>>());
                return Task.CompletedTask;
            });
        invoiceImportsRepository.RemoveRange(Arg.Any<IEnumerable<SalesInvoiceImport>>())
            .Returns(call =>
            {
                invoiceImportStore.RemoveAll(call.Arg<IEnumerable<SalesInvoiceImport>>().Contains);
                return Task.CompletedTask;
            });
        // UpdateWithoutSave captura els detalls mutats i retorna true
        deliveryNoteDetailsRepository.UpdateWithoutSave(Arg.Any<DeliveryNoteDetail>())
            .Returns(call =>
            {
                updatedDeliveryNoteDetails.Add(call.Arg<DeliveryNoteDetail>());
                return true;
            });
        // UpdateWithoutSave per a comandes captura les comandes mutades
        salesOrdersRepository.UpdateWithoutSave(Arg.Any<SalesOrderHeader>())
            .Returns(call =>
            {
                updatedSalesOrders.Add(call.Arg<SalesOrderHeader>());
                return true;
            });
        deliveryNotes.UpdateWithoutSave(Arg.Any<DeliveryNote>())
            .Returns(call =>
            {
                updatedDeliveryNote = call.Arg<DeliveryNote>();
                return true;
            });

        lifecycles.GetStatusByName(StatusConstants.Lifecycles.DeliveryNote, StatusConstants.Statuses.Entregat)
            .Returns(deliveredStatus);
        lifecycles.GetStatusByName(StatusConstants.Lifecycles.SalesOrder, StatusConstants.Statuses.ComandaFacturada)
            .Returns(invoicedOrderStatus);
        lifecycles.GetStatusByName(StatusConstants.Lifecycles.SalesOrder, StatusConstants.Statuses.ComandaServida)
            .Returns(servedOrderStatus);
        lifecycles.GetStatusByName(StatusConstants.Lifecycles.SalesOrder, StatusConstants.Statuses.Comanda)
            .Returns(pendingOrderStatus);

        var localization = Substitute.For<ILocalizationService>();
        localization.GetLocalizedString(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(call => call.ArgAt<string>(0));
        localization.GetLocalizedString(Arg.Any<string>())
            .Returns(call => call.ArgAt<string>(0));

        var service = new SalesInvoiceService(
            unitOfWork,
            Substitute.For<IEnterpriseService>(),
            dueDateService,
            Substitute.For<IDeliveryNoteService>(),
            Substitute.For<IExerciseService>(),
            localization,
            Substitute.For<ILogger<SalesInvoiceService>>());

        return new InvoiceContext(
            service,
            unitOfWork,
            transaction,
            invoice,
            deliveryNote,
            deliveryNoteDetails,
            addedInvoiceDetails,
            removedInvoiceDetails,
            updatedDeliveryNoteDetails,
            updatedSalesOrders,
            () => updatedDeliveryNote,
            invoicedOrderStatus,
            servedOrderStatus,
            pendingOrderStatus,
            deliveryNotes,
            references,
            deliveryNoteDetailsRepository,
            salesOrdersRepository);
    }

    private static Status NewStatus(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
    };

    private sealed record InvoiceContext(
        SalesInvoiceService Service,
        IUnitOfWork UnitOfWork,
        IUnitOfWorkTransaction Transaction,
        SalesInvoice Invoice,
        DeliveryNote DeliveryNote,
        List<DeliveryNoteDetail> DeliveryNoteDetails,
        List<SalesInvoiceDetail> AddedInvoiceDetails,
        List<SalesInvoiceDetail> RemovedInvoiceDetails,
        List<DeliveryNoteDetail> UpdatedDeliveryNoteDetails,
        List<SalesOrderHeader> UpdatedSalesOrders,
        Func<DeliveryNote?> GetUpdatedDeliveryNote,
        Status InvoicedOrderStatus,
        Status ServedOrderStatus,
        Status PendingOrderStatus,
        IDeliveryNoteRepository DeliveryNotesRepository,
        IRepository<Reference, Guid> ReferencesRepository,
        IRepository<DeliveryNoteDetail, Guid> DeliveryNoteDetailsRepository,
        ISalesOrderHeaderRepository SalesOrdersRepository)
    {
        public DeliveryNote? UpdatedDeliveryNote => GetUpdatedDeliveryNote();
    }
}

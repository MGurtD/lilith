using System.Data;
using System.Linq.Expressions;
using Application.Contracts;
using Application.Contracts.Ingestion;
using Application.Services.Purchase;
using Application.Tests.TestSupport;
using Domain.Entities;
using Domain.Entities.Purchase;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Purchase;

/// <summary>
/// Unit tests for receipt suggestions and linking in <see cref="PurchaseInvoiceService"/> — issue #78.
/// </summary>
public class PurchaseInvoiceReceiptsTests
{
    private static readonly Guid SupplierId = Guid.NewGuid();

    [Fact]
    public async Task Suggests_the_receipt_whose_delivery_note_number_is_on_the_invoice()
    {
        var match = Receipt("A2025/4501", 151.24m);
        var context = BuildSut(receipts: [Receipt("A2025/4400", 151.24m), match]);

        var candidates = await context.Sut.GetReceiptCandidates(SupplierId, ["a2025-4501"], 151.24m);

        var suggested = Assert.Single(candidates, c => c.Suggested);
        Assert.Equal(match.Id, suggested.Id);
        Assert.Equal(ReceiptMatchReasons.DeliveryNoteNumber, suggested.MatchReason);
    }

    [Fact]
    public async Task Without_a_number_match_suggests_the_receipts_that_add_up_to_the_base()
    {
        var first = Receipt("A1", 100m);
        var second = Receipt("A2", 51.24m);
        var context = BuildSut(receipts: [first, second, Receipt("A3", 30m)]);

        var candidates = await context.Sut.GetReceiptCandidates(SupplierId, ["X-999"], 151.24m);

        Assert.Equal(
            new[] { first.Id, second.Id }.Order(),
            candidates.Where(c => c.Suggested).Select(c => c.Id).Order());
        Assert.All(candidates.Where(c => c.Suggested), c => Assert.Equal(ReceiptMatchReasons.Amount, c.MatchReason));
    }

    [Fact]
    public async Task Ambiguous_amount_combinations_suggest_nothing()
    {
        var context = BuildSut(receipts: [Receipt("A1", 50m), Receipt("A2", 50m), Receipt("A3", 100m)]);

        var candidates = await context.Sut.GetReceiptCandidates(SupplierId, [], 100m);

        Assert.DoesNotContain(candidates, c => c.Suggested);
    }

    [Fact]
    public async Task Receipt_amount_is_the_sum_of_its_active_lines()
    {
        var receipt = Receipt("A1", 0m);
        receipt.Details.Add(new ReceiptDetail { Amount = 40m });
        receipt.Details.Add(new ReceiptDetail { Amount = 60m });
        receipt.Details.Add(new ReceiptDetail { Amount = 999m, Disabled = true });
        var context = BuildSut(receipts: [receipt]);

        var candidate = Assert.Single(await context.Sut.GetReceiptCandidates(SupplierId, [], null));

        Assert.Equal(100m, candidate.Amount);
        Assert.False(candidate.Suggested);
    }

    [Fact]
    public async Task CreateWithReceipts_creates_the_invoice_and_links_the_receipts()
    {
        var receipt = Receipt("A2025/4501", 151.24m);
        var context = BuildSut(receipts: [receipt]);
        var invoice = Invoice();

        var response = await context.Sut.CreateWithReceipts(new CreatePurchaseInvoiceWithReceiptsRequest
        {
            Invoice = invoice,
            ReceiptIds = [receipt.Id],
        });

        Assert.True(response.Result);
        Assert.Equal(invoice.Id, response.Content);
        Assert.Single(context.AddedInvoices);
        var updated = Assert.Single(context.UpdatedReceipts);
        Assert.Equal(invoice.Id, updated.PurchaseInvoiceId);
        await context.Transaction.Received(1).CommitAsync();
    }

    [Fact]
    public async Task CreateWithReceipts_rolls_back_when_a_receipt_is_already_invoiced_or_foreign()
    {
        var invoiced = Receipt("A1", 10m);
        invoiced.PurchaseInvoiceId = Guid.NewGuid();
        var foreign = Receipt("A2", 10m);
        foreign.SupplierId = Guid.NewGuid();
        var context = BuildSut(receipts: [invoiced, foreign]);

        var response = await context.Sut.CreateWithReceipts(new CreatePurchaseInvoiceWithReceiptsRequest
        {
            Invoice = Invoice(),
            ReceiptIds = [invoiced.Id, foreign.Id],
        });

        Assert.False(response.Result);
        Assert.Equal("PurchaseInvoiceReceiptNotInvoiceable", response.ErrorCode);
        Assert.Empty(context.UpdatedReceipts);
        await context.Transaction.Received(1).RollbackAsync();
        await context.Transaction.DidNotReceive().CommitAsync();
    }

    private static Receipt Receipt(string supplierNumber, decimal amount)
    {
        var receipt = new Receipt
        {
            Number = supplierNumber,
            SupplierNumber = supplierNumber,
            SupplierId = SupplierId,
            Date = new DateTime(2025, 11, 21),
        };
        if (amount != 0m) receipt.Details.Add(new ReceiptDetail { Amount = amount });
        return receipt;
    }

    private static PurchaseInvoice Invoice() => new()
    {
        SupplierId = SupplierId,
        SupplierNumber = "3.955",
        ExerciceId = Guid.NewGuid(),
        PurchaseInvoiceDate = new DateTime(2025, 11, 25),
    };

    private static TestContext BuildSut(List<Receipt> receipts)
    {
        var addedInvoices = new List<PurchaseInvoice>();
        var updatedReceipts = new List<Receipt>();

        var invoices = Substitute.For<IPurchaseInvoiceRepository>();
        invoices.FindAsync(Arg.Any<Expression<Func<PurchaseInvoice, bool>>>()).Returns([]);
        invoices.Add(Arg.Any<PurchaseInvoice>()).Returns(call =>
        {
            addedInvoices.Add(call.Arg<PurchaseInvoice>());
            return Task.CompletedTask;
        });

        var receiptRepository = Substitute.For<IReceiptRepository>();
        receiptRepository
            .FindAsync(Arg.Any<Expression<Func<Receipt, bool>>>())
            .Returns(call => receipts.AsQueryable().Where(call.Arg<Expression<Func<Receipt, bool>>>()).ToList());
        receiptRepository
            .FindAsyncWithQueryParams(Arg.Any<Expression<Func<Receipt, bool>>>(), Arg.Any<Func<IQueryable<Receipt>, IQueryable<Receipt>>?>())
            .Returns(call =>
            {
                var query = receipts.AsQueryable().Where(call.Arg<Expression<Func<Receipt, bool>>>());
                var include = call.Arg<Func<IQueryable<Receipt>, IQueryable<Receipt>>?>();
                return (include?.Invoke(query) ?? query).ToList();
            });
        receiptRepository.Update(Arg.Any<Receipt>()).Returns(call =>
        {
            updatedReceipts.Add(call.Arg<Receipt>());
            return Task.CompletedTask;
        });

        var transaction = Substitute.For<IUnitOfWorkTransaction>();
        var uow = Substitute.For<IUnitOfWork>();
        uow.PurchaseInvoices.Returns(invoices);
        uow.Receipts.Returns(receiptRepository);
        uow.BeginTransactionAsync(Arg.Any<IsolationLevel>()).Returns(transaction);

        var exercises = Substitute.For<IExerciseService>();
        exercises.GetExerciceByDate(Arg.Any<DateTime>()).Returns(new Exercise { Name = "2025" });
        exercises.GetNextCounter(Arg.Any<Guid>(), "purchaseinvoice").Returns(new GenericResponse(true, content: "0001"));

        var sut = new PurchaseInvoiceService(uow, exercises, new FormattingLocalizationService());
        return new TestContext(sut, transaction, addedInvoices, updatedReceipts);
    }

    private sealed record TestContext(
        PurchaseInvoiceService Sut,
        IUnitOfWorkTransaction Transaction,
        List<PurchaseInvoice> AddedInvoices,
        List<Receipt> UpdatedReceipts);
}

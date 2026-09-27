using System.Linq.Expressions;
using Application.Contracts;
using Application.Services.Purchase;
using Application.Tests.TestSupport;
using Domain.Entities;
using Domain.Entities.Purchase;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Purchase;

/// <summary>
/// Unit tests for the duplicate rule of <see cref="PurchaseInvoiceService"/> — issue #78.
/// An invoice may not repeat the supplier invoice number of another invoice of the same
/// supplier, on create or on update.
/// </summary>
public class PurchaseInvoiceServiceDuplicateTests
{
    private static readonly Guid SupplierId = Guid.NewGuid();
    private static readonly Guid OtherSupplierId = Guid.NewGuid();

    [Fact]
    public async Task Create_blocks_a_repeated_supplier_invoice_number_and_returns_the_existing_id()
    {
        var existing = Invoice(SupplierId, "F-2026/0042", number: "PF-0007");
        var context = BuildSut(existing);

        var response = await context.Sut.Create(Invoice(SupplierId, " f-2026/0042 "));

        Assert.False(response.Result);
        Assert.Equal("PurchaseInvoiceDuplicate", response.ErrorCode);
        Assert.Equal(existing.Id, response.Content);
        Assert.Equal("PurchaseInvoiceDuplicate|f-2026/0042|PF-0007", Assert.Single(response.Errors));
        await context.Exercises.DidNotReceiveWithAnyArgs().GetNextCounter(default, default!);
        Assert.Empty(context.Added);
    }

    [Theory]
    [InlineData("--")]
    [InlineData("")]
    public async Task Create_ignores_unknown_supplier_invoice_numbers(string supplierNumber)
    {
        var context = BuildSut(Invoice(SupplierId, supplierNumber));

        var response = await context.Sut.Create(Invoice(SupplierId, supplierNumber));

        Assert.True(response.Result);
        Assert.Single(context.Added);
    }

    [Fact]
    public async Task Create_allows_the_same_number_for_another_supplier()
    {
        var context = BuildSut(Invoice(OtherSupplierId, "F-2026/0042"));

        var response = await context.Sut.Create(Invoice(SupplierId, "F-2026/0042"));

        Assert.True(response.Result);
        Assert.Equal("0001", Assert.Single(context.Added).Number);
    }

    [Fact]
    public async Task Create_without_exercise_fails_instead_of_reporting_success()
    {
        var context = BuildSut();
        var invoice = Invoice(SupplierId, "F-1");
        invoice.ExerciceId = null;

        var response = await context.Sut.Create(invoice);

        Assert.False(response.Result);
        Assert.Equal("ExerciseInvalid", Assert.Single(response.Errors));
        Assert.Empty(context.Added);
    }

    [Fact]
    public async Task Update_blocks_changing_to_a_number_used_by_another_invoice()
    {
        var existing = Invoice(SupplierId, "F-2026/0042", number: "PF-0007");
        var edited = Invoice(SupplierId, "F-2026/0042");
        var context = BuildSut(existing, Invoice(SupplierId, "F-2026/0001", id: edited.Id));

        var response = await context.Sut.Update(edited);

        Assert.False(response.Result);
        Assert.Equal(existing.Id, response.Content);
        await context.Invoices.DidNotReceiveWithAnyArgs().Update(default!);
    }

    private static PurchaseInvoice Invoice(Guid supplierId, string supplierNumber, string number = "", Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        SupplierId = supplierId,
        SupplierNumber = supplierNumber,
        Number = number,
        ExerciceId = Guid.NewGuid(),
        PurchaseInvoiceDate = new DateTime(2026, 6, 15),
    };

    private static TestContext BuildSut(params PurchaseInvoice[] seed)
    {
        var store = seed.ToList();
        var added = new List<PurchaseInvoice>();

        var invoices = Substitute.For<IPurchaseInvoiceRepository>();
        invoices
            .FindAsync(Arg.Any<Expression<Func<PurchaseInvoice, bool>>>())
            .Returns(call => store.AsQueryable().Where(call.Arg<Expression<Func<PurchaseInvoice, bool>>>()).ToList());
        invoices
            .Add(Arg.Any<PurchaseInvoice>())
            .Returns(call =>
            {
                added.Add(call.Arg<PurchaseInvoice>());
                return Task.CompletedTask;
            });

        var uow = Substitute.For<IUnitOfWork>();
        uow.PurchaseInvoices.Returns(invoices);

        var exercises = Substitute.For<IExerciseService>();
        exercises.GetExerciceByDate(Arg.Any<DateTime>()).Returns(new Exercise { Name = "2026" });
        exercises.GetNextCounter(Arg.Any<Guid>(), "purchaseinvoice").Returns(new GenericResponse(true, content: "0001"));

        var sut = new PurchaseInvoiceService(uow, exercises, new FormattingLocalizationService());
        return new TestContext(sut, invoices, exercises, added);
    }

    private sealed record TestContext(
        PurchaseInvoiceService Sut,
        IPurchaseInvoiceRepository Invoices,
        IExerciseService Exercises,
        List<PurchaseInvoice> Added);
}

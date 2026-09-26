using System.Linq.Expressions;
using System.Text;
using Application.Contracts;
using Application.Contracts.Ingestion;
using Application.Services.Purchase;
using Application.Tests.TestSupport;
using Domain.Entities;
using Domain.Entities.Purchase;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Purchase;

/// <summary>
/// Unit tests for <see cref="InvoiceIngestionService"/> — issue #78.
/// The extractor is faked; these tests cover catalog resolution and the checks that
/// decide which draft values the operator must review.
/// </summary>
public class InvoiceIngestionServiceTests
{
    private const string ValidCif = "B12345674";
    private static readonly Tax Vat21 = new() { Name = "IVA 21%", Percentatge = 21m };
    private static readonly Tax Vat10 = new() { Name = "IVA 10%", Percentatge = 10m };
    private static readonly Tax Exempt = new() { Name = "Exempt", Percentatge = 0m };
    private static readonly Tax ReverseCharge = new() { Name = "ISP", Percentatge = 0m, IsReverseCharge = true };

    [Fact]
    public async Task Rejects_files_without_pdf_signature_before_calling_the_provider()
    {
        var context = BuildSut(new ExtractedInvoice());

        var exception = await Assert.ThrowsAsync<IngestionException>(
            () => context.Sut.IngestAsync(new MemoryStream(Encoding.ASCII.GetBytes("not a pdf")), "fake.pdf"));

        Assert.Equal(IngestionFailureKind.InvalidFile, exception.Kind);
        await context.Extractor.DidNotReceiveWithAnyArgs().ExtractAsync(default!, default!, default);
    }

    [Fact]
    public async Task Clean_invoice_resolves_supplier_and_tax_without_issues()
    {
        var supplier = new Supplier { ComercialName = "Acme", VatNumber = ValidCif };
        var context = BuildSut(CleanInvoice(vatNumber: "ES-B12.345.674"), suppliers: [supplier]);

        var response = await Ingest(context);

        Assert.Empty(response.Issues);
        Assert.Equal(supplier.Id, response.SupplierId);
        var row = Assert.Single(response.TaxBreakdown);
        Assert.Equal(Vat21.Id, row.TaxId);
        Assert.Equal(121m, response.TotalAmount);
    }

    [Fact]
    public async Task Unknown_rate_keeps_the_row_without_tax_and_flags_it()
    {
        var invoice = CleanInvoice();
        invoice.TaxRows = [new ExtractedTaxRow { TaxRate = 4m, BaseAmount = 100m, TaxAmount = 4m }];
        invoice.TotalAmount = 104m;
        var context = BuildSut(invoice);

        var response = await Ingest(context);

        var row = Assert.Single(response.TaxBreakdown);
        Assert.Null(row.TaxId);
        var issue = Assert.Single(response.Issues);
        Assert.Equal(IngestionIssueCodes.UnknownTaxRate, issue.Code);
        Assert.Equal(IngestionIssueFields.TaxBreakdown, issue.Field);
        Assert.Equal(0, issue.RowIndex);
    }

    [Fact]
    public async Task Zero_rate_prefers_the_plain_tax_over_reverse_charge()
    {
        var invoice = CleanInvoice();
        invoice.TaxRows = [new ExtractedTaxRow { TaxRate = 0m, BaseAmount = 100m, TaxAmount = 0m }];
        invoice.TotalAmount = 100m;
        var context = BuildSut(invoice);

        var response = await Ingest(context);

        Assert.Equal(Exempt.Id, Assert.Single(response.TaxBreakdown).TaxId);
        Assert.Empty(response.Issues);
    }

    [Fact]
    public async Task Surcharge_is_flagged_and_counted_in_the_printed_total()
    {
        var invoice = CleanInvoice();
        invoice.TaxRows = [new ExtractedTaxRow { TaxRate = 21m, BaseAmount = 100m, TaxAmount = 21m, SurchargeRate = 5.2m, SurchargeAmount = 5.2m }];
        invoice.TotalAmount = 126.2m;
        var context = BuildSut(invoice);

        var response = await Ingest(context);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(IngestionIssueCodes.SurchargeNotImported, issue.Code);
        Assert.Equal(0, issue.RowIndex);
        Assert.Equal(Vat21.Id, response.TaxBreakdown[0].TaxId);
    }

    [Fact]
    public async Task Withholding_amount_is_converted_to_a_percentage_over_the_bases()
    {
        var invoice = CleanInvoice();
        invoice.WithholdingAmount = 15m;
        invoice.TotalAmount = 106m;
        var context = BuildSut(invoice);

        var response = await Ingest(context);

        Assert.Equal(15m, response.ExtraTaxPercentatge);
        Assert.Empty(response.Issues);
    }

    [Fact]
    public async Task Tax_amount_that_does_not_match_base_and_rate_is_flagged()
    {
        var invoice = CleanInvoice();
        invoice.TaxRows = [new ExtractedTaxRow { TaxRate = 21m, BaseAmount = 100m, TaxAmount = 12m }];
        invoice.TotalAmount = 112m;
        var context = BuildSut(invoice);

        var response = await Ingest(context);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(IngestionIssueCodes.TaxAmountMismatch, issue.Code);
        Assert.Equal("InvoiceIngestionTaxAmountMismatch|21.00|12.00", issue.Message);
    }

    [Fact]
    public async Task Printed_total_that_does_not_match_the_computed_one_is_flagged()
    {
        var invoice = CleanInvoice();
        invoice.TotalAmount = 130m;
        var context = BuildSut(invoice);

        var response = await Ingest(context);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(IngestionIssueCodes.TotalMismatch, issue.Code);
        Assert.Equal(IngestionIssueFields.NetAmount, issue.Field);
    }

    [Fact]
    public async Task Unknown_supplier_is_flagged_and_left_empty()
    {
        var context = BuildSut(CleanInvoice(), suppliers: []);

        var response = await Ingest(context);

        Assert.Null(response.SupplierId);
        Assert.Equal(IngestionIssueCodes.SupplierNotFound, Assert.Single(response.Issues).Code);
    }

    [Fact]
    public async Task Several_suppliers_with_the_same_vat_number_are_not_auto_selected()
    {
        var context = BuildSut(CleanInvoice(), suppliers:
        [
            new Supplier { ComercialName = "Acme", VatNumber = ValidCif },
            new Supplier { ComercialName = "Acme bis", VatNumber = $"ES{ValidCif}" },
        ]);

        var response = await Ingest(context);

        Assert.Null(response.SupplierId);
        Assert.Equal(IngestionIssueCodes.SupplierAmbiguous, Assert.Single(response.Issues).Code);
    }

    [Fact]
    public async Task Disabled_suppliers_are_ignored()
    {
        var context = BuildSut(CleanInvoice(), suppliers: [new Supplier { VatNumber = ValidCif, Disabled = true }]);

        var response = await Ingest(context);

        Assert.Null(response.SupplierId);
        Assert.Equal(IngestionIssueCodes.SupplierNotFound, Assert.Single(response.Issues).Code);
    }

    [Fact]
    public async Task Spanish_vat_number_with_a_wrong_check_digit_is_flagged()
    {
        var context = BuildSut(CleanInvoice(vatNumber: "B12345678"), suppliers: []);

        var response = await Ingest(context);

        Assert.Contains(response.Issues, i => i.Code == IngestionIssueCodes.InvalidVatNumber);
    }

    [Fact]
    public async Task Missing_header_values_are_flagged()
    {
        var context = BuildSut(new ExtractedInvoice(), suppliers: []);

        var response = await Ingest(context);

        Assert.Equal(
            new[] { IngestionIssueFields.SupplierNumber, IngestionIssueFields.PurchaseInvoiceDate, IngestionIssueFields.NetAmount, IngestionIssueFields.TaxBreakdown, IngestionIssueFields.SupplierId },
            response.Issues.Where(i => i.Code == IngestionIssueCodes.MissingValue).Select(i => i.Field).ToArray());
    }

    [Fact]
    public async Task Only_fields_below_the_confidence_threshold_are_flagged()
    {
        var invoice = CleanInvoice();
        invoice.FieldConfidence = new()
        {
            [ExtractedInvoiceFields.InvoiceNumber] = 0.95m,
            [ExtractedInvoiceFields.IssueDate] = 0.4m,
            [ExtractedInvoiceFields.TaxRow(0, ExtractedInvoiceFields.TaxAmount)] = 0.5m,
        };
        var context = BuildSut(invoice);

        var response = await Ingest(context);

        Assert.Collection(response.Issues,
            issue =>
            {
                Assert.Equal(IngestionIssueFields.PurchaseInvoiceDate, issue.Field);
                Assert.Equal(IngestionIssueCodes.LowConfidence, issue.Code);
                Assert.Equal("InvoiceIngestionLowConfidence|40", issue.Message);
            },
            issue =>
            {
                Assert.Equal(IngestionIssueFields.TaxBreakdown, issue.Field);
                Assert.Equal(0, issue.RowIndex);
            });
    }

    private static Task<IngestPurchaseInvoiceResponse> Ingest(TestContext context) =>
        context.Sut.IngestAsync(new MemoryStream(Encoding.ASCII.GetBytes("%PDF-1.7 fake")), "invoice.pdf");

    private static ExtractedInvoice CleanInvoice(string vatNumber = ValidCif) => new()
    {
        InvoiceNumber = "F-2026/0042",
        IssueDate = new DateTime(2026, 6, 15),
        SupplierVatNumber = vatNumber,
        SupplierName = "Acme SL",
        TaxRows = [new ExtractedTaxRow { TaxRate = 21m, BaseAmount = 100m, TaxAmount = 21m }],
        TotalAmount = 121m,
    };

    private static TestContext BuildSut(ExtractedInvoice extracted, List<Supplier>? suppliers = null)
    {
        suppliers ??= [new Supplier { ComercialName = "Acme", VatNumber = ValidCif }];

        var extractor = Substitute.For<IInvoiceExtractor>();
        extractor.ExtractAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(extracted);

        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository
            .FindAsync(Arg.Any<Expression<Func<Supplier, bool>>>())
            .Returns(call => suppliers.AsQueryable().Where(call.Arg<Expression<Func<Supplier, bool>>>()).ToList());

        var uow = Substitute.For<IUnitOfWork>();
        uow.Suppliers.Returns(supplierRepository);
        uow.Taxes.Returns(new InMemoryRepository<Tax>([Vat21, Vat10, Exempt, ReverseCharge]));

        var settings = Options.Create(new AppSettings
        {
            Ingestion = new IngestionSettings { LowConfidenceThreshold = 0.8m },
        });

        return new TestContext(
            new InvoiceIngestionService(extractor, uow, new FormattingLocalizationService(), settings),
            extractor);
    }

    private sealed record TestContext(InvoiceIngestionService Sut, IInvoiceExtractor Extractor);

    /// <summary>Returns "Key|arg1|arg2" so tests can assert the arguments passed to each message.</summary>
    private sealed class FormattingLocalizationService : ILocalizationService
    {
        public string GetLocalizedString(string key, params object[] arguments) =>
            string.Join('|', new object[] { key }.Concat(arguments));

        public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments) =>
            GetLocalizedString(key, arguments);

        public Dictionary<string, string> GetAllTranslations() => [];
        public Dictionary<string, string> GetAllTranslationsForCulture(string culture) => [];
        public string[] GetSupportedCultures() => ["ca", "es", "en"];
    }
}

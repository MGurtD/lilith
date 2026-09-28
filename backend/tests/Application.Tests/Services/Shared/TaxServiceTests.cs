using Application.Contracts;
using Application.Services.Shared;
using Application.Tests.TestSupport;
using Domain.Entities;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Shared;

/// <summary>
/// <see cref="TaxService.RemoveTax"/> must refuse a tax in use (#159): the
/// invoice tax breakdowns cascade from the tax, so deleting it would delete them.
/// </summary>
public class TaxServiceTests
{
    private static readonly Dictionary<string, string> LocalizationKeys = new()
    {
        ["TaxInUse"] = "L'impost està en ús",
        ["EntityNotFound"] = "L'entitat amb ID {0} no existeix",
    };

    [Fact]
    public async Task RemoveTax_refuses_a_tax_in_use_and_keeps_it()
    {
        var tax = NewTax();
        var context = BuildSut(tax, inUse: true);

        var response = await context.Sut.RemoveTax(tax.Id);

        Assert.False(response.Result);
        Assert.Equal("L'impost està en ús", Assert.Single(response.Errors));
        Assert.Contains(tax, context.Taxes.Items);
    }

    [Fact]
    public async Task RemoveTax_deletes_an_unused_tax()
    {
        var tax = NewTax();
        var context = BuildSut(tax, inUse: false);

        var response = await context.Sut.RemoveTax(tax.Id);

        Assert.True(response.Result);
        Assert.DoesNotContain(tax, context.Taxes.Items);
    }

    private static Tax NewTax() => new() { Id = Guid.NewGuid(), Name = "IVA 10%", Percentatge = 10 };

    private static TestContext BuildSut(Tax tax, bool inUse)
    {
        var taxes = new InMemoryTaxRepository([tax], inUse ? [tax.Id] : []);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Taxes.Returns(taxes);

        var sut = new TaxService(unitOfWork, new KeyedLocalizationService(LocalizationKeys));
        return new TestContext(sut, taxes);
    }

    private sealed record TestContext(TaxService Sut, InMemoryTaxRepository Taxes);
}

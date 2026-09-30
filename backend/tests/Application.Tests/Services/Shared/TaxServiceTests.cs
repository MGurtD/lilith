using Application.Contracts;
using Application.Services.Shared;
using Application.Tests.TestSupport;
using Domain.Entities;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Shared;

/// <summary>
/// <see cref="TaxService.RemoveTax"/> deletes the tax; refusing a tax in use is the
/// repository's job (see MasterDataDeleteGuardTests).
/// </summary>
public class TaxServiceTests
{
    private static readonly Dictionary<string, string> LocalizationKeys = new()
    {
        ["EntityNotFound"] = "L'entitat amb ID {0} no existeix",
    };

    [Fact]
    public async Task RemoveTax_deletes_an_unused_tax()
    {
        var tax = NewTax();
        var context = BuildSut(tax);

        var response = await context.Sut.RemoveTax(tax.Id);

        Assert.True(response.Result);
        Assert.DoesNotContain(tax, context.Taxes.Items);
    }

    private static Tax NewTax() => new() { Id = Guid.NewGuid(), Name = "IVA 10%", Percentatge = 10 };

    private static TestContext BuildSut(Tax tax)
    {
        var taxes = new InMemoryTaxRepository([tax]);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Taxes.Returns(taxes);

        var sut = new TaxService(unitOfWork, new KeyedLocalizationService(LocalizationKeys));
        return new TestContext(sut, taxes);
    }

    private sealed record TestContext(TaxService Sut, InMemoryTaxRepository Taxes);
}

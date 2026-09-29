using Application.Contracts;
using Application.Services.Warehouse;
using Application.Tests.TestSupport;
using Domain.Entities.Warehouse;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Warehouse;

/// <summary>
/// Warehouses and locations in use must not be deleted (#159): locations cascade
/// from the warehouse, stock from the location and stock movements from the stock.
/// </summary>
public class WarehouseDeleteGuardTests
{
    private static readonly Dictionary<string, string> LocalizationKeys = new()
    {
        ["WarehouseInUse"] = "El magatzem {0} està en ús",
        ["LocationInUse"] = "La ubicació {0} està en ús",
        ["LocationNotFound"] = "Ubicació amb ID {0} no existeix",
        ["EntityNotFound"] = "L'entitat amb ID {0} no existeix",
    };

    [Fact]
    public async Task RemoveWarehouse_refuses_a_warehouse_whose_locations_hold_stock()
    {
        var warehouse = new Domain.Entities.Warehouse.Warehouse { Name = "Central" };
        var context = BuildSut(warehouse, new Location(), warehouseInUse: true, locationInUse: false);

        var response = await context.Sut.Remove(warehouse.Id);

        Assert.False(response.Result);
        Assert.Equal("El magatzem Central està en ús", Assert.Single(response.Errors));
        await context.Warehouses.DidNotReceive().Remove(Arg.Any<Domain.Entities.Warehouse.Warehouse>());
    }

    [Fact]
    public async Task RemoveWarehouse_deletes_an_unused_warehouse()
    {
        var warehouse = new Domain.Entities.Warehouse.Warehouse { Name = "Central" };
        var context = BuildSut(warehouse, new Location(), warehouseInUse: false, locationInUse: false);

        Assert.True((await context.Sut.Remove(warehouse.Id)).Result);
        await context.Warehouses.Received(1).Remove(warehouse);
    }

    [Fact]
    public async Task RemoveLocation_refuses_a_location_in_use()
    {
        var location = new Location { Name = "A-01" };
        var context = BuildSut(new Domain.Entities.Warehouse.Warehouse(), location, warehouseInUse: false, locationInUse: true);

        var response = await context.Sut.RemoveLocation(location.Id);

        Assert.False(response.Result);
        Assert.Equal("La ubicació A-01 està en ús", Assert.Single(response.Errors));
        await context.Locations.DidNotReceive().Remove(Arg.Any<Location>());
    }

    [Fact]
    public async Task RemoveLocation_deletes_an_unused_location()
    {
        var location = new Location { Name = "A-01" };
        var context = BuildSut(new Domain.Entities.Warehouse.Warehouse(), location, warehouseInUse: false, locationInUse: false);

        Assert.True((await context.Sut.RemoveLocation(location.Id)).Result);
        await context.Locations.Received(1).Remove(location);
    }

    private static TestContext BuildSut(
        Domain.Entities.Warehouse.Warehouse warehouse, Location location, bool warehouseInUse, bool locationInUse)
    {
        var locations = Substitute.For<IRepository<Location, Guid>>().Serving(location);
        var warehouses = Substitute.For<IWarehouseRepository>().Serving(warehouse);
        warehouses.Locations.Returns(locations);
        warehouses.IsInUse(warehouse.Id).Returns(warehouseInUse);
        warehouses.IsLocationInUse(location.Id).Returns(locationInUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Warehouses.Returns(warehouses);

        var sut = new WarehouseService(unitOfWork, new KeyedLocalizationService(LocalizationKeys));
        return new TestContext(sut, warehouses, locations);
    }

    private sealed record TestContext(
        WarehouseService Sut, IWarehouseRepository Warehouses, IRepository<Location, Guid> Locations);
}

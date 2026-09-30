using Application.Contracts;
using Application.Services.Production;
using Application.Tests.TestSupport;
using Domain.Constants;
using Domain.Entities.Production;
using Domain.Entities.Warehouse;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Production;

/// <summary>
/// Removing a workcenter removes it before its supply locations. A workcenter in use is
/// then refused before anything else changes, and its location links go with it by
/// cascade: removing a link by hand after its location failed with a concurrency error
/// (#159).
/// </summary>
public class WorkcenterServiceRemoveTests
{
    [Fact]
    public async Task Remove_deletes_the_workcenter_before_its_supply_location()
    {
        var (sut, workcenters, links, warehouseService, workcenter, location) = BuildSut();

        var response = await sut.Remove(workcenter.Id);

        Assert.True(response.Result);
        Received.InOrder(() =>
        {
            workcenters.Remove(workcenter);
            warehouseService.RemoveLocation(location.Id);
        });
        await links.DidNotReceive().Remove(Arg.Any<WorkcenterLocation>());
    }

    [Fact]
    public async Task Remove_keeps_the_supply_location_when_it_holds_stock()
    {
        var (sut, _, _, warehouseService, workcenter, location) = BuildSut();
        warehouseService.RemoveLocation(location.Id)
            .Returns<GenericResponse>(_ => throw new EntityInUseException(location.Name, ["DocumentKind.Stock"]));

        var response = await sut.Remove(workcenter.Id);

        Assert.True(response.Result);
    }

    [Fact]
    public async Task Remove_leaves_the_supply_location_alone_when_the_workcenter_is_in_use()
    {
        var (sut, workcenters, _, warehouseService, workcenter, _) = BuildSut();
        workcenters.Remove(workcenter)
            .Returns(_ => throw new EntityInUseException(workcenter.Name, ["DocumentKind.ProductionParts"]));

        await Assert.ThrowsAsync<EntityInUseException>(() => sut.Remove(workcenter.Id));

        await warehouseService.DidNotReceive().RemoveLocation(Arg.Any<Guid>());
    }

    private static (WorkcenterService Sut, IWorkcenterRepository Workcenters, IRepository<WorkcenterLocation, Guid> Links,
        IWarehouseService WarehouseService, Workcenter Workcenter, Location Location) BuildSut()
    {
        var workcenter = new Workcenter { Id = Guid.NewGuid(), Name = "Làser 1" };
        var location = new Location { Id = Guid.NewGuid(), Name = "APR-Làser 1", LocationType = LocationTypeConstants.Supply };
        var link = new WorkcenterLocation { Id = Guid.NewGuid(), WorkcenterId = workcenter.Id, LocationId = location.Id };

        var workcenters = Substitute.For<IWorkcenterRepository>();
        workcenters.Find(Arg.Any<Expression<Func<Workcenter, bool>>>()).Returns([workcenter]);

        var links = Substitute.For<IRepository<WorkcenterLocation, Guid>>();
        links.Find(Arg.Any<Expression<Func<WorkcenterLocation, bool>>>()).Returns([link]);

        var locations = Substitute.For<IRepository<Location, Guid>>();
        locations.Find(Arg.Any<Expression<Func<Location, bool>>>()).Returns([location]);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Workcenters.Returns(workcenters);
        unitOfWork.WorkcenterLocations.Returns(links);
        unitOfWork.Warehouses.Locations.Returns(locations);

        var warehouseService = Substitute.For<IWarehouseService>();
        var sut = new WorkcenterService(unitOfWork, NullLocalizationService.Instance, warehouseService);
        return (sut, workcenters, links, warehouseService, workcenter, location);
    }
}

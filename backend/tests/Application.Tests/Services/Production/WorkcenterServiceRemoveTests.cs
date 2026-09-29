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
/// Removing a workcenter must remove the link to its supply location before the
/// location itself (#159): the link cascades from the location, so removing it
/// afterwards affected no rows and failed with a concurrency error.
/// </summary>
public class WorkcenterServiceRemoveTests
{
    [Fact]
    public async Task Remove_deletes_the_location_link_before_the_supply_location()
    {
        var workcenter = new Workcenter { Id = Guid.NewGuid(), Name = "Làser 1" };
        var location = new Location { Id = Guid.NewGuid(), Name = "APR-Làser 1", LocationType = LocationTypeConstants.Supply };
        var link = new WorkcenterLocation { Id = Guid.NewGuid(), WorkcenterId = workcenter.Id, LocationId = location.Id };

        var workcenters = Substitute.For<IWorkcenterRepository>();
        workcenters.Find(Arg.Any<Expression<Func<Workcenter, bool>>>()).Returns([workcenter]);
        workcenters.IsInUse(workcenter.Id).Returns(false);

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

        var response = await sut.Remove(workcenter.Id);

        Assert.True(response.Result);
        Received.InOrder(() =>
        {
            links.Remove(link);
            warehouseService.RemoveLocation(location.Id);
            workcenters.Remove(workcenter);
        });
    }
}

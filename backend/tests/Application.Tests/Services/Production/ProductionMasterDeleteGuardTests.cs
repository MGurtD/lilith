using Application.Contracts;
using Application.Contracts.Services.Geolocalization;
using Application.Services.Production;
using Application.Tests.TestSupport;
using Domain.Entities.Production;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Production;

/// <summary>
/// Plant model and production masters in use must not be deleted (#159): operators,
/// workcenters, areas, warehouses, delivery notes, production parts, shift history,
/// phase steps and work orders cascade from them.
/// </summary>
public class ProductionMasterDeleteGuardTests
{
    private static readonly Dictionary<string, string> LocalizationKeys = new()
    {
        ["OperatorTypeInUse"] = "El tipus d'operari {0} està en ús",
        ["WorkcenterTypeInUse"] = "El tipus de màquina {0} està en ús",
        ["AreaInUse"] = "L'àrea {0} està en ús",
        ["SiteInUse"] = "El centre {0} està en ús",
        ["EnterpriseInUse"] = "L'empresa {0} està en ús",
        ["WorkcenterInUse"] = "La màquina {0} està en ús",
        ["MachineStatusInUse"] = "L'estat de màquina {0} està en ús",
        ["WorkMasterInUse"] = "La ruta de fabricació està en ús",
        ["EntityNotFound"] = "L'entitat amb ID {0} no existeix",
    };

    private static KeyedLocalizationService Localization() => new(LocalizationKeys);

    // ---------- Operator type ----------

    [Fact]
    public async Task RemoveOperatorType_refuses_a_type_in_use()
    {
        var type = new OperatorType { Name = "Soldador" };
        var (sut, repository) = BuildOperatorTypeSut(type, inUse: true);

        var response = await sut.Remove(type.Id);

        Assert.False(response.Result);
        Assert.Equal("El tipus d'operari Soldador està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<OperatorType>());
    }

    [Fact]
    public async Task RemoveOperatorType_deletes_an_unused_type()
    {
        var type = new OperatorType { Name = "Soldador" };
        var (sut, repository) = BuildOperatorTypeSut(type, inUse: false);

        Assert.True((await sut.Remove(type.Id)).Result);
        await repository.Received(1).Remove(type);
    }

    // ---------- Workcenter type ----------

    [Fact]
    public async Task RemoveWorkcenterType_refuses_a_type_in_use()
    {
        var type = new WorkcenterType { Name = "Làser" };
        var (sut, repository) = BuildWorkcenterTypeSut(type, inUse: true);

        var response = await sut.Remove(type.Id);

        Assert.False(response.Result);
        Assert.Equal("El tipus de màquina Làser està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<WorkcenterType>());
    }

    [Fact]
    public async Task RemoveWorkcenterType_deletes_an_unused_type()
    {
        var type = new WorkcenterType { Name = "Làser" };
        var (sut, repository) = BuildWorkcenterTypeSut(type, inUse: false);

        Assert.True((await sut.Remove(type.Id)).Result);
        await repository.Received(1).Remove(type);
    }

    // ---------- Area ----------

    [Fact]
    public async Task RemoveArea_refuses_an_area_in_use()
    {
        var area = new Area { Name = "Tall" };
        var (sut, repository) = BuildAreaSut(area, inUse: true);

        var response = await sut.Remove(area.Id);

        Assert.False(response.Result);
        Assert.Equal("L'àrea Tall està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<Area>());
    }

    [Fact]
    public async Task RemoveArea_deletes_an_unused_area()
    {
        var area = new Area { Name = "Tall" };
        var (sut, repository) = BuildAreaSut(area, inUse: false);

        Assert.True((await sut.Remove(area.Id)).Result);
        await repository.Received(1).Remove(area);
    }

    // ---------- Site ----------

    [Fact]
    public async Task RemoveSite_refuses_a_site_in_use()
    {
        var site = new Site { Name = "Terrassa" };
        var (sut, repository) = BuildSiteSut(site, inUse: true);

        var response = await sut.Remove(site.Id);

        Assert.False(response.Result);
        Assert.Equal("El centre Terrassa està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<Site>());
    }

    [Fact]
    public async Task RemoveSite_deletes_an_unused_site()
    {
        var site = new Site { Name = "Terrassa" };
        var (sut, repository) = BuildSiteSut(site, inUse: false);

        Assert.True((await sut.Remove(site.Id)).Result);
        await repository.Received(1).Remove(site);
    }

    // ---------- Enterprise ----------

    [Fact]
    public async Task RemoveEnterprise_refuses_an_enterprise_with_sites_and_keeps_its_branding()
    {
        var enterprise = new Enterprise { Name = "Rawcraft" };
        var (sut, repository, branding) = BuildEnterpriseSut(enterprise, inUse: true);

        var response = await sut.Remove(enterprise.Id);

        Assert.False(response.Result);
        Assert.Equal("L'empresa Rawcraft està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<Enterprise>());
        await branding.DidNotReceive().RemoveEnterpriseFiles(Arg.Any<Guid>());
    }

    [Fact]
    public async Task RemoveEnterprise_deletes_an_enterprise_without_sites()
    {
        var enterprise = new Enterprise { Name = "Rawcraft" };
        var (sut, repository, _) = BuildEnterpriseSut(enterprise, inUse: false);

        Assert.True((await sut.Remove(enterprise.Id)).Result);
        await repository.Received(1).Remove(enterprise);
    }

    // ---------- Workcenter ----------

    [Fact]
    public async Task RemoveWorkcenter_refuses_a_workcenter_in_use_and_keeps_its_locations()
    {
        var workcenter = new Workcenter { Name = "Làser 1" };
        var (sut, repository, locationLinks) = BuildWorkcenterSut(workcenter, inUse: true);

        var response = await sut.Remove(workcenter.Id);

        Assert.False(response.Result);
        Assert.Equal("La màquina Làser 1 està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<Workcenter>());
        await locationLinks.DidNotReceive().Remove(Arg.Any<WorkcenterLocation>());
    }

    [Fact]
    public async Task RemoveWorkcenter_deletes_an_unused_workcenter()
    {
        var workcenter = new Workcenter { Name = "Làser 1" };
        var (sut, repository, _) = BuildWorkcenterSut(workcenter, inUse: false);

        Assert.True((await sut.Remove(workcenter.Id)).Result);
        await repository.Received(1).Remove(workcenter);
    }

    // ---------- Machine status ----------

    [Fact]
    public async Task RemoveMachineStatus_refuses_a_status_in_use()
    {
        var status = new MachineStatus { Name = "Producció" };
        var (sut, repository) = BuildMachineStatusSut(status, inUse: true);

        var response = await sut.Remove(status.Id);

        Assert.False(response.Result);
        Assert.Equal("L'estat de màquina Producció està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<MachineStatus>());
    }

    [Fact]
    public async Task RemoveMachineStatus_deletes_an_unused_status()
    {
        var status = new MachineStatus { Name = "Producció" };
        var (sut, repository) = BuildMachineStatusSut(status, inUse: false);

        Assert.True((await sut.Remove(status.Id)).Result);
        await repository.Received(1).Remove(status);
    }

    // ---------- Production route ----------

    [Fact]
    public async Task RemoveWorkMaster_refuses_a_route_in_use()
    {
        var workMaster = new WorkMaster { ReferenceId = Guid.NewGuid() };
        var (sut, repository) = BuildWorkMasterSut(workMaster, inUse: true);

        var response = await sut.Remove(workMaster.Id);

        Assert.False(response.Result);
        Assert.Equal("La ruta de fabricació està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<WorkMaster>());
    }

    [Fact]
    public async Task RemoveWorkMaster_deletes_an_unused_route()
    {
        var workMaster = new WorkMaster { ReferenceId = Guid.NewGuid() };
        var (sut, repository) = BuildWorkMasterSut(workMaster, inUse: false);

        Assert.True((await sut.Remove(workMaster.Id)).Result);
        await repository.Received(1).Remove(workMaster);
    }

    // ---------- Builders ----------

    private static (OperatorTypeService, IOperatorTypeRepository) BuildOperatorTypeSut(OperatorType type, bool inUse)
    {
        var repository = Substitute.For<IOperatorTypeRepository>().Serving(type);
        repository.IsInUse(type.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.OperatorTypes.Returns(repository);
        return (new OperatorTypeService(unitOfWork, Localization()), repository);
    }

    private static (WorkcenterTypeService, IWorkcenterTypeRepository) BuildWorkcenterTypeSut(WorkcenterType type, bool inUse)
    {
        var repository = Substitute.For<IWorkcenterTypeRepository>().Serving(type);
        repository.IsInUse(type.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.WorkcenterTypes.Returns(repository);
        return (new WorkcenterTypeService(unitOfWork, Localization()), repository);
    }

    private static (AreaService, IAreaRepository) BuildAreaSut(Area area, bool inUse)
    {
        var repository = Substitute.For<IAreaRepository>().Serving(area);
        repository.IsInUse(area.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Areas.Returns(repository);
        return (new AreaService(unitOfWork, Localization()), repository);
    }

    private static (SiteService, ISiteRepository) BuildSiteSut(Site site, bool inUse)
    {
        var repository = Substitute.For<ISiteRepository>().Serving(site);
        repository.IsInUse(site.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Sites.Returns(repository);
        return (new SiteService(unitOfWork, Localization(), Substitute.For<IGeolocalizationService>()), repository);
    }

    private static (EnterpriseService, IEnterpriseRepository, IBrandingService) BuildEnterpriseSut(Enterprise enterprise, bool inUse)
    {
        var repository = Substitute.For<IEnterpriseRepository>().Serving(enterprise);
        repository.IsInUse(enterprise.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Enterprises.Returns(repository);
        var branding = Substitute.For<IBrandingService>();
        branding.RemoveEnterpriseFiles(enterprise.Id).Returns(new GenericResponse(true));
        return (new EnterpriseService(unitOfWork, Localization(), branding), repository, branding);
    }

    private static (WorkcenterService, IWorkcenterRepository, IRepository<WorkcenterLocation, Guid>) BuildWorkcenterSut(Workcenter workcenter, bool inUse)
    {
        var repository = Substitute.For<IWorkcenterRepository>().Serving(workcenter);
        repository.IsInUse(workcenter.Id).Returns(inUse);
        var locationLinks = Substitute.For<IRepository<WorkcenterLocation, Guid>>().Serving<IRepository<WorkcenterLocation, Guid>, WorkcenterLocation>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Workcenters.Returns(repository);
        unitOfWork.WorkcenterLocations.Returns(locationLinks);
        var sut = new WorkcenterService(unitOfWork, Localization(), Substitute.For<IWarehouseService>());
        return (sut, repository, locationLinks);
    }

    private static (MachineStatusService, IMachineStatusRepository) BuildMachineStatusSut(MachineStatus status, bool inUse)
    {
        var repository = Substitute.For<IMachineStatusRepository>().Serving(status);
        repository.IsInUse(status.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.MachineStatuses.Returns(repository);
        return (new MachineStatusService(unitOfWork, Localization()), repository);
    }

    private static (WorkMasterService, IWorkMasterRepository) BuildWorkMasterSut(WorkMaster workMaster, bool inUse)
    {
        var repository = Substitute.For<IWorkMasterRepository>().Serving(workMaster);
        repository.IsInUse(workMaster.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.WorkMasters.Returns(repository);
        return (new WorkMasterService(unitOfWork, Substitute.For<IMetricsService>(), Localization()), repository);
    }
}

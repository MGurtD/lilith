using Application.Contracts;
using Application.Services.Production;
using Application.Tests.TestSupport;
using Domain.Entities.Production;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Production;

/// <summary>
/// Validation rules applied when KO units are declared with a rejection reason breakdown.
/// Every case asserted here fails before the service touches the workcenter shift query.
/// </summary>
public class WorkcenterShiftDetailServiceRejectionTests
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        ["WorkOrderPhaseRejectionQuantityInvalid"] = "Quantity must be greater than 0",
        ["WorkOrderPhaseRejectionDuplicatedReason"] = "Duplicated reason",
        ["WorkOrderPhaseRejectionQuantityMismatch"] = "Sum {0} does not match {1}",
        ["RejectionReasonNotFound"] = "Reason {0} not found",
        ["RejectionReasonDisabled"] = "Reason {0} is disabled",
    };

    private static (WorkcenterShiftDetailService Service, InMemoryRepository<RejectionReason> Reasons, InMemoryRepository<WorkOrderPhaseRejection> Rejections)
        BuildService(params RejectionReason[] reasons)
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var reasonsRepository = new InMemoryRepository<RejectionReason>(reasons);
        var rejectionsRepository = new InMemoryRepository<WorkOrderPhaseRejection>();

        unitOfWork.RejectionReasons.Returns(reasonsRepository);
        unitOfWork.WorkOrderPhaseRejections.Returns(rejectionsRepository);

        var service = new WorkcenterShiftDetailService(
            unitOfWork,
            Substitute.For<IMetricsService>(),
            Substitute.For<IWorkOrderPhaseService>(),
            Substitute.For<IWorkOrderPhaseCloseChannel>(),
            new KeyedLocalizationService(Messages),
            Substitute.For<ILogger<WorkcenterShiftDetailService>>());

        return (service, reasonsRepository, rejectionsRepository);
    }

    private static RejectionReason Reason(string code = "DIM", bool disabled = false) =>
        new() { Code = code, Name = code, Disabled = disabled };

    private static RegisterWorkOrderPhaseRejectionsDto Request(decimal quantityKo, params WorkOrderPhaseRejectionDto[] rejections) =>
        new()
        {
            WorkcenterId = Guid.NewGuid(),
            WorkOrderPhaseId = Guid.NewGuid(),
            QuantityKo = quantityKo,
            Rejections = [.. rejections]
        };

    [Fact]
    public async Task RegisterWorkOrderPhaseRejections_WithoutRejections_SucceedsWithoutWriting()
    {
        var (service, _, rejections) = BuildService();

        var response = await service.RegisterWorkOrderPhaseRejections(Request(5));

        Assert.True(response.Result);
        Assert.Empty(rejections.Items);
    }

    [Fact]
    public async Task RegisterWorkOrderPhaseRejections_WhenSumDoesNotMatchQuantityKo_Fails()
    {
        var reason = Reason();
        var (service, _, rejections) = BuildService(reason);

        var response = await service.RegisterWorkOrderPhaseRejections(
            Request(5, new WorkOrderPhaseRejectionDto { RejectionReasonId = reason.Id, Quantity = 3 }));

        Assert.False(response.Result);
        Assert.Equal("Sum 3 does not match 5", Assert.Single(response.Errors));
        Assert.Empty(rejections.Items);
    }

    [Fact]
    public async Task RegisterWorkOrderPhaseRejections_WithNonPositiveQuantity_Fails()
    {
        var reason = Reason();
        var (service, _, _) = BuildService(reason);

        var response = await service.RegisterWorkOrderPhaseRejections(
            Request(0, new WorkOrderPhaseRejectionDto { RejectionReasonId = reason.Id, Quantity = 0 }));

        Assert.False(response.Result);
        Assert.Equal("Quantity must be greater than 0", Assert.Single(response.Errors));
    }

    [Fact]
    public async Task RegisterWorkOrderPhaseRejections_WithRepeatedReason_Fails()
    {
        var reason = Reason();
        var (service, _, _) = BuildService(reason);

        var response = await service.RegisterWorkOrderPhaseRejections(
            Request(4,
                new WorkOrderPhaseRejectionDto { RejectionReasonId = reason.Id, Quantity = 2 },
                new WorkOrderPhaseRejectionDto { RejectionReasonId = reason.Id, Quantity = 2 }));

        Assert.False(response.Result);
        Assert.Equal("Duplicated reason", Assert.Single(response.Errors));
    }

    [Fact]
    public async Task RegisterWorkOrderPhaseRejections_WithUnknownReason_Fails()
    {
        var (service, _, _) = BuildService();
        var unknownReasonId = Guid.NewGuid();

        var response = await service.RegisterWorkOrderPhaseRejections(
            Request(2, new WorkOrderPhaseRejectionDto { RejectionReasonId = unknownReasonId, Quantity = 2 }));

        Assert.False(response.Result);
        Assert.Equal($"Reason {unknownReasonId} not found", Assert.Single(response.Errors));
    }

    [Fact]
    public async Task RegisterWorkOrderPhaseRejections_WithDisabledReason_Fails()
    {
        var reason = Reason("OBS", disabled: true);
        var (service, _, _) = BuildService(reason);

        var response = await service.RegisterWorkOrderPhaseRejections(
            Request(2, new WorkOrderPhaseRejectionDto { RejectionReasonId = reason.Id, Quantity = 2 }));

        Assert.False(response.Result);
        Assert.Equal("Reason OBS is disabled", Assert.Single(response.Errors));
    }

    [Fact]
    public async Task UpdateWorkcenterShiftDetailQuantities_WithInvalidRejections_FailsBeforeUpdatingQuantities()
    {
        var reason = Reason();
        var (service, _, rejections) = BuildService(reason);

        var response = await service.UpdateWorkcenterShiftDetailQuantities(new UpdateWorkcenterShiftDetailQuantitiesDto
        {
            WorkcenterId = Guid.NewGuid(),
            WorkOrderPhaseId = Guid.NewGuid(),
            QuantityOk = 10,
            QuantityKo = 5,
            Rejections = [new WorkOrderPhaseRejectionDto { RejectionReasonId = reason.Id, Quantity = 1 }]
        });

        Assert.False(response.Result);
        Assert.Equal("Sum 1 does not match 5", Assert.Single(response.Errors));
        Assert.Empty(rejections.Items);
    }
}

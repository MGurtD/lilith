using Application.Contracts;
using Application.Services.Production;
using Application.Tests.TestSupport;
using Domain.Entities.Production;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Production;

/// <summary>
/// Uniqueness and deletion rules of the rejection reason catalogue.
/// </summary>
public class RejectionReasonServiceTests
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        ["RejectionReasonCodeDuplicate"] = "Code {0} already exists",
        ["RejectionReasonNotFound"] = "Reason {0} not found",
        ["RejectionReasonInUse"] = "Reason {0} is in use",
    };

    private static (RejectionReasonService Service, InMemoryRepository<RejectionReason> Reasons)
        BuildService(IEnumerable<RejectionReason>? reasons = null, IEnumerable<WorkOrderPhaseRejection>? rejections = null)
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var reasonsRepository = new InMemoryRepository<RejectionReason>(reasons);

        unitOfWork.RejectionReasons.Returns(reasonsRepository);
        unitOfWork.WorkOrderPhaseRejections.Returns(new InMemoryRepository<WorkOrderPhaseRejection>(rejections));

        return (new RejectionReasonService(unitOfWork, new KeyedLocalizationService(Messages)), reasonsRepository);
    }

    [Fact]
    public async Task Create_WithDuplicatedCode_Fails()
    {
        var (service, reasons) = BuildService([new RejectionReason { Code = "DIM", Name = "Dimensional" }]);

        var response = await service.Create(new RejectionReason { Code = "DIM", Name = "Another one" });

        Assert.False(response.Result);
        Assert.Equal("Code DIM already exists", Assert.Single(response.Errors));
        Assert.Single(reasons.Items);
    }

    [Fact]
    public async Task Update_WithCodeOfAnotherReason_Fails()
    {
        var existing = new RejectionReason { Code = "DIM", Name = "Dimensional" };
        var edited = new RejectionReason { Code = "SUR", Name = "Surface" };
        var (service, _) = BuildService([existing, edited]);

        edited.Code = "DIM";
        var response = await service.Update(edited);

        Assert.False(response.Result);
        Assert.Equal("Code DIM already exists", Assert.Single(response.Errors));
    }

    [Fact]
    public async Task Update_KeepingItsOwnCode_Succeeds()
    {
        var existing = new RejectionReason { Code = "DIM", Name = "Dimensional" };
        var (service, _) = BuildService([existing]);

        existing.Name = "Dimensional deviation";
        var response = await service.Update(existing);

        Assert.True(response.Result);
    }

    [Fact]
    public async Task Remove_WhenReasonHasRecordedRejections_Fails()
    {
        var reason = new RejectionReason { Code = "DIM", Name = "Dimensional" };
        var (service, reasons) = BuildService(
            [reason],
            [new WorkOrderPhaseRejection { RejectionReasonId = reason.Id, Quantity = 3 }]);

        var response = await service.Remove(reason.Id);

        Assert.False(response.Result);
        Assert.Equal("Reason DIM is in use", Assert.Single(response.Errors));
        Assert.Single(reasons.Items);
    }

    [Fact]
    public async Task Remove_WhenReasonIsUnused_Succeeds()
    {
        var reason = new RejectionReason { Code = "DIM", Name = "Dimensional" };
        var (service, reasons) = BuildService([reason]);

        var response = await service.Remove(reason.Id);

        Assert.True(response.Result);
        Assert.Empty(reasons.Items);
    }
}

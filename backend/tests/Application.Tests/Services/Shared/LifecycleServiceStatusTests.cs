using Application.Contracts;
using Application.Services.Shared;
using Application.Tests.TestSupport;
using Domain.Entities;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Shared;

/// <summary>
/// <see cref="LifecycleService.RemoveStatus"/> must refuse a status in use (#159):
/// several documents cascade from their status, so deleting it would delete them.
/// </summary>
public class LifecycleServiceStatusTests
{
    private static readonly Dictionary<string, string> LocalizationKeys = new()
    {
        ["StatusInUse"] = "L'estat {0} està en ús",
        ["EntityNotFound"] = "L'entitat amb ID {0} no existeix",
    };

    [Fact]
    public async Task RemoveStatus_refuses_a_status_in_use()
    {
        var status = new Status { Id = Guid.NewGuid(), Name = "Tancada" };
        var (sut, statuses) = BuildSut(status, inUse: true);

        var response = await sut.RemoveStatus(status.Id);

        Assert.False(response.Result);
        Assert.Equal("L'estat Tancada està en ús", Assert.Single(response.Errors));
        await statuses.DidNotReceive().Remove(Arg.Any<Status>());
    }

    [Fact]
    public async Task RemoveStatus_deletes_an_unused_status()
    {
        var status = new Status { Id = Guid.NewGuid(), Name = "Esborrany" };
        var (sut, statuses) = BuildSut(status, inUse: false);

        var response = await sut.RemoveStatus(status.Id);

        Assert.True(response.Result);
        await statuses.Received(1).Remove(status);
    }

    private static (LifecycleService Sut, IStatusRepository Statuses) BuildSut(Status status, bool inUse)
    {
        var statuses = Substitute.For<IStatusRepository>();
        statuses
            .Find(Arg.Any<Expression<Func<Status, bool>>>())
            .Returns(call => new[] { status }.AsQueryable().Where(call.Arg<Expression<Func<Status, bool>>>()).ToList());
        statuses.IsInUse(status.Id).Returns(inUse);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Lifecycles.StatusRepository.Returns(statuses);

        return (new LifecycleService(unitOfWork, new KeyedLocalizationService(LocalizationKeys)), statuses);
    }
}

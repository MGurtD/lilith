using Application.Contracts;
using Domain.Entities.Production;
using NSubstitute;

namespace Application.Tests.TestSupport;

public sealed class BrandingTestContext
{
    public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();
    public StagingRepository<Enterprise> EnterprisesStore { get; } = new();
    public StagingRepository<Domain.Entities.File> FilesStore { get; } = new();
    public Exception? CommitException { get; init; }
    public Action? AfterCommit { get; set; }
    public int CompleteCallCount { get; private set; }

    public BrandingTestContext(Enterprise enterprise)
    {
        EnterprisesStore.Store.Add(enterprise);
        UnitOfWork.Enterprises.Returns(EnterprisesStore);
        UnitOfWork.Files.Returns(FilesStore);
        UnitOfWork.CompleteAsync().Returns(_ => CompleteAsync());
    }

    private Task<int> CompleteAsync()
    {
        CompleteCallCount++;
        if (CommitException is not null)
            throw CommitException;

        EnterprisesStore.Commit();
        FilesStore.Commit();
        AfterCommit?.Invoke();
        return Task.FromResult(2);
    }
}

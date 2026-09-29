using System.Linq.Expressions;
using Application.Contracts;
using Domain.Entities;
using NSubstitute;

namespace Application.Tests.TestSupport;

/// <summary>
/// Makes <c>Find</c> and <c>Get</c> of a substituted repository serve a fixed
/// set of entities, so a service can look up the entity it is about to delete.
/// </summary>
public static class RepositorySubstituteExtensions
{
    public static TRepository Serving<TRepository, TEntity>(this TRepository repository, params TEntity[] entities)
        where TRepository : class, IRepository<TEntity, Guid>
        where TEntity : Entity
    {
        repository
            .Find(Arg.Any<Expression<Func<TEntity, bool>>>())
            .Returns(call => entities.AsQueryable().Where(call.Arg<Expression<Func<TEntity, bool>>>()).ToList());
        repository
            .Get(Arg.Any<Guid>())
            .Returns(call => Task.FromResult<TEntity?>(entities.FirstOrDefault(e => e.Id == call.Arg<Guid>())));
        return repository;
    }
}

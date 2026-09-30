using Domain.Entities;

namespace Application.Contracts;

public interface ITaxRepository : IRepository<Tax, Guid>
{
}

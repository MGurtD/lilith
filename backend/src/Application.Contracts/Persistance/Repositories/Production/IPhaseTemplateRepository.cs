using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IPhaseTemplateRepository : IRepository<PhaseTemplate, Guid>
    {
        IRepository<PhaseTemplateDetail, Guid> Details { get; }
    }
}

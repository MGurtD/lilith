using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IRejectionReasonService
    {
        Task<RejectionReason?> GetById(Guid id);
        Task<IEnumerable<RejectionReason>> GetAll();
        Task<GenericResponse> Create(RejectionReason rejectionReason);
        Task<GenericResponse> Update(RejectionReason rejectionReason);
        Task<GenericResponse> Remove(Guid id);
    }
}

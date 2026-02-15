using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IMachineStatusService
    {
        Task<MachineStatus?> GetById(Guid id);
        Task<IEnumerable<MachineStatus>> GetAll();
        Task<IEnumerable<MachineStatus>> GetAllWithReasons();
        Task<GenericResponse> Create(MachineStatus machineStatus);
        Task<GenericResponse> Update(MachineStatus machineStatus);
        Task<GenericResponse> Remove(Guid id);

        // MachineStatusReason operations
        Task<MachineStatusReason?> GetReasonById(Guid id);
        Task<GenericResponse> CreateReason(MachineStatusReason reason);
        Task<GenericResponse> UpdateReason(MachineStatusReason reason);
        Task<GenericResponse> RemoveReason(Guid id);
    }
}

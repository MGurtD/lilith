using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class MachineStatusService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IMachineStatusService
    {
        public async Task<MachineStatus?> GetById(Guid id)
        {
            return await unitOfWork.MachineStatuses.Get(id);
        }

        public async Task<IEnumerable<MachineStatus>> GetAll()
        {
            var machineStatuses = await unitOfWork.MachineStatuses.GetAll();
            return machineStatuses.OrderBy(m => m.Name);
        }

        public async Task<IEnumerable<MachineStatus>> GetAllWithReasons()
        {
            return await unitOfWork.MachineStatuses.GetAllWithReasons();
        }

        private void DisableCurrentDefault(Guid currentId)
        {
            var currentDefault = unitOfWork.MachineStatuses.Find(e => e.Id != currentId && e.Default).FirstOrDefault();
            if (currentDefault is not null)
            {
                currentDefault.Default = false;
                unitOfWork.MachineStatuses.UpdateWithoutSave(currentDefault);
            }
        }

        public async Task<GenericResponse> Create(MachineStatus machineStatus)
        {
            var exists = unitOfWork.MachineStatuses.Find(m => m.Name == machineStatus.Name).Any();
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityAlreadyExists"));
            }

            if (machineStatus.Default)
                DisableCurrentDefault(machineStatus.Id);

            await unitOfWork.MachineStatuses.Add(machineStatus);
            return new GenericResponse(true, machineStatus);
        }

        public async Task<GenericResponse> Update(MachineStatus machineStatus)
        {
            var exists = await unitOfWork.MachineStatuses.Exists(machineStatus.Id);
            if (!exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", machineStatus.Id));
            }

            if (machineStatus.Default)
                DisableCurrentDefault(machineStatus.Id);

            unitOfWork.MachineStatuses.UpdateWithoutSave(machineStatus);
            await unitOfWork.CompleteAsync();
            return new GenericResponse(true, machineStatus);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var machineStatus = unitOfWork.MachineStatuses.Find(m => m.Id == id).FirstOrDefault();
            if (machineStatus == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.MachineStatuses.Remove(machineStatus);
            return new GenericResponse(true, machineStatus);
        }

        // MachineStatusReason operations
        public async Task<MachineStatusReason?> GetReasonById(Guid id)
        {
            return await unitOfWork.MachineStatuses.Reasons.Get(id);
        }

        public async Task<GenericResponse> CreateReason(MachineStatusReason reason)
        {
            var codeExists = unitOfWork.MachineStatuses.Reasons.Find(r =>
                r.Code == reason.Code &&
                r.MachineStatusId == reason.MachineStatusId &&
                r.Id != reason.Id
            ).Any();

            if (codeExists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("MachineStatusReasonCodeDuplicate", reason.Code));
            }

            await unitOfWork.MachineStatuses.Reasons.Add(reason);
            return new GenericResponse(true, reason);
        }

        public async Task<GenericResponse> UpdateReason(MachineStatusReason reason)
        {
            var exists = await unitOfWork.MachineStatuses.Reasons.Exists(reason.Id);
            if (!exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("MachineStatusReasonNotFound", reason.Id));
            }

            var codeExists = unitOfWork.MachineStatuses.Reasons.Find(r =>
                r.Code == reason.Code &&
                r.MachineStatusId == reason.MachineStatusId &&
                r.Id != reason.Id
            ).Any();

            if (codeExists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("MachineStatusReasonCodeDuplicate", reason.Code));
            }

            unitOfWork.MachineStatuses.Reasons.UpdateWithoutSave(reason);
            await unitOfWork.CompleteAsync();
            return new GenericResponse(true, reason);
        }

        public async Task<GenericResponse> RemoveReason(Guid id)
        {
            var entity = unitOfWork.MachineStatuses.Reasons.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("MachineStatusReasonNotFound", id));
            }

            await unitOfWork.MachineStatuses.Reasons.Remove(entity);
            return new GenericResponse(true, entity);
        }
    }
}

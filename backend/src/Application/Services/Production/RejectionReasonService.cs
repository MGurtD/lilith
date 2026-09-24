using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class RejectionReasonService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IRejectionReasonService
    {
        public async Task<RejectionReason?> GetById(Guid id)
        {
            return await unitOfWork.RejectionReasons.Get(id);
        }

        public async Task<IEnumerable<RejectionReason>> GetAll()
        {
            var rejectionReasons = await unitOfWork.RejectionReasons.GetAll();
            return rejectionReasons.OrderBy(r => r.Code);
        }

        public async Task<GenericResponse> Create(RejectionReason rejectionReason)
        {
            var exists = unitOfWork.RejectionReasons.Find(r => r.Code == rejectionReason.Code).Any();
            if (exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("RejectionReasonCodeDuplicate", rejectionReason.Code));
            }

            await unitOfWork.RejectionReasons.Add(rejectionReason);
            return new GenericResponse(true, rejectionReason);
        }

        public async Task<GenericResponse> Update(RejectionReason rejectionReason)
        {
            var exists = await unitOfWork.RejectionReasons.Exists(rejectionReason.Id);
            if (!exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("RejectionReasonNotFound", rejectionReason.Id));
            }

            var duplicated = unitOfWork.RejectionReasons
                                       .Find(r => r.Code == rejectionReason.Code && r.Id != rejectionReason.Id)
                                       .Any();
            if (duplicated)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("RejectionReasonCodeDuplicate", rejectionReason.Code));
            }

            await unitOfWork.RejectionReasons.Update(rejectionReason);
            return new GenericResponse(true, rejectionReason);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var rejectionReason = unitOfWork.RejectionReasons.Find(r => r.Id == id).FirstOrDefault();
            if (rejectionReason == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("RejectionReasonNotFound", id));
            }

            var used = unitOfWork.WorkOrderPhaseRejections.Find(r => r.RejectionReasonId == id).Any();
            if (used)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("RejectionReasonInUse", rejectionReason.Code));
            }

            await unitOfWork.RejectionReasons.Remove(rejectionReason);
            return new GenericResponse(true, rejectionReason);
        }
    }
}

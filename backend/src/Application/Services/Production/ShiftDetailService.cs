using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class ShiftDetailService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IShiftDetailService
    {
        public async Task<ShiftDetail?> GetById(Guid id)
        {
            return await unitOfWork.ShiftDetails.Get(id);
        }

        public async Task<IEnumerable<ShiftDetail>> GetByShiftId(Guid shiftId)
        {
            var shiftDetails = unitOfWork.ShiftDetails.Find(s => s.ShiftId == shiftId);
            return shiftDetails.OrderBy(s => s.StartTime);
        }

        public async Task<IEnumerable<ShiftDetail>> GetAll()
        {
            var shiftDetails = await unitOfWork.ShiftDetails.GetAll();
            return shiftDetails.OrderBy(s => s.StartTime);
        }

        public async Task<GenericResponse> Create(ShiftDetail shiftDetail)
        {
            var exists = await unitOfWork.ShiftDetails.Exists(shiftDetail.Id);
            if (exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityAlreadyExists"));
            }

            await unitOfWork.ShiftDetails.Add(shiftDetail);
            return new GenericResponse(true, shiftDetail);
        }

        public async Task<GenericResponse> Update(ShiftDetail shiftDetail)
        {
            var exists = await unitOfWork.ShiftDetails.Exists(shiftDetail.Id);
            if (!exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", shiftDetail.Id));
            }

            await unitOfWork.ShiftDetails.Update(shiftDetail);
            return new GenericResponse(true, shiftDetail);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var shiftDetail = unitOfWork.ShiftDetails.Find(s => s.Id == id).FirstOrDefault();
            if (shiftDetail == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.ShiftDetails.Remove(shiftDetail);
            return new GenericResponse(true, shiftDetail);
        }
    }
}

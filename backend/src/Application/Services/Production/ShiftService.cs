using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class ShiftService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IShiftService
    {
        public async Task<Shift?> GetById(Guid id)
        {
            return await unitOfWork.Shifts.Get(id);
        }

        public async Task<IEnumerable<Shift>> GetAll()
        {
            var shifts = await unitOfWork.Shifts.GetAll();
            return shifts;  // No ordering per original controller
        }

        public async Task<GenericResponse> Create(Shift shift)
        {
            var exists = unitOfWork.Shifts.Find(s => s.Name == shift.Name).Any();
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityAlreadyExists"));
            }

            await unitOfWork.Shifts.Add(shift);
            return new GenericResponse(true, shift);
        }

        public async Task<GenericResponse> Update(Shift shift)
        {
            var exists = await unitOfWork.Shifts.Exists(shift.Id);
            if (!exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", shift.Id));
            }

            await unitOfWork.Shifts.Update(shift);
            return new GenericResponse(true, shift);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var shift = unitOfWork.Shifts.Find(s => s.Id == id).FirstOrDefault();
            if (shift == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            // Soft delete - set Disabled flag instead of removing
            shift.Disabled = true;
            await unitOfWork.Shifts.Update(shift);
            return new GenericResponse(true, shift);
        }
    }
}

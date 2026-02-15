using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class OperatorTypeService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IOperatorTypeService
    {
        public async Task<OperatorType?> GetById(Guid id)
        {
            return await unitOfWork.OperatorTypes.Get(id);
        }

        public async Task<IEnumerable<OperatorType>> GetAll()
        {
            var operatorTypes = await unitOfWork.OperatorTypes.GetAll();
            return operatorTypes.OrderBy(o => o.Name);
        }

        public async Task<GenericResponse> Create(OperatorType operatorType)
        {
            var exists = unitOfWork.OperatorTypes.Find(o => o.Name == operatorType.Name).Any();
            if (exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("OperatorTypeAlreadyExists", operatorType.Name));
            }

            await unitOfWork.OperatorTypes.Add(operatorType);
            return new GenericResponse(true, operatorType);
        }

        public async Task<GenericResponse> Update(OperatorType operatorType)
        {
            var exists = await unitOfWork.OperatorTypes.Exists(operatorType.Id);
            if (!exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", operatorType.Id));
            }

            await unitOfWork.OperatorTypes.Update(operatorType);
            return new GenericResponse(true, operatorType);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var operatorType = unitOfWork.OperatorTypes.Find(o => o.Id == id).FirstOrDefault();
            if (operatorType == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.OperatorTypes.Remove(operatorType);
            return new GenericResponse(true, operatorType);
        }
    }
}

using Application.Contracts;
using Application.Services;
using Domain.Entities;

namespace Application.Services.System
{
    public class ExerciseService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IExerciseService
    {
        public Exercise? GetExerciceByDate(DateTime dateTime)
        {
            var exercice = unitOfWork.Exercices.Find(e => dateTime >= e.StartDate && dateTime <= e.EndDate).FirstOrDefault();
            return exercice;
        }

        public async Task<GenericResponse> GetNextCounter(Guid exerciseId, string counterName)
        {
            var exercise = await unitOfWork.Exercices.Get(exerciseId);
            var counter = "";
            if (exercise == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseNotFound"));
            }

            var prefix = exercise.Name[^2..];
            var currentCounter = counterName.ToLowerInvariant() switch
            {
                "purchaseorder" => exercise.PurchaseOrderCounter,
                "purchaseinvoice" => exercise.PurchaseInvoiceCounter,
                "salesinvoice" => exercise.SalesInvoiceCounter,
                "salesorder" => exercise.SalesOrderCounter,
                "receipt" => exercise.ReceiptCounter,
                "deliverynote" => exercise.DeliveryNoteCounter,
                "budget" => exercise.BudgetCounter,
                "workorder" => exercise.WorkOrderCounter,
                _ => null
            };

            if (currentCounter is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseCounterNotFound", counterName));

            var nextCounter = int.Parse(currentCounter) + 1;
            var nextCounterValue = nextCounter.ToString("D3");
            counter = prefix + nextCounterValue;

            switch (counterName.ToLowerInvariant())
            {
                case "purchaseorder":
                    exercise.PurchaseOrderCounter = nextCounterValue;
                    break;
                case "purchaseinvoice":
                    exercise.PurchaseInvoiceCounter = nextCounterValue;
                    break;
                case "salesinvoice":
                    exercise.SalesInvoiceCounter = nextCounterValue;
                    break;
                case "salesorder":
                    exercise.SalesOrderCounter = nextCounterValue;
                    break;
                case "receipt":
                    exercise.ReceiptCounter = nextCounterValue;
                    break;
                case "deliverynote":
                    exercise.DeliveryNoteCounter = nextCounterValue;
                    break;
                case "budget":
                    exercise.BudgetCounter = nextCounterValue;
                    break;
                case "workorder":
                    exercise.WorkOrderCounter = nextCounterValue;
                    break;
            }

            await unitOfWork.Exercices.Update(exercise);
            return new GenericResponse(true, (object)counter);
        }

        // CRUD operations
        public async Task<Exercise?> GetById(Guid id)
        {
            return await unitOfWork.Exercices.Get(id);
        }

        public async Task<IEnumerable<Exercise>> GetAll()
        {
            var exercises = await unitOfWork.Exercices.GetAll();
            return exercises.OrderBy(e => e.Name);
        }

        public async Task<GenericResponse> Create(Exercise exercise)
        {
            var exists = unitOfWork.Exercices.Find(e => e.Name == exercise.Name).Any();
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("ExerciseAlreadyExists", exercise.Name));
            }

            await unitOfWork.Exercices.Add(exercise);
            return new GenericResponse(true, exercise);
        }

        public async Task<GenericResponse> Update(Exercise exercise)
        {
            var exists = await unitOfWork.Exercices.Exists(exercise.Id);
            if (!exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("ExerciseNotFound"));
            }

            await unitOfWork.Exercices.Update(exercise);
            return new GenericResponse(true, exercise);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var exercise = await unitOfWork.Exercices.Get(id);
            if (exercise == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("ExerciseNotFound"));
            }

            await unitOfWork.Exercices.Remove(exercise);
            return new GenericResponse(true, exercise);
        }
    }
}







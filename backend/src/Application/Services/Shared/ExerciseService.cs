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
            int newcounter;
            switch (counterName.ToLower())
            {
                case "purchaseorder":
                    counter = prefix + exercise.PurchaseOrderCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.PurchaseOrderCounter = newcounter.ToString()[^3..];
                    break;
                case "purchaseinvoice":
                    counter = prefix + exercise.PurchaseInvoiceCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.PurchaseInvoiceCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "salesinvoice":
                    counter = prefix + exercise.SalesInvoiceCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.SalesInvoiceCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "salesorder":
                    counter = prefix + exercise.SalesOrderCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.SalesOrderCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "receipt":
                    counter = prefix + exercise.ReceiptCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.ReceiptCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "deliverynote":
                    counter = prefix + exercise.DeliveryNoteCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.DeliveryNoteCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "budget":
                    counter = prefix + exercise.BudgetCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.BudgetCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "workorder":
                    counter = prefix + exercise.WorkOrderCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.WorkOrderCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                default:
                    return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseCounterNotFound", counterName));
            }

            await unitOfWork.Exercices.Update(exercise);
            return new GenericResponse(true, newcounter);
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







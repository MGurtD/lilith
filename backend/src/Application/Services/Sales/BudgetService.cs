using Application.Contracts;
using Domain.Entities.Sales;


namespace Application.Services.Sales
{
    public class BudgetService(
        IUnitOfWork unitOfWork,
        IExerciseService exerciseService,
        ILocalizationService localizationService) : IBudgetService
    {
        public async Task<Budget?> GetById(Guid id)
        {
            var budget = await unitOfWork.Budgets.Get(id);
            return budget;
        }

        public IEnumerable<Budget> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var budgets = unitOfWork.Budgets.Find(p => p.Date >= startDate && p.Date <= endDate);
            return budgets;
        }
        public IEnumerable<Budget> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId)
        {
            var budgets = unitOfWork.Budgets.Find(p => p.Date >= startDate && p.Date <= endDate && p.CustomerId == customerId);
            return budgets;
        }

        public async Task<GenericResponse> Create(CreateHeaderRequest createRequest)
        {
            var counterObj = await exerciseService.GetNextCounter(createRequest.ExerciseId, "budget");
            if (!counterObj.Result || counterObj.Content == null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseCounterError"));

            var budget = new Budget
            {
                Id = createRequest.Id,
                Number = counterObj.Content.ToString()!,
                Date = createRequest.Date,
                ExerciseId = createRequest.ExerciseId,
                CustomerId = createRequest.CustomerId
            };

            // Estat inicial
            if (createRequest.InitialStatusId.HasValue)
            {
                budget.StatusId = createRequest.InitialStatusId;
            }
            else
            {
                var lifecycle = unitOfWork.Lifecycles.Find(l => l.Name == StatusConstants.Lifecycles.Budget).FirstOrDefault();
                if (lifecycle == null)
                    return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNotFound", StatusConstants.Lifecycles.Budget));
                if (!lifecycle.InitialStatusId.HasValue)
                    return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNoInitialStatus", StatusConstants.Lifecycles.Budget));
                budget.StatusId = lifecycle.InitialStatusId;
            }

            await unitOfWork.Budgets.Add(budget);
            return new GenericResponse(true, budget);
        }

        public async Task<GenericResponse> Update(Budget budget)
        {
            budget.Details.Clear();

            var existingBudget = await unitOfWork.Budgets.Get(budget.Id);
            if (existingBudget == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound", budget.Id));
            }

            var statusPending = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.Budget, StatusConstants.Statuses.PendentAcceptar);
            var statusAccept = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.Budget, StatusConstants.Statuses.Acceptat);

            if (statusPending == null || statusAccept == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", "Pendent d'acceptar/Acceptat"));
            }

            if (existingBudget.StatusId == statusPending.Id && budget.StatusId == statusAccept.Id)
            {
                budget.AcceptanceDate = DateTime.Now;                
            }

            await unitOfWork.Budgets.Update(budget);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var budget = unitOfWork.Budgets.Find(p => p.Id == id).FirstOrDefault();
            if (budget == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound", id));
            }
            else
            {
                await unitOfWork.Budgets.Remove(budget);
                return new GenericResponse(true, new List<string> { });
            }
        }
        
        public async Task<GenericResponse> AddDetail(BudgetDetail detail)
        {
            await unitOfWork.Budgets.Details.Add(detail);
            return new GenericResponse(true, detail);
        }
        public async Task<GenericResponse> UpdateDetail(BudgetDetail detail)
        {
            await unitOfWork.Budgets.Details.Update(detail);
            return new GenericResponse(true, detail);
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = unitOfWork.Budgets.Details.Find(d => d.Id == id).FirstOrDefault();
            if (detail == null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetDetailNotFound", id));
            await unitOfWork.Budgets.Details.Remove(detail);

            return new GenericResponse(true, detail);
        }

        public async Task<GenericResponse> RejectOutdatedBudgets()
        {
            var status = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.Budget, StatusConstants.Statuses.PendentAcceptar);
            var rejectedstatus = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.Budget, StatusConstants.Statuses.Rebutjat);
            if (status == null || rejectedstatus == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", "Pendent d'acceptar/Rebutjat"));
            }

            var budgets =  await unitOfWork.Budgets.FindAsync(b => b.StatusId == status.Id && b.Date.AddDays(30) <= DateTime.UtcNow);           
            foreach (var budget in budgets)
            {
                budget.StatusId = rejectedstatus.Id;
                budget.AutoRejectedDate = DateTime.UtcNow;
                budget.Notes = localizationService.GetLocalizedString("BudgetAutomaticRejection", DateTime.UtcNow.ToString());
                await unitOfWork.Budgets.Update(budget);
                
            }
            return new GenericResponse(true);
        }
    }
}







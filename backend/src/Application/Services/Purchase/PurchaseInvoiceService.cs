using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Purchase;

namespace Application.Services.Purchase
{
    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExerciseService _exerciseService;
        private readonly ILocalizationService _localizationService;

        public PurchaseInvoiceService(IUnitOfWork unitOfWork, IExerciseService exerciseService, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _exerciseService = exerciseService;
            _localizationService = localizationService;
        }

        public async Task<PurchaseInvoice?> GetById(Guid id)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.Get(id);
            return invoice;
        }

        public async Task<PaymentMethod?> GetPaymentMethodById(Guid id)
        {
            return await _unitOfWork.PaymentMethods.Get(id);
        }

        public IEnumerable<PurchaseInvoice> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.PurchaseInvoiceDate >= startDate && p.PurchaseInvoiceDate <= endDate);
            return invoices;
        }

        public IEnumerable<PurchaseInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.PurchaseInvoiceDate >= startDate && p.PurchaseInvoiceDate <= endDate && p.StatusId == statusId);
            return invoices;
        }

        public IEnumerable<PurchaseInvoice> GetBetweenDatesAndExcludeStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.PurchaseInvoiceDate >= startDate && p.PurchaseInvoiceDate <= endDate && p.StatusId != statusId);
            return invoices;
        }

        public IEnumerable<PurchaseInvoice> GetBetweenDatesAndSupplier(DateTime startDate, DateTime endDate, Guid supplierId)
        {
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.PurchaseInvoiceDate >= startDate && p.PurchaseInvoiceDate <= endDate && p.SupplierId == supplierId);
            return invoices;
        }

        public IEnumerable<PurchaseInvoice> GetBetweenDatesExcludingStatusAndSupplier(DateTime startDate, DateTime endDate, Guid excludeStatusId, Guid supplierId)
        {
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.PurchaseInvoiceDate >= startDate && p.PurchaseInvoiceDate <= endDate && p.StatusId != excludeStatusId && p.SupplierId == supplierId);
            return invoices;
        }

        public async Task<IEnumerable<PurchaseInvoice>> GetByExercise(Guid exerciseId)
        {
            var exercice = await _unitOfWork.Exercices.Get(exerciseId);
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.ExerciceId == exerciseId);
            return invoices;
        }

        // TODO > Test creació de factura!!
        public async Task<GenericResponse> Create(PurchaseInvoice purchaseInvoice)
        {
            // Incrementar el comptador de factures de l'exercici
            if (purchaseInvoice.ExerciceId.HasValue)
            {
                var exercise = _exerciseService.GetExerciceByDate(purchaseInvoice.PurchaseInvoiceDate);
                if (exercise == null || exercise.Disabled)
                {
                    return new GenericResponse(false, _localizationService.GetLocalizedString("ExerciseInvalid"));
                }

                var counterObj = await _exerciseService.GetNextCounter(exercise.Id, "purchaseinvoice");
                if (counterObj != null && counterObj.Content != null)
                {
                    purchaseInvoice.Number = counterObj.Content.ToString()!;
                    await _unitOfWork.PurchaseInvoices.Add(purchaseInvoice);
                } 
                else {
                    return new GenericResponse(false, _localizationService.GetLocalizedString("ExerciseCounterError"));
                }

            }

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Update(PurchaseInvoice purchaseInvoice)
        {
            await RecreateDueDates(purchaseInvoice);
            purchaseInvoice.PurchaseInvoiceDueDates!.Clear();
            purchaseInvoice.PurchaseInvoiceImports!.Clear();
            await _unitOfWork.PurchaseInvoices.Update(purchaseInvoice);

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RecreateDueDates(PurchaseInvoice requestInvoice)
        {
            var dbDueDates = _unitOfWork.PurchaseInvoiceDueDates.Find(dd => dd.PurchaseInvoiceId == requestInvoice.Id);
            if (dbDueDates.Any())
                await _unitOfWork.PurchaseInvoiceDueDates.RemoveRange(dbDueDates);

            if (requestInvoice.PurchaseInvoiceDueDates != null)
                await _unitOfWork.PurchaseInvoiceDueDates.AddRange(requestInvoice.PurchaseInvoiceDueDates);

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> ChangeStatuses(ChangeStatusOfInvoicesRequest changeStatusesRequest)
        {
            var statusToId = changeStatusesRequest.StatusToId;
            var status = await _unitOfWork.Lifecycles.StatusRepository.Get(statusToId);
            if (status == null || status.Disabled)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("StatusNotFound", statusToId));
            }

            var purchaseInvoices = _unitOfWork.PurchaseInvoices.Find(pi => changeStatusesRequest.Ids.Contains(pi.Id));
            foreach (var invoice in purchaseInvoices)
            {
                invoice.StatusId = statusToId;
                _unitOfWork.PurchaseInvoices.UpdateWithoutSave(invoice);
            }
            await _unitOfWork.CompleteAsync();

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> ChangeStatus(ChangeStatusRequest changeStatusOfPurchaseInvoiceRequest)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.Get(changeStatusOfPurchaseInvoiceRequest.Id);
            if (invoice == null)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("PurchaseInvoiceNotFound", changeStatusOfPurchaseInvoiceRequest.Id));
            }

            var status = await _unitOfWork.Lifecycles.StatusRepository.Get(changeStatusOfPurchaseInvoiceRequest.StatusToId);
            if (status == null || status.Disabled)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("StatusNotFound", changeStatusOfPurchaseInvoiceRequest.StatusToId));
            }

            invoice.StatusId = changeStatusOfPurchaseInvoiceRequest.StatusToId;
            await _unitOfWork.PurchaseInvoices.Update(invoice);

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var invoice = _unitOfWork.PurchaseInvoices.Find(p => p.Id == id).FirstOrDefault();
            if (invoice == null)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("PurchaseInvoiceNotFound", id));
            }
            else
            {
                await _unitOfWork.PurchaseInvoices.Remove(invoice);
                return new GenericResponse(true);
            }

        }

        #region Imports

        public async Task<GenericResponse> AddImport(PurchaseInvoiceImport import)
        {
            await _unitOfWork.PurchaseInvoices.AddImport(import);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> UpdateImport(PurchaseInvoiceImport import)
        {
            await _unitOfWork.PurchaseInvoices.UpdateImport(import);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RemoveImport(Guid id)
        {
            await _unitOfWork.PurchaseInvoices.RemoveImport(id);
            return new GenericResponse(true);
        }

        #endregion

        #region DueDates CRUD
        public async Task<GenericResponse> AddDueDates(IEnumerable<PurchaseInvoiceDueDate> dueDates)
        {
            if (dueDates == null || !dueDates.Any())
                return new GenericResponse(true); // nothing to add

            await _unitOfWork.PurchaseInvoiceDueDates.AddRange(dueDates);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RemoveDueDates(IEnumerable<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return new GenericResponse(true); // nothing to remove

            var existing = _unitOfWork.PurchaseInvoiceDueDates.Find(dd => ids.Contains(dd.Id));
            if (existing.Any())
            {
                await _unitOfWork.PurchaseInvoiceDueDates.RemoveRange(existing);
            }
            return new GenericResponse(true);
        }
        #endregion

    }
}







using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Domain.Entities.Warehouse;

namespace Application.Services.Sales
{
    internal struct DeliveryNoteEntities
    {
        internal Customer Customer;
        internal Exercise Exercise;
        internal Site Site;
        internal Lifecycle Lifecycle;
    }

    public class DeliveryNoteService(
        IUnitOfWork unitOfWork,
        IEnterpriseService enterpriseService,
        IStockMovementService stockMovementService,
        ISalesOrderService salesOrderService,
        IExerciseService exerciseService,
        ILocalizationService localizationService) : IDeliveryNoteService
    {
        private readonly string lifecycleName = StatusConstants.Lifecycles.DeliveryNote;

        public async Task<DeliveryNote?> GetById(Guid id)
        {
            return await unitOfWork.DeliveryNotes.Get(id);
        }

        public IEnumerable<DeliveryNote> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var deliveryNotes = unitOfWork.DeliveryNotes.Find(p => p.CreatedOn >= startDate && p.CreatedOn <= endDate);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var deliveryNotes = unitOfWork.DeliveryNotes.Find(p => p.CreatedOn >= startDate && p.CreatedOn <= endDate && p.StatusId == statusId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId)
        {
            var deliveryNotes = unitOfWork.DeliveryNotes.Find(p => p.CreatedOn >= startDate && p.CreatedOn <= endDate && p.CustomerId == customerId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetByStatus(Guid statusId)
        {
            var deliveryNotes = unitOfWork.DeliveryNotes.Find(p => p.StatusId == statusId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetByCustomer(Guid customerId)
        {
            var deliveryNotes = unitOfWork.DeliveryNotes.Find(p => p.CustomerId == customerId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetByStatusAndCustomer(Guid statusId, Guid customerId)
        {
            var deliveryNotes = unitOfWork.DeliveryNotes.Find(p => p.StatusId == statusId && p.CustomerId == customerId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetBySalesInvoice(Guid salesInvoiceId)
        {
            var deliveryNotes = unitOfWork.DeliveryNotes.GetByInvoiceId(salesInvoiceId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetDeliveryNotesToInvoice(Guid customerId)
        {
            var deliveryNotes = unitOfWork.DeliveryNotes.Find(p => p.SalesInvoiceId == null && p.CustomerId == customerId);
            return deliveryNotes;
        }

        public async Task<IEnumerable<DeliveryNote>> GetByReferenceId(Guid referenceId)
        {
            return await unitOfWork.DeliveryNotes.GetByReferenceId(referenceId);
        }

        public async Task<GenericResponse> Create(CreateHeaderRequest createRequest)
        {
            var response = await ValidateCreateInvoiceRequest(createRequest);
            if (!response.Result) return response;

            var deliveryNoteEntities = (DeliveryNoteEntities)response.Content!;

            var counterObj = await exerciseService.GetNextCounter(deliveryNoteEntities.Exercise.Id, "deliverynote");
            if (counterObj.Content == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseCounterError"));

            var deliveryNote = new DeliveryNote()
            {
                Id = createRequest.Id,
                ExerciseId = createRequest.ExerciseId,
                CustomerId = createRequest.CustomerId,
                SiteId = deliveryNoteEntities.Site.Id,
                CreatedOn = createRequest.Date,
                Number = counterObj.Content.ToString()!,
                StatusId = deliveryNoteEntities.Lifecycle.InitialStatusId!.Value
            };
            await unitOfWork.DeliveryNotes.Add(deliveryNote);

            return new GenericResponse(true, deliveryNote);
        }

        public async Task<GenericResponse> CreateFromSalesOrder(SalesOrderHeader salesOrder)
        {
            if (!salesOrder.CustomerId.HasValue)
                return new GenericResponse(false, localizationService.GetLocalizedString("CustomerNotFound"));

            var createDto = new CreateHeaderRequest
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now,
                CustomerId = salesOrder.CustomerId.Value
            };

            var currentExercise = exerciseService.GetExerciceByDate(createDto.Date);
            if (currentExercise == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseNotFoundForDate"));

            createDto.ExerciseId = currentExercise.Id;

            var createResponse = await Create(createDto);
            if (!createResponse.Result) return createResponse;

            var deliveryNote = (DeliveryNote)createResponse.Content!;
            var addOrderResponse = await AddOrder(deliveryNote.Id, salesOrder);
            if (!addOrderResponse.Result) return addOrderResponse;

            return new GenericResponse(true, deliveryNote);
        }

        public async Task<GenericResponse> Update(DeliveryNote deliveryNote)
        {
            // Netejar dependencies per evitar col·lisions de EF
            deliveryNote.Details?.Clear();

            var exists = await unitOfWork.DeliveryNotes.Exists(deliveryNote.Id);
            if (!exists)
                return new GenericResponse(false, localizationService.GetLocalizedString("Common.IdNotExist", deliveryNote.Id));

            await unitOfWork.DeliveryNotes.Update(deliveryNote);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var deliveryNotes = await unitOfWork.DeliveryNotes.Get(id);
            if (deliveryNotes == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("Common.IdNotExist", id));

            await unitOfWork.DeliveryNotes.Remove(deliveryNotes);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Deliver(DeliveryNote deliveryNote)
        {
            var warehouseResponse = await RetriveFromWarehose(deliveryNote);
            if (!warehouseResponse.Result) return warehouseResponse;

            var orderServiceResponse = await salesOrderService.Deliver(deliveryNote.Id);
            if (!orderServiceResponse.Result) return orderServiceResponse;

            deliveryNote.DeliveryDate ??= DateTime.Now;
            await Update(deliveryNote);

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> UnDeliver(DeliveryNote deliveryNote)
        {
            var warehouseResponse = await ReturnToWarehose(deliveryNote);
            if (!warehouseResponse.Result) return warehouseResponse;

            var orderServiceResponse = await salesOrderService.UnDeliver(deliveryNote.Id);
            if (!orderServiceResponse.Result) return orderServiceResponse;

            deliveryNote.DeliveryDate = null;
            await Update(deliveryNote);

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Invoice(Guid salesInvoiceId)
        {
            return await ChangeInvoiceableStatus(salesInvoiceId, true);
        }

        public async Task<GenericResponse> UnInvoice(Guid salesInvoiceId)
        {
            return await ChangeInvoiceableStatus(salesInvoiceId, false);
        }

        public async Task<GenericResponse> ChangeInvoiceableStatus(Guid salesInvoiceId, bool isInvoiced)
        {
            var deliveryNotes = GetBySalesInvoice(salesInvoiceId);
            if (deliveryNotes != null && deliveryNotes.Any())
            {
                foreach (var deliveryNote in deliveryNotes.ToList())
                {
                    foreach (var detail in deliveryNote.Details)
                    {
                        detail.IsInvoiced = isInvoiced;
                        unitOfWork.DeliveryNotes.Details.UpdateWithoutSave(detail);
                    }

                    await unitOfWork.CompleteAsync();
                }
            }

            return new GenericResponse(true);
        }

        private async Task<GenericResponse> RetriveFromWarehose(DeliveryNote deliveryNote)
        {
            foreach (var detail in deliveryNote.Details)
            {
                detail.Reference = null;

                var stockMovement = new StockMovement
                {
                    MovementDate = DateTime.Now,
                    CreatedOn = DateTime.Now,
                    MovementType = StockMovementType.OUTPUT,
                    Description = localizationService.GetLocalizedString("Movement.AlbaranDescription", deliveryNote.Number),
                    ReferenceId = detail.ReferenceId,
                    Quantity = detail.Quantity,
                };
                var response = await stockMovementService.Create(stockMovement);
                if (!response.Result) return response;
            }

            return new GenericResponse(true);
        }

        private async Task<GenericResponse> ReturnToWarehose(DeliveryNote deliveryNote)
        {
            foreach (var detail in deliveryNote.Details)
            {
                detail.Reference = null;

                var stockMovement = new StockMovement
                {
                    MovementDate = DateTime.Now,
                    CreatedOn = DateTime.Now,
                    MovementType = StockMovementType.INPUT,
                    Description = localizationService.GetLocalizedString("Movement.ReturnDescription", deliveryNote.Number),
                    ReferenceId = detail.ReferenceId,
                    Quantity = detail.Quantity,
                };
                var response = await stockMovementService.Create(stockMovement);
                if (!response.Result) return response;
            }

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> AddOrder(Guid deliveryNoteId, SalesOrderHeader order)
        {
            // Carregar l'albarà persistit per validar el seu estat real
            var persistedDeliveryNote = await unitOfWork.DeliveryNotes.Get(deliveryNoteId);
            if (persistedDeliveryNote == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteNotFound", deliveryNoteId));

            // Validar que l'albarà no estigui entregat
            var entregat = await unitOfWork.Lifecycles.GetStatusByName(lifecycleName, StatusConstants.Statuses.Entregat);
            if (entregat == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteStatusNotFound"));
            if (persistedDeliveryNote.StatusId == entregat.Id)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteAlreadyDelivered"));

            // Validar que l'albarà no estigui facturat
            if (persistedDeliveryNote.SalesInvoiceId.HasValue)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteIsInvoiced"));

            // Guard d'idempotència: llegir l'estat real de la comanda a la BD
            // per evitar duplicats quan dues pestanyes envien la mateixa comanda simultàniament
            var currentOrder = unitOfWork.SalesOrderHeaders.Find(o => o.Id == order.Id).FirstOrDefault();
            if (currentOrder == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesOrderNotFound"));

            // Validar que el client de la comanda coincideixi amb el de l'albarà
            if (currentOrder.CustomerId != persistedDeliveryNote.CustomerId)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteCustomerMismatch"));

            if (currentOrder.DeliveryNoteId.HasValue && currentOrder.DeliveryNoteId.Value == deliveryNoteId)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesOrderAlreadyInThisNote"));

            if (currentOrder.DeliveryNoteId.HasValue && currentOrder.DeliveryNoteId.Value != deliveryNoteId)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesOrderAlreadyAssigned"));

            // Crear línies de l'albarà a partir dels SalesOrderDetails persistits (no del DTO)
            var deliveryNoteDetails = new List<DeliveryNoteDetail>();
            foreach (var salesDetail in currentOrder.SalesOrderDetails)
            {
                var deliveryNoteDetail = new DeliveryNoteDetail
                {
                    DeliveryNoteId = deliveryNoteId
                };
                deliveryNoteDetail.SetFromOrderDetail(salesDetail);
                deliveryNoteDetails.Add(deliveryNoteDetail);
            }
            await unitOfWork.DeliveryNotes.Details.AddRange(deliveryNoteDetails);

            // Associar albarà a comanda actualitzant NOMÉS DeliveryNoteId sobre l'entitat persistida,
            // conservant el StatusId real (p. ex. ComandaServida) sense sobreescriure'l amb el del DTO
            currentOrder.DeliveryNoteId = deliveryNoteId;
            var updateOrderResponse = await salesOrderService.Update(currentOrder);
            if (!updateOrderResponse.Result) return updateOrderResponse;

            return new GenericResponse(true, deliveryNoteDetails);
        }

        public async Task<GenericResponse> RemoveOrder(Guid deliveryNoteId, SalesOrderHeader order)
        {
            // Carregar l'albarà persistit per validar el seu estat real
            var persistedDeliveryNote = await unitOfWork.DeliveryNotes.Get(deliveryNoteId);
            if (persistedDeliveryNote == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteNotFound", deliveryNoteId));

            // Validar que l'albarà no estigui entregat
            var entregat = await unitOfWork.Lifecycles.GetStatusByName(lifecycleName, StatusConstants.Statuses.Entregat);
            if (entregat == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteStatusNotFound"));
            if (persistedDeliveryNote.StatusId == entregat.Id)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteAlreadyDelivered"));

            // Validar que l'albarà no estigui facturat
            if (persistedDeliveryNote.SalesInvoiceId.HasValue)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteIsInvoiced"));

            // Carregar la comanda persistida per validar la relació exacta
            var currentOrder = unitOfWork.SalesOrderHeaders.Find(o => o.Id == order.Id).FirstOrDefault();
            if (currentOrder == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesOrderNotFound"));

            // Validar que la comanda pertany exactament a aquest albarà
            if (!currentOrder.DeliveryNoteId.HasValue || currentOrder.DeliveryNoteId.Value != deliveryNoteId)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteOrderNotRelated"));

            var orderStatus = await unitOfWork.Lifecycles.GetStatusByName(
                StatusConstants.Lifecycles.SalesOrder,
                StatusConstants.Statuses.Comanda);
            if (orderStatus == null || orderStatus.Disabled)
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.Comanda));

            // Obtenir els IDs dels detalls persistits de la comanda
            var persistedDetailIds = currentOrder.SalesOrderDetails.Select(d => d.Id).ToHashSet();

            // Eliminar NOMÉS els DeliveryNoteDetails d'aquest albarà que estan lligats als detalls persistits
            var deliveryNoteDetails = unitOfWork.DeliveryNotes.Details
                .Find(d => d.DeliveryNoteId == deliveryNoteId
                           && d.SalesOrderDetailId != null
                           && persistedDetailIds.Contains(d.SalesOrderDetailId.Value))
                .ToList();

            await unitOfWork.DeliveryNotes.Details.RemoveRange(deliveryNoteDetails);

            // Alliberar i normalitzar la comanda perquè sigui assignable de nou.
            currentOrder.DeliveryNoteId = null;
            currentOrder.StatusId = orderStatus.Id;
            currentOrder.UnDeliver();
            foreach (var detail in currentOrder.SalesOrderDetails)
            {
                var updateDetailResponse = await salesOrderService.UpdateDetail(detail);
                if (!updateDetailResponse.Result) return updateDetailResponse;
            }

            var updateOrderResponse = await salesOrderService.Update(currentOrder);
            if (!updateOrderResponse.Result) return updateOrderResponse;

            return new GenericResponse(true, deliveryNoteDetails);
        }

        private async Task<GenericResponse> ValidateCreateInvoiceRequest(CreateHeaderRequest createInvoiceRequest)
        {
            var exercise = await unitOfWork.Exercices.Get(createInvoiceRequest.ExerciseId);
            if (exercise == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseNotFound"));

            var customer = await unitOfWork.Customers.Get(createInvoiceRequest.CustomerId);
            if (customer == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("CustomerNotFound"));
            if (!customer.IsValidForSales())
                return new GenericResponse(false, localizationService.GetLocalizedString("CustomerInvalid"));
            if (customer.MainAddress() == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("CustomerNoAddresses"));

            var site = await enterpriseService.GetDefaultSite();
            if (site == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SiteNotFound"));
            if (!site.IsValidForSales())
                return new GenericResponse(false, localizationService.GetLocalizedString("SiteInvalid"));

            var lifecycle = unitOfWork.Lifecycles.Find(l => l.Name == lifecycleName).FirstOrDefault();
            if (lifecycle == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNotFound", lifecycleName));
            if (!lifecycle.InitialStatusId.HasValue)
                return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNoInitialStatus", lifecycleName));

            DeliveryNoteEntities entities;
            entities.Exercise = exercise;
            entities.Customer = customer;
            entities.Site = site;
            entities.Lifecycle = lifecycle;
            return new GenericResponse(true, entities);
        }
    }
}

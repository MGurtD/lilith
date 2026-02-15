using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;


namespace Application.Services.Purchase
{
    public class PurchaseOrderService(IUnitOfWork unitOfWork, IExerciseService exerciseService, ILocalizationService localizationService) : IPurchaseOrderService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IExerciseService _exerciseService = exerciseService;
        private readonly ILocalizationService _localizationService = localizationService;
        private readonly string LifecycleName = StatusConstants.Lifecycles.PurchaseOrder;
        private readonly string LifecycleDetailsName = StatusConstants.Lifecycles.PurchaseOrderDetail;

        public async Task<PurchaseOrder?> GetById(Guid id)
        {
            return await _unitOfWork.PurchaseOrders.Get(id);
        }

        public async Task<Application.Contracts.PurchaseOrderReportResponse?> GetDtoForReportingById(Guid id)
        {
            var order = await _unitOfWork.PurchaseOrders.Get(id);
            if (order == null) return null;

            var supplier = await _unitOfWork.Suppliers.Get(order.SupplierId);
            if (supplier == null) return null;

            var site = (await _unitOfWork.Sites.FindAsync(s => !s.Disabled)).FirstOrDefault();
            if (site == null) return null;

            var referenceIds = order.Details.Select(detail => detail.ReferenceId).ToList();
            var references = await _unitOfWork.References.FindAsync(r => referenceIds.Contains(r.Id));
            var supplierReferences = _unitOfWork.Suppliers.GetSupplierReferences(supplier.Id);

            var orderDto = new PurchaseOrderReportDto()
            {
                Number = order.Number,
                Date = order.Date,
                Total = order.Details.Sum(d => d.Amount)
            };

            var orderDetails = new List<PurchaseOrderDetailReportDto>();
            foreach (var detail in order.Details)
            {
                var reference = references.FirstOrDefault(r => r.Id == detail.ReferenceId);
                var supplierReference = supplierReferences.FirstOrDefault(sr => sr.ReferenceId == detail.ReferenceId);
                var description = supplierReference != null ? $"{supplierReference.SupplierCode} - {supplierReference.SupplierDescription}" : reference!.GetFullName();

                if (!string.IsNullOrEmpty(detail.Description)) description = $"{description} - {detail.Description}";

                orderDetails.Add(new PurchaseOrderDetailReportDto()
                {
                    Description = description,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    Amount = detail.Amount
                });
            }

            return new PurchaseOrderReportResponse()
            {
                Supplier = supplier,
                Site = site,
                Order = orderDto,
                Details = orderDetails.GroupBy(d => d.Description).Select(g => new PurchaseOrderDetailReportDto()
                {
                    Description = g.Key,
                    Quantity = g.Sum(d => d.Quantity),
                    UnitPrice = g.First().UnitPrice,
                    Amount = g.Sum(d => d.Amount)
                }).ToList()
            }; 
        }

        public async Task<List<PurchaseOrder>> GetBetweenDates(DateTime startDate, DateTime endDate, Guid? supplierId, Guid? statusId)
        {
            var orders = new List<PurchaseOrder>();
            if (supplierId.HasValue)
            {
                orders = await _unitOfWork.PurchaseOrders.FindAsync(p => p.Date >= startDate && p.Date <= endDate && p.SupplierId == supplierId);
            }
            else if (statusId.HasValue)
            {
                orders = await _unitOfWork.PurchaseOrders.FindAsync(p => p.Date >= startDate && p.Date <= endDate && p.StatusId == statusId);
            }
            else
            {
                orders = await _unitOfWork.PurchaseOrders.FindAsync(p => p.Date >= startDate && p.Date <= endDate);
            }
            return orders;
        }

        public async Task<List<ReceiptOrderDetailGroup>> GetGroupedOrdersWithDetailsToReceiptBySupplier(Guid supplierId)
        {
            // Obtenir comandes amb detalls a rebre per proveïdor
            var supplierOrders = await GetOrdersWithDetailsToReceiptBySupplier(supplierId);

            // Obtenir referències, fases d'ordre de treball i ordres de treball associades als detalls
            var referenceIds = supplierOrders.SelectMany(o => o.Details).Select(d => d.ReferenceId).Distinct().ToList();
            var references = await _unitOfWork.References.FindAsync(r => referenceIds.Contains(r.Id));
            var workorderPhaseIds = supplierOrders.SelectMany(o => o.Details)
                .Where(d => d.WorkOrderPhaseId.HasValue)
                .Select(d => d.WorkOrderPhaseId!.Value)
                .Distinct()
                .ToList();
            var workorderPhases = await _unitOfWork.WorkOrders.Phases.FindAsync(p => workorderPhaseIds.Contains(p.Id));
            var workorders = await _unitOfWork.WorkOrders.FindAsync(w => workorderPhases.Select(p => p.WorkOrderId).Contains(w.Id));
            foreach (var phase in workorderPhases) 
                phase.WorkOrder = workorders.FirstOrDefault(w => w.Id == phase.WorkOrderId);

            // Agrupar detalls per referència i descripció
            var groupedDetails = supplierOrders.SelectMany(o => o.Details)
                .GroupBy(d => new { d.ReferenceId, d.Description })
                .Select(g => new ReceiptOrderDetailGroup()
                {
                    Reference = references.FirstOrDefault(r => r.Id == g.Key.ReferenceId)!,
                    Description = g.Key.Description,
                    Quantity = g.Sum(d => d.Quantity),
                    ReceivedQuantity = g.Sum(d => d.ReceivedQuantity),
                    Price = 0
                }).ToList();

            var detailToOrderMap = supplierOrders
                .SelectMany(o => o.Details.Select(d => new { DetailId = d.Id, OrderNumber = o.Number }))
                .ToDictionary(x => x.DetailId, x => x.OrderNumber);

            // Afegir detalls a cada grup
            foreach (var group in groupedDetails)
            {
                group.Details = [.. supplierOrders.SelectMany(o => o.Details)
                    .Where(d => d.ReferenceId == group.Reference.Id && d.Description == group.Description)
                    .Select(d => new ReceiptOrderDetail()
                    {
                        Id = d.Id,
                        OrderNumber = detailToOrderMap[d.Id],
                        ExpectedReceiptDate = d.ExpectedReceiptDate,
                        WorkOrder = workorderPhases.FirstOrDefault(w => w.Id == d.WorkOrderPhaseId) != null ? workorderPhases.FirstOrDefault(w => w.Id == d.WorkOrderPhaseId)!.WorkOrder!.Code : string.Empty,
                        WorkOrderPhase = workorderPhases.FirstOrDefault(w => w.Id == d.WorkOrderPhaseId) != null ? workorderPhases.FirstOrDefault(w => w.Id == d.WorkOrderPhaseId)!.Description : string.Empty,
                        Quantity = d.Quantity,
                        ReceivedQuantity = d.ReceivedQuantity
                    })];
            }

            return groupedDetails;
        }

        public async Task<List<PurchaseOrder>> GetOrdersWithDetailsToReceiptBySupplier(Guid supplierId)
        {
            var discartedStatuses = new List<Guid>();
            var statusReceived = await _unitOfWork.Lifecycles.GetStatusByName(LifecycleDetailsName, StatusConstants.Statuses.Rebuda);
            if (statusReceived != null) discartedStatuses.Add(statusReceived.Id);
            var statusCancelled = await _unitOfWork.Lifecycles.GetStatusByName(LifecycleDetailsName, StatusConstants.Statuses.Cancellada);
            if (statusCancelled != null) discartedStatuses.Add(statusCancelled.Id);

            return await _unitOfWork.PurchaseOrders.GetOrdersWithDetailsToReceiptBySupplier(supplierId, discartedStatuses);
        }

        public async Task<List<PurchaseOrder>> GetBySupplier(Guid supplierId)
        {
            return await _unitOfWork.PurchaseOrders.FindAsync(p => p.SupplierId == supplierId);
        }

        public async Task<GenericResponse> CreateFromWo(PurchaseOrderFromWO[] request)
        {
            if (request == null)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("PurchaseOrderIncompleteRequest"));
            }
            //Recuperar exercici, genèric per totes les comandes
            var exercise = _unitOfWork.Exercices.Find(ex => ex.StartDate <= DateTime.Today && ex.EndDate > DateTime.Today).FirstOrDefault();
            if (exercise == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("ExerciseNotFound"));
            
            //Crear un ID per cada bloc de proveïdor
            var oldSupplier = Guid.Empty;
            var purchaseOrderId = Guid.Empty;
            var statusId = Guid.Empty;

            var status = _unitOfWork.Lifecycles.Find(l => l.Name == LifecycleDetailsName).FirstOrDefault();
            if (status == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("LifecycleNotFound", LifecycleDetailsName));
            if (!status.InitialStatusId.HasValue) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("LifecycleNoInitialStatus", LifecycleDetailsName));

            foreach (PurchaseOrderFromWO purchaseOrder in request.OrderBy(r => r.SupplierId))
            {
                if (purchaseOrder == null) continue;
                if(oldSupplier != purchaseOrder.SupplierId)
                {
                    oldSupplier = purchaseOrder.SupplierId;
                    purchaseOrderId = Guid.NewGuid();
                    var order = new CreatePurchaseDocumentRequest()
                    {
                        Id = purchaseOrderId,
                        ExerciseId = exercise.Id,
                        SupplierId = purchaseOrder.SupplierId,
                        Date = DateTime.Today
                    };
                    var response = await Create(order);
                    if(!response.Result)
                    {
                        return new GenericResponse(false, _localizationService.GetLocalizedString("WorkOrderCreateError"));
                    }
                }
                // Obtenir referència
                var reference = await _unitOfWork.References.Get(purchaseOrder.ServiceReferenceId);
                if (reference == null) 
                    return new GenericResponse(false, _localizationService.GetLocalizedString("ReferenceNotExistent"));
                
                // Obtenir relació proveïdor-referència
                var supplierReference = await _unitOfWork.Suppliers.GetSupplierReferenceBySupplierIdAndReferenceId(purchaseOrder.SupplierId, purchaseOrder.ServiceReferenceId);

                // Crear detall de comanda
                var unitPrice = supplierReference != null ? supplierReference.SupplierPrice : reference.LastCost;
                var expectedReceiptDate = DateTime.Today.AddDays(supplierReference != null ? supplierReference.SupplyDays : 0);

                var detail = new PurchaseOrderDetail()
                {
                    PurchaseOrderId = purchaseOrderId,
                    ReferenceId = purchaseOrder.ServiceReferenceId,
                    WorkOrderPhaseId = purchaseOrder.PhaseId,
                    StatusId = status.InitialStatusId.Value,
                    Quantity = purchaseOrder.Quantity,
                    ReceivedQuantity = 0,
                    UnitPrice = unitPrice,
                    Amount = purchaseOrder.Quantity * unitPrice,
                    ExpectedReceiptDate = expectedReceiptDate
                };

                var detailResponse = await AddDetail(detail);
                if (!detailResponse.Result) {                    
                    return new GenericResponse(false, _localizationService.GetLocalizedString("WorkOrderLineCreateError"));
                }

                // Actualitzar fase de l'ordre de treball
                var phase = await _unitOfWork.WorkOrders.Phases.Get(purchaseOrder.PhaseId);
                if (phase != null) {
                    phase.PurchaseOrderId = purchaseOrderId;
                    await _unitOfWork.WorkOrders.Phases.Update(phase);
                }
            }

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Create(CreatePurchaseDocumentRequest createRequest)
        {
            var exercise = await _unitOfWork.Exercices.Get(createRequest.ExerciseId);
            if (exercise == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("ExerciseNotFound"));

            var status = _unitOfWork.Lifecycles.Find(l => l.Name == LifecycleName).FirstOrDefault();
            if (status == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("LifecycleNotFound", LifecycleName));
            if (!status.InitialStatusId.HasValue) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("LifecycleNoInitialStatus", LifecycleName));

            var counterObj = await _exerciseService.GetNextCounter(exercise.Id, "purchaseorder");
            if (counterObj == null || counterObj.Content == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("ExerciseCounterError"));

            var orderNumber = counterObj.Content.ToString()!;
            var order = new PurchaseOrder()
            {
                Id = createRequest.Id,
                ExerciseId = createRequest.ExerciseId,
                SupplierId = createRequest.SupplierId,
                Date = createRequest.Date,
                Number = orderNumber,
                StatusId = status.InitialStatusId.Value
            };
            await _unitOfWork.PurchaseOrders.Add(order);

            return new GenericResponse(true);
        }
        public async Task<GenericResponse> Update(PurchaseOrder order)
        {
            // Netejar dependencies per evitar col·lisions de EF
            order.Details?.Clear();

            var exists = await _unitOfWork.PurchaseOrders.Exists(order.Id);
            if (!exists) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.IdNotExist", order.Id));

            await _unitOfWork.PurchaseOrders.Update(order);
            return new GenericResponse(true);
        }
        public async Task<GenericResponse> Remove(Guid id)
        {
            var receipt = await _unitOfWork.PurchaseOrders.GetHeaders(id);
            if (receipt == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.IdNotExist", id));
            
            var workorderphases = await _unitOfWork.WorkOrders.Phases.Find(w => w.PurchaseOrderId == id).AsQueryable().AsNoTracking().ToListAsync();

            foreach (var phase in workorderphases)
            {
                phase.PurchaseOrderId = null;
                await _unitOfWork.WorkOrders.Phases.Update(phase);
            }

            await _unitOfWork.PurchaseOrders.Remove(receipt);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> DeterminateStatus(Guid id)
        {
            var order = await _unitOfWork.PurchaseOrders.Get(id);
            if (order == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.IdNotExist", id));

            var initialStatusId = order.StatusId;

            var cancelledStatus = await _unitOfWork.Lifecycles.GetStatusByName(LifecycleDetailsName, StatusConstants.Statuses.Cancellada);
            if (cancelledStatus == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.Cancellada));
            var cancelledDetails = order.Details.Where(d => d.StatusId == cancelledStatus.Id);
            
            var receivedStatus = await _unitOfWork.Lifecycles.GetStatusByName(LifecycleDetailsName, StatusConstants.Statuses.Rebuda);
            if (receivedStatus == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.Rebuda));
            var receivedDetails = order.Details.Where(d => d.StatusId == receivedStatus.Id);

            if (cancelledDetails.Count() == order.Details.Count)
            {
                var status = await _unitOfWork.Lifecycles.GetStatusByName(LifecycleName, StatusConstants.Statuses.Cancellada);
                if (status != null) order.StatusId = status.Id;
            }
            else if (receivedDetails.Count() == order.Details.Count)
            {
                var status = await _unitOfWork.Lifecycles.GetStatusByName(LifecycleName, StatusConstants.Statuses.Rebuda);
                if (status != null) order.StatusId = status.Id;
            }

            if (order.StatusId != initialStatusId)
            {
                await _unitOfWork.PurchaseOrders.Update(order);
            }
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> AddDetail(PurchaseOrderDetail detail)
        {
            detail.Reference = null;
            detail.UnitPrice = detail.Quantity > 0 ? detail.Amount / detail.Quantity : 0;

            await _unitOfWork.PurchaseOrders.Details.Add(detail);
            return new GenericResponse(true);
        }
        public async Task<GenericResponse> UpdateDetail(PurchaseOrderDetail detail)
        {
            var exists = await _unitOfWork.PurchaseOrders.Details.Exists(detail.Id);
            if (!exists) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.IdNotExist", detail.Id));

            detail.Reference = null;
            detail.UnitPrice = detail.Quantity > 0 ? detail.Amount / detail.Quantity : 0;
            await _unitOfWork.PurchaseOrders.Details.Update(detail);
            return new GenericResponse(true);
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = await _unitOfWork.PurchaseOrders.Details.Get(id);
            if (detail == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.IdNotExist", id));

            await _unitOfWork.PurchaseOrders.Details.Remove(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> AddReceivedQuantityAndCalculateStatus(PurchaseOrderDetail detail, int quantity)
        {
            detail.ReceivedQuantity += quantity;

            var status = await _unitOfWork.Lifecycles.GetStatusByName(LifecycleDetailsName, detail.ReceivedQuantity >= detail.Quantity ? StatusConstants.Statuses.Rebuda : StatusConstants.Statuses.RebuidaParcialment);
            if (status != null) detail.StatusId = status.Id;

            return await UpdateDetail(detail);
        }

        public async Task<GenericResponse> SubstractReceivedQuantityAndCalculateStatus(PurchaseOrderDetail detail, int quantity)
        {
            detail.ReceivedQuantity -= quantity;

            var status = await _unitOfWork.Lifecycles.GetStatusByName(LifecycleDetailsName, detail.ReceivedQuantity == 0 ? StatusConstants.Statuses.PendentRebre : StatusConstants.Statuses.RebuidaParcialment);
            if (status != null) detail.StatusId = status.Id;

            return await UpdateDetail(detail);
        }

        #region Receptions

        public async Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id)
        {
            return await _unitOfWork.PurchaseOrders.GetReceptions(id);
        }

        public async Task<GenericResponse> AddReception(PurchaseOrderReceiptDetail reception)
        {
            await _unitOfWork.PurchaseOrders.Receptions.Add(reception);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RemoveReception(PurchaseOrderReceiptDetail reception)
        {
            var orderDetail = (await _unitOfWork.PurchaseOrders.Details.FindAsync(d => d.Id == reception.PurchaseOrderDetailId)).FirstOrDefault();
            if (orderDetail == null) 
                return new GenericResponse(false, _localizationService.GetLocalizedString("PurchaseOrderDetailNotFound", reception.PurchaseOrderDetailId));

            // Eliminar recepció
            await _unitOfWork.PurchaseOrders.Receptions.Remove(reception);

            // Actualitzar detall de comanda
            await SubstractReceivedQuantityAndCalculateStatus(orderDetail, (int)reception.Quantity);

            return await DeterminateStatus(orderDetail.PurchaseOrderId);
        }        

        #endregion
    }
}








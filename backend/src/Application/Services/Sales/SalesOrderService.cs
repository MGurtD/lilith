
using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.Extensions.Logging;

namespace Application.Services.Sales
{
    public class SalesOrderService(
        IUnitOfWork unitOfWork,
        IEnterpriseService enterpriseService,
        IExerciseService exerciseService,
        IBudgetService budgetService,
        ILocalizationService localizationService,
        Microsoft.Extensions.Logging.ILogger<SalesOrderService> logger) : ISalesOrderService
    {
        public async Task<SalesOrderHeader?> GetById(Guid id)
        {
            var salesOrderHeader = await unitOfWork.SalesOrderHeaders.Get(id);
            return salesOrderHeader;
        }

        public SalesOrderHeader? GetOrderFromBudget(Guid budgetId)
        {
            var salesOrder = unitOfWork.SalesOrderHeaders.Find(p => p.BudgetId == budgetId).FirstOrDefault();
            return salesOrder;
        }

        public IEnumerable<SalesOrderHeader> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var salesOrderHeaders = unitOfWork.SalesOrderHeaders.Find(p => p.Date >= startDate && p.Date <= endDate);
            return salesOrderHeaders;
        }
        public IEnumerable<SalesOrderHeader> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId)
        {
            var invoices = unitOfWork.SalesOrderHeaders.Find(p => p.Date >= startDate && p.Date <= endDate && p.CustomerId == customerId);
            return invoices;
        }

        public IEnumerable<SalesOrderHeader> GetByDeliveryNoteId(Guid deliveryNoteId)
        {
            var orders = unitOfWork.SalesOrderHeaders.Find(p => p.DeliveryNoteId == deliveryNoteId);
            return orders;
        }

        public IEnumerable<SalesOrderHeader> GetOrdersToDeliver(Guid customerId)
        {
            var orders = unitOfWork.SalesOrderHeaders.Find(p => p.CustomerId == customerId && p.DeliveryNoteId == null);
            return orders;
        }

        public async Task<GenericResponse> CreateFromBudget(Budget budget)
        {
            var createDto = new CreateHeaderRequest
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now,
                CustomerId = budget.CustomerId
            };

            // Obtenir l'exercici actual pel nou document
            var currentExercise = exerciseService.GetExerciceByDate(createDto.Date);
            if (currentExercise == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseNotFoundForDate"));
            }
            createDto.ExerciseId = currentExercise.Id;

            var createResponse = await Create(createDto);
            if (!createResponse.Result) return createResponse;

            var salesOrder = (SalesOrderHeader)createResponse.Content!;
            salesOrder.ExpectedDate = DateTime.Now.AddDays(budget.DeliveryDays);
            salesOrder.BudgetId = budget.Id;
            salesOrder.TotalWeight = budget.TotalWeight;
            // Budget doesn't have TotalVolume yet, but we add it for parity
            var updateResponse = await Update(salesOrder);
            if (!updateResponse.Result) return updateResponse;

            var detailMap = new Dictionary<Guid, Guid>();
            foreach (var detail in budget.Details)
            {
                var salesOrderDetail = new SalesOrderDetail(detail, DateTime.Now.AddDays(budget.DeliveryDays))
                {
                    SalesOrderHeaderId = salesOrder.Id
                };
                await AddDetail(salesOrderDetail, skipExternalServices: true);
                detailMap[detail.Id] = salesOrderDetail.Id;
            }

            if (budget.Transports != null)
            {
                foreach (var transport in budget.Transports)
                {
                    var newTransport = new SalesOrderTransport
                    {
                        Id = Guid.NewGuid(),
                        SalesOrderHeaderId = salesOrder.Id,
                        TransportRateDetailId = transport.TransportRateDetailId,
                        Weight = transport.Weight,
                        Volume = transport.Volume,
                        Distance = transport.Distance,
                        Price = transport.Price,
                        Description = transport.Description,
                        Destination = transport.Destination
                    };
                    await unitOfWork.SalesOrderHeaders.Transports.Add(newTransport);
                }
            }

            if (budget.ExternalServices != null)
            {
                foreach (var extService in budget.ExternalServices)
                {
                    var newExtService = new SalesOrderExternalServices
                    {
                        Id = Guid.NewGuid(),
                        SalesOrderHeaderId = salesOrder.Id,
                        ReferenceId = extService.ReferenceId,
                        Description = extService.Description,
                        Weight = extService.Weight,
                        Volume = extService.Volume,
                        Quantity = extService.Quantity,
                        SupplierId = extService.SupplierId,
                        UnitPrice = extService.UnitPrice,
                        TotalPrice = extService.TotalPrice,
                        Details = new List<SalesOrderExternalServiceDetail>()
                    };

                    if (extService.Details != null)
                    {
                        foreach (var esd in extService.Details)
                        {
                            if (detailMap.TryGetValue(esd.BudgetDetailId, out var newDetailId))
                            {
                                newExtService.Details.Add(new SalesOrderExternalServiceDetail
                                {
                                    Id = Guid.NewGuid(),
                                    SalesOrderExternalServiceId = newExtService.Id,
                                    SalesOrderDetailId = newDetailId,
                                    Weight = esd.Weight,
                                    Volume = esd.Volume,
                                    Quantity = esd.Quantity
                                });
                            }
                        }
                    }

                    await unitOfWork.SalesOrderHeaders.ExternalServices.Add(newExtService);
                }
            }

            var acceptResponse = await budgetService.Accept(budget.Id);
            if (!acceptResponse.Result) return acceptResponse;

            return new GenericResponse(true, salesOrder);
        }

        public async Task<GenericResponse> Create(CreateHeaderRequest createRequest)
        {
            var response = await ValidateCreateInvoiceRequest(createRequest);
            if (!response.Result) return response;

            var orderEntities = (InvoiceEntities)response.Content!;

            var counterObj = await exerciseService.GetNextCounter(orderEntities.Exercise.Id, "salesorder");
            if (!counterObj.Result || counterObj.Content == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseCounterError"));

            var order = new SalesOrderHeader
            {
                Id = createRequest.Id,
                Number = counterObj.Content.ToString()!,
                Date = createRequest.Date
            };

            // Estat inicial
            if (createRequest.InitialStatusId.HasValue)
            {
                order.StatusId = createRequest.InitialStatusId;
            }
            else
            {
                var lifecycle = unitOfWork.Lifecycles.Find(l => l.Name == StatusConstants.Lifecycles.SalesOrder).FirstOrDefault();
                if (lifecycle == null)
                    return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNotFound", StatusConstants.Lifecycles.SalesOrder));
                if (!lifecycle.InitialStatusId.HasValue)
                    return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNoInitialStatus", StatusConstants.Lifecycles.SalesOrder));
                order.StatusId = lifecycle.InitialStatusId;
            }

            order.ExerciseId = orderEntities.Exercise.Id;
            order.SetCustomer(orderEntities.Customer);
            order.SetSite(orderEntities.Site);

            await unitOfWork.SalesOrderHeaders.Add(order);

            return new GenericResponse(true, order);
        }

        public async Task<GenericResponse> Update(SalesOrderHeader salesOrderHeader)
        {
            salesOrderHeader.SalesOrderDetails.Clear();

            await unitOfWork.SalesOrderHeaders.Update(salesOrderHeader);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var salesOrder = await unitOfWork.SalesOrderHeaders.Get(id);
            if (salesOrder == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound", id));
            }
            else
            {
                if (salesOrder.BudgetId.HasValue)
                {
                    var budget = await unitOfWork.Budgets.Get(salesOrder.BudgetId.Value);
                    if (budget == null)
                    {
                        return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound", salesOrder.BudgetId.Value));
                    }

                    var lifecycle = unitOfWork.Lifecycles.Find(l => l.Name == StatusConstants.Lifecycles.Budget).FirstOrDefault();
                    if (lifecycle == null)
                    {
                        return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNotFound", StatusConstants.Lifecycles.Budget));
                    }

                    if (!lifecycle.InitialStatusId.HasValue)
                    {
                        return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNoInitialStatus", StatusConstants.Lifecycles.Budget));
                    }

                    budget.StatusId = lifecycle.InitialStatusId.Value;
                    budget.AcceptanceDate = null;

                    await unitOfWork.Budgets.Update(budget);
                }

                await unitOfWork.SalesOrderHeaders.Remove(salesOrder);
                return new GenericResponse(true, new List<string> { });
            }
        }

        public async Task<GenericResponse> UpdateCosts(Guid id)
        {
            var details = unitOfWork.SalesOrderDetails.Find(e => e.SalesOrderHeaderId == id).ToList();

            foreach (SalesOrderDetail detail in details)
            {
                if (detail.WorkOrderId.HasValue)
                {
                    var workOrder = await unitOfWork.WorkOrders.Get(detail.WorkOrderId.Value);
                    if (workOrder != null)
                    {
                        detail.LastCost = (workOrder.MaterialCost + workOrder.OperatorCost + workOrder.MachineCost);
                    }
                    var workMaster = unitOfWork.WorkMasters.Find(e => e.ReferenceId == detail.ReferenceId).FirstOrDefault();
                    if (workMaster != null)
                    {
                        detail.WorkMasterCost = (workMaster.MaterialCost + workMaster.MachineCost + workMaster.OperatorCost + workMaster.ExternalCost);
                    }
                    await unitOfWork.SalesOrderHeaders.UpdateDetail(detail);
                }

            }

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> AddTransport(SalesOrderTransport transport)
        {
            await unitOfWork.SalesOrderHeaders.Transports.Add(transport);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> UpdateTransport(SalesOrderTransport transport)
        {
            var exists = await unitOfWork.SalesOrderHeaders.Transports.Get(transport.Id);
            if (exists == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("ItemNotFound", transport.Id));
            }
            exists.Weight = transport.Weight;
            exists.Volume = transport.Volume;
            exists.Distance = transport.Distance;
            exists.Price = transport.Price;
            exists.Description = transport.Description;
            exists.Destination = transport.Destination;

            await unitOfWork.SalesOrderHeaders.Transports.Update(exists);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RemoveTransport(Guid id)
        {
            var exists = await unitOfWork.SalesOrderHeaders.Transports.Get(id);
            if (exists == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ItemNotFound", id));

            await unitOfWork.SalesOrderHeaders.Transports.Remove(exists);
            return new GenericResponse(true, exists);
        }

        public async Task<GenericResponse> DistributeTransportCosts(Guid salesOrderId)
        {
            return await DistributeAllCosts(salesOrderId);
        }

        public async Task<GenericResponse> DistributeAllCosts(Guid salesOrderId)
        {
            var salesOrder = await unitOfWork.SalesOrderHeaders.Get(salesOrderId);
            if (salesOrder == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("OrderNotFound", salesOrderId));
            }

            if (salesOrder.SalesOrderDetails == null || !salesOrder.SalesOrderDetails.Any())
            {
                return new GenericResponse(false, "La comanda no té línies de detall");
            }

            var totalWeight = salesOrder.SalesOrderDetails.Sum(d => 
            {
                var master = unitOfWork.WorkMasters.Find(w => w.ReferenceId == d.ReferenceId).FirstOrDefault();
                return master?.TotalWeight * d.Quantity ?? 0;
            });

            var budgetDate = DateOnly.FromDateTime(salesOrder.Date);

            try 
            {
                var totalTransportCost = salesOrder.Transports?.Sum(t => t.Price) ?? 0;

                // 1. Ponderar Transport (pes)
                foreach (var detail in salesOrder.SalesOrderDetails)
                {
                    var master = unitOfWork.WorkMasters.Find(w => w.ReferenceId == detail.ReferenceId).FirstOrDefault();
                    var detailWeight = master?.TotalWeight * detail.Quantity ?? 0;

                    decimal totalTransportShare = totalWeight > 0 ? (detailWeight / totalWeight) * totalTransportCost : 0;
                    detail.TransportCost = detail.Quantity > 0 ? totalTransportShare / detail.Quantity : 0;
                    detail.ServiceCost = 0; // Reset
                }

                // 2. Ponderar Serveis Externs
                if (salesOrder.ExternalServices != null)
                {
                    foreach (var extService in salesOrder.ExternalServices)
                    {
                        if (extService.SupplierId == Guid.Empty || extService.UnitPrice <= 0)
                        {
                            logger.LogWarning("Servei extern {ServiceId} sense proveïdor o preu. S'omet.", extService.Id);
                            continue;
                        }

                        // Obtenir la tarifa activa del proveïdor
                        var activeRates = await unitOfWork.PurchaseRates.FindAsync(r => 
                            r.SupplierId == extService.SupplierId && 
                            r.ValidFrom <= budgetDate && 
                            r.ValidTo >= budgetDate);
                        
                        var activeRate = activeRates.FirstOrDefault();
                        if (activeRate == null)
                        {
                            logger.LogWarning("No hi ha tarifa de compra activa pel proveïdor {SupplierId} a la data {Date}", extService.SupplierId, budgetDate);
                            continue;
                        }

                        // Obtenir els detalls de la tarifa
                        var rateDetails = await unitOfWork.PurchaseRateDetails.FindAsync(d => d.PurchaseRateId == activeRate.Id && d.ReferenceId == extService.ReferenceId);
                        var rateDetail = rateDetails.FirstOrDefault();

                        int calculationType = rateDetail?.CalculationType ?? 2; // Default Unitats

                        // 0 = Volum, 1 = Pes, 2 = Unitats (default)
                        decimal totalMagnitude = calculationType switch
                        {
                            0 => extService.Volume,
                            1 => extService.Weight,
                            _ => extService.Quantity
                        };

                        if (totalMagnitude <= 0)
                        {
                            if (calculationType == 0 || calculationType == 1)
                            {
                                logger.LogWarning("La magnitud (pes/volum) del servei extern {ServiceId} és {Magnitude}, es canvia a repartir per unitats.", extService.Id, totalMagnitude);
                                calculationType = 2; // Fallback to Units
                                totalMagnitude = extService.Quantity;
                            }
                            
                            if (totalMagnitude <= 0) continue;
                        }

                        decimal totalServiceCost = extService.UnitPrice * totalMagnitude;

                        if (extService.Details != null)
                        {
                            foreach (var esd in extService.Details)
                            {
                                var detailMagnitude = calculationType switch
                                {
                                    0 => esd.Volume,
                                    1 => esd.Weight,
                                    _ => esd.Quantity
                                };

                                decimal proportion = detailMagnitude / totalMagnitude;
                                decimal totalCostShare = totalServiceCost * proportion;

                                var bDetail = salesOrder.SalesOrderDetails.FirstOrDefault(d => d.Id == esd.SalesOrderDetailId);
                                if (bDetail != null)
                                {
                                    decimal unitCostShare = bDetail.Quantity > 0 ? totalCostShare / bDetail.Quantity : 0;
                                    bDetail.ServiceCost += unitCostShare;
                                }
                            }
                        }
                    }
                }

                // 3. Recalcular totals
                foreach (var detail in salesOrder.SalesOrderDetails)
                {
                    // Costos unitaris
                    detail.TotalCost = detail.UnitCost + detail.TransportCost + detail.ServiceCost;
                    
                    // Preu unitari amb benefici i descompte (en %)
                    decimal priceWithProfit = detail.TotalCost + (detail.TotalCost * (detail.Profit / 100m));
                    detail.UnitPrice = priceWithProfit - (priceWithProfit * (detail.Discount / 100m));
                    
                    // Import total de la línia
                    detail.Amount = detail.UnitPrice * detail.Quantity;

                    unitOfWork.SalesOrderHeaders.UpdateDetail(detail).Wait();
                }
                
                await unitOfWork.CompleteAsync();
                return new GenericResponse(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error a DistributeAllCosts per la comanda {OrderId}", salesOrderId);
                return new GenericResponse(false, $"Error intern: {ex.Message}");
            }
        }

        public async Task<GenericResponse> UpdateExternalService(SalesOrderExternalServices externalService)
        {
            var exists = await unitOfWork.SalesOrderHeaders.ExternalServices.Get(externalService.Id);
            if (exists == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("ItemNotFound", externalService.Id));
            }

            exists.SupplierId = externalService.SupplierId;
            exists.UnitPrice = externalService.UnitPrice;
            exists.TotalPrice = externalService.TotalPrice;

            await unitOfWork.SalesOrderHeaders.ExternalServices.Update(exists);
            return new GenericResponse(true);
        }

        public async Task<SalesOrderDetail?> GetDetailById(Guid id)
        {
            var detail = await unitOfWork.SalesOrderDetails.Get(id);
            return detail;
        }
        public async Task<GenericResponse> AddDetail(SalesOrderDetail salesOrderDetail)
            => await AddDetail(salesOrderDetail, skipExternalServices: false);

        private async Task<GenericResponse> AddDetail(SalesOrderDetail salesOrderDetail, bool skipExternalServices)
        {
            await unitOfWork.SalesOrderHeaders.AddDetail(salesOrderDetail);

            // Afegir serveis externs si té WorkMaster (no quan ve de CreateFromBudget, ja que es copien directament)
            if (!skipExternalServices && salesOrderDetail.WorkMasterId != null)
            {
                var workmaster = await unitOfWork.WorkMasters.Get(salesOrderDetail.WorkMasterId.Value);
                if (workmaster != null)
                {
                    await AddExternalServicesFromWorkmaster(workmaster, salesOrderDetail);
                }
            }

            return new GenericResponse(true);
        }
        public async Task<GenericResponse> UpdateDetail(SalesOrderDetail salesOrderDetail)
        {
            // Recuperar el detall antic per obtenir la quantitat anterior
            var oldDetail = unitOfWork.SalesOrderDetails.Find(d => d.Id == salesOrderDetail.Id).FirstOrDefault();
            var oldQuantity = oldDetail?.Quantity ?? 0;

            if (salesOrderDetail.WorkMasterId != null)
            {
                var workmaster = await unitOfWork.WorkMasters.Get(salesOrderDetail.WorkMasterId.Value);
                if (workmaster != null)
                {
                    var quantityDiff = salesOrderDetail.Quantity - oldQuantity;
                    var volumeDiff = workmaster.Volume * quantityDiff;
                    
                    var masterForWeight = unitOfWork.WorkMasters.Find(w => w.ReferenceId == salesOrderDetail.ReferenceId).FirstOrDefault();
                    var oldWeight = (masterForWeight?.TotalWeight ?? 0) * oldQuantity;
                    var newWeight = (masterForWeight?.TotalWeight ?? 0) * salesOrderDetail.Quantity;
                    var weightDiff = newWeight - oldWeight;

                    foreach (var phase in workmaster.Phases)
                    {
                        if (phase.IsExternalWork && phase.ServiceReferenceId != null)
                        {
                            var serviceRefId = phase.ServiceReferenceId.Value;
                            var existing = unitOfWork.SalesOrderHeaders.ExternalServices
                                .Find(es => es.SalesOrderHeaderId == salesOrderDetail.SalesOrderHeaderId && es.ReferenceId == serviceRefId)
                                .FirstOrDefault();

                            if (existing != null)
                            {
                                existing.Quantity += quantityDiff;
                                existing.Volume += volumeDiff;
                                existing.Weight += weightDiff;
                                await unitOfWork.SalesOrderHeaders.ExternalServices.Update(existing);

                                var existingServiceDetail = unitOfWork.SalesOrderHeaders.ExternalServiceDetails
                                    .Find(d => d.SalesOrderExternalServiceId == existing.Id && d.SalesOrderDetailId == salesOrderDetail.Id)
                                    .FirstOrDefault();

                                if (existingServiceDetail != null)
                                {
                                    existingServiceDetail.Quantity = salesOrderDetail.Quantity;
                                    existingServiceDetail.Weight = newWeight;
                                    existingServiceDetail.Volume = workmaster.Volume * salesOrderDetail.Quantity;
                                    await unitOfWork.SalesOrderHeaders.ExternalServiceDetails.Update(existingServiceDetail);
                                }
                            }
                        }
                    }
                }
            }

            salesOrderDetail.Reference = null;
            salesOrderDetail.SalesOrderHeader = null;

            await unitOfWork.SalesOrderHeaders.UpdateDetail(salesOrderDetail);
            return new GenericResponse(true);
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = unitOfWork.SalesOrderDetails.Find(d => d.Id == id).FirstOrDefault();
            if (detail == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetDetailNotFound", id));

            // Eliminar serveis externs associats
            if (detail.WorkMasterId != null)
            {
                var workmaster = await unitOfWork.WorkMasters.Get(detail.WorkMasterId.Value);
                if (workmaster != null)
                {
                    await RemoveExternalServicesFromWorkmaster(workmaster, detail);
                }
            }

            var deleted = await unitOfWork.SalesOrderHeaders.RemoveDetail(detail);
            return new GenericResponse(true, detail);
        }

        private async Task<GenericResponse> GetStatusId(string statusName)
        {
            var lifecycle = await unitOfWork.Lifecycles.GetByName(StatusConstants.Lifecycles.SalesOrder);
            if (lifecycle == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNotFound", StatusConstants.Lifecycles.SalesOrder));

            var status = lifecycle.Statuses!.FirstOrDefault(s => s.Name == statusName);
            if (status == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", statusName));

            return new GenericResponse(true, status.Id);
        }

        private async Task<GenericResponse> ValidateCreateInvoiceRequest(CreateHeaderRequest createInvoiceRequest)
        {
            if (createInvoiceRequest.Date == DateTime.MinValue)
                return new GenericResponse(false, localizationService.GetLocalizedString("Validation.Required", "Data"));

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

            InvoiceEntities invoiceEntities;
            invoiceEntities.Exercise = exercise;
            invoiceEntities.Customer = customer;
            invoiceEntities.Site = site;
            return new GenericResponse(true, invoiceEntities);
        }

        #region DeliveryNote

        public async Task<GenericResponse> Deliver(Guid deliveryNoteId)
        {
            return await ChangeDeliveryStatus(deliveryNoteId, true);
        }

        public async Task<GenericResponse> UnDeliver(Guid deliveryNoteId)
        {
            return await ChangeDeliveryStatus(deliveryNoteId, false);
        }

        private async Task<GenericResponse> ChangeDeliveryStatus(Guid deliveryNoteId, bool isDelivered)
        {
            var orders = GetByDeliveryNoteId(deliveryNoteId);
            if (orders == null)
                return new GenericResponse(true, localizationService.GetLocalizedString("StatusTransitionNotFound"));

            var statusResponse = await GetStatusId(isDelivered ? StatusConstants.Statuses.ComandaServida : StatusConstants.Statuses.Comanda);
            if (!statusResponse.Result) return statusResponse;

            foreach (var order in orders.ToList())
            {
                // Actualitzar flag de 'servida' en els detalls
                foreach (var detail in order.SalesOrderDetails)
                {
                    detail.IsDelivered = isDelivered;
                    await UpdateDetail(detail);
                }

                // Canviar estat de la comanda
                order.StatusId = (Guid)statusResponse.Content!;
                // Asociar albarà
                if (order.DeliveryNoteId == null && isDelivered) order.DeliveryNoteId = deliveryNoteId;
                await Update(order);
            }

            return new GenericResponse(true);
        }

        #endregion

        private async Task AddExternalServicesFromWorkmaster(Domain.Entities.Production.WorkMaster workmaster, SalesOrderDetail detail)
        {
            var masterForWeight = unitOfWork.WorkMasters.Find(w => w.ReferenceId == detail.ReferenceId).FirstOrDefault();
            var lineWeight = (masterForWeight?.TotalWeight ?? 0) * detail.Quantity;
            var lineVolume = workmaster.Volume * detail.Quantity;

            foreach (var phase in workmaster.Phases)
            {
                if (phase.IsExternalWork && phase.ServiceReferenceId != null)
                {
                    var serviceRefId = phase.ServiceReferenceId.Value;
                    var existing = unitOfWork.SalesOrderHeaders.ExternalServices
                        .Find(es => es.SalesOrderHeaderId == detail.SalesOrderHeaderId && es.ReferenceId == serviceRefId)
                        .FirstOrDefault();

                    Guid externalServiceId;
                    if (existing != null)
                    {
                        existing.Quantity += detail.Quantity;
                        existing.Volume += lineVolume;
                        existing.Weight += lineWeight;
                        await unitOfWork.SalesOrderHeaders.ExternalServices.Update(existing);
                        externalServiceId = existing.Id;
                    }
                    else
                    {
                        var salesOrderExternalService = new SalesOrderExternalServices
                        {
                            SalesOrderHeaderId = detail.SalesOrderHeaderId,
                            ReferenceId = serviceRefId,
                            Description = phase.Description,
                            Weight = lineWeight,
                            Volume = lineVolume,
                            Quantity = detail.Quantity,
                        };
                        await unitOfWork.SalesOrderHeaders.ExternalServices.Add(salesOrderExternalService);
                        externalServiceId = salesOrderExternalService.Id;
                    }

                    var existingDetail = unitOfWork.SalesOrderHeaders.ExternalServiceDetails
                        .Find(d => d.SalesOrderExternalServiceId == externalServiceId && d.SalesOrderDetailId == detail.Id)
                        .FirstOrDefault();

                    if (existingDetail != null)
                    {
                        existingDetail.Quantity = detail.Quantity;
                        existingDetail.Weight = lineWeight;
                        existingDetail.Volume = lineVolume;

                        existingDetail.SalesOrderExternalService = null;
                        existingDetail.SalesOrderDetail = null;

                        await unitOfWork.SalesOrderHeaders.ExternalServiceDetails.Update(existingDetail);
                    }
                    else
                    {
                        var serviceDetail = new SalesOrderExternalServiceDetail
                        {
                            SalesOrderExternalServiceId = externalServiceId,
                            SalesOrderDetailId = detail.Id,
                            Quantity = detail.Quantity,
                            Weight = lineWeight,
                            Volume = lineVolume,
                        };
                        await unitOfWork.SalesOrderHeaders.ExternalServiceDetails.Add(serviceDetail);
                    }
                }
            }
        }

        private async Task RemoveExternalServicesFromWorkmaster(Domain.Entities.Production.WorkMaster workmaster, SalesOrderDetail detail)
        {
            foreach (var phase in workmaster.Phases)
            {
                if (phase.IsExternalWork && phase.ServiceReferenceId != null)
                {
                    var serviceRefId = phase.ServiceReferenceId.Value;
                    var existing = unitOfWork.SalesOrderHeaders.ExternalServices
                        .Find(es => es.SalesOrderHeaderId == detail.SalesOrderHeaderId && es.ReferenceId == serviceRefId)
                        .FirstOrDefault();

                    if (existing != null)
                    {
                        var serviceDetail = unitOfWork.SalesOrderHeaders.ExternalServiceDetails
                            .Find(d => d.SalesOrderExternalServiceId == existing.Id && d.SalesOrderDetailId == detail.Id)
                            .FirstOrDefault();
                        if (serviceDetail != null)
                        {
                            await unitOfWork.SalesOrderHeaders.ExternalServiceDetails.Remove(serviceDetail);
                        }

                        var masterForWeight = unitOfWork.WorkMasters.Find(w => w.ReferenceId == detail.ReferenceId).FirstOrDefault();
                        var lineWeight = (masterForWeight?.TotalWeight ?? 0) * detail.Quantity;

                        existing.Quantity -= detail.Quantity;
                        existing.Volume -= workmaster.Volume * detail.Quantity;
                        existing.Weight -= lineWeight;

                        if (existing.Quantity <= 0)
                        {
                            await unitOfWork.SalesOrderHeaders.ExternalServices.Remove(existing);
                        }
                        else
                        {
                            await unitOfWork.SalesOrderHeaders.ExternalServices.Update(existing);
                        }
                    }
                }
            }
        }
    }
}







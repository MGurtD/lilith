using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.Extensions.Logging;


namespace Application.Services.Sales
{
    public class BudgetService(
        IUnitOfWork unitOfWork,
        IExerciseService exerciseService,
        IMetricsService metricsService,
        ILocalizationService localizationService,
        ILogger<BudgetService> logger) : IBudgetService
    {
        public async Task<Budget?> GetById(Guid id)
        {
            var budget = await unitOfWork.Budgets.Get(id);
            return budget;
        }

        public async Task<GenericResponse> Accept(Guid id)
        {
            var budget = await unitOfWork.Budgets.Get(id);
            if (budget == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound", id));
            }

            var acceptedStatus = await unitOfWork.Lifecycles.GetStatusByName(
                StatusConstants.Lifecycles.Budget,
                StatusConstants.Statuses.Acceptat);

            if (acceptedStatus == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.Acceptat));
            }

            budget.StatusId = acceptedStatus.Id;
            budget.AcceptanceDate = DateTime.Now;

            await unitOfWork.Budgets.Update(budget);
            return new GenericResponse(true, budget);
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
            // Recuperar el workmaster
            if (detail.WorkMasterId != null)
            {
                var workmaster = await unitOfWork.WorkMasters.Get(detail.WorkMasterId.Value);
                // Recollir mètriques
                /*var metrics = await metricsService.GetWorkmasterMetrics(workmaster, detail.Quantity);
                
                // Afegir pes a la línia
                if (metrics.Result && metrics.Content is Domain.Entities.Production.ProductionMetrics productionMetrics)
                {
                    detail.DetailWeight = productionMetrics.TotalWeight;
                }*/

                

                var referenceTypeId = workmaster.Reference.ReferenceTypeId;
                var netWeight = decimal.Zero;
                if(referenceTypeId != null)
                {
                    var referenceType = await unitOfWork.ReferenceTypes.Get(referenceTypeId.Value);
                    if (referenceType != null)
                    {
                        netWeight = referenceType.Density * (workmaster.volume/1000);
                    }
                }
                detail.DetailWeight = netWeight;
                // Afegir pes al total del pressupost
                var budget = await unitOfWork.Budgets.Get(detail.BudgetId);
                if (budget != null)
                {
                    budget.TotalWeight += netWeight;
                    await unitOfWork.Budgets.Update(budget);
                }

                // El detall s'ha de desar ABANS d'inserir BudgetExternalServiceDetail
                // per satisfer la FK constraint BudgetDetailId
                await unitOfWork.Budgets.Details.Add(detail);
                await AddExternalServicesFromWorkmaster(workmaster, detail);
                return new GenericResponse(true, detail);
            }
            await unitOfWork.Budgets.Details.Add(detail);
            return new GenericResponse(true, detail);
        }
        public async Task<GenericResponse> UpdateDetail(BudgetDetail detail)
        {
            // Recuperar el detall antic per obtenir la quantitat anterior
            var oldDetail = unitOfWork.Budgets.Details.Find(d => d.Id == detail.Id).FirstOrDefault();
            var oldQuantity = oldDetail?.Quantity ?? 0;
            var oldWeight = detail.DetailWeight;

            if (detail.WorkMasterId != null)
            {
                var workmaster = await unitOfWork.WorkMasters.Get(detail.WorkMasterId.Value);
                var referenceTypeId = workmaster.Reference.ReferenceTypeId;
                var netWeight = decimal.Zero;
                if(referenceTypeId != null)
                {
                    var referenceType = await unitOfWork.ReferenceTypes.Get(referenceTypeId.Value);
                    if (referenceType != null)
                    {
                        netWeight = referenceType.Density * (workmaster.volume/1000);
                    }
                }
                detail.DetailWeight = netWeight;

                // Afegir pes al total del pressupost
                var budget = await unitOfWork.Budgets.Get(detail.BudgetId);
                if (budget != null)
                {
                    budget.TotalWeight += detail.DetailWeight - oldWeight;
                    await unitOfWork.Budgets.Update(budget);
                }

                // Actualitzar serveis externs: restar contribució antiga i sumar la nova
                var quantityDiff = detail.Quantity - oldQuantity;
                var volumeDiff = workmaster.volume * quantityDiff;
                var weightDiff = detail.DetailWeight * quantityDiff;
                var newLineWeight = detail.DetailWeight * detail.Quantity;
                var newLineVolume = workmaster.volume * detail.Quantity;

                foreach (var phase in workmaster.Phases)
                {
                    if (phase.IsExternalWork && phase.ServiceReferenceId != null)
                    {
                        var serviceRefId = phase.ServiceReferenceId.Value;
                        var existing = unitOfWork.Budgets.ExternalServices
                            .Find(es => es.BudgetId == detail.BudgetId && es.ReferenceId == serviceRefId)
                            .FirstOrDefault();

                        if (existing != null)
                        {
                            existing.Quantity += quantityDiff;
                            existing.Volume += volumeDiff;
                            existing.Weight += weightDiff;
                            await unitOfWork.Budgets.ExternalServices.Update(existing);

                            // Actualitzar detall de la relació N:M
                            var existingServiceDetail = unitOfWork.Budgets.ExternalServiceDetails
                                .Find(d => d.BudgetExternalServiceId == existing.Id && d.BudgetDetailId == detail.Id)
                                .FirstOrDefault();

                            if (existingServiceDetail != null)
                            {
                                existingServiceDetail.Quantity = detail.Quantity;
                                existingServiceDetail.Weight = newLineWeight;
                                existingServiceDetail.Volume = newLineVolume;
                                await unitOfWork.Budgets.ExternalServiceDetails.Update(existingServiceDetail);
                            }
                        }
                    }
                }
            }
            await unitOfWork.Budgets.Details.Update(detail);
            return new GenericResponse(true, detail);
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = unitOfWork.Budgets.Details.Find(d => d.Id == id).FirstOrDefault();
            if (detail == null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetDetailNotFound", id));

            if (detail.WorkMasterId != null)
            {
                var workmaster = await unitOfWork.WorkMasters.Get(detail.WorkMasterId.Value);
                var referenceTypeId = workmaster.Reference.ReferenceTypeId;
                var netWeight = decimal.Zero;
                if(referenceTypeId != null)
                {
                    var referenceType = await unitOfWork.ReferenceTypes.Get(referenceTypeId.Value);
                    if (referenceType != null)
                    {
                        netWeight = referenceType.Density * (workmaster.volume/1000);
                    }
                }
                detail.DetailWeight = netWeight;

                // Restar pes del total del pressupost
                var budget = await unitOfWork.Budgets.Get(detail.BudgetId);
                if (budget != null)
                {
                    budget.TotalWeight -= detail.DetailWeight;
                    await unitOfWork.Budgets.Update(budget);
                }

                // Restar contribució dels serveis externs o eliminar-los
                await RemoveExternalServicesFromWorkmaster(workmaster, detail);
            }

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

        public async Task<GenericResponse> AddTransport(BudgetTransport transport)
        {
            var budget = await unitOfWork.Budgets.Get(transport.BudgetId);
            if (budget == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound"));
            budget.TransportCost += transport.Price;
            await unitOfWork.Budgets.Transports.Add(transport);
            await unitOfWork.Budgets.Update(budget);
            return new GenericResponse(true, transport);
        }
        public async Task<GenericResponse> UpdateTransport(BudgetTransport transport)
        {
            var budget = await unitOfWork.Budgets.Get(transport.BudgetId);
            if (budget == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound"));
            await unitOfWork.Budgets.Transports.Update(transport);
            return new GenericResponse(true, transport);
        }
        public async Task<GenericResponse> RemoveTransport(Guid id)
        {
            var transport = unitOfWork.Budgets.Transports.Find(t => t.Id == id).FirstOrDefault();
            if (transport == null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetTransportNotFound", id));
            var budget = await unitOfWork.Budgets.Get(transport.BudgetId);
            if (budget == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound"));
            budget.TransportCost -= transport.Price;
            await unitOfWork.Budgets.Transports.Remove(transport);
            await unitOfWork.Budgets.Update(budget);
            return new GenericResponse(true, transport);
        }
 
        public async Task<GenericResponse> DistributeTransportCosts(Guid budgetId)
        {
            logger.LogInformation("Iniciant DistributeTransportCosts pel pressupost: {BudgetId}", budgetId);

            var budget = await unitOfWork.Budgets.Get(budgetId);
            if (budget == null)
            {
                logger.LogWarning("Pressupost no trobat: {BudgetId}", budgetId);
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound", budgetId));
            }
                        
            var totalWeight = budget.TotalWeight;
            logger.LogInformation("Pressupost {BudgetId} carregat. Pes total: {TotalWeight}, Cost transport: {TransportCost}, Línies de detall: {DetailCount}", 
                budgetId, totalWeight, budget.TransportCost, budget.Details?.Count ?? 0);

            if (budget.Details == null || !budget.Details.Any())
            {
                logger.LogWarning("El pressupost {BudgetId} no té línies de detall associades.", budgetId);
                return new GenericResponse(false, "El pressupost no té línies de detall");
            }

            if (totalWeight <= 0)
            {
                logger.LogWarning("El pressupost {BudgetId} té un pes total de {TotalWeight}. No es pot ponderar dividint per zero.", budgetId, totalWeight);
                return new GenericResponse(false, "S'ha intentat ponderar sobre un pes total de 0 o negatiu.");
            }

            try 
            {
                foreach (var detail in budget.Details)
                {                
                    detail.TransportCost = (detail.DetailWeight / totalWeight) * budget.TransportCost;
                    detail.Amount = detail.Amount + detail.TransportCost;
                    
                    // Explicitament modifiquem el detall per a que quedi registrat el Tracking i guardi a la BBDD
                    unitOfWork.Budgets.Details.UpdateWithoutSave(detail);

                    logger.LogInformation("Línia detall {DetailId} ponderada: Cost Transport = {TransportCost}, Nou Import = {Amount}", 
                        detail.Id, detail.TransportCost, detail.Amount);
                }
                
                await unitOfWork.CompleteAsync();
                logger.LogInformation("Cost de transport ponderat i desat correctament al pressupost {BudgetId}", budgetId);
                
                return new GenericResponse(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "S'ha produït un error en intentar ponderar el cost de transport pel pressupost {BudgetId}", budgetId);
                return new GenericResponse(false, $"Error intern: {ex.Message}");
            }
        }

        public async Task<GenericResponse> DistributeAllCosts(Guid budgetId)
        {
            logger.LogInformation("Iniciant DistributeAllCosts pel pressupost: {BudgetId}", budgetId);

            var budget = await unitOfWork.Budgets.Get(budgetId);
            if (budget == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound", budgetId));
            }

            if (budget.Details == null || !budget.Details.Any())
            {
                return new GenericResponse(false, "El pressupost no té línies de detall");
            }

            var totalWeight = budget.TotalWeight;
            var budgetDate = DateOnly.FromDateTime(budget.Date);

            try 
            {
                // 1. Ponderar Transport (pes)
                foreach (var detail in budget.Details)
                {
                    decimal totalTransportShare = totalWeight > 0 ? (detail.DetailWeight / totalWeight) * budget.TransportCost : 0;
                    detail.TransportCost = detail.Quantity > 0 ? totalTransportShare / detail.Quantity : 0;
                    detail.ServiceCost = 0; // Reset
                }

                // 2. Ponderar Serveis Externs
                if (budget.ExternalServices != null)
                {
                    foreach (var extService in budget.ExternalServices)
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

                        // 1 = Pes, 2 = Unitats, 3 = Volum
                        decimal totalMagnitude = calculationType switch
                        {
                            1 => extService.Weight,
                            3 => extService.Volume,
                            _ => extService.Quantity
                        };

                        if (totalMagnitude <= 0)
                        {
                            if (calculationType == 1 || calculationType == 3)
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
                                    1 => esd.Weight,
                                    3 => esd.Volume,
                                    _ => esd.Quantity
                                };

                                decimal proportion = detailMagnitude / totalMagnitude;
                                decimal totalCostShare = totalServiceCost * proportion;

                                var bDetail = budget.Details.FirstOrDefault(d => d.Id == esd.BudgetDetailId);
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
                foreach (var detail in budget.Details)
                {
                    // Costos unitaris
                    detail.TotalCost = detail.UnitCost + detail.TransportCost + detail.ServiceCost;
                    
                    // Preu unitari amb benefici i descompte (en %)
                    decimal priceWithProfit = detail.TotalCost + (detail.TotalCost * (detail.Profit / 100m));
                    detail.UnitPrice = priceWithProfit - (priceWithProfit * (detail.Discount / 100m));
                    
                    // Import total de la línia
                    detail.Amount = detail.UnitPrice * detail.Quantity;

                    unitOfWork.Budgets.Details.UpdateWithoutSave(detail);
                }
                
                await unitOfWork.CompleteAsync();
                return new GenericResponse(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error a DistributeAllCosts pel pressupost {BudgetId}", budgetId);
                return new GenericResponse(false, $"Error intern: {ex.Message}");
            }
        }

        public async Task<GenericResponse> UpdateExternalService(BudgetExternalServices externalService)
        {
            var exists = await unitOfWork.Budgets.ExternalServices.Get(externalService.Id);
            if (exists == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound"));
            }

            // Only update the manually modifiable fields from the frontend
            exists.SupplierId = externalService.SupplierId;
            exists.UnitPrice = externalService.UnitPrice;
            exists.TotalPrice = externalService.TotalPrice;

            await unitOfWork.Budgets.ExternalServices.Update(exists);
            return new GenericResponse(true, exists);
        }

        /// <summary>
        /// Per cada fase ExternalWork del workmaster, comprova si ja existeix un BudgetExternalServices
        /// amb el mateix ServiceReferenceId. Si existeix, suma quantitat, volum i pes. Si no, l'insereix.
        /// </summary>
        private async Task AddExternalServicesFromWorkmaster(Domain.Entities.Production.WorkMaster workmaster, BudgetDetail detail)
        {
            var lineWeight = detail.DetailWeight * detail.Quantity;
            var lineVolume = workmaster.volume * detail.Quantity;

            foreach (var phase in workmaster.Phases)
            {
                if (phase.IsExternalWork && phase.ServiceReferenceId != null)
                {
                    var serviceRefId = phase.ServiceReferenceId.Value;
                    var existing = unitOfWork.Budgets.ExternalServices
                        .Find(es => es.BudgetId == detail.BudgetId && es.ReferenceId == serviceRefId)
                        .FirstOrDefault();

                    Guid externalServiceId;
                    if (existing != null)
                    {
                        existing.Quantity += detail.Quantity;
                        existing.Volume += lineVolume;
                        existing.Weight += lineWeight;
                        await unitOfWork.Budgets.ExternalServices.Update(existing);
                        externalServiceId = existing.Id;
                    }
                    else
                    {
                        var budgetExternalService = new BudgetExternalServices
                        {
                            BudgetId = detail.BudgetId,
                            ReferenceId = serviceRefId,
                            Description = phase.Description,
                            Weight = lineWeight,
                            Volume = lineVolume,
                            Quantity = detail.Quantity,
                        };
                        await unitOfWork.Budgets.ExternalServices.Add(budgetExternalService);
                        externalServiceId = budgetExternalService.Id;
                    }

                    // Upsert del detall de la relació N:M
                    var existingDetail = unitOfWork.Budgets.ExternalServiceDetails
                        .Find(d => d.BudgetExternalServiceId == externalServiceId && d.BudgetDetailId == detail.Id)
                        .FirstOrDefault();

                    if (existingDetail != null)
                    {
                        existingDetail.Quantity = detail.Quantity;
                        existingDetail.Weight = lineWeight;
                        existingDetail.Volume = lineVolume;
                        await unitOfWork.Budgets.ExternalServiceDetails.Update(existingDetail);
                    }
                    else
                    {
                        var serviceDetail = new BudgetExternalServiceDetail
                        {
                            BudgetExternalServiceId = externalServiceId,
                            BudgetDetailId = detail.Id,
                            Quantity = detail.Quantity,
                            Weight = lineWeight,
                            Volume = lineVolume,
                        };
                        await unitOfWork.Budgets.ExternalServiceDetails.Add(serviceDetail);
                    }
                }
            }
        }

        /// <summary>
        /// Per cada fase ExternalWork del workmaster, resta la contribució de la línia eliminada.
        /// Si la quantitat resultant és <= 0, elimina el registre.
        /// </summary>
        private async Task RemoveExternalServicesFromWorkmaster(Domain.Entities.Production.WorkMaster workmaster, BudgetDetail detail)
        {
            foreach (var phase in workmaster.Phases)
            {
                if (phase.IsExternalWork && phase.ServiceReferenceId != null)
                {
                    var serviceRefId = phase.ServiceReferenceId.Value;
                    var existing = unitOfWork.Budgets.ExternalServices
                        .Find(es => es.BudgetId == detail.BudgetId && es.ReferenceId == serviceRefId)
                        .FirstOrDefault();

                    if (existing != null)
                    {
                        // Eliminar el detall de la relació N:M
                        var serviceDetail = unitOfWork.Budgets.ExternalServiceDetails
                            .Find(d => d.BudgetExternalServiceId == existing.Id && d.BudgetDetailId == detail.Id)
                            .FirstOrDefault();
                        if (serviceDetail != null)
                        {
                            await unitOfWork.Budgets.ExternalServiceDetails.Remove(serviceDetail);
                        }

                        existing.Quantity -= detail.Quantity;
                        existing.Volume -= workmaster.volume * detail.Quantity;
                        existing.Weight -= detail.DetailWeight * detail.Quantity;

                        if (existing.Quantity <= 0)
                        {
                            await unitOfWork.Budgets.ExternalServices.Remove(existing);
                        }
                        else
                        {
                            await unitOfWork.Budgets.ExternalServices.Update(existing);
                        }
                    }
                }
            }
        }
    }
}







using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.Extensions.Logging;
// ReSharper disable All


namespace Application.Services.Sales
{
    public class BudgetService(
        IUnitOfWork unitOfWork,
        IExerciseService exerciseService,
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
            var phaseProfits = detail.PhaseProfits?.ToList() ?? new List<BudgetDetailPhaseProfit>();
            detail.PhaseProfits = new List<BudgetDetailPhaseProfit>();

            // Recuperar el workmaster
            if (detail.WorkMasterId != null)
            {
                var workmaster = await unitOfWork.WorkMasters.Get(detail.WorkMasterId.Value);
                if (workmaster?.Reference is null)
                {
                    return new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterReferenceNotFound", detail.WorkMasterId));
                }
                
                // Recollir mètriques
                /*var metrics = await metricsService.GetWorkmasterMetrics(workmaster, detail.Quantity);
                
                // Afegir pes a la línia
                if (metrics.Result && metrics.Content is Domain.Entities.Production.ProductionMetrics productionMetrics)
                {
                    detail.DetailWeight = productionMetrics.TotalWeight;
                }*/
                
                var referenceTypeId = workmaster.Reference.ReferenceTypeId;
                var netWeight = decimal.Zero;
                if (referenceTypeId != null)
                {
                    var referenceType = await unitOfWork.ReferenceTypes.Get(referenceTypeId.Value);
                    if (referenceType != null)
                    {
                        netWeight = referenceType.Density * (workmaster.Volume / 1000);
                    }
                }
                detail.DetailWeight = netWeight * detail.Quantity;
                // Afegir pes al total del pressupost
                var budget = await unitOfWork.Budgets.Get(detail.BudgetId);
                if (budget != null)
                {
                    budget.TotalWeight += detail.DetailWeight;
                    await unitOfWork.Budgets.Update(budget);
                }

                // El detall s'ha de desar ABANS d'inserir BudgetExternalServiceDetail
                // per satisfer la FK constraint BudgetDetailId
                await unitOfWork.Budgets.Details.Add(detail);
                await AddExternalServicesFromWorkmaster(workmaster, detail);
                await SavePhaseProfits(detail.Id, phaseProfits, replaceExisting: false);
                return new GenericResponse(true, detail);
            }
            await unitOfWork.Budgets.Details.Add(detail);
            await SavePhaseProfits(detail.Id, phaseProfits, replaceExisting: false);
            return new GenericResponse(true, detail);
        }
        
        public async Task<GenericResponse> UpdateDetail(BudgetDetail detail)
        {
            var phaseProfits = detail.PhaseProfits?.ToList() ?? new List<BudgetDetailPhaseProfit>();
            detail.PhaseProfits = new List<BudgetDetailPhaseProfit>();

            // Recuperar el detall antic per obtenir la quantitat anterior
            var oldDetail = unitOfWork.Budgets.Details.Find(d => d.Id == detail.Id).FirstOrDefault();
            var oldQuantity = oldDetail?.Quantity ?? 0;
            var oldWeight = detail.DetailWeight;

            if (detail.WorkMasterId != null)
            {
                var workmaster = await unitOfWork.WorkMasters.Get(detail.WorkMasterId.Value);
                if (workmaster?.Reference is null)
                {
                    return new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterReferenceNotFound", detail.WorkMasterId));
                }
                var reference = workmaster.Reference;
                var referenceTypeId = reference.ReferenceTypeId;
                var netWeight = decimal.Zero;
                if (referenceTypeId != null)
                {
                    var referenceType = await unitOfWork.ReferenceTypes.Get(referenceTypeId.Value);
                    if (referenceType != null)
                    {
                        netWeight = referenceType.Density * (workmaster.Volume / 1000);
                    }
                }
                detail.DetailWeight = netWeight * detail.Quantity;

                // Afegir pes al total del pressupost
                var budget = await unitOfWork.Budgets.Get(detail.BudgetId);
                if (budget != null)
                {
                    budget.TotalWeight += detail.DetailWeight - oldWeight;
                    await unitOfWork.Budgets.Update(budget);
                }

                // Actualitzar serveis externs: restar contribució antiga i sumar la nova
                var quantityDiff = detail.Quantity - oldQuantity;
                var volumeDiff = workmaster.Volume * quantityDiff;
                var weightDiff = detail.DetailWeight - oldWeight;
                var newLineWeight = detail.DetailWeight;
                var newLineVolume = workmaster.Volume * detail.Quantity;

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
            // Nullify navigation properties to avoid EF tracking issues
            detail.Reference = null;
            detail.Budget = null;
            detail.WorkMaster = null;

            await unitOfWork.Budgets.Details.Update(detail);
            await SavePhaseProfits(detail.Id, phaseProfits, replaceExisting: true);
            return new GenericResponse(true, detail);
        }

        private async Task SavePhaseProfits(Guid budgetDetailId, IEnumerable<BudgetDetailPhaseProfit> phaseProfits, bool replaceExisting)
        {
            if (replaceExisting)
            {
                var existing = unitOfWork.Budgets.DetailPhaseProfits
                    .Find(p => p.BudgetDetailId == budgetDetailId)
                    .ToList();
                if (existing.Count > 0)
                {
                    await unitOfWork.Budgets.DetailPhaseProfits.RemoveRange(existing);
                }
            }

            var rows = phaseProfits
                .Where(p => p != null)
                .Select(p => new BudgetDetailPhaseProfit
                {
                    Id = p.Id == Guid.Empty ? Guid.NewGuid() : p.Id,
                    BudgetDetailId = budgetDetailId,
                    WorkMasterPhaseDetailId = p.WorkMasterPhaseDetailId,
                    ProfitPercentage = p.ProfitPercentage,
                })
                .ToList();

            if (rows.Count > 0)
            {
                await unitOfWork.Budgets.DetailPhaseProfits.AddRange(rows);
            }
        }

        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = unitOfWork.Budgets.Details.Find(d => d.Id == id).FirstOrDefault();
            if (detail == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetDetailNotFound", id));

            if (detail.WorkMasterId != null)
            {
                var workmaster = await unitOfWork.WorkMasters.Get(detail.WorkMasterId.Value);
                if (workmaster == null)
                {
                    return new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterNotFound", detail.WorkMasterId.Value));
                }

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

            var budgets = await unitOfWork.Budgets.FindAsync(b => b.StatusId == status.Id && b.Date.AddDays(30) <= DateTime.UtcNow);
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

                    // Nullify navigation properties to avoid EF tracking issues
                    detail.Reference = null;
                    detail.Budget = null;
                    detail.WorkMaster = null;

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
                    logger.LogInformation("Processant {Count} serveis externs pel pressupost {BudgetId}", budget.ExternalServices.Count(), budgetId);
                    foreach (var extService in budget.ExternalServices)
                    {
                        logger.LogInformation("Processant servei extern: {ServiceId} (Referència: {ReferenceId})", extService.Id, extService.ReferenceId);

                        if (extService.SupplierId == Guid.Empty || extService.UnitPrice <= 0)
                        {
                            logger.LogWarning("Servei extern {ServiceId} omet: sense proveïdor (SupplierId: {SupplierId}) o preu unitari zero/negatiu (UnitPrice: {UnitPrice})",
                                extService.Id, extService.SupplierId, extService.UnitPrice);
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
                            logger.LogWarning("No hi ha tarifa de compra activa pel proveïdor {SupplierId} a la data {Date}. S'ometrà el servei {ServiceId}.", extService.SupplierId, budgetDate, extService.Id);
                            continue;
                        }

                        // Obtenir els detalls de la tarifa
                        var rateDetails = await unitOfWork.PurchaseRateDetails.FindAsync(d => d.PurchaseRateId == activeRate.Id && d.ReferenceId == extService.ReferenceId);
                        var rateDetail = rateDetails.FirstOrDefault();

                        int calculationType = rateDetail?.CalculationType ?? 2; // Default Unitats
                        string calculationTypeName = calculationType switch
                        {
                            0 => "Volum",
                            1 => "Pes",
                            _ => "Unitats"
                        };

                        logger.LogInformation("Tarifa trobada: {RateId}. Tipus de càlcul: {Type} ({TypeName})", activeRate.Id, calculationType, calculationTypeName);
                        logger.LogInformation("Detall Tarifa trobada: {RateId}. Tipus de càlcul: {Type} ({TypeName})", rateDetail?.Id ?? Guid.Empty, calculationType, calculationTypeName);

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
                                logger.LogWarning("La magnitud ({TypeName}) del servei extern {ServiceId} és {Magnitude}. Reintentant per Unitats.", calculationTypeName, extService.Id, totalMagnitude);
                                calculationType = 2; // Fallback to Units
                                totalMagnitude = extService.Quantity;
                            }

                            if (totalMagnitude <= 0)
                            {
                                logger.LogWarning("La magnitud final del servei extern {ServiceId} segueix sent 0. S'omet.", extService.Id);
                                continue;
                            }
                        }

                        decimal totalServiceCost = extService.UnitPrice * totalMagnitude;
                        logger.LogInformation("Cost total del servei {ServiceId}: {TotalCost} (Preu: {UnitPrice} * Magnitud: {Magnitude})",
                            extService.Id, totalServiceCost, extService.UnitPrice, totalMagnitude);

                        if (extService.Details != null)
                        {
                            logger.LogInformation("Distribuint cost del servei {ServiceId} entre {DetailCount} línies de detall", extService.Id, extService.Details.Count());
                            foreach (var esd in extService.Details)
                            {
                                var detailMagnitude = calculationType switch
                                {
                                    0 => esd.Volume,
                                    1 => esd.Weight,
                                    _ => esd.Quantity
                                };

                                decimal proportion = totalMagnitude > 0 ? detailMagnitude / totalMagnitude : 0;
                                decimal totalCostShare = totalServiceCost * proportion;

                                var bDetail = budget.Details.FirstOrDefault(d => d.Id == esd.BudgetDetailId);
                                if (bDetail != null)
                                {
                                    decimal unitCostShare = bDetail.Quantity > 0 ? totalCostShare / bDetail.Quantity : 0;
                                    bDetail.ServiceCost += unitCostShare;
                                    logger.LogInformation("  -> Línia detall {BudgetDetailId}: Proporció {Proportion:P2}, Cost total compartit {TotalShare}, Cost unitari afegit {UnitShare}",
                                        bDetail.Id, proportion, totalCostShare, unitCostShare);
                                }
                                else
                                {
                                    logger.LogWarning("  -> No s'ha trobat la línia de detall {BudgetDetailId} associada al servei extern {ServiceId}", esd.BudgetDetailId, extService.Id);
                                }
                            }
                        }
                        else
                        {
                            logger.LogWarning("El servei extern {ServiceId} no té detalls (ExternalServiceDetails) per distribuir el cost.", extService.Id);
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

                    // Nullify navigation properties to avoid EF tracking issues
                    detail.Reference = null;
                    detail.Budget = null;
                    detail.WorkMaster = null;

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

        public async Task<GenericResponse> Clone(Guid id, Guid newId)
        {
            var source = await unitOfWork.Budgets.Get(id);
            if (source == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("BudgetNotFound", id));

            var counterObj = await exerciseService.GetNextCounter(source.ExerciseId, "budget");
            if (!counterObj.Result || counterObj.Content == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseCounterError"));

            var lifecycle = unitOfWork.Lifecycles.Find(l => l.Name == StatusConstants.Lifecycles.Budget).FirstOrDefault();
            if (lifecycle == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNotFound", StatusConstants.Lifecycles.Budget));
            if (!lifecycle.InitialStatusId.HasValue)
                return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNoInitialStatus", StatusConstants.Lifecycles.Budget));

            var newBudget = new Budget
            {
                Id = newId,
                Number = counterObj.Content.ToString()!,
                Date = DateTime.Now,
                ExerciseId = source.ExerciseId,
                CustomerId = source.CustomerId,
                DeliveryDays = source.DeliveryDays,
                UserNotes = source.UserNotes,
                TotalWeight = source.TotalWeight,
                TransportCost = source.TransportCost,
                StatusId = lifecycle.InitialStatusId,
            };

            await unitOfWork.Budgets.AddWithoutSave(newBudget);

            // Mapa oldDetailId -> newDetailId per mantenir les FKs dels ExternalServiceDetails
            var detailIdMap = new Dictionary<Guid, Guid>();

            if (source.Details != null)
            {
                foreach (var detail in source.Details)
                {
                    var newDetailId = Guid.NewGuid();
                    detailIdMap[detail.Id] = newDetailId;

                    var newDetail = new BudgetDetail
                    {
                        Id = newDetailId,
                        BudgetId = newId,
                        ReferenceId = detail.ReferenceId,
                        WorkMasterId = detail.WorkMasterId,
                        Description = detail.Description,
                        Quantity = detail.Quantity,
                        Profit = detail.Profit,
                        ProductionProfit = detail.ProductionProfit,
                        MaterialProfit = detail.MaterialProfit,
                        ExternalProfit = detail.ExternalProfit,
                        Discount = detail.Discount,
                        UnitCost = detail.UnitCost,
                        ProductionCost = detail.ProductionCost,
                        MaterialCost = detail.MaterialCost,
                        TransportCost = detail.TransportCost,
                        ServiceCost = detail.ServiceCost,
                        TotalCost = detail.TotalCost,
                        UnitPrice = detail.UnitPrice,
                        Amount = detail.Amount,
                        DetailWeight = detail.DetailWeight,
                        UserNotes = detail.UserNotes,
                    };
                    await unitOfWork.Budgets.Details.AddWithoutSave(newDetail);
                }
            }

            if (source.Transports != null)
            {
                foreach (var transport in source.Transports)
                {
                    var newTransport = new BudgetTransport
                    {
                        Id = Guid.NewGuid(),
                        BudgetId = newId,
                        TransportRateDetailId = transport.TransportRateDetailId,
                        LogisticSupplierId = transport.LogisticSupplierId,
                        DestinationSupplierId = transport.DestinationSupplierId,
                        Weight = transport.Weight,
                        Volume = transport.Volume,
                        Distance = transport.Distance,
                        Price = transport.Price,
                        Description = transport.Description,
                        Destination = transport.Destination,
                    };
                    await unitOfWork.Budgets.Transports.AddWithoutSave(newTransport);
                }
            }

            if (source.ExternalServices != null)
            {
                foreach (var extService in source.ExternalServices)
                {
                    var newExtServiceId = Guid.NewGuid();
                    var newExtService = new BudgetExternalServices
                    {
                        Id = newExtServiceId,
                        BudgetId = newId,
                        ReferenceId = extService.ReferenceId,
                        Description = extService.Description,
                        Weight = extService.Weight,
                        Volume = extService.Volume,
                        Quantity = extService.Quantity,
                        SupplierId = extService.SupplierId,
                        UnitPrice = extService.UnitPrice,
                        TotalPrice = extService.TotalPrice,
                    };
                    await unitOfWork.Budgets.ExternalServices.AddWithoutSave(newExtService);

                    if (extService.Details != null)
                    {
                        foreach (var esd in extService.Details)
                        {
                            if (!detailIdMap.TryGetValue(esd.BudgetDetailId, out var newBudgetDetailId))
                                continue;

                            var newEsd = new BudgetExternalServiceDetail
                            {
                                Id = Guid.NewGuid(),
                                BudgetExternalServiceId = newExtServiceId,
                                BudgetDetailId = newBudgetDetailId,
                                Quantity = esd.Quantity,
                                Weight = esd.Weight,
                                Volume = esd.Volume,
                            };
                            await unitOfWork.Budgets.ExternalServiceDetails.AddWithoutSave(newEsd);
                        }
                    }
                }
            }

            // Un sol SaveChanges transaccional per a tota l'operació
            await unitOfWork.CompleteAsync();

            return new GenericResponse(true, newBudget);
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
            var lineWeight = detail.DetailWeight;
            var lineVolume = workmaster.Volume * detail.Quantity;

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

                        existingDetail.BudgetExternalService = null;
                        existingDetail.BudgetDetail = null;

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
        /// Si la quantitat resultant és menor o igual a 0, elimina el registre.
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
                        existing.Volume -= workmaster.Volume * detail.Quantity;
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







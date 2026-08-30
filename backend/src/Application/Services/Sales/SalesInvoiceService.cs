
using Application.Contracts;
using Application.Utils;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Application.Services.Sales
{
    internal struct InvoiceEntities
    {
        internal Customer Customer;
        internal Exercise Exercise;
        internal Site Site;
    }

    public class SalesInvoiceService(
        IUnitOfWork unitOfWork,
        IEnterpriseService enterpriseService,
        IDueDateService dueDateService,
        IDeliveryNoteService deliveryNoteService,
        IExerciseService exerciseService,
        ILocalizationService localizationService,
        ILogger<SalesInvoiceService> logger) : ISalesInvoiceService
    {
        private readonly string LifecycleName = StatusConstants.Lifecycles.SalesInvoice;

        public async Task<SalesInvoice?> GetById(Guid id)
        {
            var invoices = await unitOfWork.SalesInvoices.Get(id);
            return invoices;
        }
        public async Task<SalesInvoice?> GetHeaderById(Guid id)
        {
            var invoices = await unitOfWork.SalesInvoices.GetHeader(id);
            return invoices;
        }

        public IEnumerable<SalesInvoice> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var invoice = unitOfWork.SalesInvoices.Find(p => p.InvoiceDate >= startDate && p.InvoiceDate <= endDate);
            return invoice;
        }
        public IEnumerable<SalesInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var invoices = unitOfWork.SalesInvoices.Find(p => p.InvoiceDate >= startDate && p.InvoiceDate <= endDate && p.StatusId == statusId);
            return invoices;
        }
        public IEnumerable<SalesInvoice> GetBetweenDatesAndExcludeStatus(DateTime startDate, DateTime endDate, Guid excludeStatusId)
        {
            var invoices = unitOfWork.SalesInvoices.Find(p => p.InvoiceDate >= startDate && p.InvoiceDate <= endDate && p.StatusId != excludeStatusId);
            return invoices;
        }
        public IEnumerable<SalesInvoice> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId)
        {
            var invoices = unitOfWork.SalesInvoices.Find(p => p.InvoiceDate >= startDate && p.InvoiceDate <= endDate && p.CustomerId == customerId);
            return invoices;
        }
        public IEnumerable<SalesInvoice> GetByCustomer(Guid customerId)
        {
            var invoices = unitOfWork.SalesInvoices.Find(p => p.CustomerId == customerId);
            return invoices;
        }
        public IEnumerable<SalesInvoice> GetByStatus(Guid statusId)
        {
            var invoices = unitOfWork.SalesInvoices.Find(p => p.StatusId == statusId);
            return invoices;
        }
        public IEnumerable<SalesInvoice> GetByExercise(Guid exerciseId)
        {
            var invoices = unitOfWork.SalesInvoices.Find(p => p.ExerciseId == exerciseId);
            return invoices;
        }

        public async Task<GenericResponse> Create(CreateHeaderRequest createInvoiceRequest)
        {
            var response = await ValidateCreateInvoiceRequest(createInvoiceRequest);
            if (!response.Result) return response;

            var invoiceEntities = (InvoiceEntities)response.Content!;
            var counterObj = await exerciseService.GetNextCounter(invoiceEntities.Exercise.Id, "salesinvoice");
            if (counterObj == null || counterObj.Content == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseCounterError"));

            var counter = counterObj.Content.ToString();
            var invoice = new SalesInvoice
            {
                Id = createInvoiceRequest.Id,
                InvoiceNumber = counter!,
                InvoiceDate = createInvoiceRequest.Date
            };

            var lifecycle = unitOfWork.Lifecycles.Find(l => l.Name == StatusConstants.Lifecycles.SalesInvoice).FirstOrDefault();
            if (lifecycle == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNotFound", StatusConstants.Lifecycles.SalesInvoice));
            if (!lifecycle.InitialStatusId.HasValue)
                return new GenericResponse(false, localizationService.GetLocalizedString("LifecycleNoInitialStatus", StatusConstants.Lifecycles.SalesInvoice));

            var verifactuInitialStatusId = await unitOfWork.Lifecycles.GetInitialStatusByName(StatusConstants.Lifecycles.Verifactu);

            invoice.ExerciseId = invoiceEntities.Exercise.Id;
            invoice.StatusId = lifecycle.InitialStatusId.Value;
            invoice.IntegrationStatusId = verifactuInitialStatusId;
            invoice.SetCustomer(invoiceEntities.Customer);
            invoice.SetSite(invoiceEntities.Site);

            await unitOfWork.SalesInvoices.Add(invoice);

            return new GenericResponse(true, invoice);
        }

        private async Task<string> GetNextInvoiceCounter(Guid exerciceId)
        {
            var counterObj = await exerciseService.GetNextCounter(exerciceId, "salesinvoice");
            if (counterObj == null || counterObj.Content == null) return string.Empty;
            var counter = counterObj.Content.ToString();
            return counter!;
        }

        public async Task<GenericResponse> CreateRectificative(CreateRectificativeInvoiceRequest dto)
        {
            var originalInvoice = await unitOfWork.SalesInvoices.Get(dto.Id);
            if (originalInvoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("InvoiceRectifyNotFound"));
            if (originalInvoice.SalesInvoiceDetails.Count == 0)
                return new GenericResponse(false, localizationService.GetLocalizedString("InvoiceRectifyNoDetails"));

            var negativeNumber = await GetNextInvoiceCounter(Guid.Parse(originalInvoice.ExerciseId!.Value.ToString()));
            var verifactuInitialStatusId = await unitOfWork.Lifecycles.GetInitialStatusByName(StatusConstants.Lifecycles.Verifactu);

            var orderId = Guid.NewGuid();
            var negativeInvoice = new SalesInvoice()
            {
                Id = orderId,
                ParentSalesInvoiceId = dto.Id,
                InvoiceNumber = negativeNumber,
                InvoiceDate = DateTime.Now,
                PaymentMethodId = originalInvoice.PaymentMethodId,
                CustomerId = originalInvoice.CustomerId,
                SiteId = originalInvoice.SiteId,
                ExerciseId = originalInvoice.ExerciseId,
                StatusId = originalInvoice.StatusId,
                IntegrationStatusId = verifactuInitialStatusId,
                Name = originalInvoice.Name,
                Address = originalInvoice.Address,
                BaseAmount = originalInvoice.BaseAmount * -1,
                City = originalInvoice.City,
                PostalCode = originalInvoice.PostalCode,
                Country = originalInvoice.Country,
                Region = originalInvoice.Region,
                VatNumber = originalInvoice.VatNumber,
                CreatedOn = DateTime.Now,
                CustomerAccountNumber = originalInvoice.CustomerAccountNumber,
                CustomerAddress = originalInvoice.CustomerAddress,
                CustomerCity = originalInvoice.CustomerCity,
                CustomerCode = originalInvoice.CustomerCode,
                CustomerComercialName = originalInvoice.CustomerComercialName,
                CustomerCountry = originalInvoice.CustomerCountry,
                CustomerPostalCode = originalInvoice.CustomerPostalCode,
                CustomerRegion = originalInvoice.CustomerRegion,
                CustomerTaxName = originalInvoice.CustomerTaxName,
                CustomerVatNumber = originalInvoice.CustomerVatNumber,
                GrossAmount = originalInvoice.GrossAmount * -1,
                TaxAmount = originalInvoice.TaxAmount * -1,
                NetAmount = originalInvoice.NetAmount * -1,
                TransportAmount = originalInvoice.TransportAmount * -1,
                SalesInvoiceDetails = originalInvoice.SalesInvoiceDetails.Select(d => new SalesInvoiceDetail()
                {
                    Id = Guid.NewGuid(),
                    SalesInvoiceId = orderId,
                    Description = d.Description,
                    Amount = d.Amount * -1,
                    Quantity = d.Quantity,
                    UnitCost = d.UnitCost * -1,
                    UnitPrice = d.UnitPrice * -1,
                    DeliveryNoteDetailId = d.DeliveryNoteDetailId,
                    TaxId = d.TaxId,
                    TotalCost = d.TotalCost * -1,
                }).ToList(),
                SalesInvoiceDueDates = originalInvoice.SalesInvoiceDueDates.Select(d => new SalesInvoiceDueDate()
                {
                    Id = Guid.NewGuid(),
                    SalesInvoiceId = orderId,
                    Amount = d.Amount * -1,
                    DueDate = d.DueDate,
                }).ToList(),
                SalesInvoiceImports = originalInvoice.SalesInvoiceImports.Select(i => new SalesInvoiceImport()
                {
                    Id = Guid.NewGuid(),
                    SalesInvoiceId = orderId,
                    BaseAmount = i.BaseAmount * -1,
                    NetAmount = i.NetAmount * -1,
                    TaxAmount = i.TaxAmount * -1,
                    TaxId = i.TaxId,
                }).ToList()
            };
            await unitOfWork.SalesInvoices.Add(negativeInvoice);

            if (!dto.CreateCorrectionInvoice)
            {
                return new GenericResponse(true, negativeInvoice);
            }

            // Crear la factura rectificativa amb l'import corregit
            var import = originalInvoice.SalesInvoiceImports.FirstOrDefault();
            var tax = await unitOfWork.Taxes.Get(import!.TaxId);
            if (tax == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("InvoiceOriginalTaxNotFound"));

            var rectificativeNumber = await GetNextInvoiceCounter(Guid.Parse(originalInvoice.ExerciseId!.Value.ToString()));
            var rectificativeInvoice = new SalesInvoice()
            {
                Id = Guid.NewGuid(),
                ParentSalesInvoiceId = dto.Id,
                InvoiceNumber = rectificativeNumber,
                InvoiceDate = DateTime.Now,
                PaymentMethodId = originalInvoice.PaymentMethodId,
                CustomerId = originalInvoice.CustomerId,
                SiteId = originalInvoice.SiteId,
                ExerciseId = originalInvoice.ExerciseId,
                StatusId = originalInvoice.StatusId,
                IntegrationStatusId = verifactuInitialStatusId,
                Name = originalInvoice.Name,
                Address = originalInvoice.Address,
                BaseAmount = originalInvoice.BaseAmount,
                City = originalInvoice.City,
                PostalCode = originalInvoice.PostalCode,
                Country = originalInvoice.Country,
                Region = originalInvoice.Region,
                VatNumber = originalInvoice.VatNumber,
                CreatedOn = DateTime.Now,
                CustomerAccountNumber = originalInvoice.CustomerAccountNumber,
                CustomerAddress = originalInvoice.CustomerAddress,
                CustomerCity = originalInvoice.CustomerCity,
                CustomerCode = originalInvoice.CustomerCode,
                CustomerComercialName = originalInvoice.CustomerComercialName,
                CustomerCountry = originalInvoice.CustomerCountry,
                CustomerPostalCode = originalInvoice.CustomerPostalCode,
                CustomerRegion = originalInvoice.CustomerRegion,
                CustomerTaxName = originalInvoice.CustomerTaxName,
                CustomerVatNumber = originalInvoice.CustomerVatNumber
            };
            await unitOfWork.SalesInvoices.Add(rectificativeInvoice);
            await AddDetail(new SalesInvoiceDetail()
            {
                Id = Guid.NewGuid(),
                SalesInvoiceId = rectificativeInvoice.Id,
                Quantity = 1,
                Description = localizationService.GetLocalizedString("InvoiceRectifyDescription", originalInvoice.InvoiceNumber),
                UnitPrice = dto.Quantity,
                Amount = dto.Quantity,
                UnitCost = dto.Quantity,
                TotalCost = dto.Quantity,
                TaxId = tax.Id,
            });
            //await GenerateDueDates(rectificativeInvoice);
            await UpdateImportsAndHeaderAmounts(rectificativeInvoice);

            return new GenericResponse(true, rectificativeInvoice);
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

            InvoiceEntities invoiceEntities;
            invoiceEntities.Exercise = exercise;
            invoiceEntities.Customer = customer;
            invoiceEntities.Site = site;
            return new GenericResponse(true, invoiceEntities);
        }

        public async Task<GenericResponse> Update(SalesInvoice invoice)
        {
            var currentInvoice = await unitOfWork.SalesInvoices.Get(invoice.Id);
            if (currentInvoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", invoice.Id));

            // Recuperar estat actual y nou
            var lifecycle = await unitOfWork.Lifecycles.GetByName(LifecycleName);
            var currentStatus = lifecycle!.Statuses!.FirstOrDefault(s => s.Id == currentInvoice.StatusId);
            var updatedStatus = lifecycle!.Statuses!.FirstOrDefault(s => s.Id == invoice.StatusId);

            // Netejar dependencies per evitar col·lisions de EF
            currentInvoice = null;
            invoice.SalesInvoiceDetails.Clear();
            invoice.SalesInvoiceImports.Clear();
            invoice.SalesInvoiceDueDates.Clear();

            // Actualizar la factura y l'albarà d'entrega relacionat
            await unitOfWork.SalesInvoices.Update(invoice);
            await GenerateDueDates(invoice);
            await UpdateRelatedDeliveryNote(invoice.Id, currentStatus!, updatedStatus!);

            return new GenericResponse(true, invoice);
        }

        public async Task<GenericResponse> UpdateCustomerDataAsync(Guid id, SalesInvoiceCustomerDataUpdateDto dto)
        {
            // Validate CIF BEFORE any persistence. UpdateCustomer is now blocking on
            // invalid CIF (issue #69 follow-up), so this guards the invoice + sibling
            // batch from being committed with a malformed CustomerVatNumber that the
            // subsequent Customer-master propagation would reject.
            if (!SpanishFiscalIdValidator.IsValidSpanishFiscalId(dto.CustomerVatNumber))
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("CustomerCifInvalid"));
            }

            var invoice = await unitOfWork.SalesInvoices.Get(id);
            if (invoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", id));

            // Hard-block if any VerifactuRequest for this invoice has Success=true —
            // once accepted by AEAT, fiscal data is sealed.
            var verifactuRequests = await unitOfWork.VerifactuRequests
                .FindAsync(r => r.SalesInvoiceId == id);
            if (verifactuRequests.Any(r => r.Success))
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("SalesInvoiceCustomerDataNotEditable"));
            }

            // Allow edits while IntegrationStatusId ∈ { Pendent, Error }.
            var pendingStatusId = await unitOfWork.Lifecycles
                .GetInitialStatusByName(StatusConstants.Lifecycles.Verifactu);
            var errorStatus = await unitOfWork.Lifecycles
                .GetStatusByName(StatusConstants.Lifecycles.Verifactu, StatusConstants.Statuses.Error);

            var allowedStatusIds = new List<Guid?>();
            if (pendingStatusId.HasValue) allowedStatusIds.Add(pendingStatusId);
            if (errorStatus != null) allowedStatusIds.Add(errorStatus.Id);

            if (invoice.IntegrationStatusId == null
                || !allowedStatusIds.Contains(invoice.IntegrationStatusId))
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("SalesInvoiceCustomerDataInvalidStatus"));
            }

            // Always copy the 9 customer fields onto this invoice header.
            invoice.CustomerComercialName = dto.CustomerComercialName;
            invoice.CustomerTaxName = dto.CustomerTaxName;
            invoice.CustomerVatNumber = dto.CustomerVatNumber;
            invoice.CustomerAccountNumber = dto.CustomerAccountNumber;
            invoice.CustomerAddress = dto.CustomerAddress;
            invoice.CustomerCity = dto.CustomerCity;
            invoice.CustomerPostalCode = dto.CustomerPostalCode;
            invoice.CustomerRegion = dto.CustomerRegion;
            invoice.CustomerCountry = dto.CustomerCountry;

            // Issue #69 follow-up: queue the main invoice together with the sibling
            // invoices in a single EF Core change-tracker batch, then commit exactly
            // once at the end. A single SaveChangesAsync is wrapped in an implicit
            // transaction by the EF Core provider, so the main invoice and every
            // sibling either all persist or none do — no more half-applied state
            // if CompleteAsync throws halfway.
            //
            // We must clear navigation properties and collections BEFORE the
            // Update call. SalesInvoiceRepository.Get uses AsNoTracking() with
            // Include(s => s.Customer) (and Site, SalesInvoiceDetails, …), so the
            // detached invoice has those nav props populated. dbSet.Update walks
            // the graph and would otherwise start tracking every reachable entity
            // — including the Customer — and a subsequent Customers.Update would
            // collide on "another instance with the same key value is already
            // being tracked". Same defensive cleanup as SalesOrderService.Update.
            invoice.Customer = null;
            invoice.Site = null;
            invoice.ParentSalesInvoice = null;
            invoice.SalesInvoiceDetails.Clear();
            invoice.SalesInvoiceImports.Clear();
            invoice.SalesInvoiceDueDates.Clear();
            invoice.VerifactuRequests.Clear();
            unitOfWork.SalesInvoices.UpdateWithoutSave(invoice);

            // Issue #69 follow-up: when requested by the user, propagate the same
            // fiscal data to every other SalesInvoice of the same Customer that is
            // still pending/errored on Verifactu. We never touch invoices that have
            // already been successfully integrated with AEAT.
            var propagatedCount = 0;
            if (dto.PropagateToAll && invoice.CustomerId.HasValue && invoice.CustomerId != Guid.Empty)
            {
                var siblingInvoices = (await unitOfWork.SalesInvoices
                    .FindAsync(s => s.CustomerId == invoice.CustomerId
                        && s.Id != invoice.Id
                        && s.IntegrationStatusId != null
                        && allowedStatusIds.Contains(s.IntegrationStatusId)))
                    .ToList();

                foreach (var sibling in siblingInvoices)
                {
                    sibling.CustomerComercialName = dto.CustomerComercialName;
                    sibling.CustomerTaxName = dto.CustomerTaxName;
                    sibling.CustomerVatNumber = dto.CustomerVatNumber;
                    sibling.CustomerAccountNumber = dto.CustomerAccountNumber;
                    sibling.CustomerAddress = dto.CustomerAddress;
                    sibling.CustomerCity = dto.CustomerCity;
                    sibling.CustomerPostalCode = dto.CustomerPostalCode;
                    sibling.CustomerRegion = dto.CustomerRegion;
                    sibling.CustomerCountry = dto.CustomerCountry;

                    // Sibling invoices come back AsNoTracking; their nav props
                    // are not populated, but collections may be initialised to
                    // empty lists. Clearing keeps the graph walk in Update
                    // predictable.
                    sibling.SalesInvoiceDetails.Clear();
                    sibling.SalesInvoiceImports.Clear();
                    sibling.SalesInvoiceDueDates.Clear();
                    sibling.VerifactuRequests.Clear();

                    unitOfWork.SalesInvoices.UpdateWithoutSave(sibling);
                    propagatedCount++;
                }
            }

            // Issue #69 follow-up: propagate the corrected fiscal data to the
            // linked Customer master record so future invoices inherit the
            // corrected values. Only fiscal fields are propagated — the
            // Customer master address (CustomerAddress collection) is the single
            // source of truth for the customer master record and is edited
            // through the dedicated Customer / Address endpoints, not from the
            // invoice screen. CIF was already validated at the top of this
            // method, and we deliberately do NOT run CustomerService's full
            // fiscal validation (which would also block on incomplete fiscal
            // address, a field we don't propagate from here).
            //
            // Repository<Customer>.Get returns the entity tracked (FindAsync).
            // We clear Address/Contacts/SalesInvoices/Budgets collections before
            // Update so the graph walk in Update does not pull in any related
            // entities that the change-tracker might already be holding onto
            // from the invoice-side Includes above.
            if (invoice.CustomerId.HasValue && invoice.CustomerId != Guid.Empty)
            {
                var customer = await unitOfWork.Customers.Get(invoice.CustomerId.Value);
                if (customer != null)
                {
                    customer.ComercialName = dto.CustomerComercialName;
                    customer.TaxName = dto.CustomerTaxName;
                    customer.VatNumber = dto.CustomerVatNumber;
                    customer.AccountNumber = dto.CustomerAccountNumber;

                    customer.Address.Clear();
                    customer.Contacts.Clear();

                    unitOfWork.Customers.UpdateWithoutSave(customer);
                }
            }

            // Commit main invoice + all queued siblings + the customer master
            // record in a single SaveChanges (implicit transaction). If the
            // commit fails we surface a failed GenericResponse to the admin
            // instead of leaving any of those three entities half-applied.
            try
            {
                await unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to commit customer fiscal data update for invoice {InvoiceId} (propagatedCount={PropagatedCount})",
                    invoice.Id, propagatedCount);
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("SalesInvoiceCustomerDataUpdateFailed"));
            }

            if (propagatedCount > 0)
            {
                logger.LogInformation(
                    "Propagated customer fiscal data to {Count} sibling invoices of customer {CustomerId}",
                    propagatedCount, invoice.CustomerId);
            }

            var message = propagatedCount > 0
                ? localizationService.GetLocalizedString(
                    "SalesInvoiceCustomerDataPropagated", propagatedCount)
                : localizationService.GetLocalizedString("SalesInvoiceCustomerDataUpdated");

            return new GenericResponse(true, message)
            {
                Content = new
                {
                    propagatedInvoiceCount = propagatedCount,
                },
            };
        }

        /// <summary>
        /// Returns the list of sibling SalesInvoices (same customer, same status
        /// set: Pendent | Error) that would be updated if the user confirms
        /// propagation. Used by the frontend to render the confirmation dialog.
        /// </summary>
        public async Task<SalesInvoiceCustomerDataUpdatePropagationResponse> GetPendingPropagationInvoicesAsync(Guid invoiceId)
        {
            var invoice = await unitOfWork.SalesInvoices.Get(invoiceId);
            if (invoice == null || !invoice.CustomerId.HasValue || invoice.CustomerId == Guid.Empty)
                return new SalesInvoiceCustomerDataUpdatePropagationResponse();

            var pendingStatusId = await unitOfWork.Lifecycles
                .GetInitialStatusByName(StatusConstants.Lifecycles.Verifactu);
            var errorStatus = await unitOfWork.Lifecycles
                .GetStatusByName(StatusConstants.Lifecycles.Verifactu, StatusConstants.Statuses.Error);

            var allowedStatusIds = new List<Guid?>();
            if (pendingStatusId.HasValue) allowedStatusIds.Add(pendingStatusId);
            if (errorStatus != null) allowedStatusIds.Add(errorStatus.Id);

            var siblings = (await unitOfWork.SalesInvoices
                .FindAsync(s => s.CustomerId == invoice.CustomerId
                    && s.Id != invoice.Id
                    && s.IntegrationStatusId != null
                    && allowedStatusIds.Contains(s.IntegrationStatusId)))
                .Select(s => s.Id)
                .ToList();

            return new SalesInvoiceCustomerDataUpdatePropagationResponse
            {
                PendingInvoicesCount = siblings.Count,
                PendingInvoiceIds = siblings,
            };
        }

        private async Task UpdateRelatedDeliveryNote(Guid invoiceId, Status currentStatus, Status updatedStatus)
        {
            // Accions relacionades amb l'albarà d'entrega
            if (currentStatus != null && updatedStatus != null && currentStatus.Id != updatedStatus.Id)
            {
                if (updatedStatus.Name == StatusConstants.Statuses.Cobrada)
                {
                    await deliveryNoteService.Invoice(invoiceId);
                }
                else if (currentStatus.Name == StatusConstants.Statuses.Cobrada)
                {
                    await deliveryNoteService.UnInvoice(invoiceId);
                }
            }
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var invoice = unitOfWork.SalesInvoices.Find(p => p.Id == id).FirstOrDefault();
            if (invoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", id));

            var invoiceDeliveryNotes = unitOfWork.DeliveryNotes.Find(d => d.SalesInvoiceId == id);
            if (invoiceDeliveryNotes != null && invoiceDeliveryNotes.Any())
            {
                foreach (var note in invoiceDeliveryNotes)
                {
                    note.SalesInvoiceId = null;
                    unitOfWork.DeliveryNotes.UpdateWithoutSave(note);
                }
                await unitOfWork.CompleteAsync();
            }

            await unitOfWork.SalesInvoices.Remove(invoice);
            return new GenericResponse(true, new List<string> { });
        }

        private async Task<GenericResponse> GenerateDueDates(SalesInvoice invoice)
        {
            var paymentMethod = await unitOfWork.PaymentMethods.Get(invoice.PaymentMethodId);
            if (paymentMethod == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("PaymentMethodNotFound"));

            var dbInvoice = await unitOfWork.SalesInvoices.Get(invoice.Id);
            if (dbInvoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNoPodeuModificar"));

            // Esborrar venciments actuals
            var currentDueDates = unitOfWork.SalesInvoices.InvoiceDueDates.Find(d => d.SalesInvoiceId == invoice.Id);
            if (currentDueDates.Any())
                await unitOfWork.SalesInvoices.InvoiceDueDates.RemoveRange(currentDueDates);

            // Generar nous venciments
            var newDueDates = new List<SalesInvoiceDueDate>();

            var dueDates = dueDateService.GenerateDueDates(paymentMethod, invoice.InvoiceDate, invoice.NetAmount);
            foreach (var dueDate in dueDates)
            {
                newDueDates.Add(new SalesInvoiceDueDate()
                {
                    SalesInvoiceId = invoice.Id,
                    Amount = dueDate.Amount,
                    DueDate = dueDate.Date
                });
            }
            if (dueDates.Any()) await unitOfWork.SalesInvoices.InvoiceDueDates.AddRange(newDueDates);

            return new GenericResponse(true, newDueDates);
        }

        public async Task<GenericResponse> ChangeStatuses(ChangeStatusOfInvoicesRequest changeStatusesRequest)
        {
            var statusToId = changeStatusesRequest.StatusToId;
            var status = await unitOfWork.Lifecycles.StatusRepository.Get(statusToId);
            if (status == null || status.Disabled)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", statusToId));
            }

            var invoices = unitOfWork.SalesInvoices.Find(pi => changeStatusesRequest.Ids.Contains(pi.Id));
            foreach (var invoice in invoices)
            {
                invoice.StatusId = statusToId;
                unitOfWork.SalesInvoices.UpdateWithoutSave(invoice);
            }
            await unitOfWork.CompleteAsync();

            return new GenericResponse(true);
        }

        #region Details    

        public async Task<GenericResponse> AddDetail(SalesInvoiceDetail invoiceDetail)
        {
            var invoice = await unitOfWork.SalesInvoices.Get(invoiceDetail.SalesInvoiceId);
            if (invoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", invoiceDetail.SalesInvoiceId));

            await unitOfWork.SalesInvoices.InvoiceDetails.Add(invoiceDetail);

            // Generar imports i actualizar imports de la capçalera
            invoice.SalesInvoiceDetails.Add(invoiceDetail);
            return await UpdateImportsAndHeaderAmounts(invoice);
        }
        public async Task<GenericResponse> UpdateDetail(SalesInvoiceDetail invoiceDetail)
        {
            var invoice = await unitOfWork.SalesInvoices.Get(invoiceDetail.SalesInvoiceId);
            if (invoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", invoiceDetail.SalesInvoiceId));

            var detail = unitOfWork.SalesInvoices.InvoiceDetails.Find(p => p.Id == invoiceDetail.Id).FirstOrDefault();
            if (detail == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceDetailNotFound", invoiceDetail.Id));

            await unitOfWork.SalesInvoices.InvoiceDetails.Update(invoiceDetail);

            // Generar imports i actualizar imports de la capçalera
            return await UpdateImportsAndHeaderAmounts(invoice);
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = unitOfWork.SalesInvoices.InvoiceDetails.Find(p => p.Id == id).FirstOrDefault();
            if (detail == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceDetailNotFound", id));

            var invoice = await unitOfWork.SalesInvoices.Get(detail.SalesInvoiceId);
            if (invoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", detail.SalesInvoiceId));

            await unitOfWork.SalesInvoices.InvoiceDetails.Remove(detail);
            var memoryDetail = invoice.SalesInvoiceDetails.First(d => d.Id == detail.Id);
            invoice.SalesInvoiceDetails.Remove(memoryDetail);

            // Generar imports i actualizar imports de la capçalera
            return await UpdateImportsAndHeaderAmounts(invoice);
        }
        #endregion

        #region Imports
        /// <summary>
        /// - Suma els imports de cada impost de les lineas de la factura
        /// - Crea un registre per impost a SalesOrderImports
        /// - Calcula els imports de la capçalera (Tax, Base, Gross, Net)
        /// </summary>
        /// <param name="invoice">SalesInvoice</param>
        private async Task<GenericResponse> UpdateImportsAndHeaderAmounts(SalesInvoice invoice, bool updateLifecycle = true)
        {
            await RemoveImports(invoice);

            // Obtenir sumatori d'imports agrupat per impost
            var invoiceImports = unitOfWork.SalesInvoices.InvoiceDetails.Find(d => d.SalesInvoiceId == invoice.Id)
                .GroupBy(d => d.TaxId)
                .Select(d => new SalesInvoiceImport()
                {
                    SalesInvoiceId = invoice.Id,
                    TaxId = d.First().TaxId,
                    BaseAmount = d.Sum(d => d.Amount),
                }).ToList();
            // Aplicar impostos
            foreach (var invoiceImport in invoiceImports)
            {
                Tax? tax = await unitOfWork.Taxes.Get(invoiceImport.TaxId);
                if (tax != null)
                {
                    invoiceImport.TaxAmount = tax.ApplyTax(invoiceImport.BaseAmount);
                    invoiceImport.NetAmount = invoiceImport.BaseAmount + invoiceImport.TaxAmount;
                }
            }
            await unitOfWork.SalesInvoices.InvoiceImports.AddRange(invoiceImports);

            invoice.SalesInvoiceImports = [.. invoiceImports];
            invoice.CalculateAmountsFromImports();
            if (updateLifecycle) return await Update(invoice);

            invoice.SalesInvoiceDetails.Clear();
            invoice.SalesInvoiceImports.Clear();
            invoice.SalesInvoiceDueDates.Clear();
            await unitOfWork.SalesInvoices.Update(invoice);
            return await GenerateDueDates(invoice);
        }
        private async Task RemoveImports(SalesInvoice invoice)
        {
            var salesImports = unitOfWork.SalesInvoices.InvoiceImports.Find(i => i.SalesInvoiceId == invoice.Id);
            if (salesImports.Any())
                await unitOfWork.SalesInvoices.InvoiceImports.RemoveRange(salesImports);
        }

        #endregion

        #region DeliveryNotes
        public async Task<GenericResponse> AddDeliveryNote(Guid id, DeliveryNote deliveryNote)
        {
            return await ExecuteInTransaction(
                () => AddDeliveryNoteInternal(id, deliveryNote.Id));
        }

        private async Task<GenericResponse> AddDeliveryNoteInternal(Guid id, Guid deliveryNoteId)
        {
            var invoice = await unitOfWork.SalesInvoices.GetHeader(id);
            if (invoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", id));

            var persistedDeliveryNote = await unitOfWork.DeliveryNotes.Get(deliveryNoteId);
            if (persistedDeliveryNote == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteNotFound", deliveryNoteId));

            var deliveredStatus = await unitOfWork.Lifecycles.GetStatusByName(
                StatusConstants.Lifecycles.DeliveryNote,
                StatusConstants.Statuses.Entregat);
            if (deliveredStatus == null || deliveredStatus.Disabled)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteStatusNotFound"));
            if (persistedDeliveryNote.StatusId != deliveredStatus.Id)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteNotDelivered"));
            if (persistedDeliveryNote.SalesInvoiceId.HasValue)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteIsInvoiced"));
            if (invoice.CustomerId != persistedDeliveryNote.CustomerId)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteInvoiceCustomerMismatch"));

            var invoicedOrderStatus = await unitOfWork.Lifecycles.GetStatusByName(
                StatusConstants.Lifecycles.SalesOrder,
                StatusConstants.Statuses.ComandaFacturada);
            if (invoicedOrderStatus == null || invoicedOrderStatus.Disabled)
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.ComandaFacturada));

            var tax = unitOfWork.Taxes.Find(t => t.Percentatge == 21).FirstOrDefault();
            if (tax == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("TaxNotFound"));

            var deliveryNoteDetails = unitOfWork.DeliveryNotes.Details
                .Find(d => d.DeliveryNoteId == persistedDeliveryNote.Id)
                .ToList();
            if (deliveryNoteDetails.Count == 0)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteHasNoDetails"));

            var invoiceDetails = new List<SalesInvoiceDetail>();
            var detailsWithoutTax = new List<(SalesInvoiceDetail Detail, Guid ReferenceId)>();
            foreach (var deliveryNoteDetail in deliveryNoteDetails)
            {
                var salesInvoiceDetail = new SalesInvoiceDetail
                {
                    SalesInvoiceId = id
                };

                salesInvoiceDetail.SetDeliveryNoteDetail(deliveryNoteDetail);
                if (salesInvoiceDetail.TaxId == Guid.Empty)
                    detailsWithoutTax.Add((salesInvoiceDetail, deliveryNoteDetail.ReferenceId));

                invoiceDetails.Add(salesInvoiceDetail);
                deliveryNoteDetail.IsInvoiced = true;
            }

            var referenceIds = detailsWithoutTax.Select(item => item.ReferenceId).ToHashSet();
            var referencesById = referenceIds.Count == 0
                ? new Dictionary<Guid, Domain.Entities.Shared.Reference>()
                : unitOfWork.References.Find(reference => referenceIds.Contains(reference.Id))
                    .ToDictionary(reference => reference.Id);
            foreach (var (detail, referenceId) in detailsWithoutTax)
                detail.TaxId = referencesById.GetValueOrDefault(referenceId)?.TaxId ?? tax.Id;
            // --- línies de factura ---
            await unitOfWork.SalesInvoices.InvoiceDetails.AddRange(invoiceDetails);

            // --- actualització de detalls d'albarà (agrupada) ---
            foreach (var deliveryNoteDetail in deliveryNoteDetails)
                unitOfWork.DeliveryNotes.Details.UpdateWithoutSave(deliveryNoteDetail);
            await unitOfWork.CompleteAsync();

            // --- recàlcul d'imports ---
            var amountResponse = await UpdateImportsAndHeaderAmounts(invoice, updateLifecycle: false);
            if (!amountResponse.Result) return amountResponse;

            persistedDeliveryNote.SalesInvoiceId = id;
            persistedDeliveryNote.SalesInvoice = null;
            persistedDeliveryNote.Details.Clear();
            unitOfWork.DeliveryNotes.UpdateWithoutSave(persistedDeliveryNote);

            var salesOrders = unitOfWork.SalesOrderHeaders
                .Find(order => order.DeliveryNoteId == persistedDeliveryNote.Id)
                .ToList();
            foreach (var salesOrder in salesOrders)
            {
                salesOrder.StatusId = invoicedOrderStatus.Id;
                salesOrder.SalesOrderDetails.Clear();
                unitOfWork.SalesOrderHeaders.UpdateWithoutSave(salesOrder);
            }
            await unitOfWork.CompleteAsync();

            return new GenericResponse(true, invoiceDetails);
        }

        public async Task<GenericResponse> RemoveDeliveryNote(Guid id, DeliveryNote deliveryNote)
        {
            return await ExecuteInTransaction(
                () => RemoveDeliveryNoteInternal(id, deliveryNote.Id));
        }

        private async Task<GenericResponse> RemoveDeliveryNoteInternal(Guid id, Guid deliveryNoteId)
        {
            var invoice = await unitOfWork.SalesInvoices.GetHeader(id);
            if (invoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", id));

            var persistedDeliveryNote = await unitOfWork.DeliveryNotes.Get(deliveryNoteId);
            if (persistedDeliveryNote == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteNotFound", deliveryNoteId));
            if (persistedDeliveryNote.SalesInvoiceId != id)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteNotInThisInvoice"));

            var deliveryNoteDetails = unitOfWork.DeliveryNotes.Details
                .Find(detail => detail.DeliveryNoteId == persistedDeliveryNote.Id)
                .ToList();
            var detailIds = deliveryNoteDetails.Select(detail => detail.Id).ToHashSet();

            var invoiceDetails = unitOfWork.SalesInvoices.InvoiceDetails
                .Find(detail => detail.SalesInvoiceId == id
                    && detail.DeliveryNoteDetailId != null
                    && detailIds.Contains(detail.DeliveryNoteDetailId.Value))
                .ToList();

            // --- determinació de l'estat objectiu (abans de mutar) ---
            var deliveredStatus = await unitOfWork.Lifecycles.GetStatusByName(
                StatusConstants.Lifecycles.DeliveryNote,
                StatusConstants.Statuses.Entregat);
            if (deliveredStatus == null || deliveredStatus.Disabled)
                return new GenericResponse(false, localizationService.GetLocalizedString("DeliveryNoteStatusNotFound"));

            var targetStatusName = persistedDeliveryNote.StatusId == deliveredStatus.Id
                ? StatusConstants.Statuses.ComandaServida
                : StatusConstants.Statuses.Comanda;
            var targetOrderStatus = await unitOfWork.Lifecycles.GetStatusByName(
                StatusConstants.Lifecycles.SalesOrder,
                targetStatusName);
            if (targetOrderStatus == null || targetOrderStatus.Disabled)
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", targetStatusName));

            // --- línies de factura ---
            await unitOfWork.SalesInvoices.InvoiceDetails.RemoveRange(invoiceDetails);

            // --- actualització de detalls d'albarà (agrupada) ---
            foreach (var deliveryNoteDetail in deliveryNoteDetails)
            {
                deliveryNoteDetail.IsInvoiced = false;
                unitOfWork.DeliveryNotes.Details.UpdateWithoutSave(deliveryNoteDetail);
            }
            await unitOfWork.CompleteAsync();

            // --- recàlcul d'imports ---
            var amountResponse = await UpdateImportsAndHeaderAmounts(invoice, updateLifecycle: false);
            if (!amountResponse.Result) return amountResponse;

            persistedDeliveryNote.SalesInvoiceId = null;
            persistedDeliveryNote.SalesInvoice = null;
            persistedDeliveryNote.Details.Clear();
            unitOfWork.DeliveryNotes.UpdateWithoutSave(persistedDeliveryNote);

            var salesOrders = unitOfWork.SalesOrderHeaders
                .Find(order => order.DeliveryNoteId == persistedDeliveryNote.Id)
                .ToList();
            foreach (var salesOrder in salesOrders)
            {
                salesOrder.StatusId = targetOrderStatus.Id;
                salesOrder.SalesOrderDetails.Clear();
                unitOfWork.SalesOrderHeaders.UpdateWithoutSave(salesOrder);
            }
            await unitOfWork.CompleteAsync();

            return new GenericResponse(true, invoiceDetails);
        }


        private async Task<GenericResponse> ExecuteInTransaction(
            Func<Task<GenericResponse>> action)
        {
            await using var transaction = await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var response = await action();
                if (!response.Result)
                {
                    await transaction.RollbackAsync();
                    return response;
                }

                await transaction.CommitAsync();
                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion
    }
}




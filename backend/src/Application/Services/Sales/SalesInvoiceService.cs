
using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Sales;

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
        ILocalizationService localizationService) : ISalesInvoiceService
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

            // Crear la factura rectificativa
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
        private async Task<GenericResponse> UpdateImportsAndHeaderAmounts(SalesInvoice invoice)
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
            return await Update(invoice);
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
            var invoice = unitOfWork.SalesInvoices.Find(i => i.Id == id).FirstOrDefault();
            if (invoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", id));

            var tax = unitOfWork.Taxes.Find(t => t.Percentatge == 21).FirstOrDefault();
            if (tax == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("TaxNotFound"));

            // Crear les lines de la factura segons les línes de l'albarà
            var deliveryNoteDetails = unitOfWork.DeliveryNotes.Details.Find(d => d.DeliveryNoteId == deliveryNote.Id);
            var invoiceDetails = new List<SalesInvoiceDetail>();
            foreach (var deliveryNoteDetail in deliveryNoteDetails.ToList())
            {
                var salesInvoiceDetail = new SalesInvoiceDetail
                {
                    SalesInvoiceId = id
                };

                salesInvoiceDetail.SetDeliveryNoteDetail(deliveryNoteDetail);
                if (salesInvoiceDetail.TaxId == Guid.Empty)
                {
                    var reference = await unitOfWork.References.Get(deliveryNoteDetail.ReferenceId);
                    if (reference != null)
                    {
                        salesInvoiceDetail.TaxId = reference.TaxId ?? tax.Id;
                    }
                    else
                    {
                        salesInvoiceDetail.TaxId = tax.Id;
                    }
                }

                invoiceDetails.Add(salesInvoiceDetail);
            }
            await unitOfWork.SalesInvoices.InvoiceDetails.AddRange(invoiceDetails);

            // Actualizar taules relacionades
            await UpdateImportsAndHeaderAmounts(invoice);

            // Associar la factura per evitar la selecció de l'albarà d'entrega a altre factures
            deliveryNote.SalesInvoiceId = id;
            await deliveryNoteService.Update(deliveryNote);

            //Marcar la comanda com a comanda facturada
            //Recuperar la comanda 
            var salesOrder = unitOfWork.SalesOrderHeaders.Find(q => q.DeliveryNoteId == deliveryNote.Id).FirstOrDefault();

            var status = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.SalesOrder, StatusConstants.Statuses.ComandaFacturada);
            if (status == null || status.Disabled)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.ComandaFacturada));
            }
            salesOrder!.StatusId = status.Id;
            await unitOfWork.SalesOrderHeaders.Update(salesOrder);

            return new GenericResponse(true, invoiceDetails);
        }

        public async Task<GenericResponse> RemoveDeliveryNote(Guid id, DeliveryNote deliveryNote)
        {
            var invoice = unitOfWork.SalesInvoices.Find(i => i.Id == id).FirstOrDefault();
            if (invoice == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("SalesInvoiceNotFound", id));

            var detailIds = unitOfWork.DeliveryNotes.Details.Find(d => d.DeliveryNoteId == deliveryNote.Id).Select(d => d.Id).ToList();

            var deliveryNoteDetails = unitOfWork.SalesInvoices.InvoiceDetails.Find(d => d.DeliveryNoteDetailId != null && detailIds.Contains(d.DeliveryNoteDetailId.Value));
            // Eliminar els detalls associats a l'albarà
            await unitOfWork.SalesInvoices.InvoiceDetails.RemoveRange(deliveryNoteDetails);

            // Actualizar taules relacionades
            await UpdateImportsAndHeaderAmounts(invoice);

            // Alliberar l'albarà perquè sigui assignable de nou a una factura
            deliveryNote.SalesInvoiceId = null;
            await deliveryNoteService.Update(deliveryNote);

            var status = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.SalesOrder, StatusConstants.Statuses.ComandaServida);
            if (status == null || status.Disabled)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.ComandaServida));
            }

            var salesOrder = unitOfWork.SalesOrderHeaders.Find(q => q.DeliveryNoteId == deliveryNote.Id).FirstOrDefault();
            if (salesOrder != null)
            {
                salesOrder.StatusId = status.Id;
                await unitOfWork.SalesOrderHeaders.Update(salesOrder);
            }

            return new GenericResponse(true, deliveryNoteDetails);
        }
        #endregion
    }
}







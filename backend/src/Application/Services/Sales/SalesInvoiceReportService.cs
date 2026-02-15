using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Services.Sales
{
    public class SalesInvoiceReportService(IUnitOfWork unitOfWork,
        ISalesOrderService salesOrderService,
        IDeliveryNoteService deliveryNoteService,
        IVerifactuIntegrationService verifactuIntegrationService,
        ILocalizationService localizationService) : ISalesInvoiceReportService
    {
        public async Task<Application.Contracts.InvoiceReportDto?> GetReportById(Guid id)
        {
            // Consultar la factura i recuperar la informació necessària
            var invoice = await unitOfWork.SalesInvoices.Get(id);
            if (invoice is null) return null;

            // Recuperar dades auxiliars
            if (!invoice.CustomerId.HasValue) return null;
            var customer = await unitOfWork.Customers.Get(invoice.CustomerId.Value);
            if (customer is null) return null;

            if (!invoice.SiteId.HasValue) return null;
            var site = await unitOfWork.Sites.Get(invoice.SiteId.Value);
            if (site is null) return null;

            var paymentMethod = await unitOfWork.PaymentMethods.Get(invoice.PaymentMethodId);
            if (paymentMethod is null) return null;

            // Obtenir peticions Verifactu
            var verifactuRequest = (await verifactuIntegrationService.GetInvoiceRequests(id))
                .Where(r => r.Success)
                .OrderByDescending(r => r.CreatedOn)
                .FirstOrDefault();

            // Crear DTO per a la factura
            var reportDto = new InvoiceReportDto(customer.PreferredLanguage, localizationService)
            {
                Date = invoice.InvoiceDate,
                Number = invoice.InvoiceNumber,
                DueDate = invoice.SalesInvoiceDueDates.OrderByDescending(d => d.DueDate).FirstOrDefault()!.DueDate,
                Total = invoice.SalesInvoiceImports.Sum(i => i.NetAmount),
                Customer = customer,
                Site = site,
                PaymentMethod = paymentMethod,
                QrCodeUrl = verifactuRequest?.QrCodeUrl ?? string.Empty,
                QrCodeReportTag = !string.IsNullOrWhiteSpace(verifactuRequest?.QrCodeUrl) ? "+++IMAGE qrCodeData+++" : string.Empty,
                Imports = [.. invoice.SalesInvoiceImports.Select(import => new InvoiceReportDtoTaxImport()
                {
                    Name = import.Tax!.Name,
                    BaseAmount = import.BaseAmount,
                    NetAmount = import.NetAmount,
                    TaxAmount = import.TaxAmount,
                    Percentatge = import.Tax!.Percentatge
                })]
            };

            // Obtenir albarans de la factura
            var deliveryNotes = deliveryNoteService.GetBySalesInvoice(id).OrderBy(d => d.Number);

            // Obtener referencies de les línies de l'albarà
            var referenceIds = deliveryNotes.SelectMany(deliveryNotes => deliveryNotes.Details).Select(d => d.ReferenceId).ToList();
            var references = unitOfWork.References.Find(r => referenceIds.Contains(r.Id)).ToList();

            foreach (var deliveryNote in deliveryNotes)
            {
                var orders = salesOrderService.GetByDeliveryNoteId(deliveryNote.Id);
                var customerOrderNumbers = string.Join(",", [.. orders.Select(orders => orders.CustomerNumber)]);
                var deliveryDate = (deliveryNote.DeliveryDate ?? DateTime.Now).ToString("dd/MM/yyyy");

                reportDto.DeliveryNotes.Add(new InvoiceReportDtoDeliveryNote()
                {
                    Header = localizationService.GetLocalizedStringForCulture(
                        "Report.SalesInvoice.DeliveryNoteHeader",
                        customer.PreferredLanguage,
                        deliveryNote.Number,
                        deliveryDate,
                        customerOrderNumbers),
                    Number = deliveryNote.Number,
                    Date = deliveryNote.CreatedOn,
                    Details = [.. deliveryNote.Details.Select(detail => new SalesInvoiceDetail()
                    {
                        Amount = detail?.Amount ?? 0,
                        Description = $"{references.FirstOrDefault(r => r.Id == detail!.ReferenceId)!.GetShortName()} - {detail?.Description}",
                        Quantity = detail?.Quantity ?? 0,
                        TotalCost = detail?.TotalCost ?? 0,
                        UnitCost = detail?.UnitCost ?? 0,
                        UnitPrice = detail?.UnitPrice ?? 0
                    })],
                    Total = deliveryNote.Details.Sum(detail => detail.Amount)
                });
            }

            // Linies lliures
            var hasDetailsWithoutDeliveryNote = invoice.SalesInvoiceDetails.Any(d => d.DeliveryNoteDetailId == null);
            if (hasDetailsWithoutDeliveryNote)
            {
                reportDto.DeliveryNotes.Add(new InvoiceReportDtoDeliveryNote()
                {
                    Header = localizationService.GetLocalizedStringForCulture("DeliveryNoteWithoutAlbaran", customer.PreferredLanguage),
                    Date = invoice.InvoiceDate,
                    Number = "--",
                    Details = [.. invoice.SalesInvoiceDetails.Where(d => d.DeliveryNoteDetailId == null)],
                    Total = invoice.SalesInvoiceDetails.Where(d => d.DeliveryNoteDetailId == null).Sum(d => d.Amount)
                });
            }

            return reportDto;
        }
    }
}










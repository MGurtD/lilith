using Application.Contracts;
using Application.Services;
using Domain.Entities.Purchase;
using Domain.Entities.Shared;
using System.Text;

namespace Application.Services.System
{
    public class ReferenceService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IReferenceService
    {
        // New read operations
        public async Task<IEnumerable<Reference>> GetAllReferences()
        {
            var references = await unitOfWork.References.GetAll();
            return references.OrderBy(r => r.Code);
        }

        public async Task<IEnumerable<Reference>> GetAllSales()
        {
            var references = unitOfWork.References.Find(r => r.Sales);
            return references.OrderBy(r => r.Code);
        }

        public async Task<IEnumerable<Reference>> GetAllSalesByCustomerId(Guid customerId)
        {
            var references = unitOfWork.References.Find(r => r.CustomerId == customerId);
            return references.OrderBy(r => r.Code);
        }

        public async Task<IEnumerable<Reference>> GetAllPurchase(string? categoryName = null)
        {
            IEnumerable<Reference> references;
            if (string.IsNullOrEmpty(categoryName))
            {
                references = unitOfWork.References.Find(r => r.Purchase);
            }
            else
            {
                references = unitOfWork.References.Find(r => r.Purchase && r.CategoryName == categoryName);
            }
            return references.OrderBy(r => r.Code);
        }

        public async Task<IEnumerable<Reference>> GetAllProduction()
        {
            var references = unitOfWork.References.Find(r => r.Production);
            return references.OrderBy(r => r.Code);
        }

        public async Task<Reference?> GetReferenceById(Guid id)
        {
            return await unitOfWork.References.Get(id);
        }

        public async Task<IEnumerable<SupplierReference>> GetReferenceSuppliers(Guid referenceId)
        {
            var supplierReferences = await unitOfWork.Suppliers.GetSuppliersReferencesFromReference(referenceId);
            return supplierReferences;
        }

        public async Task<IEnumerable<ReferenceFormat>> GetReferenceFormats()
        {
            var formats = await unitOfWork.ReferenceFormats.GetAll();
            return formats.OrderBy(f => f.Code);
        }

        public async Task<ReferenceFormat?> GetReferenceFormatById(Guid id)
        {
            return await unitOfWork.ReferenceFormats.Get(id);
        }

        // New write operations
        public async Task<GenericResponse> CreateReference(Reference reference)
        {
            // Check if reference already exists
            var exists = unitOfWork.References.Find(r => r.Id == reference.Id).Any();
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityAlreadyExists"));
            }

            // Format the code with reference type if applicable
            reference.Code = await GetReferenceCode(reference);

            // Persist
            await unitOfWork.References.Add(reference);
            return new GenericResponse(true, reference);
        }

        public async Task<GenericResponse> UpdateReference(Reference reference)
        {
            // Clear navigation properties to avoid EF tracking issues
            reference.ReferenceType = null;
            reference.ReferenceFormat = null;
            reference.Tax = null;
            reference.Customer = null;

            // Check if reference exists
            var existing = await unitOfWork.References.Get(reference.Id);
            if (existing == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", reference.Id));
            }

            // Format the code with reference type if applicable
            reference.Code = await GetReferenceCode(reference);

            // Update
            await unitOfWork.References.Update(reference);
            return new GenericResponse(true, reference);
        }

        public async Task<GenericResponse> RemoveReference(Guid id)
        {
            var reference = unitOfWork.References.Find(r => r.Id == id).FirstOrDefault();
            if (reference == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.References.Remove(reference);
            return new GenericResponse(true, reference);
        }

        // Private helper method
        private async Task<string> GetReferenceCode(Reference reference)
        {
            if (reference.ReferenceTypeId.HasValue)
            {
                var referenceType = await unitOfWork.ReferenceTypes.Get(reference.ReferenceTypeId.Value);
                if (referenceType is not null && !reference.Code.Contains($" ({referenceType.Name})"))
                {
                    var codeParts = reference.Code.Split(" (");
                    if (codeParts.Length >= 1)
                    {
                        return $"{codeParts[0]} ({referenceType.Name})";
                    }
                    else
                    {
                        return $"{reference.Code} ({referenceType.Name})";
                    }
                }
            }

            return reference.Code;
        }

        // Existing methods
        public GenericResponse CanDelete(Guid referenceId)
        {
            var sb = new StringBuilder();

            var resp = true;
            sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.Header"));

            if (unitOfWork.SalesOrderDetails.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.PurchaseOrders"));
            }
            if (unitOfWork.Receipts.Details.Find(p => p.ReferenceId.Equals(referenceId)).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.Receipts"));
            }
            if (unitOfWork.StockMovements.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.StockMovements"));
            }
            if (unitOfWork.WorkMasters.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.ProductionRoute"));
            }
            if (unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.BillOfMaterials"));
            }

            return new GenericResponse(resp, sb.ToString());
        }

        public async Task<List<Reference>> GetReferenceByCategory(string categoryName)
        {
            var categoryReferences = await unitOfWork.References.FindAsync(r => r.CategoryName == categoryName);
            return categoryReferences;
        }

        public async Task<GenericResponse> UpdatePriceFromReceipt(Receipt receipt)
        {
            if (receipt == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("InvoiceInvalidFormat"));
            }
            var supplierId = receipt.SupplierId;
            var format = new ReferenceFormat();
            foreach (ReceiptDetail detail in receipt.Details)
            {
                detail.Reference = await unitOfWork.References.Get(detail.ReferenceId);
                if (detail.Reference != null && detail.Reference.ReferenceFormatId.HasValue)
                {
                    format = await unitOfWork.ReferenceFormats.Get(detail.Reference.ReferenceFormatId.Value);
                }
                decimal price;
                if (detail.Reference!.CategoryName == "Material")
                {
                    if (format!.Code != "UNITATS")
                    {
                        price = detail.KilogramPrice;
                    }
                    else
                    {
                        price = detail.UnitPrice;
                    }

                }
                else
                {
                    price = detail.Amount / detail.Quantity;
                }
                detail.Reference.LastCost = price;

                var supplierreference = await unitOfWork.Suppliers.GetSupplierReferenceBySupplierAndId(detail.ReferenceId, supplierId);
                if (supplierreference == null)
                {
                                        
                    var newsupplierReference = new SupplierReference()
                    {
                        Id = Guid.NewGuid(),
                        ReferenceId = detail.ReferenceId,
                        SupplierId = supplierId,
                        SupplierCode = detail.Reference.Code,
                        SupplierDescription = detail.Reference.Description,
                        SupplierPrice = price,
                        SupplyDays = 0
                    };
                    await unitOfWork.Suppliers.AddSupplierReference(newsupplierReference);
                }
                else
                {
                    supplierreference.SupplierPrice = price;
                    await unitOfWork.Suppliers.UpdateSupplierReference(supplierreference);
                }
                await unitOfWork.References.Update(detail.Reference);
            }
            return new GenericResponse(true);

        }
        public async Task<decimal> GetPrice(Guid referenceId, Guid? supplierId)
        {
            var supplierReference = new SupplierReference();
            if (supplierId != null)
                supplierReference = await unitOfWork.Suppliers.GetSupplierReferenceBySupplierAndId(referenceId, supplierId.Value);

            var reference = await unitOfWork.References.Get(referenceId);
            if (supplierReference != null)
                return supplierReference.SupplierPrice;
            if (reference != null)
                return reference.Price;

            return decimal.Zero;
        }
    }
}






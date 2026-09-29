using Application.Contracts;
using Domain.Constants;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Transport;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class SupplierRepository : Repository<Supplier, Guid>, ISupplierRepository
    {
        private readonly IRepository<SupplierContact, Guid> _supplierContactRepository;
        private readonly IRepository<SupplierReference, Guid> _supplierReferenceRepository;

        public SupplierRepository(ApplicationDbContext context) : base(context)
        {
            _supplierContactRepository = new Repository<SupplierContact, Guid>(context);
            _supplierReferenceRepository = new Repository<SupplierReference, Guid>(context);
        }

        public override async Task<Supplier?> Get(Guid id)
        {
            return await dbSet.Include(s => s.Contacts).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public override async Task<IEnumerable<Supplier>> GetAll()
        {
            return await dbSet.Include(s => s.Contacts).AsNoTracking().ToListAsync();
        }

        public SupplierContact? GetContactById(Guid id)
        {
            var contact = _supplierContactRepository.Find(c => c.Id == id).FirstOrDefault();
            return contact;
        }

        public async Task AddContact(SupplierContact contact)
        {
            await _supplierContactRepository.Add(contact);
        }

        public async Task RemoveContact(SupplierContact contact)
        {
            await _supplierContactRepository.Remove(contact);
        }

        public async Task UpdateContact(SupplierContact contact)
        {
            await _supplierContactRepository.Update(contact);
        }

        public IEnumerable<SupplierReference> GetSupplierReferences(Guid id)
        {
            return _supplierReferenceRepository.Find(r => r.SupplierId == id);
        }

        public IEnumerable<Supplier> GetLogisticSuppliers()
        {
            return dbSet.Include(s => s.Type)
                        .Where(s => s.Type != null && s.Type.Name == SupplierTypeConstants.Logistic)
                        .ToList();
        }

        public async Task<SupplierReference?> GetSupplierReferenceById(Guid supplierReferenceId)
        {
            return await _supplierReferenceRepository.Get(supplierReferenceId);
        }
        public async Task<SupplierReference?> GetSupplierReferenceBySupplierAndId(Guid referenceId, Guid supplierId)
        {
            var supplierReference = (await _supplierReferenceRepository.FindAsync(r => r.SupplierId == supplierId && r.ReferenceId == referenceId)).FirstOrDefault();
            return supplierReference;
        }

        public async Task AddSupplierReference(SupplierReference reference)
        {
            await _supplierReferenceRepository.Add(reference);
        }

        public async Task UpdateSupplierReference(SupplierReference reference)
        {
            await _supplierReferenceRepository.Update(reference);
        }

        public async Task RemoveSupplierReference(SupplierReference reference)
        {
            await _supplierReferenceRepository.Remove(reference);
        }

        public IEnumerable<Supplier> GetReferenceSuppliers(Guid referenceId)
        {
            var supplierReferences = _supplierReferenceRepository.Find(sr => sr.ReferenceId == referenceId).ToList();

            var supplierIds = supplierReferences.Select(sr => sr.SupplierId).Distinct();
            
            var suppliers = dbSet.Where(s => supplierIds.Contains(s.Id) && s.Disabled == false).ToList();

            return suppliers;            
        }

        public async Task<List<SupplierReference>> GetSuppliersReferencesFromReference(Guid referenceId)
        {
            var supplierReferences = await _supplierReferenceRepository.FindAsync(sr => sr.ReferenceId == referenceId);
            return supplierReferences;

        }

        public async Task<SupplierReference?> GetSupplierReferenceBySupplierIdAndReferenceId(Guid supplierId, Guid referenceId)
        {
            var supplierReference = (await _supplierReferenceRepository.FindAsync(s => s.SupplierId == supplierId && s.ReferenceId == referenceId)).FirstOrDefault();
            return supplierReference;
        }

        public async Task<List<string>> GetAccountNumbersUsedInPurchaseInvoices()
        {
            return await context.Set<PurchaseInvoice>()
                .Include(pi => pi.Supplier)
                .Where(pi => pi.Supplier != null && pi.Supplier.AccountNumber != string.Empty)
                .Select(pi => pi.Supplier!.AccountNumber)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }

        // Purchase orders and invoices reference the supplier with a cascading
        // foreign key and receipts have no foreign key at all, so deleting a supplier
        // in use would silently delete or orphan them. Transport rates and the budget
        // and sales order external services and transports have no foreign key either.
        public async Task<bool> IsInUse(Guid supplierId)
        {
            return await context.Set<PurchaseOrder>().AnyAsync(e => e.SupplierId == supplierId)
                || await context.Set<Receipt>().AnyAsync(e => e.SupplierId == supplierId)
                || await context.Set<PurchaseInvoice>().AnyAsync(e => e.SupplierId == supplierId)
                || await context.Set<PurchaseRate>().AnyAsync(e => e.SupplierId == supplierId)
                || await context.Set<TransportRate>().AnyAsync(e => e.SupplierId == supplierId)
                || await context.Set<BudgetExternalServices>().AnyAsync(e => e.SupplierId == supplierId)
                || await context.Set<BudgetTransport>().AnyAsync(e => e.LogisticSupplierId == supplierId || e.DestinationSupplierId == supplierId)
                || await context.Set<SalesOrderExternalServices>().AnyAsync(e => e.SupplierId == supplierId);
        }
    }
}

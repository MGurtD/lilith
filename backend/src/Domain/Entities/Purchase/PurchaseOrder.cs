namespace Domain.Entities.Purchase;

public class PurchaseOrder : Entity
{
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public Guid ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }
    //public Guid? SiteId { get; set; }
    //public Site? Site { get; set; }
    public Guid StatusId { get; set; }
    public Status? Status { get; set; }

    public ICollection<PurchaseOrderDetail> Details { get; set; } = [];
}
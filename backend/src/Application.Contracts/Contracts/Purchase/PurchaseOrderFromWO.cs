using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class PurchaseOrderFromWO
{
    [Required]
    public Guid WorkorderId { get; set; }
    [Required]
    public required string WorkorderDescription {  get; set; }
    [Required]
    public Guid PhaseId { get; set; }
    [Required]
    public required string PhaseDescription { get; set; }
    [Required]
    public Guid ServiceReferenceId { get; set; }
    [Required]
    public required string ServiceReferenceName { get; set; }
    [Required]
    public Guid SupplierId { get; set; }
    [Required]
    public int Quantity { get; set; }
}
